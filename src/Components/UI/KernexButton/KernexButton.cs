using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexButton : PanelContainer
{
    [Signal] public delegate void PressedEventHandler();

    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorGoldBright = new(0.910f, 0.753f, 0.376f);
    private static readonly Color ColorBorderIdle = new(0.831f, 0.659f, 0.294f, 0.25f);
    private static readonly Color ColorBorderHover = new(0.831f, 0.659f, 0.294f, 0.6f);
    private static readonly Color ColorBgIdle = new(0.831f, 0.659f, 0.294f, 0.05f);
    private static readonly Color ColorBgHover = new(0.831f, 0.659f, 0.294f, 0.12f);
    private static readonly Color ColorBgPressed = new(0.831f, 0.659f, 0.294f, 0.2f);
    private static readonly Color ColorTextIdle = new(0.75f, 0.75f, 0.78f);

    [Export] public string Text { get; set; } = "BUTTON";
    [Export] public bool Disabled { get; set; }
    [Export] public int FontSize { get; set; } = 16;

    private Label _label;
    private StyleBoxFlat _style;
    private Tween _hoverTween;
    private bool _hovering;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        CustomMinimumSize = new Vector2(220, 0);

        _style = new StyleBoxFlat
        {
            BgColor = ColorBgIdle,
            BorderColor = ColorBorderIdle,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
            ContentMarginLeft = 24, ContentMarginTop = 12,
            ContentMarginRight = 24, ContentMarginBottom = 12,
            AntiAliasing = true,
        };
        AddThemeStyleboxOverride("panel", _style);

        _label = new Label
        {
            Text = Text.ToUpperInvariant(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.Fill,
        };
        _label.AddThemeColorOverride("font_color", ColorTextIdle);
        _label.AddThemeFontSizeOverride("font_size", FontSize);

        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Bold.ttf");
        if (font != null)
            _label.AddThemeFontOverride("font", font);

        AddChild(_label);

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        GuiInput += OnGuiInput;
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (_label != null && _label.Text != Text.ToUpperInvariant())
            _label.Text = Text.ToUpperInvariant();
    }

    private void OnMouseEntered()
    {
        if (Disabled) return;
        _hovering = true;
        AnimateTo(ColorBgHover, ColorBorderHover, ColorGold);
    }

    private void OnMouseExited()
    {
        _hovering = false;
        AnimateTo(ColorBgIdle, ColorBorderIdle, ColorTextIdle);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (Disabled) return;

        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            _style.BgColor = ColorBgPressed;
            EmitSignal(SignalName.Pressed);
        }
        else if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left })
        {
            _style.BgColor = _hovering ? ColorBgHover : ColorBgIdle;
        }
    }

    private void AnimateTo(Color bg, Color border, Color text)
    {
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel();
        _hoverTween.TweenProperty(_style, "bg_color", bg, 0.15f);
        _hoverTween.TweenProperty(_style, "border_color", border, 0.15f);
        _hoverTween.TweenMethod(Callable.From<Color>(c => _label.AddThemeColorOverride("font_color", c)),
            _label.GetThemeColor("font_color"), text, 0.15f);
    }

    public void SetText(string text)
    {
        Text = text;
        if (_label != null) _label.Text = text.ToUpperInvariant();
    }

    public void SetDisabled(bool disabled)
    {
        Disabled = disabled;
        if (_label != null)
            _label.Modulate = disabled ? new Color(1, 1, 1, 0.3f) : Colors.White;
    }
}
