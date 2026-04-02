using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Screens.DevTools;

public partial class AxiaTestScene
{
	private void OnAgentSelected(long index)
	{
		_selectedAgent = index == 0 ? _axia.MainAgent : _proactiveAgent;
		var isMainAxia = index == 0;

		// Toggle AXIA-specific section visibility
		if (_sections.TryGetValue("AxiaHeader", out var axiaSection))
			axiaSection.content.Visible = isMainAxia;
		if (_sections.TryGetValue("ProactiveHeader", out var proactiveSection))
			proactiveSection.content.Visible = isMainAxia;

		// Load selected agent's config into inference sliders
		var cfg = _selectedAgent.Config;
		_loadingConfig = true;
		_temperatureSlider.Value = cfg.Temperature;
		_topPSlider.Value = cfg.TopP;
		_topKSlider.Value = cfg.TopK;
		_minPSlider.Value = cfg.MinP;
		_maxTokensSlider.Value = cfg.MaxTokens;
		_repeatPenaltySlider.Value = cfg.RepeatPenalty;
		_freqPenaltySlider.Value = cfg.FrequencyPenalty;
		_presPenaltySlider.Value = cfg.PresencePenalty;
		_seedSlider.Value = cfg.Seed;
		_mirostatSelector.Selected = cfg.MirostatMode;
		_mirostatTauSlider.Value = cfg.MirostatTau;
		_mirostatEtaSlider.Value = cfg.MirostatEta;

		if (!string.IsNullOrWhiteSpace(cfg.SystemPrompt))
			_systemPromptEdit.Text = cfg.SystemPrompt;

		_modelPathInput.Text = cfg.ModelPath;
		_loadingConfig = false;

		Log($"Agent selected: {cfg.DisplayName}");
	}

	private void OnLevelChanged(long index)
	{
		var level = (AxiaLevel)(int)index;
		_axia.SetLevel(level);
		_axiaPanel.SetLevel(level);

		var data = _axia.GetCurrentLevelData();
		if (data != null)
		{
			_responseDelaySlider.Value = data.ResponseDelay;
			_glitchSlider.Value = data.GlitchIntensity;
		}

		Log($"Level -> {(int)level} ({level})");
		SaveConfig();
	}

	private async void OnMessageSubmitted(string message)
	{
		_axiaPanel.AppendUserMessage(message);
		_axiaPanel.SetThinking(true);
		_proactiveEngine?.NotifyUserMessage();
		_config.ProactiveGaveUp = false;
		_config.ProactiveAttempt = 0;

		var startTime = Time.GetTicksMsec();

		try
		{
			// Sync debug controls to engine before each request
			UpdateGameContext();
			ApplyInferenceParams();

			var response = await _axia.GetResponseAsync(message);
			var elapsed = Time.GetTicksMsec() - startTime;

			_axiaPanel.AppendAxiaMessage(response.Text);
			SetAxiaEmotion(response.Emotion);
			_lastResponseTimeLabel.Text = $"Response: {elapsed}ms";
			_lastTokenCountLabel.Text = $"Chars: {response.Text.Length}";
			Log($"[{elapsed}ms] [{_axia.CurrentLevel}] [{response.Emotion}] {Truncate(response.Text, 70)}");

			UpdateMemoryLabels();

			// Trigger background summarization if needed
			if (_memoryStore.UnsummarizedCount > 20)
				_ = SummarizeOldMessagesAsync();
		}
		catch (Exception ex)
		{
			_axiaPanel.AppendAxiaMessage($"[ERROR] {ex.Message}");
			Log($"[ERROR] {ex.Message}");
		}
		finally
		{
			_axiaPanel.SetThinking(false);
		}
	}

