using System;
using System.Threading.Tasks;
using Godot;

namespace ProjectKernex.Systems.AI;

public static class ModelDownloader
{
    public static readonly ModelInfo MainModel = new(
        "qwen2.5-7b-instruct-q4_k_m.gguf",
        "https://huggingface.co/bartowski/Qwen2.5-7B-Instruct-GGUF/resolve/main/Qwen2.5-7B-Instruct-Q4_K_M.gguf",
        4_680_000_000L
    );

    public static readonly ModelInfo LightModel = new(
        "qwen2.5-3b-instruct-q4_k_m.gguf",
        "https://huggingface.co/Qwen/Qwen2.5-3B-Instruct-GGUF/resolve/main/qwen2.5-3b-instruct-q4_k_m.gguf",
        2_030_000_000L
    );

    public static string GetModelsDir()
    {
        var dir = ProjectSettings.GlobalizePath("user://models");
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetModelPath(ModelInfo model)
    {
        return System.IO.Path.Combine(GetModelsDir(), model.FileName);
    }

    public static bool ModelExists(ModelInfo model)
    {
        var path = GetModelPath(model);
        return System.IO.File.Exists(path) && new System.IO.FileInfo(path).Length > 100_000;
    }

    public static async Task<bool> DownloadModelAsync(ModelInfo model, Action<float> onProgress = null, System.Threading.CancellationToken ct = default)
    {
        var path = GetModelPath(model);
        var tempPath = path + ".downloading";

        try
        {
            // Run entirely on thread pool to not block Godot
            return await System.Threading.Tasks.Task.Run(async () =>
            {
                var handler = new System.Net.Http.HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                };
                using var client = new System.Net.Http.HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(60);
                client.DefaultRequestHeaders.Add("User-Agent", "ProjectKernex/1.0");

                using var response = await client.GetAsync(model.Url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? model.ExpectedSize;
                long downloaded = 0;

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new System.IO.FileStream(
                    tempPath,
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.Write,
                    System.IO.FileShare.None,
                    bufferSize: 1024 * 1024, // 1 MB buffer
                    useAsync: true
                );

                var buffer = new byte[1024 * 1024]; // 1 MB read chunks
                int bytesRead;
                var lastReport = DateTime.UtcNow;

                while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                    downloaded += bytesRead;

                    if ((DateTime.UtcNow - lastReport).TotalMilliseconds > 500)
                    {
                        onProgress?.Invoke((float)downloaded / totalBytes);
                        lastReport = DateTime.UtcNow;
                    }
                }

                await fileStream.FlushAsync();
                fileStream.Close();
                onProgress?.Invoke(1f);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                System.IO.File.Move(tempPath, path);

                return true;
            });
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ModelDownloader] Failed to download {model.FileName}: {ex.Message}");
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
            return false;
        }
    }

    public record ModelInfo(string FileName, string Url, long ExpectedSize);
}
