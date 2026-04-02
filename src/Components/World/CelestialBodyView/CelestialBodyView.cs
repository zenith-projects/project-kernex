using Godot;
using ProjectKernex.Resources.World;

namespace ProjectKernex.Components.World;

[GlobalClass, Tool]
public partial class CelestialBodyView : TextureRect
{
    [Signal] public delegate void ClickedEventHandler(CelestialBodyData data);

    [Export] public CelestialBodyData Data { get; set; }

    private ShaderMaterial _material;
    private static Shader _starShader;

    public override void _Ready()
    {
        _starShader ??= GD.Load<Shader>("res://src/Assets/Shaders/star.gdshader");

        MouseFilter = MouseFilterEnum.Stop;
        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;

        var img = Image.CreateEmpty(2, 2, false, Image.Format.Rgba8);
        img.Fill(Colors.White);
        Texture = ImageTexture.CreateFromImage(img);

        _material = new ShaderMaterial { Shader = _starShader };
        Material = _material;

        if (Data != null) ApplyData();

        GuiInput += OnInput;
    }

    public void SetData(CelestialBodyData data)
    {
        Data = data;
        if (_material != null) ApplyData();
    }

    private void ApplyData()
    {
        if (Data == null || _material == null) return;

        _material.SetShaderParameter("core_color", Data.CoreColor);
        _material.SetShaderParameter("corona_color", Data.CoronaColor);
        _material.SetShaderParameter("intensity", Data.Luminosity);
        _material.SetShaderParameter("pulse_speed", Data.PulseSpeed);
        _material.SetShaderParameter("seed", (float)(Data.Seed % 1000));
        _material.SetShaderParameter("accretion_disk", Data.HasAccretionDisk);
        _material.SetShaderParameter("body_type", (int)Data.Type);

        // Log scale — convert R☉ to R⊕ for consistent scale with planets
        var visualSize = 120f + Mathf.Log(1f + Data.Radius * 109f) * 80f;
        CustomMinimumSize = Vector2.One * Mathf.Clamp(visualSize, 100f, 800f);
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
