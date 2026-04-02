using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Components.HUD;

public partial class AxiaAvatar : Control
{
    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorGoldDim = new(0.831f, 0.659f, 0.294f, 0.3f);
    private static readonly Color ColorCorrupted = new(0.5f, 0.2f, 0.2f);
    private static readonly Color ColorBooting = new(0.6f, 0.5f, 0.2f);
    private static readonly Color ColorFunctional = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorAdvanced = new(0.91f, 0.75f, 0.38f);

    private PanelContainer _avatarRect;
    private Label _levelLabel;

    public override void _Ready()
    {
        _avatarRect = GetNode<PanelContainer>("AvatarRect");
        _levelLabel = GetNode<Label>("AvatarRect/LevelLabel");

        _levelLabel.AddThemeColorOverride("font_color", ColorGold);
        _levelLabel.AddThemeFontSizeOverride("font_size", 11);
        _levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _levelLabel.VerticalAlignment = VerticalAlignment.Center;
    }

    public void SetLevel(AxiaLevel level)
    {
        _levelLabel.Text = $"LV.{(int)level}";

        var color = level switch
        {
            AxiaLevel.Corrupted => ColorCorrupted,
            AxiaLevel.Booting => ColorBooting,
            AxiaLevel.Functional => ColorFunctional,
            _ => ColorAdvanced
        };

        // Border color indicates level
        var style = new StyleBoxFlat
        {
            BgColor = new Color(color.R, color.G, color.B, 0.1f),
            BorderColor = new Color(color.R, color.G, color.B, 0.6f),
            BorderWidthLeft = 2, BorderWidthTop = 2,
            BorderWidthRight = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            AntiAliasing = true,
        };
        _avatarRect.AddThemeStyleboxOverride("panel", style);

        _levelLabel.AddThemeColorOverride("font_color", color);
    }
}
