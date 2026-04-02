using System;
using System.Collections.Generic;
using Godot;
using ProjectKernex.Components.HUD;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Screens.DevTools;

/// <summary>
/// KIRA debug/test scene with chat panel (left) and debug controls (right).
/// Theme: src/Assets/Themes/Kira/kira_debug_theme.tres; glass shader provides frosted background.
/// </summary>
public partial class KiraTestScene : Control
{
	// Gold accent color for targeted overrides (headers, values, status labels)
	private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f, 1f);
	private static readonly Color ColorGoldBright = new(0.910f, 0.753f, 0.376f, 1f);
	private static readonly Color ColorTextSecondary = new(0.502f, 0.502f, 0.502f, 1f);

	private KiraPanel _kiraPanel;
	private KiraEngine _kira;
	private ScriptedDialogueProvider _scriptedProvider;
	private AgentConfig _mainAgentConfig;
	private AgentConfig _proactiveAgentConfig;
	private KiraDebugConfig _config;
	private bool _loadingConfig;

	// Memory + Proactive
	private AgentMemoryStore _memoryStore; // convenience ref → _kira.MainAgent.Memory
	private KiraProactiveEngine _proactiveEngine;
	private AgentRunner _proactiveAgent;
	private volatile string _downloadMainStatus = "";
	private volatile float _downloadMainPct = -1f;
	private volatile string _downloadLightStatus = "";
	private volatile float _downloadLightPct = -1f;
	private volatile string _downloadError = "";
	private bool _downloading;
	private System.Threading.CancellationTokenSource _downloadCts;

	// Download overlay UI
	private PanelContainer _downloadOverlay;
	private ProgressBar _downloadMainBar;
	private Label _downloadMainLabel;
	private ProgressBar _downloadLightBar;
	private Label _downloadLightLabel;
	private Label _downloadErrorLabel;

	// Emotion + Portrait
	private LineEdit _emotionDisplay;
	private TextureRect _kiraPortrait;
	private TextureRect _kiraPortraitNext;
	private PanelContainer _kiraPortraitFrame;
	private Tween _emotionTween;
	private readonly Dictionary<string, Texture2D> _emotionTextures = new();

	// Model
	private Button _cancelDownloadButton;
	private LineEdit _modelPathInput;
	private Button _loadModelButton;
	private Button _unloadModelButton;
	private Label _llmStatusLabel;
	private HSlider _gpuLayersSlider;
	private Label _gpuLayersValue;
	private HSlider _contextSizeSlider;
	private Label _contextSizeValue;

	// KIRA
	private OptionButton _levelSelector;
	// _llmToggle removed — LLM always active for level 3+
	private HSlider _responseDelaySlider;
	private Label _responseDelayValue;
	private HSlider _glitchSlider;
	private Label _glitchValue;

	// Inference
	private HSlider _temperatureSlider, _topPSlider, _topKSlider, _minPSlider;
	private Label _temperatureValue, _topPValue, _topKValue, _minPValue;
	private HSlider _maxTokensSlider, _repeatPenaltySlider, _freqPenaltySlider, _presPenaltySlider;
	private Label _maxTokensValue, _repeatPenaltyValue, _freqPenaltyValue, _presPenaltyValue;
	private HSlider _seedSlider;
	private Label _seedValue;
	private OptionButton _mirostatSelector;
	private HSlider _mirostatTauSlider, _mirostatEtaSlider;
	private Label _mirostatTauValue, _mirostatEtaValue;

	// Prompt
	private TextEdit _systemPromptEdit;
	private Button _resetPromptButton;
	private TextEdit _antiPromptsEdit;

	// Game Context
	private HSlider _hullSlider, _powerSlider, _dronesActiveSlider, _dronesIdleSlider, _storageSlider;
	private Label _hullValue, _powerValue, _dronesActiveValue, _dronesIdleValue, _storageValue;
	private OptionButton _threatSelector;
	private LineEdit _sectorInput;
	private LineEdit _factionInput;

	// Output
	private RichTextLabel _statusLog;
	private Button _clearLogButton;
	private Button _clearChatButton;
	private Label _lastResponseTimeLabel;
	private Label _lastTokenCountLabel;

	// Proactive debug controls
	private CheckButton _proactiveEnabledToggle;
	private HSlider _proactiveIntervalSlider;
	private Label _proactiveIntervalValue;
	private Label _proactiveCountLabel;
	private Label _proactiveLastCheckLabel;
	private LineEdit _proactiveModelPathInput;
	private Button _forceCheckButton;

	// Agent selector
	private OptionButton _agentSelector;
	private AgentRunner _selectedAgent;

	// Memory debug controls
	private Label _memoryMessagesLabel;
	private Label _memorySummariesLabel;
	private Button _clearMemoryButton;

	// Node path prefix for debug panel controls
	private const string D = "ScreenMargin/HSplitContainer/DebugPanel/ScrollContainer/MarginContainer/ControlsLayout/";

	// Collapsible section tracking
	private readonly Dictionary<string, (Button btn, VBoxContainer content)> _sections = new();

	public override void _Ready()
	{
		_kiraPanel = GetNode<KiraPanel>("ScreenMargin/HSplitContainer/KiraPanel");

		_config = KiraDebugConfig.LoadOrCreate();

		GetDebugControls();
		ApplyAccentColors();
		SetupTextEdits();
		SetupKira();
		SetupAllControls();
		LoadConfigToUI();

		// Convenience ref to memory managed by AgentRunner
		_memoryStore = (AgentMemoryStore)_kira.MainAgent.Memory;

		// Initialize proactive engine with sub-agent
		_proactiveEngine = new KiraProactiveEngine();
		_proactiveEngine.SetAgent(_proactiveAgent);
		_proactiveEngine.ProactiveMessageReady += OnProactiveMessage;

		_kiraPanel.MessageSubmitted += OnMessageSubmitted;
		_kiraPanel.SetLevel((KiraLevel)_config.KiraLevel);

		// Restore recent chat messages from memory
		RestoreChatFromMemory();
		SetKiraEmotion(_config.CurrentEmotion ?? "NEUTRAL");

		CreateDownloadOverlay();
		_kiraPanel.FocusInput();

		Log("KIRA Test Scene ready. Config loaded.");
		Log($"Memory: {_memoryStore.MessageCount} messages, {_memoryStore.SummaryCount} summaries");
		UpdateMemoryLabels();
		AutoLoadModel();
	}

	public override void _Process(double delta)
	{
		_proactiveEngine?.Update((float)delta);

		// Update download overlay on main thread
		if (_downloading && _downloadOverlay != null)
		{
			if (_downloadMainPct >= 0)
			{
				_downloadMainBar.Value = _downloadMainPct * 100;
				_downloadMainLabel.Text = _downloadMainStatus;
			}
			if (_downloadLightPct >= 0)
			{
				_downloadLightBar.Value = _downloadLightPct * 100;
				_downloadLightLabel.Text = _downloadLightStatus;
			}
			if (!string.IsNullOrEmpty(_downloadError))
			{
				_downloadErrorLabel.Text = _downloadError;
				_downloadError = "";
			}
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest || what == NotificationPredelete)
			SaveConfig();
	}

	private void Log(string message) =>
		_statusLog?.AppendText($"[{Time.GetTicksMsec() / 1000f:F1}s] {message}\n");

	private static string Truncate(string text, int maxLength) =>
		text.Length <= maxLength ? text : text[..maxLength] + "...";

	private void CreateDownloadOverlay()
	{
		// Overlay covers the KiraPanel (left side)
		_downloadOverlay = new PanelContainer();
		_kiraPanel.AddChild(_downloadOverlay);
		_downloadOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_downloadOverlay.Visible = false;
		_downloadOverlay.ZIndex = 10;

		var panelStyle = new StyleBoxFlat
		{
			BgColor = new Color(0.04f, 0.04f, 0.055f, 0.97f),
			CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
			CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
		};
		_downloadOverlay.AddThemeStyleboxOverride("panel", panelStyle);

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 40);
		margin.AddThemeConstantOverride("margin_right", 40);
		margin.AddThemeConstantOverride("margin_top", 30);
		margin.AddThemeConstantOverride("margin_bottom", 30);
		_downloadOverlay.AddChild(margin);

		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 12);
		vbox.SizeFlagsVertical = SizeFlags.ShrinkCenter;
		margin.AddChild(vbox);

		// Title
		var title = new Label { Text = "Downloading AI Models...", HorizontalAlignment = HorizontalAlignment.Center };
		title.AddThemeColorOverride("font_color", ColorGold);
		title.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(title);

		var subtitle = new Label { Text = "KIRA needs these models to function. This only happens once.", HorizontalAlignment = HorizontalAlignment.Center };
		subtitle.AddThemeColorOverride("font_color", ColorTextSecondary);
		subtitle.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(subtitle);

		vbox.AddChild(new HSeparator());

		// Main model (7B)
		var mainHeader = new Label { Text = "Main Model — Qwen2.5-7B (4.4 GB)" };
		mainHeader.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		vbox.AddChild(mainHeader);

		_downloadMainBar = new ProgressBar { CustomMinimumSize = new Vector2(0, 22), Value = 0 };
		StyleProgressBar(_downloadMainBar);
		vbox.AddChild(_downloadMainBar);

		_downloadMainLabel = new Label { Text = "Waiting...", HorizontalAlignment = HorizontalAlignment.Center };
		_downloadMainLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
		_downloadMainLabel.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(_downloadMainLabel);

		vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

		// Light model (3B)
		var lightHeader = new Label { Text = "Light Model — Qwen2.5-3B (2.0 GB)" };
		lightHeader.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		vbox.AddChild(lightHeader);

		_downloadLightBar = new ProgressBar { CustomMinimumSize = new Vector2(0, 22), Value = 0 };
		StyleProgressBar(_downloadLightBar);
		vbox.AddChild(_downloadLightBar);

		_downloadLightLabel = new Label { Text = "Waiting...", HorizontalAlignment = HorizontalAlignment.Center };
		_downloadLightLabel.AddThemeColorOverride("font_color", ColorTextSecondary);
		_downloadLightLabel.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(_downloadLightLabel);

		vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

		// Error label
		_downloadErrorLabel = new Label { Text = "", HorizontalAlignment = HorizontalAlignment.Center };
		_downloadErrorLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f));
		_downloadErrorLabel.AddThemeFontSizeOverride("font_size", 13);
		_downloadErrorLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		vbox.AddChild(_downloadErrorLabel);

		// Cancel button
		_cancelDownloadButton = new Button { Text = "Cancel Download" };
		_cancelDownloadButton.Pressed += OnCancelDownloadPressed;
		vbox.AddChild(_cancelDownloadButton);
	}

	private static void StyleProgressBar(ProgressBar bar)
	{
		var bg = new StyleBoxFlat
		{
			BgColor = new Color(0.08f, 0.08f, 0.10f),
			CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
			AntiAliasing = true,
		};
		var fill = new StyleBoxFlat
		{
			BgColor = new Color(0.831f, 0.659f, 0.294f, 0.8f),
			CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
			AntiAliasing = true,
		};
		bar.AddThemeStyleboxOverride("background", bg);
		bar.AddThemeStyleboxOverride("fill", fill);
		bar.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
		bar.AddThemeFontSizeOverride("font_size", 11);
	}

	private void InsertSectionSeparators()
	{
		var layout = GetNode<VBoxContainer>(D.TrimEnd('/'));

		// Remove all existing HSeparators (they got misplaced by reparenting)
		var toRemove = new System.Collections.Generic.List<Node>();
		foreach (var child in layout.GetChildren())
		{
			if (child is HSeparator) toRemove.Add(child);
		}
		foreach (var sep in toRemove)
			sep.QueueFree();

		// Insert new separators after each content VBox (between sections)
		var sectionKeys = new System.Collections.Generic.List<string>(_sections.Keys);
		for (int i = 0; i < sectionKeys.Count - 1; i++)
		{
			var (_, content) = _sections[sectionKeys[i]];
			var sep = new HSeparator();
			var style = new StyleBoxLine
			{
				Color = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.10f),
				Thickness = 1,
				GrowBegin = 0,
				GrowEnd = 0,
			};
			sep.AddThemeStyleboxOverride("separator", style);
			sep.AddThemeConstantOverride("separation", 8);
			layout.AddChild(sep);
			layout.MoveChild(sep, content.GetIndex() + 1);
		}
	}

	private void MakeCollapsibleSection(string headerNodeName, bool startCollapsed)
	{
		var layout = GetNode<VBoxContainer>(D.TrimEnd('/'));
		var headerLabel = GetNode<Label>(D + headerNodeName);
		var headerIndex = headerLabel.GetIndex();
		var headerText = headerLabel.Text;

		// Collect nodes between this header and next header/separator
		// Separators stay OUTSIDE (always visible as dividers)
		var children = new System.Collections.Generic.List<Control>();
		for (int i = headerIndex + 1; i < layout.GetChildCount(); i++)
		{
			var child = layout.GetChild(i);
			if (child is Label lbl && lbl.Name.ToString().EndsWith("Header"))
				break;
			if (child is HSeparator)
				break; // Leave separator in place
			children.Add((Control)child);
		}

		// Create toggle button to replace the label
		var btn = new Button
		{
			Text = startCollapsed ? $"▶  {headerText}" : $"▼  {headerText}",
			Alignment = HorizontalAlignment.Center,
			ClipText = false,
		};
		btn.AddThemeColorOverride("font_color", ColorGold);
		btn.AddThemeColorOverride("font_hover_color", ColorGoldBright);
		btn.AddThemeColorOverride("font_pressed_color", ColorGoldBright);
		btn.AddThemeFontSizeOverride("font_size", 15);

		// Button style with vertical padding
		var btnStyle = new StyleBoxFlat
		{
			BgColor = new Color(0, 0, 0, 0),
			ContentMarginTop = 4, ContentMarginBottom = 4,
		};
		var btnHover = new StyleBoxFlat
		{
			BgColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.05f),
			ContentMarginTop = 4, ContentMarginBottom = 4,
			CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
		};
		btn.AddThemeStyleboxOverride("normal", btnStyle);
		btn.AddThemeStyleboxOverride("hover", btnHover);
		btn.AddThemeStyleboxOverride("pressed", btnStyle);
		btn.AddThemeStyleboxOverride("focus", btnStyle);

		// Replace label with button
		layout.AddChild(btn);
		layout.MoveChild(btn, headerIndex);
		headerLabel.QueueFree();

		// Create content container with breathing room
		var content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.Fill };
		content.AddThemeConstantOverride("separation", 5);
		layout.AddChild(content);
		layout.MoveChild(content, headerIndex + 1);

		// Reparent children into content container
		foreach (var child in children)
		{
			child.GetParent().RemoveChild(child);
			content.AddChild(child);
		}

		content.Visible = !startCollapsed;

		// Toggle handler
		btn.Pressed += () =>
		{
			content.Visible = !content.Visible;
			btn.Text = content.Visible ? $"▼  {headerText}" : $"▶  {headerText}";
			SaveCollapsedState();
		};

		_sections[headerNodeName] = (btn, content);
	}

	private void SaveCollapsedState()
	{
		var collapsed = new System.Collections.Generic.List<string>();
		foreach (var (name, (_, content)) in _sections)
		{
			if (!content.Visible) collapsed.Add(name);
		}
		_config.CollapsedSections = string.Join(",", collapsed);
		if (!_loadingConfig) SaveConfig();
	}
}
