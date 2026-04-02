using Godot;

namespace ProjectKernex.Components.UI;

public enum CornerPosition { TopLeft, TopRight, BottomLeft, BottomRight }

[GlobalClass, Tool]
public partial class KernexCorner : Control
{
    [Export] public CornerPosition Corner { get; set; } = CornerPosition.TopLeft;
    [Export(PropertyHint.Range, "4,200,1")] public float ArmLength { get; set; } = 8f;
    [Export(PropertyHint.Range, "0.5,6,0.5")] public float Thickness { get; set; } = 2f;
    [Export] public Color LineColor { get; set; } = new(1f, 1f, 1f, 1f);

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(ArmLength, ArmLength);
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Draw()
    {
        var w = Size.X;
        var h = Size.Y;
        var arm = Mathf.Min(ArmLength, Mathf.Min(w, h));
        var t = Thickness;

        // Draw as a filled L-shape polygon — no broken corner
        Vector2[] points;

        switch (Corner)
        {
            case CornerPosition.TopLeft:
                points = new Vector2[]
                {
                    new(0, 0), new(arm, 0),
                    new(arm, t), new(t, t),
                    new(t, arm), new(0, arm),
                };
                break;
            case CornerPosition.TopRight:
                points = new Vector2[]
                {
                    new(w - arm, 0), new(w, 0),
                    new(w, arm), new(w - t, arm),
                    new(w - t, t), new(w - arm, t),
                };
                break;
            case CornerPosition.BottomLeft:
                points = new Vector2[]
                {
                    new(0, h - arm), new(t, h - arm),
                    new(t, h - t), new(arm, h - t),
                    new(arm, h), new(0, h),
                };
                break;
            default: // BottomRight
                points = new Vector2[]
                {
                    new(w - t, h - arm), new(w, h - arm),
                    new(w, h), new(w - arm, h),
                    new(w - arm, h - t), new(w - t, h - t),
                };
                break;
        }

        DrawColoredPolygon(points, LineColor);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            QueueRedraw();
    }
}
