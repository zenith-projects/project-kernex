using Godot;
using ProjectKernex.Components.HUD;
using ProjectKernex.Core.Enums;
using ProjectKernex.Core.Interfaces;
using ProjectKernex.Systems.Shell;
using ProjectKernex.Systems.Shell.Commands;

namespace ProjectKernex.Nodes.Shell;

public partial class ShellOverlay : CanvasLayer
{
    [Signal] public delegate void CommandExecutedEventHandler(string command, string output);

    private ShellDisplay _display;
    private ShellInput _input;
    private CommandRegistry _registry;

    public CommandRegistry Registry => _registry;

    public override void _Ready()
    {
        _display = GetNode<ShellDisplay>("Panel/MainLayout/ShellDisplay");
        _input = GetNode<ShellInput>("Panel/MainLayout/ShellInput");

        _registry = new CommandRegistry();
        RegisterBuiltInCommands();

        _input.CommandSubmitted += OnCommandSubmitted;

        CallDeferred(MethodName.FocusInput);
    }

    public void RegisterCommand(ICommand command) => _registry.Register(command);

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

    private void RegisterBuiltInCommands()
    {
        _registry.Register(new HelpCommand(_registry));
        _registry.Register(new ClearCommand());
        _registry.Register(new StatusCommand());
        _registry.Register(new WhoamiCommand());
        _registry.Register(new UptimeCommand());
    }

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
