using Godot;

namespace ProjectKernex.Resources.AI;

[GlobalClass]
public partial class AgentConfig : Resource
{
    // Identity
    [ExportGroup("Identity")]
    [Export] public string AgentId { get; set; } = "unnamed";
    [Export] public string DisplayName { get; set; } = "Agent";

    // Model
    [ExportGroup("Model")]
    [Export] public string ModelPath { get; set; } = "";
    [Export] public int GpuLayers { get; set; } = 0;
    [Export] public uint ContextSize { get; set; } = 2048;

    // Inference
    [ExportGroup("Inference")]
    [Export(PropertyHint.Range, "0.0,2.0,0.05")] public float Temperature { get; set; } = 0.7f;
    [Export(PropertyHint.Range, "0.0,1.0,0.05")] public float TopP { get; set; } = 0.9f;
    [Export(PropertyHint.Range, "0,100,1")] public int TopK { get; set; } = 40;
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float MinP { get; set; } = 0.05f;
    [Export(PropertyHint.Range, "32,2048,16")] public int MaxTokens { get; set; } = 256;
    [Export(PropertyHint.Range, "1.0,2.0,0.05")] public float RepeatPenalty { get; set; } = 1.1f;
    [Export(PropertyHint.Range, "0.0,2.0,0.05")] public float FrequencyPenalty { get; set; } = 0.0f;
    [Export(PropertyHint.Range, "0.0,2.0,0.05")] public float PresencePenalty { get; set; } = 0.0f;
    [Export] public int Seed { get; set; } = -1;
    [Export(PropertyHint.Enum, "Disabled,Mirostat 1,Mirostat 2")] public int MirostatMode { get; set; } = 0;
    [Export(PropertyHint.Range, "0.0,10.0,0.1")] public float MirostatTau { get; set; } = 5.0f;
    [Export(PropertyHint.Range, "0.0,1.0,0.01")] public float MirostatEta { get; set; } = 0.1f;

    // Prompt
    [ExportGroup("Prompt")]
    [Export(PropertyHint.MultilineText)] public string SystemPrompt { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string AntiPrompts { get; set; } = "User:\nOperator:";

    // Memory
    [ExportGroup("Memory")]
    [Export] public bool MemoryEnabled { get; set; } = true;

    // Response
    [ExportGroup("Response")]
    [Export(PropertyHint.Range, "0.0,5.0,0.1")] public float ResponseDelay { get; set; } = 0.0f;
    [Export] public string[] AllowedContextKeys { get; set; } = [];

    /// <summary>
    /// Returns the anti-prompts as a string array (split by newlines).
    /// </summary>
    public string[] GetAntiPromptsArray() =>
        AntiPrompts.Split('\n', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
}
