using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using ProjectKernex.Core.Enums;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Screens.DevTools;

public partial class KiraTestScene
{
	private void OnLevelChanged(long index)
	{
		var level = (KiraLevel)(int)index;
		_kira.SetLevel(level);
		_kiraPanel.SetLevel(level);

		var data = _kira.GetCurrentLevelData();
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
		_kiraPanel.AppendUserMessage(message);
		_kiraPanel.SetThinking(true);
		_proactiveEngine?.NotifyUserMessage();
		_config.ProactiveGaveUp = false;
		_config.ProactiveAttempt = 0;

		var startTime = Time.GetTicksMsec();

		try
		{
			string response;

			if (_llmProvider is { IsModelLoaded: true } && (int)_kira.CurrentLevel >= 3)
			{
				var ctx = GetGameContextDict();
				var contextBlock = BuildContextBlock(ctx);
				var memoryBlock = _memoryStore.BuildMemoryBlock(message);
				var systemPrompt = _systemPromptEdit.Text + "\n\n" + memoryBlock + contextBlock;

				ApplyInferenceParams();

				var history = _memoryStore.GetRecentAsTuples(15);
				response = await _llmProvider.GenerateWithHistoryAsync(systemPrompt, history, message);
			}
			else
			{
				var delay = (float)_responseDelaySlider.Value;
				var glitch = (float)_glitchSlider.Value;

				if (delay > 0)
					await Task.Delay(TimeSpan.FromSeconds(delay));

				response = await _scriptedProvider.GetResponseAsync(message, _kira.CurrentLevel, GetGameContextDict());

				if (_kira.CurrentLevel == KiraLevel.Corrupted && glitch > 0)
					response = GlitchTextGenerator.Glitch(response, glitch);
			}

			var elapsed = Time.GetTicksMsec() - startTime;

			var parsed = KiraResponse.Parse(response);
			_kiraPanel.AppendKiraMessage(parsed.Text);
			SetKiraEmotion(parsed.Emotion);
			_lastResponseTimeLabel.Text = $"Response: {elapsed}ms";
			_lastTokenCountLabel.Text = $"Chars: {parsed.Text.Length}";
			Log($"[{elapsed}ms] [{_kira.CurrentLevel}] [{parsed.Emotion}] {Truncate(parsed.Text, 70)}");

			// Save to persistent memory
			_memoryStore.SaveMessage("user", message);
			_memoryStore.SaveMessage("assistant", parsed.Text, parsed.Emotion);
			UpdateMemoryLabels();

			// Trigger background summarization if needed
			if (_memoryStore.UnsummarizedCount > 20)
				_ = SummarizeOldMessagesAsync();
		}
		catch (Exception ex)
		{
			_kiraPanel.AppendKiraMessage($"[ERROR] {ex.Message}");
			Log($"[ERROR] {ex.Message}");
		}
		finally
		{
			_kiraPanel.SetThinking(false);
		}
	}

