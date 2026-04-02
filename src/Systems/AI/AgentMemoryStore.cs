using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;

namespace ProjectKernex.Systems.AI;

public class AgentMemoryStore
{
    private const string MessagesFile = "messages.json";
    private const string SummariesFile = "summaries.json";

    private readonly string _memoryDir;
    private readonly string _agentLabel;
    private readonly List<AgentMemoryEntry> _messages = new();
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

    public AgentMemoryStore(string memoryDir, string agentLabel = "Agent")
    {
        _memoryDir = memoryDir;
        _agentLabel = agentLabel;
    }

    public void Load()
    {
        EnsureDirectory();
        LoadMessages();
        LoadSummaries();
    }

    public void SaveMessage(string role, string content, string tag = "")
    {
        var entry = new AgentMemoryEntry
        {
            Role = role,
            Content = content,
            Tag = tag,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Summarized = false,
        };
        _messages.Add(entry);
        PersistMessages();
    }

    public List<AgentMemoryEntry> GetRecent(int limit)
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

    public List<AgentMemoryEntry> GetUnsummarizedBatch(int batchSize)
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

    public List<AgentMemoryEntry> Search(string query, int topK = 5)
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

        if (_summaries.Count > 0)
        {
            sb.AppendLine($"{_agentLabel}'s long-term memories:");
            foreach (var summary in _summaries)
                sb.AppendLine($"- {summary}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            var relevant = Search(userMessage, 5);
            var recentTimestamp = _messages.Count > 15
                ? _messages[^15].Timestamp
                : 0;
            relevant = relevant.Where(m => m.Timestamp < recentTimestamp).ToList();

            if (relevant.Count > 0)
            {
                sb.AppendLine("Relevant past messages:");
                foreach (var m in relevant)
                {
                    var role = m.Role == "user" ? "Operator" : _agentLabel;
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
        if (!DirAccess.DirExistsAbsolute(_memoryDir))
            DirAccess.MakeDirRecursiveAbsolute(_memoryDir);
    }

    private void LoadMessages()
    {
        var path = _memoryDir + MessagesFile;
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var json = file.GetAsText();
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var entries = JsonSerializer.Deserialize<List<AgentMemoryEntry>>(json);
            if (entries == null) return;

            _messages.Clear();
            _messages.AddRange(entries);
            _summarizedUpTo = _messages.Count(m => m.Summarized);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AgentMemory] Failed to load messages: {ex.Message}");
        }
    }

    private void LoadSummaries()
    {
        var path = _memoryDir + SummariesFile;
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
            GD.PrintErr($"[AgentMemory] Failed to load summaries: {ex.Message}");
        }
    }

    private void PersistMessages()
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(_messages, new JsonSerializerOptions { WriteIndented = false });
        using var file = FileAccess.Open(_memoryDir + MessagesFile, FileAccess.ModeFlags.Write);
        file?.StoreString(json);
    }

    private void PersistSummaries()
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(_summaries, new JsonSerializerOptions { WriteIndented = false });
        using var file = FileAccess.Open(_memoryDir + SummariesFile, FileAccess.ModeFlags.Write);
        file?.StoreString(json);
    }
}
