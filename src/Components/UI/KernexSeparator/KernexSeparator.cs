using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexSeparator : HSeparator
{
    private static readonly Color ColorAccent = new(0.6f, 0.65f, 0.75f, 0.15f);

    public override void _Ready()
    {
        var style = new StyleBoxLine
        {
            Color = ColorAccent,
            Thickness = 1,
            GrowBegin = 0,
            GrowEnd = 0,
        };
        AddThemeStyleboxOverride("separator", style);
        AddThemeConstantOverride("separation", 16);
    }
}
