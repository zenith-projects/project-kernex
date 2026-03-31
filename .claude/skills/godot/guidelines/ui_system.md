# UI System Reference

Complete reference for Godot 4's Control node system, themes, and UI patterns using C#.

---

## Control Node Fundamentals

### Base Properties

Every Control node has:

| Property | Description |
|----------|-------------|
| `AnchorLeft/Top/Right/Bottom` | Positioning relative to parent edges (0.0 to 1.0) |
| `OffsetLeft/Top/Right/Bottom` | Pixel offset from anchor points |
| `SizeFlagsHorizontal/Vertical` | How node grows/shrinks in containers |
| `CustomMinimumSize` | Minimum size constraint |
| `MouseFilter` | Input handling: `Stop`, `Pass`, `Ignore` |
| `FocusMode` | Keyboard/gamepad focus: `None`, `Click`, `All` |

### Anchor Presets

```csharp
// Full Rect (fill parent)
// anchor_left=0, anchor_top=0, anchor_right=1, anchor_bottom=1

// Top-Left (default)
// anchor_left=0, anchor_top=0, anchor_right=0, anchor_bottom=0

// Center
// anchor_left=0.5, anchor_top=0.5, anchor_right=0.5, anchor_bottom=0.5

// Bottom-Wide (like a toolbar)
// anchor_left=0, anchor_top=1, anchor_right=1, anchor_bottom=1
```

### Size Flags

```csharp
// In containers, size flags control how children distribute space
SizeFlagsHorizontal = (int)Control.SizeFlags.Fill;        // Fill available space
SizeFlagsHorizontal = (int)Control.SizeFlags.Expand;       // Expand to take more space
SizeFlagsHorizontal = (int)Control.SizeFlags.ExpandFill;   // Expand AND fill
SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkBegin;  // Shrink to start
SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkCenter; // Shrink to center
SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkEnd;    // Shrink to end
```

---

## Container Nodes

Containers are the backbone of UI layout. **Never manually position UI elements — use containers.**

### VBoxContainer / HBoxContainer
Arranges children vertically or horizontally with automatic spacing.

```ini
[node name="Menu" type="VBoxContainer" parent="."]
# In TSCN:
theme_override_constants/separation = 10

[node name="Title" type="Label" parent="Menu"]
text = "Main Menu"

[node name="PlayButton" type="Button" parent="Menu"]
text = "Play"

[node name="QuitButton" type="Button" parent="Menu"]
text = "Quit"
```

### GridContainer
Grid layout with fixed columns.

```ini
[node name="ItemGrid" type="GridContainer" parent="."]
columns = 4
theme_override_constants/h_separation = 5
theme_override_constants/v_separation = 5
```

### MarginContainer
Adds padding around a single child.

```ini
[node name="Margins" type="MarginContainer" parent="."]
theme_override_constants/margin_left = 20
theme_override_constants/margin_top = 20
theme_override_constants/margin_right = 20
theme_override_constants/margin_bottom = 20
```

### CenterContainer
Centers a single child node.

### PanelContainer
Container with a styled background (uses StyleBox from theme).

### ScrollContainer
Makes content scrollable when it overflows.

### TabContainer
Tabbed interface — each child is a tab page. The child's `Name` becomes the tab title.

---

## Theme System

### Creating Themes in C#

