using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.AI;

namespace ProjectKernex.Systems.AI;

public sealed class KiraEngine : IDisposable
{
    public KiraLevel CurrentLevel { get; private set; } = KiraLevel.Corrupted;
    public bool IsProcessing { get; private set; }
    public AgentRunner MainAgent { get; }

    private readonly ScriptedDialogueProvider _scriptedProvider;
    private readonly Dictionary<int, KiraLevelData> _levelData = new();

    public KiraEngine(AgentConfig mainConfig, ScriptedDialogueProvider scriptedProvider)
    {
        _scriptedProvider = scriptedProvider;
        MainAgent = new AgentRunner(mainConfig);
    }

    public void SetLevel(KiraLevel level) => CurrentLevel = level;

    public void RegisterLevelData(KiraLevelData data) => _levelData[(int)data.Level] = data;

    public void UpdateGameContext(string key, string value) => MainAgent.SetContext(key, value);

    public void UpdateGameContext(Dictionary<string, string> context) => MainAgent.SetContext(context);

    public void ClearGameContext() => MainAgent.ClearContext();

    public KiraLevelData GetCurrentLevelData() =>
        _levelData.TryGetValue((int)CurrentLevel, out var data) ? data : null;

    public async Task<KiraResponse> GetResponseAsync(string userMessage, CancellationToken ct = default)
    {
        if (IsProcessing) return KiraResponse.Parse("[KIRA is still processing...]");
        IsProcessing = true;

        try
        {
            var levelData = GetCurrentLevelData();
            var delay = levelData?.ResponseDelay ?? GetDefaultDelay();

            if (delay > 0)
            {
                var jitter = (float)(Random.Shared.NextDouble() * 0.3 * delay);
                await Task.Delay(TimeSpan.FromSeconds(delay + jitter), ct);
            }

            string raw;
            if ((int)CurrentLevel <= 2)
            {
                raw = await _scriptedProvider.GetResponseAsync(userMessage, CurrentLevel, MainAgent.GetContext());
            }
            else if (MainAgent.Provider is { IsModelLoaded: true })
            {
                raw = await GetLlmResponseAsync(userMessage, levelData, ct);
            }
            else
            {
                raw = await _scriptedProvider.GetResponseAsync(userMessage, CurrentLevel, MainAgent.GetContext());
            }

            var response = KiraResponse.Parse(raw);

            MainAgent.Memory?.SaveMessage("user", userMessage);
            MainAgent.Memory?.SaveMessage("assistant", response.Text, response.Emotion);

            return response;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task<string> GetLlmResponseAsync(string userMessage, KiraLevelData levelData, CancellationToken ct)
    {
        var memoryBlock = MainAgent.Memory?.BuildMemoryBlock(userMessage) ?? "";
        var systemPrompt = KiraPromptBuilder.Build(CurrentLevel, MainAgent.GetContext(), levelData, memoryBlock);

        // Use the overload that accepts a custom system prompt
        return await MainAgent.GetResponseAsync(systemPrompt, userMessage, ct);
    }

    private float GetDefaultDelay() => CurrentLevel switch
    {
        KiraLevel.Corrupted => 1.5f,
        KiraLevel.Booting => 0.8f,
        KiraLevel.Functional => 0.3f,
        _ => 0.1f
    };

    public void Dispose() => MainAgent.Dispose();
}
