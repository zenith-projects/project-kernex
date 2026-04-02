using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Resources.World;

[GlobalClass]
public partial class PlanetData : Resource
{
    [ExportGroup("Identity")]
    [Export] public PlanetType Type { get; set; }
    [Export] public long Seed { get; set; }
    [Export] public string Name { get; set; } = "";

    [ExportGroup("Physical")]
    [Export] public float Radius { get; set; } = 1f;
    [Export] public float Mass { get; set; } = 1f;
    [Export] public float OrbitalDistance { get; set; }
    [Export] public float OrbitalSpeed { get; set; }
    [Export] public float RotationSpeed { get; set; } = 0.1f;
    [Export] public float Temperature { get; set; } = 300f;

    [ExportGroup("Visual")]
    [Export] public Color PrimaryColor { get; set; } = Colors.Gray;
    [Export] public Color SecondaryColor { get; set; } = Colors.DarkGray;
    [Export] public Color AtmosphereColor { get; set; } = new(0.4f, 0.6f, 1f, 0.3f);
    [Export(PropertyHint.Range, "0,1,0.05")] public float AtmosphereIntensity { get; set; }
    [Export(PropertyHint.Range, "0,1,0.05")] public float CloudCoverage { get; set; }
    [Export] public bool HasRings { get; set; }
    [Export(PropertyHint.Range, "0,1,0.05")] public float RingScale { get; set; }

    [ExportGroup("Satellites")]
    [Export] public bool HasMoons { get; set; }
    [Export] public int MoonCount { get; set; }

    [ExportGroup("Gameplay")]
    [Export(PropertyHint.Range, "0,1,0.05")] public float ResourceRichness { get; set; }
    [Export(PropertyHint.Range, "0,1,0.05")] public float ThreatLevel { get; set; }
    [Export] public bool IsHabitable { get; set; }
}
