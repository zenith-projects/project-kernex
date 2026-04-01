using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectKernex.Core.Enums;
using ProjectKernex.Core.Interfaces;
using ProjectKernex.Resources.AI;

namespace ProjectKernex.Systems.AI;

public sealed class KiraEngine
{
    private static readonly Random Rng = new();

    public KiraLevel CurrentLevel { get; private set; } = KiraLevel.Corrupted;
    public bool IsProcessing { get; private set; }

    private readonly ScriptedDialogueProvider _scriptedProvider;
    private ILlmProvider _llmProvider;
    private readonly Dictionary<string, string> _gameContext = new();
    private readonly Dictionary<int, KiraLevelData> _levelData = new();

    public KiraEngine(ScriptedDialogueProvider scriptedProvider)
    {
        _scriptedProvider = scriptedProvider;
    }

    public void SetLevel(KiraLevel level) => CurrentLevel = level;

    public void RegisterLevelData(KiraLevelData data) => _levelData[(int)data.Level] = data;

    public void SetLlmProvider(ILlmProvider provider) => _llmProvider = provider;

    public void UpdateGameContext(string key, string value) => _gameContext[key] = value;

    public void ClearGameContext() => _gameContext.Clear();

    public KiraLevelData GetCurrentLevelData() =>
        _levelData.TryGetValue((int)CurrentLevel, out var data) ? data : null;

    public async Task<string> GetResponseAsync(string userMessage, CancellationToken ct = default)
    {
        if (IsProcessing) return "[KIRA is still processing...]";
        IsProcessing = true;

        try
        {
            var levelData = GetCurrentLevelData();
            var delay = levelData?.ResponseDelay ?? GetDefaultDelay();

            if (delay > 0)
            {
                var jitter = (float)(Rng.NextDouble() * 0.3 * delay);
                await Task.Delay(TimeSpan.FromSeconds(delay + jitter), ct);
            }

            if ((int)CurrentLevel <= 2)
                return await _scriptedProvider.GetResponseAsync(userMessage, CurrentLevel, _gameContext);

            if (_llmProvider is { IsModelLoaded: true })
                return await GetLlmResponseAsync(userMessage, levelData, ct);

            return await _scriptedProvider.GetResponseAsync(userMessage, CurrentLevel, _gameContext);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task<string> GetLlmResponseAsync(string userMessage, KiraLevelData levelData, CancellationToken ct)
    {
        var systemPrompt = KiraPromptBuilder.Build(CurrentLevel, _gameContext, levelData);
        return await _llmProvider.GenerateAsync(systemPrompt, userMessage, ct);
    }

    private float GetDefaultDelay() => CurrentLevel switch
    {
        KiraLevel.Corrupted => 1.5f,
        KiraLevel.Booting => 0.8f,
        KiraLevel.Functional => 0.3f,
        _ => 0.1f
    };
}
