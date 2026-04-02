using System;
using System.Collections.Generic;
using Godot;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Screens.DevTools;

public partial class AxiaTestScene
{
	private void GetDebugControls()
	{
		// Agent selector
		_agentSelector = GetNode<OptionButton>(D + "AgentSelector");

		// Model
		_modelPathInput = GetNode<LineEdit>(D + "ModelPathInput");
		_loadModelButton = GetNode<Button>(D + "ModelButtons/LoadModelButton");
		_unloadModelButton = GetNode<Button>(D + "ModelButtons/UnloadModelButton");
		// CancelDownloadButton is created programmatically in CreateDownloadOverlay
		_llmStatusLabel = GetNode<Label>(D + "LlmStatusLabel");
		_gpuLayersSlider = GetNode<HSlider>(D + "GpuLayers/Slider");
		_gpuLayersValue = GetNode<Label>(D + "GpuLayers/Value");
		_contextSizeSlider = GetNode<HSlider>(D + "ContextSize/Slider");
		_contextSizeValue = GetNode<Label>(D + "ContextSize/Value");

		// AXIA
		_levelSelector = GetNode<OptionButton>(D + "LevelSelector");
		_emotionDisplay = GetNode<LineEdit>(D + "EmotionRow/EmotionValue");
		_axiaPortraitFrame = GetNode<PanelContainer>(D + "AxiaPortraitMargin/AxiaPortraitFrame");
		_axiaPortrait = GetNode<TextureRect>(D + "AxiaPortraitMargin/AxiaPortraitFrame/AxiaPortrait");

		// Create overlay TextureRect for crossfade transitions
		_axiaPortraitNext = new TextureRect
		{
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
			MouseFilter = MouseFilterEnum.Ignore,
			Modulate = new Color(1, 1, 1, 0),
		};
		_axiaPortrait.AddSibling(_axiaPortraitNext);
		_axiaPortraitNext.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		// LlmToggle removed
		_responseDelaySlider = GetNode<HSlider>(D + "ResponseDelay/Slider");
		_responseDelayValue = GetNode<Label>(D + "ResponseDelay/Value");
		_glitchSlider = GetNode<HSlider>(D + "Glitch/Slider");
		_glitchValue = GetNode<Label>(D + "Glitch/Value");

		// Inference
		_temperatureSlider = GetNode<HSlider>(D + "Temperature/Slider");
		_temperatureValue = GetNode<Label>(D + "Temperature/Value");
		_topPSlider = GetNode<HSlider>(D + "TopP/Slider");
		_topPValue = GetNode<Label>(D + "TopP/Value");
		_topKSlider = GetNode<HSlider>(D + "TopK/Slider");
		_topKValue = GetNode<Label>(D + "TopK/Value");
		_minPSlider = GetNode<HSlider>(D + "MinP/Slider");
		_minPValue = GetNode<Label>(D + "MinP/Value");
		_maxTokensSlider = GetNode<HSlider>(D + "MaxTokens/Slider");
		_maxTokensValue = GetNode<Label>(D + "MaxTokens/Value");
		_repeatPenaltySlider = GetNode<HSlider>(D + "RepeatPenalty/Slider");
		_repeatPenaltyValue = GetNode<Label>(D + "RepeatPenalty/Value");
		_freqPenaltySlider = GetNode<HSlider>(D + "FreqPenalty/Slider");
		_freqPenaltyValue = GetNode<Label>(D + "FreqPenalty/Value");
		_presPenaltySlider = GetNode<HSlider>(D + "PresPenalty/Slider");
		_presPenaltyValue = GetNode<Label>(D + "PresPenalty/Value");
		_seedSlider = GetNode<HSlider>(D + "Seed/Slider");
		_seedValue = GetNode<Label>(D + "Seed/Value");
		_mirostatSelector = GetNode<OptionButton>(D + "MirostatSelector");
		_mirostatTauSlider = GetNode<HSlider>(D + "MirostatTau/Slider");
		_mirostatTauValue = GetNode<Label>(D + "MirostatTau/Value");
		_mirostatEtaSlider = GetNode<HSlider>(D + "MirostatEta/Slider");
		_mirostatEtaValue = GetNode<Label>(D + "MirostatEta/Value");

		// Prompt
		_systemPromptEdit = GetNode<TextEdit>(D + "SystemPromptEdit");
		_resetPromptButton = GetNode<Button>(D + "ResetPromptButton");
		_antiPromptsEdit = GetNode<TextEdit>(D + "AntiPromptsEdit");

		// Context
		_hullSlider = GetNode<HSlider>(D + "Hull/Slider");
		_hullValue = GetNode<Label>(D + "Hull/Value");
		_powerSlider = GetNode<HSlider>(D + "Power/Slider");
		_powerValue = GetNode<Label>(D + "Power/Value");
		_dronesActiveSlider = GetNode<HSlider>(D + "DronesActive/Slider");
		_dronesActiveValue = GetNode<Label>(D + "DronesActive/Value");
		_dronesIdleSlider = GetNode<HSlider>(D + "DronesIdle/Slider");
		_dronesIdleValue = GetNode<Label>(D + "DronesIdle/Value");
		_storageSlider = GetNode<HSlider>(D + "Storage/Slider");
		_storageValue = GetNode<Label>(D + "Storage/Value");
		_threatSelector = GetNode<OptionButton>(D + "ThreatSelector");
		_sectorInput = GetNode<LineEdit>(D + "SectorInput");
		_factionInput = GetNode<LineEdit>(D + "FactionInput");

		// Output
		_statusLog = GetNode<RichTextLabel>(D + "StatusLog");
		_clearLogButton = GetNode<Button>(D + "OutputButtons/ClearLogButton");
		_clearChatButton = GetNode<Button>(D + "OutputButtons/ClearChatButton");
		_lastResponseTimeLabel = GetNode<Label>(D + "LastResponseTimeLabel");
		_lastTokenCountLabel = GetNode<Label>(D + "LastTokenCountLabel");

		// Proactive
		_proactiveModelPathInput = GetNode<LineEdit>(D + "ProactiveModelPathInput");
		_proactiveEnabledToggle = GetNode<CheckButton>(D + "ProactiveEnabledToggle");
		_proactiveIntervalSlider = GetNode<HSlider>(D + "ProactiveInterval/Slider");
		_proactiveIntervalValue = GetNode<Label>(D + "ProactiveInterval/Value");
		_proactiveCountLabel = GetNode<Label>(D + "ProactiveCountLabel");
		_proactiveLastCheckLabel = GetNode<Label>(D + "ProactiveLastCheckLabel");
		_forceCheckButton = GetNode<Button>(D + "ForceCheckButton");

		// Memory
		_memoryMessagesLabel = GetNode<Label>(D + "MemoryMessagesLabel");
		_memorySummariesLabel = GetNode<Label>(D + "MemorySummariesLabel");
		_clearMemoryButton = GetNode<Button>(D + "ClearMemoryButton");
	}

