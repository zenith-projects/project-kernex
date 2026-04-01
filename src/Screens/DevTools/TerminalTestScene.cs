using Godot;
using ProjectKernex.Nodes.Terminal;
using ProjectKernex.Systems.AI;
using ProjectKernex.Systems.Terminal.Commands;

namespace ProjectKernex.Screens.DevTools;

public partial class TerminalTestScene : Control
{
    private TerminalOverlay _terminal;
    private KiraEngine _kira;

    public override void _Ready()
    {
        _terminal = GetNode<TerminalOverlay>("TerminalOverlay");

        _kira = new KiraEngine(new ScriptedDialogueProvider());
        _kira.SetLevel(Core.Enums.KiraLevel.Corrupted);

        _terminal.Registry.Register(new HelpCommand(_terminal.Registry));
        _terminal.Registry.Register(new ClearCommand());
        _terminal.Registry.Register(new StatusCommand());
        _terminal.Registry.Register(new WhoamiCommand());
        _terminal.Registry.Register(new UptimeCommand());
        _terminal.Registry.Register(new KiraCommand(_kira));

        _terminal.ShowWelcomeBanner();
    }
}
