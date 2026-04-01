using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;

namespace ProjectKernex.Systems.AI;

public class KiraMemoryStore
{
    private const string MemoryDir = "user://kira_memory/";
    private const string MessagesFile = "messages.json";
    private const string SummariesFile = "summaries.json";

    private readonly List<KiraMemoryEntry> _messages = new();
    private readonly List<string> _summaries = new();
    private int _summarizedUpTo;

    private static readonly string[] StopWords =
    [
        "the", "a", "an", "is", "are", "was", "were", "be", "been", "being",
        "have", "has", "had", "do", "does", "did", "will", "would", "could",
        "should", "may", "might", "shall", "can", "to", "of", "in", "for",
        "on", "with", "at", "by", "from", "as", "into", "about", "what",
        "which", "who", "when", "where", "how", "that", "this", "it", "i",
        "you", "we", "they", "me", "my", "your", "our", "and", "or", "not",
        "no", "so", "if", "but", "then", "than", "too", "very", "just"
    ];

    public int MessageCount => _messages.Count;
    public int SummaryCount => _summaries.Count;
    public int UnsummarizedCount => _messages.Count - _summarizedUpTo;

    public void Load()
    {
        EnsureDirectory();
        LoadMessages();
        LoadSummaries();
    }

    public void SaveMessage(string role, string content, string emotion = "")
    {
        var entry = new KiraMemoryEntry
        {
            Role = role,
            Content = content,
            Emotion = emotion,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Summarized = false,
        };
        _messages.Add(entry);
        PersistMessages();
    }

    public List<KiraMemoryEntry> GetRecent(int limit)
    {
        return _messages.TakeLast(limit).ToList();
    }

    public List<(string role, string content)> GetRecentAsTuples(int limit)
    {
        return _messages
            .TakeLast(limit)
            .Select(m => (m.Role, m.Content))
            .ToList();
    }

    public List<string> GetSummaries() => new(_summaries);

    public List<KiraMemoryEntry> GetUnsummarizedBatch(int batchSize)
    {
        return _messages
            .Skip(_summarizedUpTo)
            .Take(batchSize)
            .ToList();
    }

    public void AddSummaryAndMark(string summary, int batchSize)
    {
        _summaries.Add(summary);
        _summarizedUpTo = Math.Min(_summarizedUpTo + batchSize, _messages.Count);

        for (int i = 0; i < _summarizedUpTo && i < _messages.Count; i++)
            _messages[i].Summarized = true;

        PersistSummaries();
        PersistMessages();
    }

    public List<KiraMemoryEntry> Search(string query, int topK = 5)
    {
        var keywords = ExtractKeywords(query);
        if (keywords.Count == 0) return new();

        return _messages
            .Where(m => keywords.Any(kw =>
                m.Content.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(m =>
                keywords.Count(kw => m.Content.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .ThenByDescending(m => m.Timestamp)
            .Take(topK)
            .ToList();
    }

    public string BuildMemoryBlock(string userMessage = "")
    {
        var sb = new StringBuilder();

        // Long-term summaries
        if (_summaries.Count > 0)
        {
            sb.AppendLine("KIRA's long-term memories:");
            foreach (var summary in _summaries)
                sb.AppendLine($"- {summary}");
            sb.AppendLine();
        }

        // Keyword search for relevant past messages (if user message provided)
        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            var relevant = Search(userMessage, 5);
            // Filter out messages already in recent history
            var recentTimestamp = _messages.Count > 15
                ? _messages[^15].Timestamp
                : 0;
            relevant = relevant.Where(m => m.Timestamp < recentTimestamp).ToList();

            if (relevant.Count > 0)
            {
                sb.AppendLine("Relevant past messages:");
                foreach (var m in relevant)
                {
                    var role = m.Role == "user" ? "Operator" : "KIRA";
                    sb.AppendLine($"[{role}] \"{Truncate(m.Content, 150)}\"");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public void ClearAll()
    {
        _messages.Clear();
        _summaries.Clear();
        _summarizedUpTo = 0;
        PersistMessages();
        PersistSummaries();
    }

    private static List<string> ExtractKeywords(string text)
    {
        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => w.Trim('.', ',', '!', '?', ':', ';', '"', '\'').ToLowerInvariant())
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .Distinct()
            .ToList();
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max] + "...";

    private void EnsureDirectory()
    {
        if (!DirAccess.DirExistsAbsolute(MemoryDir))
            DirAccess.MakeDirRecursiveAbsolute(MemoryDir);
    }

    private void LoadMessages()
    {
        var path = MemoryDir + MessagesFile;
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var json = file.GetAsText();
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var entries = JsonSerializer.Deserialize<List<KiraMemoryEntry>>(json);
            if (entries == null) return;

            _messages.Clear();
            _messages.AddRange(entries);
            _summarizedUpTo = _messages.Count(m => m.Summarized);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[KiraMemory] Failed to load messages: {ex.Message}");
        }
    }

    private void LoadSummaries()
    {
        var path = MemoryDir + SummariesFile;
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var json = file.GetAsText();
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var summaries = JsonSerializer.Deserialize<List<string>>(json);
            if (summaries != null)
            {
                _summaries.Clear();
                _summaries.AddRange(summaries);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[KiraMemory] Failed to load summaries: {ex.Message}");
        }
    }

    private void PersistMessages()
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(_messages, new JsonSerializerOptions { WriteIndented = false });
        using var file = FileAccess.Open(MemoryDir + MessagesFile, FileAccess.ModeFlags.Write);
        file?.StoreString(json);
    }

    private void PersistSummaries()
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(_summaries, new JsonSerializerOptions { WriteIndented = false });
        using var file = FileAccess.Open(MemoryDir + SummariesFile, FileAccess.ModeFlags.Write);
        file?.StoreString(json);
    }
}
