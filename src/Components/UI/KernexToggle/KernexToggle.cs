using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexToggle : HBoxContainer
{
    [Signal] public delegate void ToggledEventHandler(bool on);

    private static readonly Color ColorAccent = new(0.7f, 0.75f, 0.85f);
    private static readonly Color ColorAccentDim = new(0.6f, 0.65f, 0.75f, 0.3f);
    private static readonly Color ColorBg = new(0.12f, 0.12f, 0.15f);
    private static readonly Color ColorBgOn = new(0.6f, 0.65f, 0.75f, 0.2f);
    private static readonly Color ColorTextDim = new(0.55f, 0.55f, 0.58f);

    [Export] public string Text { get; set; } = "Toggle";
    [Export] public bool Value { get; set; }

    private Label _label;
    private Button _hitArea;
    private Panel _track;
    private Panel _knob;
    private StyleBoxFlat _trackStyle;
    private StyleBoxFlat _knobStyle;
    private bool _ready;

    private const float TrackW = 44f;
    private const float TrackH = 24f;
    private const float KnobSize = 16f;
    private const float KnobOffX = 4f;
    private const float KnobOnX = 24f;
    private const float KnobY = 4f;

    public override void _Ready()
    {
        _ready = true;
        AddThemeConstantOverride("separation", 12);

        // Label — clickable too
        _label = new Label
        {
            Text = Text,
            SizeFlagsHorizontal = SizeFlags.Fill,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Stop,
        };
        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/moonhouse.ttf");
        if (font != null)
        {
            var variation = new FontVariation { BaseFont = font, VariationEmbolden = 0.4f };
            _label.AddThemeFontOverride("font", variation);
        }
        _label.AddThemeFontSizeOverride("font_size", 14);
        _label.AddThemeColorOverride("font_color", ColorTextDim);
        _label.GuiInput += OnToggleInput;
        AddChild(_label);

        // Invisible button covering the track area for reliable clicks
        _hitArea = new Button
        {
            CustomMinimumSize = new Vector2(TrackW, TrackH),
            Flat = true,
            MouseFilter = MouseFilterEnum.Stop,
        };
        // Make button fully transparent
        _hitArea.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
        _hitArea.AddThemeStyleboxOverride("hover", new StyleBoxEmpty());
        _hitArea.AddThemeStyleboxOverride("pressed", new StyleBoxEmpty());
        _hitArea.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
        _hitArea.Pressed += OnTogglePressed;
        AddChild(_hitArea);

        // Track (rounded pill shape)
        _trackStyle = new StyleBoxFlat
        {
            BgColor = ColorBg,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            BorderColor = ColorAccentDim,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            AntiAliasing = true,
        };
        _track = new Panel
        {
            Size = new Vector2(TrackW, TrackH),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _track.AddThemeStyleboxOverride("panel", _trackStyle);
        _hitArea.AddChild(_track);

        // Knob (circle)
        _knobStyle = new StyleBoxFlat
        {
            BgColor = ColorAccentDim,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            AntiAliasing = true,
        };
        _knob = new Panel
        {
            Size = new Vector2(KnobSize, KnobSize),
            Position = new Vector2(Value ? KnobOnX : KnobOffX, KnobY),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _knob.AddThemeStyleboxOverride("panel", _knobStyle);
        _hitArea.AddChild(_knob);

        ApplyVisuals(false);
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() || !_ready) return;
        if (_label != null && _label.Text != Text)
            _label.Text = Text;
    }

    private void OnToggleInput(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            OnTogglePressed();
    }

    private void OnTogglePressed()
    {
        if (Engine.IsEditorHint()) return;
        Value = !Value;
        ApplyVisuals(true);
        EmitSignal(SignalName.Toggled, Value);
    }

    private void ApplyVisuals(bool animate)
    {
        if (!_ready) return;

        var targetX = Value ? KnobOnX : KnobOffX;
        var knobColor = Value ? ColorAccent : ColorAccentDim;
        var trackColor = Value ? ColorBgOn : ColorBg;
        var borderColor = Value ? ColorAccent : ColorAccentDim;
        var labelColor = Value ? Colors.White : ColorTextDim;

        if (animate && !Engine.IsEditorHint())
        {
            var tween = CreateTween().SetParallel();
            tween.TweenProperty(_knob, "position:x", targetX, 0.15f)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(_knobStyle, "bg_color", knobColor, 0.15f);
            tween.TweenProperty(_trackStyle, "bg_color", trackColor, 0.15f);
            tween.TweenProperty(_trackStyle, "border_color", borderColor, 0.15f);
            tween.TweenMethod(Callable.From<Color>(c => _label.AddThemeColorOverride("font_color", c)),
                _label.GetThemeColor("font_color"), labelColor, 0.15f);
        }
        else
        {
            _knob.Position = new Vector2(targetX, KnobY);
            _knobStyle.BgColor = knobColor;
            _trackStyle.BgColor = trackColor;
            _trackStyle.BorderColor = borderColor;
            _label.AddThemeColorOverride("font_color", labelColor);
        }
    }

    public void SetValue(bool on)
    {
        Value = on;
        if (_ready) ApplyVisuals(false);
    }
}
