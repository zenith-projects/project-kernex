using Godot;

namespace ProjectKernex.Components.UI;

public enum KernexLabelStyle
{
    Title,
    Subtitle,
    Body,
    Caption,
    Monospace,
}

[GlobalClass, Tool]
public partial class KernexLabel : Label
{
    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorTextPrimary = new(0.75f, 0.75f, 0.78f);
    private static readonly Color ColorTextDim = new(0.45f, 0.45f, 0.47f);

    [Export] public KernexLabelStyle Style { get; set; } = KernexLabelStyle.Body;

    private KernexLabelStyle _appliedStyle = (KernexLabelStyle)(-1);

    public override void _Ready() => ApplyStyle();

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (_appliedStyle != Style) ApplyStyle();
    }

    private void ApplyStyle()
    {
        _appliedStyle = Style;

        var fontRegular = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Regular.ttf");
        var fontBold = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Bold.ttf");

        switch (Style)
        {
            case KernexLabelStyle.Title:
                AddThemeFontOverride("font", fontBold ?? fontRegular);
                AddThemeFontSizeOverride("font_size", 36);
                AddThemeColorOverride("font_color", Colors.White);
                UppercaseText();
                break;

            case KernexLabelStyle.Subtitle:
                AddThemeFontOverride("font", fontRegular);
                AddThemeFontSizeOverride("font_size", 14);
                AddThemeColorOverride("font_color", ColorTextDim);
                UppercaseText();
                break;

            case KernexLabelStyle.Body:
                AddThemeFontOverride("font", fontRegular);
                AddThemeFontSizeOverride("font_size", 14);
                AddThemeColorOverride("font_color", ColorTextPrimary);
                break;

            case KernexLabelStyle.Caption:
                AddThemeFontOverride("font", fontRegular);
                AddThemeFontSizeOverride("font_size", 11);
                AddThemeColorOverride("font_color", ColorTextDim);
                break;

            case KernexLabelStyle.Monospace:
                AddThemeFontOverride("font", fontRegular);
                AddThemeFontSizeOverride("font_size", 13);
                AddThemeColorOverride("font_color", ColorGold);
                break;
        }
    }

    private void UppercaseText()
    {
        Uppercase = true;
    }
}
