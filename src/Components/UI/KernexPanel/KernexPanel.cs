using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexPanel : PanelContainer
{
    [Export] public bool ShowBorder { get; set; } = true;
    [Export(PropertyHint.Range, "0.0,1.0,0.05")] public float BackgroundAlpha { get; set; } = 0.85f;
    [Export] public bool ShowHeader { get; set; }
    [Export] public string HeaderText { get; set; } = "";
    [Export] public int CornerRadius { get; set; } = 8;

    private static readonly Color ColorBorderGold = new(0.831f, 0.659f, 0.294f, 0.2f);
    private static readonly Color ColorBg = new(0.07f, 0.07f, 0.10f);

    private StyleBoxFlat _panelStyle;
    private Label _headerLabel;
    private MarginContainer _contentMargin;

    public override void _Ready()
    {
        ApplyStyle();

        if (ShowHeader)
            CreateHeader();
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        _panelStyle ??= new StyleBoxFlat();
        _panelStyle.BgColor = new Color(ColorBg.R, ColorBg.G, ColorBg.B, BackgroundAlpha);
        _panelStyle.CornerRadiusTopLeft = CornerRadius;
        _panelStyle.CornerRadiusTopRight = CornerRadius;
        _panelStyle.CornerRadiusBottomLeft = CornerRadius;
        _panelStyle.CornerRadiusBottomRight = CornerRadius;
        _panelStyle.ContentMarginLeft = 16;
        _panelStyle.ContentMarginTop = 12;
        _panelStyle.ContentMarginRight = 16;
        _panelStyle.ContentMarginBottom = 12;
        _panelStyle.AntiAliasing = true;

        if (ShowBorder)
        {
            _panelStyle.BorderColor = ColorBorderGold;
            _panelStyle.BorderWidthLeft = 1;
            _panelStyle.BorderWidthTop = 1;
            _panelStyle.BorderWidthRight = 1;
            _panelStyle.BorderWidthBottom = 1;
        }
        else
        {
            _panelStyle.BorderWidthLeft = 0;
            _panelStyle.BorderWidthTop = 0;
            _panelStyle.BorderWidthRight = 0;
            _panelStyle.BorderWidthBottom = 0;
        }

        AddThemeStyleboxOverride("panel", _panelStyle);
    }

    private void CreateHeader()
    {
        _headerLabel = new Label
        {
            Text = HeaderText.ToUpperInvariant(),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        _headerLabel.AddThemeColorOverride("font_color", new Color(0.831f, 0.659f, 0.294f));
        _headerLabel.AddThemeFontSizeOverride("font_size", 13);

        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Bold.ttf");
        if (font != null)
            _headerLabel.AddThemeFontOverride("font", font);

        AddChild(_headerLabel);
        MoveChild(_headerLabel, 0);
    }
}