	private void ApplyInferenceParams()
	{
		var cfg = _selectedAgent?.Config ?? _mainAgentConfig;
		cfg.Temperature = (float)_temperatureSlider.Value;
		cfg.TopP = (float)_topPSlider.Value;
		cfg.TopK = (int)_topKSlider.Value;
		cfg.MinP = (float)_minPSlider.Value;
		cfg.MaxTokens = (int)_maxTokensSlider.Value;
		cfg.RepeatPenalty = (float)_repeatPenaltySlider.Value;
		cfg.FrequencyPenalty = (float)_freqPenaltySlider.Value;
		cfg.PresencePenalty = (float)_presPenaltySlider.Value;
		cfg.Seed = (int)_seedSlider.Value;
		cfg.MirostatMode = _mirostatSelector.Selected;
		cfg.MirostatTau = (float)_mirostatTauSlider.Value;
		cfg.MirostatEta = (float)_mirostatEtaSlider.Value;
		cfg.AntiPrompts = _antiPromptsEdit.Text;
	}

	private async void OnLoadModelPressed()
	{
		var path = _modelPathInput.Text.Trim();
		if (string.IsNullOrEmpty(path))
		{
			Log("[ERROR] Enter a .gguf model path");
			return;
		}

		_loadModelButton.Disabled = true;
		_llmStatusLabel.Text = "LLM: Loading...";
		Log($"Loading: {path} (GPU: {(int)_gpuLayersSlider.Value}, ctx: {(int)_contextSizeSlider.Value})");

		_mainAgentConfig.ModelPath = path;
		_mainAgentConfig.GpuLayers = (int)_gpuLayersSlider.Value;
		_mainAgentConfig.ContextSize = (uint)_contextSizeSlider.Value;

		var success = await _axia.MainAgent.LoadModelAsync();

		if (success)
		{
			_llmStatusLabel.Text = "LLM: Loaded";
			Log("Model loaded!");
		}
		else
		{
			_llmStatusLabel.Text = "LLM: Failed";
			Log("[ERROR] Failed to load model");
		}

		_loadModelButton.Disabled = false;
	}

	private void OnUnloadModelPressed()
	{
		_axia.MainAgent.UnloadModel();
		_llmStatusLabel.Text = "LLM: Not loaded";
		Log("Model unloaded");
	}

	private void OnCancelDownloadPressed()
	{
		_downloadCts?.Cancel();
		Log("Cancelling download...");
	}

	private async void AutoLoadModel()
	{
		var needsMainDl = !ModelDownloader.ModelExists(ModelDownloader.MainModel);
		var needsLightDl = !ModelDownloader.ModelExists(ModelDownloader.LightModel);

		if (needsMainDl || needsLightDl)
		{
			_downloading = true;
			_downloadOverlay.Visible = true;
			_downloadCts = new System.Threading.CancellationTokenSource();

			if (!needsMainDl) { _downloadMainLabel.Text = "Already downloaded"; _downloadMainBar.Value = 100; }
			if (!needsLightDl) { _downloadLightLabel.Text = "Already downloaded"; _downloadLightBar.Value = 100; }
		}

		// Download and load main model (7B)
		await EnsureAndLoadAgent(
			ModelDownloader.MainModel,
			_modelPathInput,
			_axia.MainAgent,
			_mainAgentConfig,
			(int)_gpuLayersSlider.Value,
			(uint)_contextSizeSlider.Value,
			p => { _downloadMainPct = p; _downloadMainStatus = FormatProgress(ModelDownloader.MainModel, p); },
			() => Log("7B model loaded!")
		);

		// Download and load light model (3B)
		await EnsureAndLoadAgent(
			ModelDownloader.LightModel,
			_proactiveModelPathInput,
			_proactiveAgent,
			_proactiveAgentConfig,
			0, 1024,
			p => { _downloadLightPct = p; _downloadLightStatus = FormatProgress(ModelDownloader.LightModel, p); },
			() =>
			{
				_proactiveEngine.Enabled = _config.ProactiveEnabled;
				_proactiveEngine.GaveUp = _config.ProactiveGaveUp;
				_proactiveEngine.Attempt = _config.ProactiveAttempt;
				Log("3B model loaded — proactive engine active!");
				if (_memoryStore.MessageCount == 0)
					_proactiveEngine.ActivateForIntroduction();
			}
		);

		// Hide overlay
		if (_downloading)
		{
			_downloading = false;
			_downloadOverlay.Visible = false;
		}
	}

