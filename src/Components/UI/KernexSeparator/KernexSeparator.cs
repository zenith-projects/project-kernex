using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexSeparator : HSeparator
{
    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f, 0.15f);

    public override void _Ready()
    {
        var style = new StyleBoxLine
        {
            Color = ColorGold,
            Thickness = 1,
            GrowBegin = 0,
            GrowEnd = 0,
        };
        AddThemeStyleboxOverride("separator", style);
        AddThemeConstantOverride("separation", 16);
    }
}
