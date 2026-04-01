using Godot;

namespace ProjectKernex.Resources.AI;

[GlobalClass]
public partial class KiraLevelData : Resource
{
    [Export] public int Level { get; set; }
    [Export] public string DisplayName { get; set; } = "";
    [Export] public bool UsesLlm { get; set; }
    [Export(PropertyHint.Range, "0.0,3.0,0.1")] public float ResponseDelay { get; set; }
    [Export(PropertyHint.Range, "0.0,1.0,0.05")] public float GlitchIntensity { get; set; }
    [Export] public Color OutputColor { get; set; } = new(0.0f, 1.0f, 0.8f);
    [Export] public string[] AllowedContextKeys { get; set; } = [];
    [Export(PropertyHint.MultilineText)] public string PersonalityNotes { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string SystemPromptTemplate { get; set; } = "";
}
