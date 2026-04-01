using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectKernex.Core.Interfaces;

public interface ILlmProvider
{
    bool IsModelLoaded { get; }
    bool IsProcessing { get; }
    Task<bool> LoadModelAsync(string modelPath);
    Task<string> GenerateAsync(string systemPrompt, string userMessage, CancellationToken ct = default);
    Task<string> GenerateWithHistoryAsync(string systemPrompt, List<(string role, string content)> history, string userMessage, CancellationToken ct = default);
    void UnloadModel();
}
