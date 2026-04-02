using Godot;
using ProjectKernex.Components.UI;

namespace ProjectKernex.Screens.MainMenu;

public partial class MainMenu : Control
{
    private KernexButton _newGameButton;
    private KernexButton _continueButton;
    private KernexButton _settingsButton;
    private KernexButton _creditsButton;
    private KernexButton _quitButton;
    private VBoxContainer _buttonsContainer;

    public override void _Ready()
    {
        _buttonsContainer = GetNode<VBoxContainer>("ContentLayer/CenterContent/ButtonsContainer");
        _newGameButton = GetNode<KernexButton>("ContentLayer/CenterContent/ButtonsContainer/NewGameButton");
        _continueButton = GetNode<KernexButton>("ContentLayer/CenterContent/ButtonsContainer/ContinueButton");
        _settingsButton = GetNode<KernexButton>("ContentLayer/CenterContent/ButtonsContainer/SettingsButton");
        _creditsButton = GetNode<KernexButton>("ContentLayer/CenterContent/ButtonsContainer/CreditsButton");
        _quitButton = GetNode<KernexButton>("ContentLayer/CenterContent/ButtonsContainer/QuitButton");

        _newGameButton.Pressed += OnNewGamePressed;
        _continueButton.Pressed += OnContinuePressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _creditsButton.Pressed += OnCreditsPressed;
        _quitButton.Pressed += OnQuitPressed;

        // Disable continue if no save exists
        _continueButton.SetDisabled(!HasSaveGame());

        // Stagger animation: buttons slide in one by one
        AnimateButtonsIn();
    }

    private void AnimateButtonsIn()
    {
        var buttons = _buttonsContainer.GetChildren();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] is not Control btn) continue;

            btn.Modulate = new Color(1, 1, 1, 0);
            btn.Position += new Vector2(40, 0);

            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(btn, "modulate:a", 1.0f, 0.3f)
                .SetDelay(0.1f * i)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(btn, "position:x", btn.Position.X - 40, 0.3f)
                .SetDelay(0.1f * i)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
        }
    }

    private static bool HasSaveGame()
    {
        return FileAccess.FileExists("user://savegame.json");
    }

    private void OnNewGamePressed()
    {
        GD.Print("[MainMenu] New Game — not implemented yet");
    }

    private void OnContinuePressed()
    {
        GD.Print("[MainMenu] Continue — not implemented yet");
    }

    private void OnSettingsPressed()
    {
        GetNode<Autoloads.ScreenManager>("/root/ScreenManager")
            .ChangeScreen("res://src/Screens/Settings/Settings.tscn");
    }

    private void OnCreditsPressed()
    {
        GD.Print("[MainMenu] Credits — not implemented yet");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
