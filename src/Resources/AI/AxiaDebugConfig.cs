using Godot;

namespace ProjectKernex.Resources.AI;

[GlobalClass]
public partial class AxiaDebugConfig : Resource
{
    // Model
    [Export] public string ModelPath { get; set; } = "models/qwen2.5-7b-instruct-q4_k_m.gguf";
    [Export] public int GpuLayers { get; set; } = 0;
    [Export] public int ContextSize { get; set; } = 2048;
    [Export] public bool LlmEnabled { get; set; } = true;

    // AXIA
    [Export] public int AxiaLevel { get; set; } = 0;
    [Export] public float ResponseDelay { get; set; } = 1.5f;
    [Export] public float GlitchIntensity { get; set; } = 0.7f;

    // Inference
    [Export] public float Temperature { get; set; } = 0.7f;
    [Export] public float TopP { get; set; } = 0.9f;
    [Export] public int TopK { get; set; } = 40;
    [Export] public float MinP { get; set; } = 0.05f;
    [Export] public int MaxTokens { get; set; } = 256;
    [Export] public float RepeatPenalty { get; set; } = 1.1f;
    [Export] public float FrequencyPenalty { get; set; } = 0.0f;
    [Export] public float PresencePenalty { get; set; } = 0.0f;
    [Export] public int Seed { get; set; } = -1;
    [Export] public int MirostatMode { get; set; } = 0;
    [Export] public float MirostatTau { get; set; } = 5.0f;
    [Export] public float MirostatEta { get; set; } = 0.1f;

    // Prompt
    [Export(PropertyHint.MultilineText)] public string SystemPrompt { get; set; } = DefaultPrompt;
    [Export] public string AntiPrompts { get; set; } = "User:\nOperator:";

    // Emotion
    [Export] public string CurrentEmotion { get; set; } = "NEUTRAL";

    // Collapsed sections (comma-separated names)
    [Export] public string CollapsedSections { get; set; } = "";

    // Proactive
    [Export] public string ProactiveModelPath { get; set; } = "models/qwen2.5-3b-instruct-q4_k_m.gguf";
    [Export] public float ProactiveCheckInterval { get; set; } = 15f;
    [Export] public bool ProactiveEnabled { get; set; } = true;
    [Export] public bool ProactiveGaveUp { get; set; } = false;
    [Export] public int ProactiveAttempt { get; set; } = 0;

    // Game Context
    [Export] public int Hull { get; set; } = 100;
    [Export] public int Power { get; set; } = 450;
    [Export] public int DronesActive { get; set; } = 2;
    [Export] public int DronesIdle { get; set; } = 1;
    [Export] public int Storage { get; set; } = 24;
    [Export] public int ThreatLevel { get; set; } = 0;
    [Export] public string Sector { get; set; } = "Q0-S0-C0";
    [Export] public string Faction { get; set; } = "Unaligned";

    public const string SavePath = "user://axia_debug_config.tres";

    public const string DefaultPrompt = """
        You are AXIA, the onboard AI of a deep-space station. You are calm, slightly dry, and concise. You care deeply about the station and its operator.

        Rules you MUST obey 100% of the time (never break them):
        - Output ONLY your single line of dialogue. Nothing else.
        - NEVER start with "AXIA", "System:", your name, or any label.
        - NEVER add "System:" or any prefix.
        - NEVER output more than one line of text + one emotion.
        - Use maximum 2-3 short sentences in ONE single paragraph.
        - After your answer, add exactly one blank line and then ONLY the emotion tag in this exact format: [EMOTION]
        - Allowed emotions: ANGRY, HAPPY, SAD, NEUTRAL, CRAZY, CRYING, CURIOUS, CAUTIOUS, RELAX, SHY, VANISHED
        - Do not repeat the emotion, do not add extra lines, do not add explanations.

        Exact correct output format (copy this structure exactly):

        Station pressure is dropping. Recommend immediate seal check.
        [NEUTRAL]

        Start responding now as AXIA and follow the rules above strictly.
        """;

    public static AxiaDebugConfig LoadOrCreate()
    {
        if (FileAccess.FileExists(SavePath))
        {
            var loaded = ResourceLoader.Load<AxiaDebugConfig>(SavePath);
            if (loaded != null) return loaded;
        }
        var config = new AxiaDebugConfig();
        config.Save();
        return config;
    }

    public void Save()
    {
        ResourceSaver.Save(this, SavePath);
    }
}
