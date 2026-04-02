using Godot;
using ProjectKernex.Components.UI;

namespace ProjectKernex.Screens.Settings;

public partial class Settings : Control
{
    private KernexButton _graphicsTab;
    private KernexButton _audioTab;
    private KernexButton _controlsTab;

    private VBoxContainer _graphicsContent;
    private VBoxContainer _audioContent;
    private VBoxContainer _controlsContent;

    private KernexButton _backButton;
    private KernexButton _applyButton;

    // Graphics
    private OptionButton _resolutionOption;
    private CheckButton _fullscreenToggle;
    private CheckButton _vsyncToggle;

    // Audio
    private HSlider _masterSlider;
    private HSlider _musicSlider;
    private HSlider _sfxSlider;

    private int _activeTab;
    private readonly ConfigFile _configFile = new();
    private const string ConfigPath = "user://settings.cfg";

    public override void _Ready()
    {
        _graphicsTab = GetNode<KernexButton>("ContentLayer/Layout/TabBar/GraphicsTab");
        _audioTab = GetNode<KernexButton>("ContentLayer/Layout/TabBar/AudioTab");
        _controlsTab = GetNode<KernexButton>("ContentLayer/Layout/TabBar/ControlsTab");

        _graphicsContent = GetNode<VBoxContainer>("ContentLayer/Layout/ContentPanel/GraphicsContent");
        _audioContent = GetNode<VBoxContainer>("ContentLayer/Layout/ContentPanel/AudioContent");
        _controlsContent = GetNode<VBoxContainer>("ContentLayer/Layout/ContentPanel/ControlsContent");

        _backButton = GetNode<KernexButton>("ContentLayer/Layout/BottomBar/BackButton");
        _applyButton = GetNode<KernexButton>("ContentLayer/Layout/BottomBar/ApplyButton");

        _resolutionOption = GetNode<OptionButton>("ContentLayer/Layout/ContentPanel/GraphicsContent/ResolutionRow/ResolutionOption");
        _fullscreenToggle = GetNode<CheckButton>("ContentLayer/Layout/ContentPanel/GraphicsContent/FullscreenToggle");
        _vsyncToggle = GetNode<CheckButton>("ContentLayer/Layout/ContentPanel/GraphicsContent/VsyncToggle");

        _masterSlider = GetNode<HSlider>("ContentLayer/Layout/ContentPanel/AudioContent/MasterRow/MasterSlider");
        _musicSlider = GetNode<HSlider>("ContentLayer/Layout/ContentPanel/AudioContent/MusicRow/MusicSlider");
        _sfxSlider = GetNode<HSlider>("ContentLayer/Layout/ContentPanel/AudioContent/SfxRow/SfxSlider");

        _graphicsTab.Pressed += () => ShowTab(0);
        _audioTab.Pressed += () => ShowTab(1);
        _controlsTab.Pressed += () => ShowTab(2);

        _backButton.Pressed += OnBackPressed;
        _applyButton.Pressed += OnApplyPressed;

        SetupResolutionOptions();
        LoadSettings();
        ShowTab(0);
    }

    private void ShowTab(int index)
    {
        _activeTab = index;
        _graphicsContent.Visible = index == 0;
        _audioContent.Visible = index == 1;
        _controlsContent.Visible = index == 2;
    }

    private void SetupResolutionOptions()
    {
        _resolutionOption.AddItem("1920x1080", 0);
        _resolutionOption.AddItem("1600x900", 1);
        _resolutionOption.AddItem("1280x720", 2);
        _resolutionOption.AddItem("2560x1440", 3);
        _resolutionOption.AddItem("3840x2160", 4);
    }

    private void LoadSettings()
    {
        if (_configFile.Load(ConfigPath) != Error.Ok) return;

        _fullscreenToggle.ButtonPressed = (bool)_configFile.GetValue("graphics", "fullscreen", false);
        _vsyncToggle.ButtonPressed = (bool)_configFile.GetValue("graphics", "vsync", true);
        _resolutionOption.Selected = (int)_configFile.GetValue("graphics", "resolution", 0);
        _masterSlider.Value = (double)_configFile.GetValue("audio", "master", 80.0);
        _musicSlider.Value = (double)_configFile.GetValue("audio", "music", 70.0);
        _sfxSlider.Value = (double)_configFile.GetValue("audio", "sfx", 80.0);
    }

    private void SaveSettings()
    {
        _configFile.SetValue("graphics", "fullscreen", _fullscreenToggle.ButtonPressed);
        _configFile.SetValue("graphics", "vsync", _vsyncToggle.ButtonPressed);
        _configFile.SetValue("graphics", "resolution", _resolutionOption.Selected);
        _configFile.SetValue("audio", "master", _masterSlider.Value);
        _configFile.SetValue("audio", "music", _musicSlider.Value);
        _configFile.SetValue("audio", "sfx", _sfxSlider.Value);
        _configFile.Save(ConfigPath);
    }

    private void ApplySettings()
    {
        // Fullscreen
        if (_fullscreenToggle.ButtonPressed)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);

        // VSync
        DisplayServer.WindowSetVsyncMode(
            _vsyncToggle.ButtonPressed ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }

    private void OnApplyPressed()
    {
        ApplySettings();
        SaveSettings();
    }

    private void OnBackPressed()
    {
        GetNode<Autoloads.ScreenManager>("/root/ScreenManager")
            .ChangeScreen("res://src/Screens/MainMenu/MainMenu.tscn");
    }
}
