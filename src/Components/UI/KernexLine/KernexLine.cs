using Godot;

namespace ProjectKernex.Components.UI;

public enum LineOrientation { Horizontal, Vertical }
public enum LineEdge { Start, Center, End }

[GlobalClass, Tool]
public partial class KernexLine : Control
{
    [Export] public LineOrientation Orientation { get; set; } = LineOrientation.Horizontal;
    [Export(PropertyHint.Range, "0.5,6,0.5")] public float Thickness { get; set; } = 2f;
    [Export] public Color LineColor { get; set; } = new(0.7f, 0.75f, 0.85f, 0.3f);
    [Export] public LineEdge Edge { get; set; } = LineEdge.Start;
    [Export(PropertyHint.Range, "0,50,1")] public float Padding { get; set; } = 0f;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        if (Orientation == LineOrientation.Horizontal)
            CustomMinimumSize = new Vector2(0, 8);
        else
            CustomMinimumSize = new Vector2(8, 0);
    }

    public override void _Draw()
    {
        var t = Thickness;
        Vector2[] points;

        if (Orientation == LineOrientation.Horizontal)
        {
            var y = Edge switch
            {
                LineEdge.Start => 0f,
                LineEdge.End => Size.Y - t,
                _ => (Size.Y - t) / 2,
            };
            points = new Vector2[]
            {
                new(Padding, y), new(Size.X - Padding, y),
                new(Size.X - Padding, y + t), new(Padding, y + t),
            };
        }
        else
        {
            var x = Edge switch
            {
                LineEdge.Start => 0f,
                LineEdge.End => Size.X - t,
                _ => (Size.X - t) / 2,
            };
            points = new Vector2[]
            {
                new(x, Padding), new(x + t, Padding),
                new(x + t, Size.Y - Padding), new(x, Size.Y - Padding),
            };
        }

        DrawColoredPolygon(points, LineColor);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            QueueRedraw();
    }
}
