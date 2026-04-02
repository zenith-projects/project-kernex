using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Components.HUD;

public partial class ShellDisplay : PanelContainer
{
    [Export] public int MaxLines { get; set; } = 500;
    [Export] public Color SuccessColor { get; set; } = new(0.3f, 0.9f, 0.3f);
    [Export] public Color ErrorColor { get; set; } = new(1.0f, 0.3f, 0.3f);
    [Export] public Color InfoColor { get; set; } = new(0.7f, 0.7f, 0.7f);
    [Export] public Color WarningColor { get; set; } = new(1.0f, 0.8f, 0.2f);
    [Export] public Color AxiaColor { get; set; } = new(0.0f, 1.0f, 0.8f);
    [Export] public Color SystemColor { get; set; } = new(0.5f, 0.5f, 0.5f);

    private RichTextLabel _output;
    private ScrollContainer _scroll;
    private int _lineCount;

    public override void _Ready()
    {
        _scroll = GetNode<ScrollContainer>("MarginContainer/ScrollContainer");
        _output = GetNode<RichTextLabel>("MarginContainer/ScrollContainer/Output");
        _output.BbcodeEnabled = true;
        _output.ScrollFollowing = true;
    }

    public void AppendLine(string text, CommandResultType type = CommandResultType.Info)
    {
        var color = GetColor(type);
        var hex = color.ToHtml(false);
        _output.AppendText($"[color=#{hex}]{EscapeBbcode(text)}[/color]\n");
        _lineCount++;
        TrimIfNeeded();
    }

    public void AppendLines(string[] lines, CommandResultType type = CommandResultType.Info)
    {
        foreach (var line in lines)
            AppendLine(line, type);
    }

    public void AppendRaw(string bbcodeText)
    {
        _output.AppendText(bbcodeText + "\n");
        _lineCount++;
        TrimIfNeeded();
    }

    public void Clear()
    {
        _output.Clear();
        _lineCount = 0;
    }

    private Color GetColor(CommandResultType type) => type switch
    {
        CommandResultType.Success => SuccessColor,
        CommandResultType.Error => ErrorColor,
        CommandResultType.Info => InfoColor,
        CommandResultType.Warning => WarningColor,
        CommandResultType.Axia => AxiaColor,
        CommandResultType.System => SystemColor,
        _ => InfoColor
    };

    private static string EscapeBbcode(string text) =>
        text.Replace("[", "[lb]");

    private void TrimIfNeeded()
    {
        if (_lineCount <= MaxLines) return;
        var fullText = _output.Text;
        var firstNewline = fullText.IndexOf('\n');
        if (firstNewline < 0) return;
        _output.Clear();
        _output.AppendText(fullText[(firstNewline + 1)..]);
        _lineCount--;
    }
}
