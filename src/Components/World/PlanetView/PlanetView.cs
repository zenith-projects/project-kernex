using Godot;
using ProjectKernex.Resources.World;

namespace ProjectKernex.Components.World;

[GlobalClass, Tool]
public partial class PlanetView : TextureRect
{
    [Signal] public delegate void ClickedEventHandler(PlanetData data);

    [Export] public PlanetData Data { get; set; }

    private ShaderMaterial _material;
    private static Shader _planetShader;

    public override void _Ready()
    {
        _planetShader ??= GD.Load<Shader>("res://src/Assets/Shaders/planet.gdshader");

        MouseFilter = MouseFilterEnum.Stop;
        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;

        // White square placeholder texture
        var img = Image.CreateEmpty(2, 2, false, Image.Format.Rgba8);
        img.Fill(Colors.White);
        Texture = ImageTexture.CreateFromImage(img);

        _material = new ShaderMaterial { Shader = _planetShader };
        Material = _material;

        if (Data != null) ApplyData();

        GuiInput += OnInput;
    }

    public void SetData(PlanetData data)
    {
        Data = data;
        if (_material != null) ApplyData();
    }

    public void Generate(long seed)
    {
        var system = Systems.Exploration.SystemGenerator.Generate(seed);
        if (system.Planets.Count > 0)
            SetData(system.Planets[0]);
    }

    private void ApplyData()
    {
        if (Data == null || _material == null) return;

        _material.SetShaderParameter("primary_color", Data.PrimaryColor);
        _material.SetShaderParameter("secondary_color", Data.SecondaryColor);
        _material.SetShaderParameter("atmosphere_color", Data.AtmosphereColor);
        _material.SetShaderParameter("atmosphere_intensity", Data.AtmosphereIntensity);
        _material.SetShaderParameter("cloud_coverage", Data.CloudCoverage);
        _material.SetShaderParameter("rotation_speed", Data.RotationSpeed);
        _material.SetShaderParameter("planet_type", (int)Data.Type);
        _material.SetShaderParameter("has_rings", Data.HasRings);
        if (Data.HasRings)
        {
            // Ring color derived from planet type
            var ringCol = Data.Type switch
            {
                Core.Enums.PlanetType.GasGiant => new Color(0.8f, 0.75f, 0.6f, 0.5f),
                Core.Enums.PlanetType.IceWorld => new Color(0.7f, 0.8f, 0.95f, 0.4f),
                Core.Enums.PlanetType.Crystalline => new Color(0.6f, 0.5f, 0.9f, 0.45f),
                _ => new Color(0.75f, 0.7f, 0.6f, 0.35f),
            };
            _material.SetShaderParameter("ring_color", ringCol);
            _material.SetShaderParameter("ring_tilt", 0.15f + (Data.Seed % 100) / 100f * 0.4f);
            _material.SetShaderParameter("ring_scale", Data.RingScale);
        }
        _material.SetShaderParameter("seed", (float)(Data.Seed % 1000));
        _material.SetShaderParameter("light_direction", new Vector2(-0.5f, -0.7f));

        // Log scale — large enough for deep zoom
        var visualSize = 80f + Mathf.Log(1f + Data.Radius) * 180f;
        if (Data.HasRings) visualSize *= 1.3f + Data.RingScale * 0.7f; // bigger rings need more space
        CustomMinimumSize = Vector2.One * visualSize;
    }

    private void OnInput(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            EmitSignal(SignalName.Clicked, Data);
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (Data != null && _material != null) ApplyData();
    }
}
