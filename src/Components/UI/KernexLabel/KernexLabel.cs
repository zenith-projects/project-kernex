using Godot;

namespace ProjectKernex.Components.UI;

public enum KernexLabelStyle
{
    Title,
    Subtitle,
    Body,
    Caption,
    Monospace,
    Logo,
}

[GlobalClass, Tool]
public partial class KernexLabel : Label
{
    private static readonly Color ColorAccent = new(0.7f, 0.75f, 0.85f);
    private static readonly Color ColorTextPrimary = new(0.75f, 0.75f, 0.78f);
    private static readonly Color ColorTextDim = new(0.45f, 0.45f, 0.47f);

    [Export] public KernexLabelStyle Style { get; set; } = KernexLabelStyle.Body;
    [Export] public int FontSize { get; set; } = 0;
    [Export] public int LetterSpacing { get; set; } = 0;

    [ExportGroup("Shadow")]
    [Export] public bool ShadowEnabled { get; set; }
    [Export] public Color ShadowColor { get; set; } = new(0, 0, 0, 0.6f);
    [Export] public Vector2 ShadowOffset { get; set; } = new(1, 1);
    [Export(PropertyHint.Range, "0,20,1")] public int ShadowSize { get; set; } = 4;

    private KernexLabelStyle _lastStyle = (KernexLabelStyle)(-1);
    private int _lastSize;
    private int _lastSpacing;
    private bool _lastShadow;
    private Color _lastShadowColor;
    private Vector2 _lastShadowOffset;
    private int _lastShadowSize;

    public override void _Ready() => CallDeferred(MethodName.ApplyStyle);

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (_lastStyle != Style || _lastSize != FontSize || _lastSpacing != LetterSpacing
            || _lastShadow != ShadowEnabled || _lastShadowColor != ShadowColor
            || _lastShadowOffset != ShadowOffset || _lastShadowSize != ShadowSize)
            ApplyStyle();
    }

    private void ApplyStyle()
    {
        _lastStyle = Style;
        _lastSize = FontSize;
        _lastSpacing = LetterSpacing;
        _lastShadow = ShadowEnabled;
        _lastShadowColor = ShadowColor;
        _lastShadowOffset = ShadowOffset;
        _lastShadowSize = ShadowSize;

        var fontRegular = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Regular.ttf");
        var fontBold = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Bold.ttf");
        var fontDisplay = ResourceLoader.Load<Font>("res://src/Assets/Fonts/moonhouse.ttf");

        switch (Style)
        {
            case KernexLabelStyle.Title:
                SetFont(fontDisplay ?? fontBold, FontSize > 0 ? FontSize : 28, Colors.White);
                Uppercase = true;
                break;

            case KernexLabelStyle.Subtitle:
                var subtitleVariation = new FontVariation();
                subtitleVariation.BaseFont = fontDisplay ?? fontRegular;
                subtitleVariation.VariationEmbolden = 0.8f;
                SetFont(subtitleVariation, FontSize > 0 ? FontSize : 14, Colors.White);
                Uppercase = true;
                break;

            case KernexLabelStyle.Body:
                SetFont(fontRegular, FontSize > 0 ? FontSize : 14, ColorTextPrimary);
                break;

            case KernexLabelStyle.Caption:
                SetFont(fontRegular, FontSize > 0 ? FontSize : 11, ColorTextDim);
                break;

            case KernexLabelStyle.Monospace:
                SetFont(fontRegular, FontSize > 0 ? FontSize : 13, ColorAccent);
                break;

            case KernexLabelStyle.Logo:
                SetFont(fontDisplay ?? fontBold, FontSize > 0 ? FontSize : 52, Colors.White);
                Uppercase = true;
                break;
        }

        ApplyLabelSettings();
    }

    private void ApplyLabelSettings()
    {
        // If a LabelSettings was configured in the Inspector (.tscn), keep it
        if (LabelSettings != null) return;

        // Only create LabelSettings via code at runtime if needed
        if (Engine.IsEditorHint()) return;

        if (!ShadowEnabled && LetterSpacing == 0) return;

        var font = GetThemeFont("font");
        var size = GetThemeFontSize("font_size");
        var color = GetThemeColor("font_color");

        if (LetterSpacing != 0 && font != null)
        {
            var variation = new FontVariation();
            variation.BaseFont = font;
            variation.SpacingGlyph = LetterSpacing;
            font = variation;
        }

        var settings = new LabelSettings
        {
            Font = font,
            FontSize = size,
            FontColor = color,
        };

        if (ShadowEnabled)
        {
            settings.ShadowColor = ShadowColor;
            settings.ShadowOffset = ShadowOffset;
            settings.ShadowSize = ShadowSize;
        }

        LabelSettings = settings;
    }

    private void SetFont(Font font, int size, Color color)
    {
        AddThemeFontOverride("font", font);
        AddThemeFontSizeOverride("font_size", size);
        AddThemeColorOverride("font_color", color);
    }
}
