using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Resources.World;

[GlobalClass]
public partial class CelestialBodyData : Resource
{
    [ExportGroup("Identity")]
    [Export] public CelestialBodyType Type { get; set; }
    [Export] public long Seed { get; set; }
    [Export] public string Name { get; set; } = "";

    [ExportGroup("Physical")]
    [Export] public float Radius { get; set; } = 1f;
    [Export] public float Mass { get; set; } = 1f;
    [Export] public float Temperature { get; set; } = 5778f;
    [Export] public float Luminosity { get; set; } = 1f;

    [ExportGroup("Visual")]
    [Export] public Color CoreColor { get; set; } = Colors.White;
    [Export] public Color CoronaColor { get; set; } = Colors.Yellow;
    [Export] public float PulseSpeed { get; set; }
    [Export] public bool HasAccretionDisk { get; set; }

    [ExportGroup("System")]
    [Export] public int MaxPlanets { get; set; } = 8;
    [Export] public float HabitableZoneStart { get; set; } = 0.8f;
    [Export] public float HabitableZoneEnd { get; set; } = 1.5f;
}
