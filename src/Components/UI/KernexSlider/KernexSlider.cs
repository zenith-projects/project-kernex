using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexSlider : HBoxContainer
{
    [Signal] public delegate void ValueChangedEventHandler(float value);

    private static readonly Color ColorAccent = new(0.7f, 0.75f, 0.85f);
    private static readonly Color ColorAccentDim = new(0.6f, 0.65f, 0.75f, 0.3f);
    private static readonly Color ColorTrack = new(0.10f, 0.10f, 0.13f);
    private static readonly Color ColorFill = new(0.6f, 0.65f, 0.75f, 0.35f);
    private static readonly Color ColorTextDim = new(0.55f, 0.55f, 0.58f);

    [Export] public string Text { get; set; } = "Slider";
    [Export] public float MinValue { get; set; } = 0f;
    [Export] public float MaxValue { get; set; } = 100f;
    [Export] public float Value { get; set; } = 50f;
    [Export] public float Step { get; set; } = 1f;
    [Export] public bool ShowValue { get; set; } = true;
    [Export] public int LabelWidth { get; set; } = 180;

    private Label _label;
    private HSlider _slider;
    private Label _valueLabel;
    private bool _ready;

    public override void _Ready()
    {
        _ready = true;
        AddThemeConstantOverride("separation", 12);

        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/moonhouse.ttf");
        var fontVariation = font != null ? new FontVariation { BaseFont = font, VariationEmbolden = 0.4f } : null;

        // Label
        _label = new Label
        {
            Text = Text,
            CustomMinimumSize = new Vector2(LabelWidth, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (fontVariation != null) _label.AddThemeFontOverride("font", fontVariation);
        _label.AddThemeFontSizeOverride("font_size", 14);
        _label.AddThemeColorOverride("font_color", ColorTextDim);
        AddChild(_label);

        // Slider
        _slider = new HSlider
        {
            MinValue = MinValue,
            MaxValue = MaxValue,
            Value = Value,
            Step = Step,
            SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
            CustomMinimumSize = new Vector2(120, 0),
        };
        StyleSlider();
        _slider.ValueChanged += OnSliderChanged;
        AddChild(_slider);

        // Value label
        if (ShowValue)
        {
            _valueLabel = new Label
            {
                Text = FormatValue(Value),
                CustomMinimumSize = new Vector2(50, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            if (fontVariation != null) _valueLabel.AddThemeFontOverride("font", fontVariation);
            _valueLabel.AddThemeFontSizeOverride("font_size", 13);
            _valueLabel.AddThemeColorOverride("font_color", ColorAccent);
            AddChild(_valueLabel);
        }
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() || !_ready) return;
        if (_label != null && _label.Text != Text) _label.Text = Text;
    }

    private void StyleSlider()
    {
        // Track bg
        var trackBg = new StyleBoxFlat
        {
            BgColor = ColorTrack,
            CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
            ContentMarginTop = 3, ContentMarginBottom = 3,
            AntiAliasing = true,
        };
        _slider.AddThemeStyleboxOverride("slider", trackBg);

        // Fill
        var fillStyle = new StyleBoxFlat
        {
            BgColor = ColorFill,
            CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
            ContentMarginTop = 3, ContentMarginBottom = 3,
            AntiAliasing = true,
        };
        _slider.AddThemeStyleboxOverride("grabber_area", fillStyle);
        _slider.AddThemeStyleboxOverride("grabber_area_highlight", fillStyle);

        // Grabber
        var grabber = CreateGrabberTexture(ColorAccent);
        var grabberHl = CreateGrabberTexture(new Color(0.95f, 0.82f, 0.45f));
        _slider.AddThemeIconOverride("grabber", grabber);
        _slider.AddThemeIconOverride("grabber_highlight", grabberHl);
    }

    private static Texture2D CreateGrabberTexture(Color color)
    {
        var img = Image.CreateEmpty(14, 14, false, Image.Format.Rgba8);
        var center = new Vector2(7, 7);
        for (int x = 0; x < 14; x++)
            for (int y = 0; y < 14; y++)
            {
                var dist = new Vector2(x, y).DistanceTo(center);
                if (dist <= 5.5f)
                    img.SetPixel(x, y, color);
                else if (dist <= 6.5f)
                    img.SetPixel(x, y, new Color(color.R, color.G, color.B, 0.4f));
                else
                    img.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        return ImageTexture.CreateFromImage(img);
    }

    private void OnSliderChanged(double value)
    {
        Value = (float)value;
        if (_valueLabel != null)
            _valueLabel.Text = FormatValue(Value);
        EmitSignal(SignalName.ValueChanged, Value);
    }

    private string FormatValue(float v) =>
        Step >= 1 ? $"{(int)v}" : $"{v:F1}";

    public void SetValue(float v)
    {
        Value = v;
        if (_slider != null) _slider.Value = v;
        if (_valueLabel != null) _valueLabel.Text = FormatValue(v);
    }
}
