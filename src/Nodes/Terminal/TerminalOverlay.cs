using System.Threading.Tasks;
using Godot;
using ProjectKernex.Components.HUD;
using ProjectKernex.Core.Enums;
using ProjectKernex.Core.Interfaces;
using ProjectKernex.Systems.Terminal;

namespace ProjectKernex.Nodes.Terminal;

public partial class TerminalOverlay : CanvasLayer
{
    [Signal] public delegate void CommandExecutedEventHandler(string command, string output);

    private TerminalDisplay _display;
    private TerminalInput _input;
    private CommandRegistry _registry;

    public CommandRegistry Registry => _registry;

    public override void _Ready()
    {
        _display = GetNode<TerminalDisplay>("Panel/MainLayout/TerminalDisplay");
        _input = GetNode<TerminalInput>("Panel/MainLayout/TerminalInput");

        _registry = new CommandRegistry();
        _input.CommandSubmitted += OnCommandSubmitted;

        CallDeferred(MethodName.FocusInput);
    }

    public void ShowWelcomeBanner()
    {
        _display.AppendLine("==========================================", CommandResultType.System);
        _display.AppendLine("  vsh — Kernex Shell v0.1", CommandResultType.System);
        _display.AppendLine("  Type 'help' for available commands", CommandResultType.System);
        _display.AppendLine("==========================================", CommandResultType.System);
        _display.AppendLine("", CommandResultType.System);
    }

    public void AppendLine(string text, CommandResultType type = CommandResultType.Info) =>
        _display.AppendLine(text, type);

    public void AppendLines(string[] lines, CommandResultType type = CommandResultType.Info) =>
        _display.AppendLines(lines, type);

    private async void OnCommandSubmitted(string rawInput)
    {
        _display.AppendLine($"vsh> {rawInput}", CommandResultType.System);

        var parsed = CommandParser.Parse(rawInput);
        if (string.IsNullOrEmpty(parsed.CommandName)) return;

        var command = _registry.GetCommand(parsed.CommandName);
        if (command == null)
        {
            _display.AppendLine($"vsh: command not found: {parsed.CommandName}. Type 'help' for available commands.", CommandResultType.Error);
            return;
        }

        if (parsed.HasFlag("help") || parsed.HasFlag("h"))
        {
            _display.AppendLine(command.Usage, CommandResultType.Info);
            return;
        }

        try
        {
            var result = await command.ExecuteAsync(parsed);
            HandleResult(result);
            EmitSignal(SignalName.CommandExecuted, rawInput, string.Join("\n", result.Lines));
        }
        catch (System.Exception ex)
        {
            _display.AppendLine($"Error: {ex.Message}", CommandResultType.Error);
        }
    }

    private void HandleResult(CommandResult result)
    {
        if (result.Type == CommandResultType.Clear)
        {
            _display.Clear();
            return;
        }

        _display.AppendLines(result.Lines, result.Type);
    }

    private void FocusInput() => _input.FocusInput();
}
