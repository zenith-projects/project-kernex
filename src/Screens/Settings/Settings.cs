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
    private KernexDropdown _resolutionDropdown;
    private KernexToggle _fullscreenToggle;
    private KernexToggle _vsyncToggle;

    // Audio
    private KernexSlider _masterSlider;
    private KernexSlider _musicSlider;
    private KernexSlider _sfxSlider;

    private readonly ConfigFile _configFile = new();
    private const string ConfigPath = "user://settings.cfg";

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        _graphicsTab = GetNode<KernexButton>("Layout/TabBar/GraphicsTab");
        _audioTab = GetNode<KernexButton>("Layout/TabBar/AudioTab");
        _controlsTab = GetNode<KernexButton>("Layout/TabBar/ControlsTab");

        _graphicsContent = GetNode<VBoxContainer>("Layout/ContentPanel/GraphicsContent");
        _audioContent = GetNode<VBoxContainer>("Layout/ContentPanel/AudioContent");
        _controlsContent = GetNode<VBoxContainer>("Layout/ContentPanel/ControlsContent");

        _backButton = GetNode<KernexButton>("Layout/BottomBar/BackButton");
        _applyButton = GetNode<KernexButton>("Layout/BottomBar/ApplyButton");

        _resolutionDropdown = GetNode<KernexDropdown>("Layout/ContentPanel/GraphicsContent/ResolutionDropdown");
        _fullscreenToggle = GetNode<KernexToggle>("Layout/ContentPanel/GraphicsContent/FullscreenToggle");
        _vsyncToggle = GetNode<KernexToggle>("Layout/ContentPanel/GraphicsContent/VsyncToggle");

        _masterSlider = GetNode<KernexSlider>("Layout/ContentPanel/AudioContent/MasterSlider");
        _musicSlider = GetNode<KernexSlider>("Layout/ContentPanel/AudioContent/MusicSlider");
        _sfxSlider = GetNode<KernexSlider>("Layout/ContentPanel/AudioContent/SfxSlider");

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
        _graphicsContent.Visible = index == 0;
        _audioContent.Visible = index == 1;
        _controlsContent.Visible = index == 2;
    }

    private void SetupResolutionOptions()
    {
        _resolutionDropdown.AddItem("1920x1080");
        _resolutionDropdown.AddItem("1600x900");
        _resolutionDropdown.AddItem("1280x720");
        _resolutionDropdown.AddItem("2560x1440");
        _resolutionDropdown.AddItem("3840x2160");
    }

    private void LoadSettings()
    {
        if (_configFile.Load(ConfigPath) != Error.Ok) return;

        _fullscreenToggle.SetValue((bool)_configFile.GetValue("graphics", "fullscreen", false));
        _vsyncToggle.SetValue((bool)_configFile.GetValue("graphics", "vsync", true));
        _resolutionDropdown.Selected = (int)_configFile.GetValue("graphics", "resolution", 0);
        _masterSlider.SetValue((float)(double)_configFile.GetValue("audio", "master", 80.0));
        _musicSlider.SetValue((float)(double)_configFile.GetValue("audio", "music", 70.0));
        _sfxSlider.SetValue((float)(double)_configFile.GetValue("audio", "sfx", 80.0));
    }

    private void SaveSettings()
    {
        _configFile.SetValue("graphics", "fullscreen", _fullscreenToggle.Value);
        _configFile.SetValue("graphics", "vsync", _vsyncToggle.Value);
        _configFile.SetValue("graphics", "resolution", _resolutionDropdown.Selected);
        _configFile.SetValue("audio", "master", (double)_masterSlider.Value);
        _configFile.SetValue("audio", "music", (double)_musicSlider.Value);
        _configFile.SetValue("audio", "sfx", (double)_sfxSlider.Value);
        _configFile.Save(ConfigPath);
    }

    private void ApplySettings()
    {
        if (_fullscreenToggle.Value)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);

        DisplayServer.WindowSetVsyncMode(
            _vsyncToggle.Value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }

    private void OnApplyPressed()
    {
        ApplySettings();
        SaveSettings();
    }

    private void OnBackPressed()
    {
        // SettingsPanel → ContentLayer → MainMenu
        var mainMenu = GetParent()?.GetParent<MainMenu.MainMenu>();
        mainMenu?.HideSettings();
    }
}
