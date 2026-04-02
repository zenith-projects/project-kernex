using Godot;
using ProjectKernex.Nodes.Shell;
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;
using ProjectKernex.Systems.Shell.Commands;

namespace ProjectKernex.Screens.DevTools;

public partial class ShellTestScene : Control
{
    private ShellOverlay _shell;
    private KiraEngine _kira;

    public override void _Ready()
    {
        _shell = GetNode<ShellOverlay>("ShellOverlay");

        var config = new AgentConfig { AgentId = "kira-shell", DisplayName = "KIRA" };
        _kira = new KiraEngine(config, new ScriptedDialogueProvider());
        _kira.SetLevel(Core.Enums.KiraLevel.Corrupted);

        // Only register external commands — built-ins are auto-registered by ShellOverlay
        _shell.RegisterCommand(new KiraCommand(_kira));

        _shell.ShowWelcomeBanner();
    }
}
