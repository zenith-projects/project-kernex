using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using LLama;
using LLama.Common;
using LLama.Sampling;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.AI;

public sealed class LlamaSharpProvider : ILlmProvider, IDisposable
{
    public bool IsModelLoaded => _model != null;
    public bool IsProcessing => _processing > 0;

    // Model params (set BEFORE LoadModelAsync)
    public int GpuLayerCount { get; set; } = 0;
    public uint ContextSize { get; set; } = 2048;

    // Inference params (can change between calls)
    public float Temperature { get; set; } = 0.7f;
    public float TopP { get; set; } = 0.9f;
    public int TopK { get; set; } = 40;
    public float MinP { get; set; } = 0.05f;
    public int MaxTokens { get; set; } = 256;
    public float RepeatPenalty { get; set; } = 1.1f;
    public float FrequencyPenalty { get; set; } = 0.0f;
    public float PresencePenalty { get; set; } = 0.0f;
    public int Seed { get; set; } = -1;
    public int MirostatMode { get; set; } = 0;
    public float MirostatTau { get; set; } = 5.0f;
    public float MirostatEta { get; set; } = 0.1f;
    public string[] AntiPrompts { get; set; } = ["User:", "\nUser:", "Operator:"];

    private LLamaWeights _model;
    private LLamaContext _context;
    private ModelParams _modelParams;
    private int _processing;

    public async Task<bool> LoadModelAsync(string modelPath)
    {
        try
        {
            UnloadModel();

            _modelParams = new ModelParams(modelPath)
            {
                ContextSize = ContextSize,
                GpuLayerCount = GpuLayerCount,
            };

            _model = await LLamaWeights.LoadFromFileAsync(_modelParams);
            _context = _model.CreateContext(_modelParams);
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[LlamaSharp] Failed to load model: {ex.Message}");
            UnloadModel();
            return false;
        }
    }

    public Task<string> GenerateAsync(string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        return GenerateWithHistoryAsync(systemPrompt, null, userMessage, ct);
    }

    public async Task<string> GenerateWithHistoryAsync(string systemPrompt, List<(string role, string content)> history, string userMessage, CancellationToken ct = default)
    {
        if (!IsModelLoaded)
            return "[LLM not loaded]";

        if (Interlocked.CompareExchange(ref _processing, 1, 0) != 0)
            return "[LLM is busy]";

        try
        {
            return await Task.Run(async () =>
            {
                // Clear KV cache to ensure fresh context every call
                _context.NativeHandle.MemoryClear(true);

                var executor = new InteractiveExecutor(_context);

                var pipeline = new DefaultSamplingPipeline
                {
                    Temperature = Temperature,
                    TopP = TopP,
                    TopK = TopK,
                    MinP = MinP,
                    RepeatPenalty = RepeatPenalty,
                    FrequencyPenalty = FrequencyPenalty,
                    PresencePenalty = PresencePenalty,
                };

                if (Seed >= 0)
                    pipeline.Seed = (uint)Seed;

                var inferenceParams = new InferenceParams
                {
                    SamplingPipeline = pipeline,
                    MaxTokens = MaxTokens,
                    AntiPrompts = AntiPrompts.ToList(),
                };

                // Build ChatML prompt with conversation history
                var prompt = new StringBuilder();
                prompt.Append($"<|im_start|>system\n{systemPrompt}<|im_end|>\n");

                if (history != null)
                {
                    foreach (var (role, content) in history)
                        prompt.Append($"<|im_start|>{role}\n{content}<|im_end|>\n");
                }

                prompt.Append($"<|im_start|>user\n{userMessage}<|im_end|>\n<|im_start|>assistant\n");

                var sb = new StringBuilder();
                await foreach (var token in executor.InferAsync(prompt.ToString(), inferenceParams, ct))
                {
                    sb.Append(token);
                    if (sb.Length > MaxTokens * 8) break;
                }

                return CleanResponse(sb.ToString());
            }, ct);
        }
        finally
        {
            Interlocked.Exchange(ref _processing, 0);
        }
    }

    private string CleanResponse(string raw)
    {
        var text = raw.Trim();

        // Strip role prefixes from the start
        string[] prefixes = ["System:", "KIRA:", "Assistant:", "Bot:"];
        foreach (var p in prefixes)
        {
            if (text.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                text = text[p.Length..].TrimStart();
        }

        // Remove anti-prompt leaks
        foreach (var ap in AntiPrompts)
        {
            var idx = text.IndexOf(ap, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                text = text[..idx].Trim();
        }

        // Remove role-play artifacts (LLM continuing as another role)
        string[] cutPoints = ["You:", "KIRA:", "Assistant:", "System:", "User:", "Operator:", "<|", "###", "\nNote:", "\nNote "];
        foreach (var a in cutPoints)
        {
            var idx = text.IndexOf(a, StringComparison.OrdinalIgnoreCase);
            if (idx > 10)
                text = text[..idx].Trim();
        }

        // Remove asterisk roleplay actions like *smiles*, *nods*
        text = Regex.Replace(text, @"\*[^*]+\*", "").Trim();

        // Collapse multiple spaces/newlines
        text = Regex.Replace(text, @"\s{2,}", " ").Trim();

        return text;
    }

    public void UnloadModel()
    {
        _context?.Dispose();
        _context = null;
        _model?.Dispose();
        _model = null;
        _modelParams = null;
    }

    public void Dispose() => UnloadModel();
}