```csharp
public partial class ThemeSetup : Control
{
    public override void _Ready()
    {
        var theme = new Theme();

        // Button normal style
        var btnNormal = new StyleBoxFlat();
        btnNormal.BgColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        btnNormal.SetCornerRadiusAll(8);
        btnNormal.SetContentMarginAll(12);
        btnNormal.BorderWidthBottom = 2;
        btnNormal.BorderColor = new Color(0.3f, 0.3f, 0.4f, 1f);

        // Button hover style
        var btnHover = new StyleBoxFlat();
        btnHover.BgColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        btnHover.SetCornerRadiusAll(8);
        btnHover.SetContentMarginAll(12);
        btnHover.BorderWidthBottom = 2;
        btnHover.BorderColor = new Color(0.5f, 0.5f, 0.6f, 1f);

        // Button pressed style
        var btnPressed = new StyleBoxFlat();
        btnPressed.BgColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        btnPressed.SetCornerRadiusAll(8);
        btnPressed.SetContentMarginAll(12);

        // Apply to theme
        theme.SetStylebox("normal", "Button", btnNormal);
        theme.SetStylebox("hover", "Button", btnHover);
        theme.SetStylebox("pressed", "Button", btnPressed);
        theme.SetColor("font_color", "Button", new Color(0.9f, 0.9f, 0.95f));
        theme.SetFontSize("font_size", "Button", 18);

        // Panel style
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.SetCornerRadiusAll(12);
        panelStyle.SetContentMarginAll(16);
        panelStyle.BorderWidthBottom = 1;
        panelStyle.BorderWidthTop = 1;
        panelStyle.BorderWidthLeft = 1;
        panelStyle.BorderWidthRight = 1;
        panelStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
        theme.SetStylebox("panel", "PanelContainer", panelStyle);

        // Apply theme to root control
        Theme = theme;
    }
}
```

### Theme Resources (.tres)

Best practice: Create theme files and share across scenes.

```ini
[gd_resource type="Theme" load_steps=4 format=3]

[sub_resource type="StyleBoxFlat" id="btn_normal"]
bg_color = Color(0.15, 0.15, 0.2, 1)
corner_radius_top_left = 8
corner_radius_top_right = 8
corner_radius_bottom_left = 8
corner_radius_bottom_right = 8

[sub_resource type="StyleBoxFlat" id="btn_hover"]
bg_color = Color(0.2, 0.2, 0.3, 1)
corner_radius_top_left = 8
corner_radius_top_right = 8
corner_radius_bottom_left = 8
corner_radius_bottom_right = 8

[sub_resource type="StyleBoxFlat" id="panel_bg"]
bg_color = Color(0.1, 0.1, 0.15, 0.95)
corner_radius_top_left = 12
corner_radius_top_right = 12
corner_radius_bottom_left = 12
corner_radius_bottom_right = 12

[resource]
Button/styles/normal = SubResource("btn_normal")
Button/styles/hover = SubResource("btn_hover")
Button/font_colors/font_color = Color(0.9, 0.9, 0.95, 1)
PanelContainer/styles/panel = SubResource("panel_bg")
```

### Theme Inheritance

Themes propagate down the tree. Set a theme on a parent Control, and all children inherit it. Children can override specific values:

```csharp
// Override a single font size on a specific label
myLabel.AddThemeFontSizeOverride("font_size", 24);

// Override a color
myButton.AddThemeColorOverride("font_color", Colors.Red);

// Override a stylebox
myPanel.AddThemeStyleboxOverride("panel", customStylebox);
```

---

## Common UI Patterns

### Main Menu

```
MainMenu (CanvasLayer)
└── Control (Full Rect)
    └── MarginContainer (Full Rect, margins: 40px)
        └── VBoxContainer (center alignment)
            ├── TextureRect (logo)
            ├── HSeparator
            └── VBoxContainer (buttons, separation: 10)
                ├── Button "New Game"
                ├── Button "Continue"
                ├── Button "Settings"
                └── Button "Quit"
```

```csharp
public partial class MainMenu : CanvasLayer
{
    public override void _Ready()
    {
        var newGame = GetNode<Button>("%NewGameButton");
        var continueBtn = GetNode<Button>("%ContinueButton");
        var settings = GetNode<Button>("%SettingsButton");
        var quit = GetNode<Button>("%QuitButton");

        newGame.Pressed += OnNewGame;
        continueBtn.Pressed += OnContinue;
        settings.Pressed += OnSettings;
        quit.Pressed += OnQuit;

        // Set initial focus for gamepad
        newGame.GrabFocus();
    }

    private void OnNewGame() => GetTree().ChangeSceneToFile("res://scenes/levels/Level1.tscn");
    private void OnContinue() => SaveManager.Instance.LoadGame();
    private void OnSettings() => GetNode<Control>("SettingsPanel").Visible = true;
    private void OnQuit() => GetTree().Quit();
}
```

