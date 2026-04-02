using Godot;

namespace ProjectKernex.Resources.World;

[GlobalClass]
public partial class SolarSystemData : Resource
{
    [Export] public long Seed { get; set; }
    [Export] public string SystemName { get; set; } = "";
    [Export] public CelestialBodyData CentralBody { get; set; }
    [Export] public Godot.Collections.Array<PlanetData> Planets { get; set; } = new();
    [Export] public float SystemSpacing { get; set; } = 1f;
}
