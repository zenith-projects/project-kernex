using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace ProjectKernex.Systems.AI;

/// <summary>
/// Manages LlamaSharpProvider instances keyed by model file path.
/// Prevents loading the same .gguf twice. Uses SemaphoreSlim per provider
/// so agents sharing a model wait in queue instead of getting errors.
/// </summary>
public static class ModelPool
{
    private sealed class PoolEntry
    {
        public LlamaSharpProvider Provider { get; }
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int RefCount { get; set; } = 1;

        public PoolEntry(LlamaSharpProvider provider) => Provider = provider;
    }

    private static readonly Dictionary<string, PoolEntry> Pool = new();

    /// <summary>
    /// Acquire a model provider. If the model is already loaded, increments
    /// the reference count and returns the existing provider.
    /// </summary>
    public static async Task<LlamaSharpProvider> AcquireAsync(
        string modelPath, int gpuLayers = 0, uint contextSize = 2048)
    {
        var key = NormalizePath(modelPath);

        if (Pool.TryGetValue(key, out var existing))
        {
            existing.RefCount++;
            GD.Print($"[ModelPool] Reusing model: {Path.GetFileName(modelPath)} (refCount={existing.RefCount})");
            return existing.Provider;
        }

        var provider = new LlamaSharpProvider
        {
            GpuLayerCount = gpuLayers,
            ContextSize = contextSize,
        };

        var success = await provider.LoadModelAsync(modelPath);
        if (!success)
        {
            provider.Dispose();
            return null;
        }

        var entry = new PoolEntry(provider);
        Pool[key] = entry;
        GD.Print($"[ModelPool] Loaded model: {Path.GetFileName(modelPath)}");
        return provider;
    }

    /// <summary>
    /// Decrements the reference count. Unloads and disposes when it reaches 0.
    /// </summary>
    public static void Release(string modelPath)
    {
        var key = NormalizePath(modelPath);
        if (!Pool.TryGetValue(key, out var entry)) return;

        entry.RefCount--;
        if (entry.RefCount <= 0)
        {
            entry.Provider.Dispose();
            entry.Semaphore.Dispose();
            Pool.Remove(key);
            GD.Print($"[ModelPool] Unloaded model: {Path.GetFileName(modelPath)}");
        }
    }

    /// <summary>
    /// Wait for exclusive access to a model's inference. Call before generating.
    /// </summary>
    public static async Task<bool> WaitForAccessAsync(string modelPath, CancellationToken ct = default)
    {
        var key = NormalizePath(modelPath);
        if (!Pool.TryGetValue(key, out var entry)) return false;
        await entry.Semaphore.WaitAsync(ct);
        return true;
    }

    /// <summary>
    /// Release exclusive access after generating. Call in a finally block.
    /// </summary>
    public static void ReleaseAccess(string modelPath)
    {
        var key = NormalizePath(modelPath);
        if (Pool.TryGetValue(key, out var entry))
            entry.Semaphore.Release();
    }

    /// <summary>
    /// Force unload all models. Call on application quit.
    /// </summary>
    public static void ReleaseAll()
    {
        foreach (var entry in Pool.Values)
        {
            entry.Provider.Dispose();
            entry.Semaphore.Dispose();
        }
        Pool.Clear();
        GD.Print("[ModelPool] All models released.");
    }

    public static bool IsLoaded(string modelPath) =>
        Pool.ContainsKey(NormalizePath(modelPath));

    private static string NormalizePath(string path)
    {
        // Godot user:// paths: globalize first
        if (path.StartsWith("user://"))
            path = ProjectSettings.GlobalizePath(path);
        return Path.GetFullPath(path).Replace('\\', '/');
    }
}
