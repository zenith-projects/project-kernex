using Godot;
using ProjectKernex.Nodes.Shell;
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;
using ProjectKernex.Systems.Shell.Commands;

namespace ProjectKernex.Screens.DevTools;

public partial class ShellTestScene : Control
{
    private ShellOverlay _shell;
    private AxiaEngine _axia;

    public override void _Ready()
    {
        _shell = GetNode<ShellOverlay>("ShellOverlay");

        var config = new AgentConfig { AgentId = "axia-shell", DisplayName = "AXIA" };
        _axia = new AxiaEngine(config, new ScriptedDialogueProvider());
        _axia.SetLevel(Core.Enums.AxiaLevel.Corrupted);

        // Only register external commands — built-ins are auto-registered by ShellOverlay
        _shell.RegisterCommand(new AxiaCommand(_axia));

        _shell.ShowWelcomeBanner();
    }
}