	/// <summary>
	/// Apply targeted gold accent color overrides to specific elements.
	/// The theme handles all base styling; this only customizes elements that need
	/// gold highlighting beyond what the theme provides.
	/// </summary>
	private void ApplyAccentColors()
	{
		// Agent selector tooltip
		_agentSelector.TooltipText = "Select which AI agent's parameters to view and edit.";

		// Style nodes BEFORE reparenting (paths still valid)
		string[] secondaryLabels = { "MirostatLabel", "AntiPromptsLabel", "ThreatLabel", "SectorLabel", "FactionLabel", "EmotionRow/EmotionLabel" };
		foreach (var name in secondaryLabels)
			GetNodeOrNull<Label>(D + name)?.AddThemeColorOverride("font_color", ColorTextSecondary);

		// Slider accents BEFORE reparenting
		ApplySliderAccents();

		// Convert section headers into collapsible buttons (reparents nodes)
		string[] headers = { "AxiaHeader", "ModelHeader", "InferenceHeader", "PromptHeader", "ContextHeader", "OutputHeader", "ProactiveHeader", "MemoryHeader" };
		var collapsed = new HashSet<string>(
			(_config.CollapsedSections ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

		foreach (var name in headers)
			MakeCollapsibleSection(name, collapsed.Contains(name));

		// Insert separators between collapsible sections
		InsertSectionSeparators();

		// Status/metric labels: gold (using saved references, not paths)
		_llmStatusLabel.AddThemeColorOverride("font_color", ColorGold);
		_lastResponseTimeLabel.AddThemeColorOverride("font_color", ColorGold);
		_lastTokenCountLabel.AddThemeColorOverride("font_color", ColorGold);
		_proactiveCountLabel.AddThemeColorOverride("font_color", ColorGold);
		_proactiveLastCheckLabel.AddThemeColorOverride("font_color", ColorGold);
		_memoryMessagesLabel.AddThemeColorOverride("font_color", ColorGold);
		_memorySummariesLabel.AddThemeColorOverride("font_color", ColorGold);

		// Tooltips on non-slider controls
		_levelSelector.TooltipText = "AXIA evolution level. Levels 0-2 use scripted responses. Level 3+ activates the LLM.";
		_emotionDisplay.TooltipText = "Current AXIA emotion detected from the last response. Parsed from [EMOTION] tags. Read-only.";
		_emotionDisplay.AddThemeColorOverride("font_color", ColorGold);

		// Portrait frame styling
		var frameStyle = new StyleBoxFlat
		{
			BgColor = new Color(0.04f, 0.04f, 0.055f, 0.9f),
			BorderColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.35f),
			BorderWidthLeft = 2, BorderWidthTop = 2,
			BorderWidthRight = 2, BorderWidthBottom = 2,
			CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
			CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
			ContentMarginLeft = 4, ContentMarginTop = 4,
			ContentMarginRight = 4, ContentMarginBottom = 4,
			AntiAliasing = true,
		};
		_axiaPortraitFrame.AddThemeStyleboxOverride("panel", frameStyle);

		// Load emotion textures
		foreach (var emotion in AxiaResponse.ValidEmotions)
		{
			var path = $"res://src/Assets/Textures/Axia/axia-{emotion.ToLowerInvariant()}-v2.png";
			var tex = GD.Load<Texture2D>(path);
			if (tex != null)
				_emotionTextures[emotion] = tex;
		}

		// Set initial portrait
		SetAxiaEmotion("NEUTRAL");

		// Resize portrait height to maintain 3:4 aspect ratio
		_axiaPortraitFrame.Resized += UpdatePortraitHeight;
		CallDeferred(MethodName.UpdatePortraitHeight);
		// LLM always active for level 3+ (no toggle needed)
		_mirostatSelector.TooltipText = "Mirostat adaptive sampling. Disabled = standard sampling. Mirostat 1/2 = auto-adjusts randomness to maintain target perplexity.";
		// Prompt tooltip
		_modelPathInput.TooltipText = "Absolute path to a .gguf model file (e.g. Qwen2.5-7B-Instruct-Q4_K_M.gguf).";
		_loadModelButton.TooltipText = "Load the model into memory. Takes a few seconds depending on model size and GPU layers.";
		_unloadModelButton.TooltipText = "Unload the model from memory to free RAM/VRAM.";
		_threatSelector.TooltipText = "Simulated threat level. Injected into AXIA's context.";
		_sectorInput.TooltipText = "Current sector coordinates. Injected into AXIA's context.";
		_factionInput.TooltipText = "Player's faction alignment. Injected into AXIA's context.";
	}

	private void ApplySliderAccents()
	{
		(HSlider slider, Label value, string namePath, string tooltip)[] sliderRows =
		{
			(_gpuLayersSlider, _gpuLayersValue, "GpuLayers", "Number of model layers offloaded to GPU. 0 = CPU only. Higher = faster but uses VRAM."),
			(_contextSizeSlider, _contextSizeValue, "ContextSize", "Max tokens the model can see at once. Larger = more memory of conversation, but slower and uses more RAM."),
			(_responseDelaySlider, _responseDelayValue, "ResponseDelay", "Artificial delay before AXIA responds (scripted mode). Simulates processing time for immersion."),
			(_glitchSlider, _glitchValue, "Glitch", "Text corruption intensity for Level 0 (Corrupted). 0 = clean text, 1 = fully garbled."),
			(_temperatureSlider, _temperatureValue, "Temperature", "Randomness of responses. Low (0.1) = deterministic and focused. High (1.5+) = creative but chaotic."),
			(_topPSlider, _topPValue, "TopP", "Nucleus sampling. Only considers tokens within this cumulative probability. Lower = more focused, higher = more diverse."),
			(_topKSlider, _topKValue, "TopK", "Only sample from the top K most likely tokens. 0 = disabled. Lower = more predictable."),
			(_minPSlider, _minPValue, "MinP", "Minimum probability threshold. Tokens below this relative probability are filtered out. Helps cut low-quality tokens."),
			(_maxTokensSlider, _maxTokensValue, "MaxTokens", "Maximum length of AXIA's response in tokens (~4 chars each). Higher = longer answers but slower."),
			(_repeatPenaltySlider, _repeatPenaltyValue, "RepeatPenalty", "Penalizes repeated words/phrases. 1.0 = no penalty. Higher = less repetition."),
			(_freqPenaltySlider, _freqPenaltyValue, "FreqPenalty", "Penalizes tokens based on how often they appeared. Reduces word frequency bias."),
			(_presPenaltySlider, _presPenaltyValue, "PresPenalty", "Penalizes tokens that have appeared at all. Encourages topic diversity."),
			(_seedSlider, _seedValue, "Seed", "Random seed for reproducible outputs. -1 = random each time. Same seed + same input = same output."),
			(_mirostatTauSlider, _mirostatTauValue, "MirostatTau", "Target perplexity for Mirostat sampling. Lower = more focused, higher = more surprising."),
			(_mirostatEtaSlider, _mirostatEtaValue, "MirostatEta", "Mirostat learning rate. How fast it adapts. Higher = more responsive to context changes."),
			(_hullSlider, _hullValue, "Hull", "Station hull integrity percentage. Injected into AXIA's context."),
			(_powerSlider, _powerValue, "Power", "Station power output in kW. Injected into AXIA's context."),
			(_dronesActiveSlider, _dronesActiveValue, "DronesActive", "Number of drones currently deployed. Injected into AXIA's context."),
			(_dronesIdleSlider, _dronesIdleValue, "DronesIdle", "Number of idle drones in hangar. Injected into AXIA's context."),
			(_storageSlider, _storageValue, "Storage", "Station storage capacity usage %. Injected into AXIA's context."),
			(_proactiveIntervalSlider, _proactiveIntervalValue, "ProactiveInterval", "Seconds between proactive checks. Lower = more frequent AXIA messages."),
		};

		var grabberNormal = CreateGrabberTexture(ColorGold);
		var grabberHighlight = CreateGrabberTexture(ColorGoldBright);
		var grabberDisabled = CreateGrabberTexture(ColorTextSecondary);

		foreach (var (slider, valueLabel, namePath, tooltip) in sliderRows)
		{
			valueLabel.AddThemeColorOverride("font_color", ColorGold);
			slider.AddThemeIconOverride("grabber", grabberNormal);
			slider.AddThemeIconOverride("grabber_highlight", grabberHighlight);
			slider.AddThemeIconOverride("grabber_disabled", grabberDisabled);

			// Add tooltip to the name label and slider
			var nameLabel = GetNode<Label>(D + namePath + "/Name");
			nameLabel.TooltipText = tooltip;
			nameLabel.MouseFilter = MouseFilterEnum.Stop;
			slider.TooltipText = tooltip;
		}
	}

	/// <summary>
	/// Configure TextEdits to auto-size height and shrink vertically.
	/// </summary>
	private void SetupTextEdits()
	{
		ConfigureTextEdit(_systemPromptEdit);
		ConfigureTextEdit(_antiPromptsEdit);
	}

	private static void ConfigureTextEdit(TextEdit edit)
	{
		edit.ScrollFitContentHeight = true;
		edit.CustomMinimumSize = new Vector2(0, 0);
		edit.SizeFlagsVertical = SizeFlags.ShrinkBegin;
	}

	/// <summary>
	/// Create a small circular grabber texture for sliders.
	/// </summary>
	private static Texture2D CreateGrabberTexture(Color color)
	{
		var img = Image.CreateEmpty(14, 14, false, Image.Format.Rgba8);
		var center = new Vector2(7, 7);
		for (int x = 0; x < 14; x++)
			for (int y = 0; y < 14; y++)
			{
				var dist = new Vector2(x, y).DistanceTo(center);
				if (dist <= 5.5f)
					img.SetPixel(x, y, color);
				else if (dist <= 6.5f)
					img.SetPixel(x, y, new Color(color.R, color.G, color.B, 0.4f));
				else
					img.SetPixel(x, y, new Color(0, 0, 0, 0));
			}
		return ImageTexture.CreateFromImage(img);
	}

	private void SetupAllControls()
	{
		// Agent selector
		_agentSelector.AddItem($"{_mainAgentConfig.DisplayName} (Main 7B)", 0);
		_agentSelector.AddItem($"{_proactiveAgentConfig.DisplayName} (Proactive 3B)", 1);
		_agentSelector.Selected = 0;
		_selectedAgent = _axia.MainAgent;
		_agentSelector.ItemSelected += OnAgentSelected;

		// Model
		_loadModelButton.Pressed += OnLoadModelPressed;
		_unloadModelButton.Pressed += OnUnloadModelPressed;
		BindSlider(_gpuLayersSlider, _gpuLayersValue);
		BindSlider(_contextSizeSlider, _contextSizeValue);

		// AXIA
		foreach (var level in Enum.GetValues<AxiaLevel>())
			_levelSelector.AddItem($"{(int)level} — {level}", (int)level);
		_levelSelector.ItemSelected += OnLevelChanged;
		// LlmToggle removed
		BindSlider(_responseDelaySlider, _responseDelayValue);
		BindSlider(_glitchSlider, _glitchValue);

		// Inference
		BindSlider(_temperatureSlider, _temperatureValue);
		BindSlider(_topPSlider, _topPValue);
		BindSlider(_topKSlider, _topKValue);
		BindSlider(_minPSlider, _minPValue);
		BindSlider(_maxTokensSlider, _maxTokensValue);
		BindSlider(_repeatPenaltySlider, _repeatPenaltyValue);
		BindSlider(_freqPenaltySlider, _freqPenaltyValue);
		BindSlider(_presPenaltySlider, _presPenaltyValue);
		BindSlider(_seedSlider, _seedValue);
		_mirostatSelector.AddItem("Disabled", 0);
		_mirostatSelector.AddItem("Mirostat 1", 1);
		_mirostatSelector.AddItem("Mirostat 2", 2);
		_mirostatSelector.ItemSelected += _ => SaveConfig();
		BindSlider(_mirostatTauSlider, _mirostatTauValue);
		BindSlider(_mirostatEtaSlider, _mirostatEtaValue);

		// Prompt
		_resetPromptButton.Pressed += () =>
		{
			_systemPromptEdit.Text = AxiaDebugConfig.DefaultPrompt;
			SaveConfig();
		};
		_systemPromptEdit.TextChanged += SaveConfig;
		_antiPromptsEdit.TextChanged += SaveConfig;
		// LlmToggle removed
		_modelPathInput.TextChanged += _ => SaveConfig();

		// Context
		BindSlider(_hullSlider, _hullValue, _ => UpdateGameContext());
		BindSlider(_powerSlider, _powerValue, _ => UpdateGameContext());
		BindSlider(_dronesActiveSlider, _dronesActiveValue, _ => UpdateGameContext());
		BindSlider(_dronesIdleSlider, _dronesIdleValue, _ => UpdateGameContext());
		BindSlider(_storageSlider, _storageValue, _ => UpdateGameContext());
		_threatSelector.AddItem("LOW", 0);
		_threatSelector.AddItem("MEDIUM", 1);
		_threatSelector.AddItem("HIGH", 2);
		_threatSelector.AddItem("CRITICAL", 3);
		_threatSelector.ItemSelected += _ => { UpdateGameContext(); SaveConfig(); };
		_sectorInput.TextChanged += _ => { UpdateGameContext(); SaveConfig(); };
		_factionInput.TextChanged += _ => { UpdateGameContext(); SaveConfig(); };

		// Output
		_clearLogButton.Pressed += () => _statusLog.Clear();
		_clearChatButton.Pressed += () =>
		{
			_axiaPanel.ClearChat();
			_memoryStore.ClearAll();
			UpdateMemoryLabels();
			Log("Chat and memory cleared.");
		};
		_lastResponseTimeLabel.Text = "Response: ---";
		_lastTokenCountLabel.Text = "Chars: ---";

		// Proactive
		_proactiveModelPathInput.TextChanged += _ => SaveConfig();
		_proactiveModelPathInput.TooltipText = "Path to the 3B .gguf model used for proactive decisions and summarization.";
		_proactiveEnabledToggle.Toggled += on =>
		{
			if (_proactiveEngine != null) _proactiveEngine.Enabled = on;
			SaveConfig();
		};
		BindSlider(_proactiveIntervalSlider, _proactiveIntervalValue, v =>
		{
			// Delays are escalating (15s, 20s, 30s) — slider reserved for future tuning
		});
		_forceCheckButton.Pressed += OnForceCheckPressed;

		// Memory
		_clearMemoryButton.Pressed += () =>
		{
			_memoryStore.ClearAll();
			_axiaPanel.ClearChat();
			UpdateMemoryLabels();
			Log("Memory cleared.");
		};
	}

	private void BindSlider(HSlider slider, Label valueLabel, Action<double> extraCallback = null)
	{
		// Initial text from current slider value
		valueLabel.Text = slider.Step >= 1 ? $"{(int)slider.Value}" : $"{slider.Value:F2}";
		slider.ValueChanged += v =>
		{
			valueLabel.Text = slider.Step >= 1 ? $"{(int)v}" : $"{v:F2}";
			extraCallback?.Invoke(v);
			SaveConfig();
		};
	}

	private void SetupAxia()
	{
		_scriptedProvider = new ScriptedDialogueProvider();

		_mainAgentConfig = new AgentConfig
		{
			AgentId = "axia",
			DisplayName = "AXIA",
			MemoryEnabled = true,
		};

		_proactiveAgentConfig = new AgentConfig
		{
			AgentId = "axia-proactive",
			DisplayName = "AXIA (Proactive)",
			MemoryEnabled = false, // shares parent memory
		};

		_axia = new AxiaEngine(_mainAgentConfig, _scriptedProvider);
		_axia.SetLevel(AxiaLevel.Corrupted);

		// Create proactive sub-agent sharing AXIA's memory
		_proactiveAgent = _axia.MainAgent.AddSubAgent(_proactiveAgentConfig, shareMemory: true);

		for (int i = 0; i <= 9; i++)
			LoadLevelData($"res://src/Resources/AI/axia_level_{i}.tres", i);

		UpdateGameContext();
	}

	private void LoadLevelData(string path, int expectedLevel)
	{
		var data = GD.Load<Resources.AI.AxiaLevelData>(path);
		if (data != null)
		{
			_axia.RegisterLevelData(data);
			Log($"Loaded level data: {expectedLevel} ({data.DisplayName})");
		}
		else
		{
			Log($"[WARN] Failed to load level data: {path}");
		}
	}

	private void LoadConfigToUI()
	{
		_loadingConfig = true;

		var mainModelPath = string.IsNullOrWhiteSpace(_config.ModelPath)
			? "models/qwen2.5-7b-instruct-q4_k_m.gguf"
			: _config.ModelPath;
		_modelPathInput.Text = mainModelPath.Contains(":/") || mainModelPath.Contains(":\\")
			? mainModelPath
			: ProjectSettings.GlobalizePath($"res://{mainModelPath}");
		_gpuLayersSlider.Value = _config.GpuLayers;
		_contextSizeSlider.Value = _config.ContextSize;
		// LlmToggle removed
		_levelSelector.Selected = _config.AxiaLevel;
		_responseDelaySlider.Value = _config.ResponseDelay;
		_glitchSlider.Value = _config.GlitchIntensity;
		_temperatureSlider.Value = _config.Temperature;
		_topPSlider.Value = _config.TopP;
		_topKSlider.Value = _config.TopK;
		_minPSlider.Value = _config.MinP;
		_maxTokensSlider.Value = _config.MaxTokens;
		_repeatPenaltySlider.Value = _config.RepeatPenalty;
		_freqPenaltySlider.Value = _config.FrequencyPenalty;
		_presPenaltySlider.Value = _config.PresencePenalty;
		_seedSlider.Value = _config.Seed;
		_mirostatSelector.Selected = _config.MirostatMode;
		_mirostatTauSlider.Value = _config.MirostatTau;
		_mirostatEtaSlider.Value = _config.MirostatEta;
		_systemPromptEdit.Text = string.IsNullOrWhiteSpace(_config.SystemPrompt)
			? AxiaDebugConfig.DefaultPrompt
			: _config.SystemPrompt;
		_antiPromptsEdit.Text = string.IsNullOrWhiteSpace(_config.AntiPrompts)
			? "User:\nOperator:"
			: _config.AntiPrompts;
		_hullSlider.Value = _config.Hull;
		_powerSlider.Value = _config.Power;
		_dronesActiveSlider.Value = _config.DronesActive;
		_dronesIdleSlider.Value = _config.DronesIdle;
		_storageSlider.Value = _config.Storage;
		_threatSelector.Selected = _config.ThreatLevel;
		_sectorInput.Text = _config.Sector;
		_factionInput.Text = _config.Faction;

		// Proactive model path
		var proPath = string.IsNullOrWhiteSpace(_config.ProactiveModelPath)
			? "models/qwen2.5-3b-instruct-q4_k_m.gguf"
			: _config.ProactiveModelPath;
		_proactiveModelPathInput.Text = proPath.Contains(":/") || proPath.Contains(":\\")
			? proPath
			: ProjectSettings.GlobalizePath($"res://{proPath}");

		_loadingConfig = false;
	}

	private void SaveConfig()
	{
		if (_loadingConfig) return;
		_config.ModelPath = _modelPathInput.Text;
		_config.GpuLayers = (int)_gpuLayersSlider.Value;
		_config.ContextSize = (int)_contextSizeSlider.Value;
		_config.LlmEnabled = true;
		_config.AxiaLevel = _levelSelector.Selected;
		_config.ResponseDelay = (float)_responseDelaySlider.Value;
		_config.GlitchIntensity = (float)_glitchSlider.Value;
		_config.Temperature = (float)_temperatureSlider.Value;
		_config.TopP = (float)_topPSlider.Value;
		_config.TopK = (int)_topKSlider.Value;
		_config.MinP = (float)_minPSlider.Value;
		_config.MaxTokens = (int)_maxTokensSlider.Value;
		_config.RepeatPenalty = (float)_repeatPenaltySlider.Value;
		_config.FrequencyPenalty = (float)_freqPenaltySlider.Value;
		_config.PresencePenalty = (float)_presPenaltySlider.Value;
		_config.Seed = (int)_seedSlider.Value;
		_config.MirostatMode = _mirostatSelector.Selected;
		_config.MirostatTau = (float)_mirostatTauSlider.Value;
		_config.MirostatEta = (float)_mirostatEtaSlider.Value;
		_config.SystemPrompt = _systemPromptEdit.Text;
		_config.AntiPrompts = _antiPromptsEdit.Text;
		_config.Hull = (int)_hullSlider.Value;
		_config.Power = (int)_powerSlider.Value;
		_config.DronesActive = (int)_dronesActiveSlider.Value;
		_config.DronesIdle = (int)_dronesIdleSlider.Value;
		_config.Storage = (int)_storageSlider.Value;
		_config.ThreatLevel = _threatSelector.Selected;
		_config.Sector = _sectorInput.Text;
		_config.Faction = _factionInput.Text;
		_config.ProactiveModelPath = _proactiveModelPathInput.Text;
		_config.Save();
	}

}
