using Godot;

namespace ProjectKernex.Screens.Root;

public partial class Root : Control
{
    [Export] public PackedScene MainMenuScene { get; set; }

    public override void _Ready()
    {
        var mainMenu = MainMenuScene?.Instantiate();
        if (mainMenu != null)
        {
            AddChild(mainMenu);
            GetNode<Autoloads.ScreenManager>("/root/ScreenManager").SetInitialScreen(mainMenu);
        }
    }
}
