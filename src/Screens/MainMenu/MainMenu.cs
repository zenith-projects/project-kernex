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
	private VBoxContainer _menuContent;
	private Control _settingsPanel;

	public override void _Ready()
	{
		_menuContent = GetNode<VBoxContainer>("ContentLayer/MenuContent");
		_settingsPanel = GetNode<Control>("ContentLayer/SettingsPanel");

		_newGameButton = GetNode<KernexButton>("ContentLayer/MenuContent/ButtonsContainer/NewGameButton");
		_continueButton = GetNode<KernexButton>("ContentLayer/MenuContent/ButtonsContainer/ContinueButton");
		_settingsButton = GetNode<KernexButton>("ContentLayer/MenuContent/ButtonsContainer/SettingsButton");
		_creditsButton = GetNode<KernexButton>("ContentLayer/MenuContent/ButtonsContainer/CreditsButton");
		_quitButton = GetNode<KernexButton>("ContentLayer/MenuContent/ButtonsContainer/QuitButton");

		_newGameButton.Pressed += OnNewGamePressed;
		_continueButton.Pressed += OnContinuePressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_creditsButton.Pressed += OnCreditsPressed;
		_quitButton.Pressed += OnQuitPressed;

		_continueButton.SetDisabled(!HasSaveGame());

		AnimateButtonsIn();
	}

	private void AnimateButtonsIn()
	{
		var buttons = GetNode<VBoxContainer>("ContentLayer/MenuContent/ButtonsContainer").GetChildren();
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

	public void ShowSettings()
	{
		_menuContent.Visible = false;
		_settingsPanel.Visible = true;
	}

	public void HideSettings()
	{
		_settingsPanel.Visible = false;
		_menuContent.Visible = true;
	}

	private static bool HasSaveGame() => FileAccess.FileExists("user://savegame.json");

	private void OnNewGamePressed() => GD.Print("[MainMenu] New Game — not implemented yet");
	private void OnContinuePressed() => GD.Print("[MainMenu] Continue — not implemented yet");
	private void OnSettingsPressed() => ShowSettings();
	private void OnCreditsPressed() => GD.Print("[MainMenu] Credits — not implemented yet");
	private void OnQuitPressed() => GetTree().Quit();
}