	private async Task EnsureAndLoadAgent(
		ModelDownloader.ModelInfo modelInfo,
		LineEdit pathInput,
		AgentRunner agent,
		AgentConfig agentConfig,
		int gpuLayers, uint contextSize,
		Action<float> onProgress,
		Action onLoaded)
	{
		var path = ModelDownloader.GetModelPath(modelInfo);
		pathInput.Text = path;

		// Download if missing
		if (!ModelDownloader.ModelExists(modelInfo))
		{
			Log($"Downloading {modelInfo.FileName}...");

			var success = await ModelDownloader.DownloadModelAsync(modelInfo, onProgress, _downloadCts?.Token ?? default);

			if (!success)
			{
				var cancelled = _downloadCts?.IsCancellationRequested == true;
				_downloadError = cancelled
					? $"Download cancelled: {modelInfo.FileName}"
					: $"Failed to download {modelInfo.FileName}. Check disk space and internet connection.";
				Log(cancelled ? $"Download cancelled: {modelInfo.FileName}" : $"[ERROR] Failed to download {modelInfo.FileName}");
				return;
			}

			onProgress(1f);
			Log($"Downloaded {modelInfo.FileName}");
		}

		// Configure and load via AgentRunner + ModelPool
		_llmStatusLabel.Text = $"Loading {modelInfo.FileName}...";
		Log($"Loading: {modelInfo.FileName}");

		agentConfig.ModelPath = path;
		agentConfig.GpuLayers = gpuLayers;
		agentConfig.ContextSize = contextSize;

		var loaded = await agent.LoadModelAsync();
		if (loaded)
		{
			_llmStatusLabel.Text = "LLM: Loaded";
			onLoaded();
		}
		else
		{
			_downloadError = $"Failed to load {modelInfo.FileName}. File may be corrupted.";
			Log($"[ERROR] Failed to load {modelInfo.FileName}");
		}
	}

	private static string FormatProgress(ModelDownloader.ModelInfo model, float progress)
	{
		var pct = (int)(progress * 100);
		var dlMb = (int)(model.ExpectedSize * progress / 1_000_000);
		var totalMb = (int)(model.ExpectedSize / 1_000_000);
		return $"{pct}%  —  {dlMb} / {totalMb} MB";
	}

	private void UpdateGameContext()
	{
		_axia.UpdateGameContext(GetGameContextDict());
	}

	private Dictionary<string, string> GetGameContextDict()
	{
		var threat = _threatSelector.Selected switch { 0 => "LOW", 1 => "MEDIUM", 2 => "HIGH", _ => "CRITICAL" };
		return new()
		{
			["hull"] = $"{(int)_hullSlider.Value}%",
			["power"] = $"{(int)_powerSlider.Value} kW",
			["drones"] = $"{(int)_dronesActiveSlider.Value} active, {(int)_dronesIdleSlider.Value} idle",
			["modules"] = "3 online",
			["storage"] = $"{(int)_storageSlider.Value}%",
			["threat"] = threat,
			["sector"] = _sectorInput.Text,
			["faction"] = _factionInput.Text,
		};
	}

	private void RestoreChatFromMemory()
	{
		var recent = _memoryStore.GetRecent(30);
		foreach (var entry in recent)
		{
			if (entry.Role == "user")
				_axiaPanel.AppendUserMessage(entry.Content);
			else
				_axiaPanel.AppendAxiaMessage(entry.Content);
		}
	}

	private void UpdateMemoryLabels()
	{
		_memoryMessagesLabel.Text = $"Messages: {_memoryStore.MessageCount}";
		_memorySummariesLabel.Text = $"Summaries: {_memoryStore.SummaryCount}";
	}

	private void OnProactiveMessage(string rawMessage)
	{
		CallDeferred(MethodName.HandleProactiveMessage, rawMessage);
	}