### HUD (Heads-Up Display)

```
HUD (CanvasLayer, layer=10)
└── MarginContainer (Full Rect, margins: 16px)
    └── VBoxContainer (Full Rect)
        ├── HBoxContainer (top bar)
        │   ├── TextureRect (heart icon)
        │   ├── ProgressBar (health)
        │   ├── Control (spacer, expand)
        │   └── Label (score)
        ├── Control (spacer, expand fill)
        └── HBoxContainer (bottom bar)
            ├── TextureRect (ammo icon)
            └── Label (ammo count)
```

```csharp
public partial class HUD : CanvasLayer
{
    private ProgressBar _healthBar;
    private Label _scoreLabel;
    private Label _ammoLabel;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("%HealthBar");
        _scoreLabel = GetNode<Label>("%ScoreLabel");
        _ammoLabel = GetNode<Label>("%AmmoLabel");
    }

    public void UpdateHealth(int current, int max)
    {
        _healthBar.MaxValue = max;
        var tween = CreateTween();
        tween.TweenProperty(_healthBar, "value", (double)current, 0.3)
             .SetTrans(Tween.TransitionType.Quad);

        // Color feedback
        float ratio = (float)current / max;
        _healthBar.Modulate = ratio < 0.3f ? Colors.Red :
                              ratio < 0.6f ? Colors.Yellow : Colors.Green;
    }

    public void UpdateScore(int score) => _scoreLabel.Text = $"Score: {score}";
    public void UpdateAmmo(int current, int max) => _ammoLabel.Text = $"{current}/{max}";
}
```

### Inventory Grid

```csharp
public partial class InventoryUI : Control
{
    [Export] public PackedScene SlotScene { get; set; }
    [Export] public int SlotCount { get; set; } = 20;
    [Export] public int Columns { get; set; } = 5;

    private GridContainer _grid;
    private Label _itemName;
    private RichTextLabel _itemDesc;

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("%ItemGrid");
        _itemName = GetNode<Label>("%ItemName");
        _itemDesc = GetNode<RichTextLabel>("%ItemDescription");
        _grid.Columns = Columns;

        // Create slots
        for (int i = 0; i < SlotCount; i++)
        {
            var slot = SlotScene.Instantiate<InventorySlot>();
            slot.SlotIndex = i;
            slot.SlotClicked += OnSlotClicked;
            _grid.AddChild(slot);
        }
    }

    private void OnSlotClicked(int index, ItemData item)
    {
        if (item != null)
        {
            _itemName.Text = item.ItemName;
            _itemDesc.Text = item.Description;
        }
    }

    public void SetItem(int slotIndex, ItemData item)
    {
        var slot = _grid.GetChild<InventorySlot>(slotIndex);
        slot.SetItem(item);
    }
}
```

### Dialogue Box

```csharp
public partial class DialogueBox : CanvasLayer
{
    [Signal] public delegate void DialogueFinishedEventHandler();

    private RichTextLabel _textLabel;
    private Label _nameLabel;
    private TextureRect _portrait;
    private VBoxContainer _choicesContainer;
    private float _charDelay = 0.03f;
    private bool _isTyping;

    public override void _Ready()
    {
        _textLabel = GetNode<RichTextLabel>("%DialogueText");
        _nameLabel = GetNode<Label>("%SpeakerName");
        _portrait = GetNode<TextureRect>("%Portrait");
        _choicesContainer = GetNode<VBoxContainer>("%Choices");
        Visible = false;
    }

    public async void ShowDialogue(string speaker, string text, Texture2D portrait = null)
    {
        Visible = true;
        _nameLabel.Text = speaker;
        _portrait.Texture = portrait;
        _textLabel.Text = text;
        _textLabel.VisibleCharacters = 0;
        _isTyping = true;

        // Typewriter effect
        for (int i = 0; i < text.Length; i++)
        {
            _textLabel.VisibleCharacters = i + 1;
            await ToSignal(GetTree().CreateTimer(_charDelay), SceneTreeTimer.SignalName.Timeout);

            if (!_isTyping) // Skip was pressed
            {
                _textLabel.VisibleCharacters = -1; // Show all
                break;
            }
        }
        _isTyping = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        if (@event.IsActionPressed("ui_accept"))
        {
            if (_isTyping)
                _isTyping = false; // Skip typewriter
            else
                EmitSignal(SignalName.DialogueFinished);
        }
    }
}
```