	private void ApplyInferenceParams()
	{
		_llmProvider.Temperature = (float)_temperatureSlider.Value;
		_llmProvider.TopP = (float)_topPSlider.Value;
		_llmProvider.TopK = (int)_topKSlider.Value;
		_llmProvider.MinP = (float)_minPSlider.Value;
		_llmProvider.MaxTokens = (int)_maxTokensSlider.Value;
		_llmProvider.RepeatPenalty = (float)_repeatPenaltySlider.Value;
		_llmProvider.FrequencyPenalty = (float)_freqPenaltySlider.Value;
		_llmProvider.PresencePenalty = (float)_presPenaltySlider.Value;
		_llmProvider.Seed = (int)_seedSlider.Value;
		_llmProvider.MirostatMode = _mirostatSelector.Selected;
		_llmProvider.MirostatTau = (float)_mirostatTauSlider.Value;
		_llmProvider.MirostatEta = (float)_mirostatEtaSlider.Value;

		var antiPrompts = _antiPromptsEdit.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		_llmProvider.AntiPrompts = antiPrompts;
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

		_llmProvider ??= new LlamaSharpProvider();
		_llmProvider.GpuLayerCount = (int)_gpuLayersSlider.Value;
		_llmProvider.ContextSize = (uint)_contextSizeSlider.Value;

		var success = await _llmProvider.LoadModelAsync(path);

		if (success)
		{
			_llmStatusLabel.Text = "LLM: Loaded";
			_kira.SetLlmProvider(_llmProvider);

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
		_llmProvider?.UnloadModel();
		// LlmToggle removed
		_kira.SetLlmProvider(null);
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

		// Download main model
		await EnsureAndLoadModel(
			ModelDownloader.MainModel,
			_modelPathInput,
			provider =>
			{
				_llmProvider = provider;
				_kira.SetLlmProvider(provider);
			},
			(int)_gpuLayersSlider.Value,
			(uint)_contextSizeSlider.Value,
			p => { _downloadMainPct = p; _downloadMainStatus = FormatProgress(ModelDownloader.MainModel, p); }
		);

		// Download light model
		await EnsureAndLoadModel(
			ModelDownloader.LightModel,
			_proactiveModelPathInput,
			provider =>
			{
				_lightLlmProvider = provider;
				_proactiveEngine.SetLightModel(provider);
				_proactiveEngine.Enabled = _config.ProactiveEnabled;
				_proactiveEngine.GaveUp = _config.ProactiveGaveUp;
				_proactiveEngine.Attempt = _config.ProactiveAttempt;
				Log("3B model loaded — proactive engine active!");
				if (_memoryStore.MessageCount == 0)
					_proactiveEngine.ActivateForIntroduction();
			},
			0, 1024,
			p => { _downloadLightPct = p; _downloadLightStatus = FormatProgress(ModelDownloader.LightModel, p); }
		);

		// Hide overlay
		if (_downloading)
		{
			_downloading = false;
			_downloadOverlay.Visible = false;
		}
	}

	private async Task EnsureAndLoadModel(
		ModelDownloader.ModelInfo modelInfo,
		LineEdit pathInput,
		Action<LlamaSharpProvider> onLoaded,
		int gpuLayers, uint contextSize,
		Action<float> onProgress)
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

		// Load model
		_llmStatusLabel.Text = $"Loading {modelInfo.FileName}...";
		Log($"Loading: {modelInfo.FileName}");

		var provider = new LlamaSharpProvider
		{
			GpuLayerCount = gpuLayers,
			ContextSize = contextSize
		};

		var loaded = await provider.LoadModelAsync(path);
		if (loaded)
		{
			_llmStatusLabel.Text = "LLM: Loaded";
			onLoaded(provider);
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
		foreach (var (key, value) in GetGameContextDict())
			_kira.UpdateGameContext(key, value);
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

	private static string BuildContextBlock(Dictionary<string, string> ctx)
	{
		var sb = new System.Text.StringBuilder("Current station state: ");
		var parts = new System.Collections.Generic.List<string>();
		foreach (var (key, value) in ctx)
			parts.Add($"{key} {value}");
		sb.Append(string.Join(", ", parts));
		sb.Append('.');
		return sb.ToString();
	}

	private void RestoreChatFromMemory()
	{
		var recent = _memoryStore.GetRecent(30);
		foreach (var entry in recent)
		{
			if (entry.Role == "user")
				_kiraPanel.AppendUserMessage(entry.Content);
			else
				_kiraPanel.AppendKiraMessage(entry.Content);
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
		var parsed = KiraResponse.Parse(rawMessage);
		_kiraPanel.AppendKiraMessage(parsed.Text);
		SetKiraEmotion(parsed.Emotion);
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
		tween.TweenProperty(_kiraPortraitFrame, "modulate", new Color(1.3f, 1.1f, 0.7f), 0.25f);
		tween.TweenProperty(_kiraPortraitFrame, "modulate", Colors.White, 0.25f);
		tween.TweenProperty(_kiraPortraitFrame, "modulate", new Color(1.3f, 1.1f, 0.7f), 0.25f);
		tween.TweenProperty(_kiraPortraitFrame, "modulate", Colors.White, 0.25f);
	}

	private async void OnForceCheckPressed()
	{
		if (_proactiveEngine == null || _lightLlmProvider is not { IsModelLoaded: true })
		{
			Log("3B model not loaded — cannot force proactive check");
			return;
		}
		Log("Forcing proactive check...");
		await _proactiveEngine.ForceCheck();
	}

	private async Task SummarizeOldMessagesAsync()
	{
		if (_lightLlmProvider is not { IsModelLoaded: true }) return;

		var batch = _memoryStore.GetUnsummarizedBatch(20);
		if (batch.Count < 20) return;

		var conversation = new System.Text.StringBuilder();
		foreach (var m in batch)
		{
			var role = m.Role == "user" ? "Operator" : "KIRA";
			conversation.AppendLine($"[{role}] {m.Content}");
		}

		var prompt = "Summarize this conversation in 3-5 short bullet points. " +
					 "Focus on: what the operator asked, what KIRA answered, important facts or decisions. " +
					 "Be concise. Output only the bullet points, nothing else.";

		var summary = await _lightLlmProvider.GenerateAsync(prompt, conversation.ToString());
		if (!string.IsNullOrWhiteSpace(summary))
		{
			_memoryStore.AddSummaryAndMark(summary, batch.Count);
			UpdateMemoryLabels();
			Log($"Memory summarized: {_memoryStore.SummaryCount} summaries total");
		}
	}

	private void UpdatePortraitHeight()
	{
		var width = _kiraPortraitFrame.Size.X;
		if (width > 0)
			_kiraPortrait.CustomMinimumSize = new Vector2(0, width * 4f / 3f);
	}

	private void SetKiraEmotion(string emotion)
	{
		_emotionDisplay.Text = emotion;
		_config.CurrentEmotion = emotion;
		if (!_loadingConfig) SaveConfig();

		Texture2D newTex = null;
		if (!_emotionTextures.TryGetValue(emotion, out newTex))
			_emotionTextures.TryGetValue("NEUTRAL", out newTex);
		if (newTex == null || newTex == _kiraPortrait.Texture) return;

		// Crossfade: put new texture on overlay, fade in, then swap
		_emotionTween?.Kill();
		_kiraPortraitNext.Texture = newTex;
		_kiraPortraitNext.Modulate = new Color(1, 1, 1, 0);

		_emotionTween = CreateTween();
		_emotionTween.TweenProperty(_kiraPortraitNext, "modulate:a", 1.0f, 0.25f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
		_emotionTween.TweenCallback(Callable.From(() =>
		{
			_kiraPortrait.Texture = newTex;
			_kiraPortraitNext.Modulate = new Color(1, 1, 1, 0);
		}));
	}

}
