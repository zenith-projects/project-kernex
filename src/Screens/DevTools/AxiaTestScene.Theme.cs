using System;
using System.Collections.Generic;
using Godot;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Screens.DevTools;

public partial class AxiaTestScene
{
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
}
