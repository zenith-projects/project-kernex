using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using ProjectKernex.Resources.AI;

namespace ProjectKernex.Systems.AI;

/// <summary>
/// Generic AI agent runtime. Handles model access (via ModelPool), memory,
/// context injection, prompt assembly, inference, and sub-agents.
/// </summary>
public class AgentRunner : IDisposable
{
    public AgentConfig Config { get; }
    public AgentMemoryStore Memory { get; }
    public LlamaSharpProvider Provider { get; private set; }
    public bool IsProcessing { get; private set; }
    public IReadOnlyList<AgentRunner> SubAgents => _subAgents;

    private readonly List<AgentRunner> _subAgents = new();
    private readonly Dictionary<string, string> _context = new();
    private bool _disposed;

    /// <summary>
    /// Create an agent runner from config.
    /// If sharedMemory is provided, uses it instead of creating a new one.
    /// </summary>
    public AgentRunner(AgentConfig config, AgentMemoryStore sharedMemory = null)
    {
        Config = config;

        if (sharedMemory != null)
        {
            Memory = sharedMemory;
        }
        else if (config.MemoryEnabled)
        {
            var memDir = $"user://agent_memory/{config.AgentId}/";
            Memory = new AgentMemoryStore(memDir, config.DisplayName);
            Memory.Load();
        }
    }

    /// <summary>
    /// Load the model from ModelPool. Returns false if loading fails.
    /// </summary>
    public async Task<bool> LoadModelAsync()
    {
        if (string.IsNullOrWhiteSpace(Config.ModelPath))
            return false;

        Provider = await ModelPool.AcquireAsync(Config.ModelPath, Config.GpuLayers, Config.ContextSize);
        return Provider != null;
    }

    /// <summary>
    /// Release the model back to the pool.
    /// </summary>
    public void UnloadModel()
    {
        if (Provider != null && !string.IsNullOrWhiteSpace(Config.ModelPath))
        {
            ModelPool.Release(Config.ModelPath);
            Provider = null;
        }
    }

    // --- Context ---

    public void SetContext(string key, string value) => _context[key] = value;

    public void SetContext(Dictionary<string, string> context)
    {
        _context.Clear();
        foreach (var (key, value) in context)
            _context[key] = value;
    }

    public Dictionary<string, string> GetContext() => new(_context);

    public void ClearContext() => _context.Clear();

    // --- Inference ---

    /// <summary>
    /// Generate a response. Applies inference params from Config, builds prompt
    /// with context and memory, calls the model, saves to memory.
    /// Returns raw LLM output (caller handles parsing).
    /// </summary>
    public async Task<string> GetResponseAsync(string userMessage, CancellationToken ct = default)
    {
        if (Provider is not { IsModelLoaded: true })
            return $"[{Config.DisplayName}: model not loaded]";

        if (IsProcessing)
            return $"[{Config.DisplayName}: busy]";

        IsProcessing = true;

        try
        {
            // Response delay with jitter
            if (Config.ResponseDelay > 0)
            {
                var jitter = (float)(Random.Shared.NextDouble() * 0.3 * Config.ResponseDelay);
                await Task.Delay(TimeSpan.FromSeconds(Config.ResponseDelay + jitter), ct);
            }

            ApplyInferenceParams();

            var systemPrompt = BuildPrompt(userMessage);
            var history = Memory?.GetRecentAsTuples(15);

            // Wait for model access (queued via SemaphoreSlim)
            await ModelPool.WaitForAccessAsync(Config.ModelPath, ct);
            try
            {
                var raw = await Provider.GenerateWithHistoryAsync(systemPrompt, history, userMessage, ct);

                Memory?.SaveMessage("user", userMessage);
                Memory?.SaveMessage("assistant", raw);

                return raw;
            }
            finally
            {
                ModelPool.ReleaseAccess(Config.ModelPath);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Generate with a custom system prompt (overrides Config.SystemPrompt).
    /// Used by KiraEngine to inject KiraPromptBuilder output.
    /// </summary>
    public async Task<string> GetResponseAsync(string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        if (Provider is not { IsModelLoaded: true })
            return $"[{Config.DisplayName}: model not loaded]";

        if (IsProcessing)
            return $"[{Config.DisplayName}: busy]";

        IsProcessing = true;

        try
        {
            ApplyInferenceParams();

            var history = Memory?.GetRecentAsTuples(15);

            await ModelPool.WaitForAccessAsync(Config.ModelPath, ct);
            try
            {
                return await Provider.GenerateWithHistoryAsync(systemPrompt, history, userMessage, ct);
            }
            finally
            {
                ModelPool.ReleaseAccess(Config.ModelPath);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    // --- Sub-agents ---

    /// <summary>
    /// Create a child agent. If shareMemory is true, the child uses this agent's memory.
    /// </summary>
    public AgentRunner AddSubAgent(AgentConfig config, bool shareMemory = false)
    {
        var memory = shareMemory ? Memory : null;
        var sub = new AgentRunner(config, sharedMemory: memory);
        _subAgents.Add(sub);
        return sub;
    }

    public AgentRunner GetSubAgent(string agentId) =>
        _subAgents.Find(s => s.Config.AgentId == agentId);

    // --- Internal ---

    private void ApplyInferenceParams()
    {
        Provider.Temperature = Config.Temperature;
        Provider.TopP = Config.TopP;
        Provider.TopK = Config.TopK;
        Provider.MinP = Config.MinP;
        Provider.MaxTokens = Config.MaxTokens;
        Provider.RepeatPenalty = Config.RepeatPenalty;
        Provider.FrequencyPenalty = Config.FrequencyPenalty;
        Provider.PresencePenalty = Config.PresencePenalty;
        Provider.Seed = Config.Seed;
        Provider.MirostatMode = Config.MirostatMode;
        Provider.MirostatTau = Config.MirostatTau;
        Provider.MirostatEta = Config.MirostatEta;
        Provider.AntiPrompts = Config.GetAntiPromptsArray();
    }

    private string BuildPrompt(string userMessage)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Config.SystemPrompt))
            sb.AppendLine(Config.SystemPrompt);

        // Inject game context filtered by allowed keys
        if (Config.AllowedContextKeys is { Length: > 0 } && _context.Count > 0)
        {
            var parts = new List<string>();
            foreach (var key in Config.AllowedContextKeys)
            {
                if (_context.TryGetValue(key, out var value))
                    parts.Add($"{key} {value}");
            }
            if (parts.Count > 0)
                sb.AppendLine($"Current state: {string.Join(", ", parts)}.");
        }

        // Inject memory block
        var memoryBlock = Memory?.BuildMemoryBlock(userMessage) ?? "";
        if (!string.IsNullOrWhiteSpace(memoryBlock))
        {
            sb.AppendLine();
            sb.Append(memoryBlock);
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var sub in _subAgents)
            sub.Dispose();
        _subAgents.Clear();

        UnloadModel();
    }
}
