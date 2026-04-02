using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexHatch : Control
{
    [Export(PropertyHint.Range, "3,30,1")] public float LineSpacing { get; set; } = 8f;
    [Export(PropertyHint.Range, "0.5,4,0.5")] public float LineThickness { get; set; } = 2f;
    [Export(PropertyHint.Range, "5,50,1")] public float LineLength { get; set; } = 12f;
    [Export(PropertyHint.Range, "-90,90,5")] public float Angle { get; set; } = 45f;
    [Export] public Color LineColor { get; set; } = new(0.7f, 0.75f, 0.85f, 0.25f);
    [Export] public bool Horizontal { get; set; } = true;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Draw()
    {
        var w = Size.X;
        var h = Size.Y;
        var rad = Mathf.DegToRad(Angle);
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);
        var halfLen = LineLength * 0.5f;
        var halfT = LineThickness * 0.5f;

        // Direction along the line
        var dx = cos * halfLen;
        var dy = sin * halfLen;
        // Perpendicular for thickness
        var px = -sin * halfT;
        var py = cos * halfT;

        if (Horizontal)
        {
            var cy = h / 2;
            var count = (int)(w / LineSpacing);
            var startX = (w - count * LineSpacing) / 2 + LineSpacing / 2;

            for (int i = 0; i <= count; i++)
            {
                var cx = startX + i * LineSpacing;
                DrawColoredPolygon(new Vector2[]
                {
                    new(cx - dx + px, cy - dy + py),
                    new(cx + dx + px, cy + dy + py),
                    new(cx + dx - px, cy + dy - py),
                    new(cx - dx - px, cy - dy - py),
                }, LineColor);
            }
        }
        else
        {
            var cx = w / 2;
            var count = (int)(h / LineSpacing);
            var startY = (h - count * LineSpacing) / 2 + LineSpacing / 2;

            for (int i = 0; i <= count; i++)
            {
                var cy = startY + i * LineSpacing;
                DrawColoredPolygon(new Vector2[]
                {
                    new(cx - dx + px, cy - dy + py),
                    new(cx + dx + px, cy + dy + py),
                    new(cx + dx - px, cy + dy - py),
                    new(cx - dx - px, cy - dy - py),
                }, LineColor);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            QueueRedraw();
    }
}
