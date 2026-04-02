using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexDropdown : HBoxContainer
{
    [Signal] public delegate void ItemSelectedEventHandler(int index);

    private static readonly Color ColorAccent = new(0.7f, 0.75f, 0.85f);
    private static readonly Color ColorAccentDim = new(0.6f, 0.65f, 0.75f, 0.25f);
    private static readonly Color ColorBg = new(0.08f, 0.08f, 0.10f, 0.95f);
    private static readonly Color ColorTextDim = new(0.55f, 0.55f, 0.58f);
    private static readonly Color ColorText = new(0.75f, 0.75f, 0.78f);

    [Export] public string Text { get; set; } = "Option";
    [Export] public int LabelWidth { get; set; } = 180;

    private Label _label;
    private OptionButton _option;
    private bool _ready;

    public int Selected
    {
        get => _option?.Selected ?? -1;
        set { if (_option != null) _option.Selected = value; }
    }

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

        // OptionButton
        _option = new OptionButton
        {
            SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
        };
        StyleOptionButton(fontVariation);
        _option.ItemSelected += OnItemSelected;
        AddChild(_option);
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() || !_ready) return;
        if (_label != null && _label.Text != Text) _label.Text = Text;
    }

    private void StyleOptionButton(Font fontVariation)
    {
        if (fontVariation != null) _option.AddThemeFontOverride("font", fontVariation);
        _option.AddThemeFontSizeOverride("font_size", 13);
        _option.AddThemeColorOverride("font_color", ColorText);
        _option.AddThemeColorOverride("font_hover_color", Colors.White);
        _option.AddThemeColorOverride("font_focus_color", ColorAccent);

        var normal = new StyleBoxFlat
        {
            BgColor = ColorBg,
            BorderColor = ColorAccentDim,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
            ContentMarginLeft = 10, ContentMarginTop = 6,
            ContentMarginRight = 10, ContentMarginBottom = 6,
            AntiAliasing = true,
        };
        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BorderColor = new Color(ColorAccent.R, ColorAccent.G, ColorAccent.B, 0.5f);
        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = new Color(ColorAccent.R, ColorAccent.G, ColorAccent.B, 0.1f);

        _option.AddThemeStyleboxOverride("normal", normal);
        _option.AddThemeStyleboxOverride("hover", hover);
        _option.AddThemeStyleboxOverride("pressed", pressed);
        _option.AddThemeStyleboxOverride("focus", hover);

        // Popup panel style
        var popup = _option.GetPopup();
        if (popup != null)
        {
            var popupStyle = new StyleBoxFlat
            {
                BgColor = new Color(0.06f, 0.06f, 0.08f, 0.97f),
                BorderColor = ColorAccentDim,
                BorderWidthLeft = 1, BorderWidthTop = 1,
                BorderWidthRight = 1, BorderWidthBottom = 1,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 6, ContentMarginTop = 4,
                ContentMarginRight = 6, ContentMarginBottom = 4,
                AntiAliasing = true,
            };
            popup.AddThemeStyleboxOverride("panel", popupStyle);
            if (fontVariation != null) popup.AddThemeFontOverride("font", fontVariation);
            popup.AddThemeFontSizeOverride("font_size", 13);
            popup.AddThemeColorOverride("font_color", ColorText);
            popup.AddThemeColorOverride("font_hover_color", Colors.White);

            var hoverItem = new StyleBoxFlat
            {
                BgColor = new Color(ColorAccent.R, ColorAccent.G, ColorAccent.B, 0.12f),
                CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3,
                CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
            };
            popup.AddThemeStyleboxOverride("hover", hoverItem);
        }
    }

    private void OnItemSelected(long index)
    {
        EmitSignal(SignalName.ItemSelected, (int)index);
    }

    public void AddItem(string label, int id = -1)
    {
        _option?.AddItem(label, id);
    }

    public void Clear()
    {
        _option?.Clear();
    }
}
