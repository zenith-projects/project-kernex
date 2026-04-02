using Godot;

namespace ProjectKernex.Components.UI;

[GlobalClass, Tool]
public partial class KernexButton : PanelContainer
{
	[Signal] public delegate void PressedEventHandler();

	private static readonly Color ColorAccent = new(0.7f, 0.75f, 0.85f);
	private static readonly Color ColorAccentBright = new(1.0f, 1.0f, 1.0f);
	private static readonly Color ColorTextIdle = new(0.65f, 0.65f, 0.68f);
	private static readonly Color ColorTextHover = Colors.White;
	private static readonly Color ColorTextPressed = new(0.7f, 0.75f, 0.85f);

	[Export] public string Text { get; set; } = "BUTTON";
	[Export] public bool Disabled { get; set; }
	[Export] public int FontSize { get; set; } = 21;

	private Label _label;
	private StyleBoxEmpty _style;
	private Tween _hoverTween;
	private bool _hovering;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		CustomMinimumSize = new Vector2(220, 0);

		// Transparent background — ghost button
		_style = new StyleBoxEmpty
		{
			ContentMarginLeft = 24, ContentMarginTop = 10,
			ContentMarginRight = 24, ContentMarginBottom = 10,
		};
		AddThemeStyleboxOverride("panel", _style);

		_label = new Label
		{
			Text = Text.ToUpperInvariant(),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			SizeFlagsHorizontal = SizeFlags.Fill,
		};
		_label.AddThemeColorOverride("font_color", ColorTextIdle);
		_label.AddThemeFontSizeOverride("font_size", FontSize);

		var baseFont = ResourceLoader.Load<Font>("res://src/Assets/Fonts/moonhouse.ttf");
		if (baseFont != null)
		{
			var variation = new FontVariation();
			variation.BaseFont = baseFont;
			variation.VariationEmbolden = 0.4f;
			_label.AddThemeFontOverride("font", variation);
		}

		AddChild(_label);

		Resized += () => PivotOffset = Size / 2;
		CallDeferred(MethodName.UpdatePivot);

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		GuiInput += OnGuiInput;
	}

	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint()) return;
		if (_label != null && _label.Text != Text.ToUpperInvariant())
			_label.Text = Text.ToUpperInvariant();
	}

	private void OnMouseEntered()
	{
		if (Disabled) return;
		_hovering = true;
		AnimateHover(ColorTextHover, 1.12f);
	}

	private void OnMouseExited()
	{
		_hovering = false;
		AnimateHover(ColorTextIdle, 1.0f);
	}

	private void OnGuiInput(InputEvent @event)
	{
		if (Disabled) return;

		if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
		{
			_label.AddThemeColorOverride("font_color", ColorTextPressed);
			EmitSignal(SignalName.Pressed);
		}
		else if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left })
		{
			_label.AddThemeColorOverride("font_color", _hovering ? ColorTextHover : ColorTextIdle);
		}
	}

	private void AnimateHover(Color targetColor, float targetScale)
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenMethod(
			Callable.From<Color>(c => _label.AddThemeColorOverride("font_color", c)),
			_label.GetThemeColor("font_color"), targetColor, 0.2f)
			.SetEase(Tween.EaseType.Out);
		_hoverTween.TweenProperty(this, "scale", Vector2.One * targetScale, 0.15f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
	}

	private void UpdatePivot() => PivotOffset = Size / 2;

	public void SetText(string text)
	{
		Text = text;
		if (_label != null) _label.Text = text.ToUpperInvariant();
	}

	public void SetDisabled(bool disabled)
	{
		Disabled = disabled;
		if (_label != null)
			_label.Modulate = disabled ? new Color(1, 1, 1, 0.25f) : Colors.White;
	}
}