	private void HandleProactiveMessage(string rawMessage)
	{
		var parsed = AxiaResponse.Parse(rawMessage);
		_axiaPanel.AppendAxiaMessage(parsed.Text);
		SetAxiaEmotion(parsed.Emotion);
		_memoryStore.SaveMessage("assistant", parsed.Text, parsed.Emotion);
		UpdateMemoryLabels();
		FlashPortraitBorder();
		Log($"[PROACTIVE] [{parsed.Emotion}] {Truncate(parsed.Text, 70)}");

		// Persist proactive state
		_config.ProactiveAttempt = _proactiveEngine.Attempt;
		_config.ProactiveGaveUp = _proactiveEngine.GaveUp;
		SaveConfig();
	}

	private void FlashPortraitBorder()
	{
		var tween = CreateTween();
		tween.TweenProperty(_axiaPortraitFrame, "modulate", new Color(1.3f, 1.1f, 0.7f), 0.25f);
		tween.TweenProperty(_axiaPortraitFrame, "modulate", Colors.White, 0.25f);
		tween.TweenProperty(_axiaPortraitFrame, "modulate", new Color(1.3f, 1.1f, 0.7f), 0.25f);
		tween.TweenProperty(_axiaPortraitFrame, "modulate", Colors.White, 0.25f);
	}

	private async void OnForceCheckPressed()
	{
		if (_proactiveAgent?.Provider is not { IsModelLoaded: true })
		{
			Log("3B model not loaded — cannot force proactive check");
			return;
		}
		Log("Forcing proactive check...");
		await _proactiveEngine.ForceCheck();
	}

	private async Task SummarizeOldMessagesAsync()
	{
		if (_proactiveAgent?.Provider is not { IsModelLoaded: true }) return;

		var batch = _memoryStore.GetUnsummarizedBatch(20);
		if (batch.Count < 20) return;

		var conversation = new System.Text.StringBuilder();
		foreach (var m in batch)
		{
			var role = m.Role == "user" ? "Operator" : "AXIA";
			conversation.AppendLine($"[{role}] {m.Content}");
		}

		var prompt = "Summarize this conversation in 3-5 short bullet points. " +
					 "Focus on: what the operator asked, what AXIA answered, important facts or decisions. " +
					 "Be concise. Output only the bullet points, nothing else.";

		var summary = await _proactiveAgent.GetResponseAsync(prompt, conversation.ToString());
		if (!string.IsNullOrWhiteSpace(summary))
		{
			_memoryStore.AddSummaryAndMark(summary, batch.Count);
			UpdateMemoryLabels();
			Log($"Memory summarized: {_memoryStore.SummaryCount} summaries total");
		}
	}

	private void UpdatePortraitHeight()
	{
		var width = _axiaPortraitFrame.Size.X;
		if (width > 0)
			_axiaPortrait.CustomMinimumSize = new Vector2(0, width * 4f / 3f);
	}

	private void SetAxiaEmotion(string emotion)
	{
		_emotionDisplay.Text = emotion;
		_config.CurrentEmotion = emotion;
		if (!_loadingConfig) SaveConfig();

		Texture2D newTex = null;
		if (!_emotionTextures.TryGetValue(emotion, out newTex))
			_emotionTextures.TryGetValue("NEUTRAL", out newTex);
		if (newTex == null || newTex == _axiaPortrait.Texture) return;

		// Crossfade: put new texture on overlay, fade in, then swap
		_emotionTween?.Kill();
		_axiaPortraitNext.Texture = newTex;
		_axiaPortraitNext.Modulate = new Color(1, 1, 1, 0);

		_emotionTween = CreateTween();
		_emotionTween.TweenProperty(_axiaPortraitNext, "modulate:a", 1.0f, 0.25f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
		_emotionTween.TweenCallback(Callable.From(() =>
		{
			_axiaPortrait.Texture = newTex;
			_axiaPortraitNext.Modulate = new Color(1, 1, 1, 0);
		}));
	}

}