### Pause Menu

```csharp
public partial class PauseMenu : CanvasLayer
{
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // Work during pause
        Visible = false;

        GetNode<Button>("%ResumeButton").Pressed += Resume;
        GetNode<Button>("%SettingsButton").Pressed += OpenSettings;
        GetNode<Button>("%MainMenuButton").Pressed += GoToMainMenu;
        GetNode<Button>("%QuitButton").Pressed += QuitGame;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            TogglePause();
            GetViewport().SetInputAsHandled();
        }
    }

    private void TogglePause()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
        if (Visible)
            GetNode<Button>("%ResumeButton").GrabFocus();
    }

    private void Resume() => TogglePause();
    private void OpenSettings() { /* Show settings */ }
    private void GoToMainMenu()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
    }
    private void QuitGame() => GetTree().Quit();
}
```

---

## Gamepad & Keyboard Navigation

### Focus Setup

```csharp
public override void _Ready()
{
    // Set initial focus
    GetNode<Button>("FirstButton").GrabFocus();
}
```

### Automatic Focus Chain in Containers

VBoxContainer and HBoxContainer automatically set up focus neighbors for their children. For custom layouts:

```csharp
private void SetupFocusChain(Control[] controls)
{
    for (int i = 0; i < controls.Length; i++)
    {
        if (i > 0)
            controls[i].FocusNeighborTop = controls[i - 1].GetPath();
        if (i < controls.Length - 1)
            controls[i].FocusNeighborBottom = controls[i + 1].GetPath();
    }
    // Wrap around
    controls[0].FocusNeighborTop = controls[^1].GetPath();
    controls[^1].FocusNeighborBottom = controls[0].GetPath();
}
```

### Focus Styling

In theme, set different StyleBoxes for the `focus` state:
```ini
[resource]
Button/styles/focus = SubResource("btn_focus_style")
```

---

## UI Animations

### Transition Patterns

```csharp
// Fade in
public void FadeIn()
{
    Modulate = new Color(1, 1, 1, 0);
    Visible = true;
    CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3f);
}

// Slide from bottom
public void SlideIn()
{
    var targetY = Position.Y;
    Position = new Vector2(Position.X, GetViewportRect().Size.Y);
    Visible = true;
    CreateTween()
        .SetTrans(Tween.TransitionType.Back)
        .SetEase(Tween.EaseType.Out)
        .TweenProperty(this, "position:y", targetY, 0.4f);
}

// Scale popup
public void PopIn()
{
    Scale = Vector2.Zero;
    Visible = true;
    CreateTween()
        .SetTrans(Tween.TransitionType.Elastic)
        .SetEase(Tween.EaseType.Out)
        .TweenProperty(this, "scale", Vector2.One, 0.5f);
}
```

---

## Responsive Design

```csharp
public partial class ResponsiveUI : Control
{
    public override void _Ready()
    {
        GetViewport().SizeChanged += OnViewportSizeChanged;
        OnViewportSizeChanged();
    }

    private void OnViewportSizeChanged()
    {
        var size = GetViewportRect().Size;
        float ratio = size.X / size.Y;

        if (ratio < 1.5f)
        {
            // Portrait / narrow — stack vertically
            var container = GetNode<BoxContainer>("Layout");
            // Switch to VBoxContainer layout
        }
        else
        {
            // Landscape / wide — use horizontal layout
        }
    }
}
```

### Display Settings in project.godot

```ini
[display]
window/size/viewport_width=1920
window/size/viewport_height=1080
window/stretch/mode="canvas_items"
window/stretch/aspect="expand"
```

- `canvas_items` mode: UI scales with window size
- `expand` aspect: Content expands to fill, no black bars
