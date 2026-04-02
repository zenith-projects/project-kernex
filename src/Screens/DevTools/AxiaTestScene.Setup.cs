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
