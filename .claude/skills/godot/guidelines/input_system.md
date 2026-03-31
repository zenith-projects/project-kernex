# Input System Reference

Complete reference for Godot 4.6 input: events, actions, gamepad, and the processing pipeline.

---

## Input Processing Pipeline (8 Steps)

Every input event flows through this pipeline in order:

| Step | Callback | Purpose |
|------|----------|---------|
| 1 | Window management | Resize, move embedded windows |
| 2 | Focused window routing | Routes to current viewport |
| 3 | `_Input()` | Standard input — can mark as handled |
| 4 | `Control._GuiInput()` | UI controls — respects `MouseFilter` |
| 5 | `_ShortcutInput()` | Key/button shortcuts (even with GUI focus) |
| 6 | `_UnhandledKeyInput()` | Key-only fallback |
| 7 | `_UnhandledInput()` | **Gameplay input goes here** |
| 8 | Physics picking | Ray-cast for 2D/3D click detection |

Events propagate in **reverse depth-first order** (bottom to top of scene tree).

### Which Callback to Use

| Callback | Use For |
|----------|---------|
| `_Input()` | Global shortcuts (screenshot, toggle debug) |
| `_GuiInput()` | Custom Control nodes |
| `_ShortcutInput()` | Shortcuts that work even when a TextEdit has focus |
| `_UnhandledInput()` | **All gameplay input** (movement, attack, interact) |

```csharp
// Gameplay input — lets UI consume events first
public override void _UnhandledInput(InputEvent @event)
{
    if (@event.IsActionPressed("interact"))
    {
        Interact();
        GetViewport().SetInputAsHandled(); // Stop propagation
    }
}
```

---

## Input Actions (InputMap)

Define named actions in **Project Settings > Input Map**. Bind multiple keys/buttons to each action. This is the recommended approach for all gameplay input.

### Polling (Continuous State)

```csharp
public override void _PhysicsProcess(double delta)
{
    // 2D movement vector (normalized, with deadzone)
    Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

    // Single axis
    float horizontal = Input.GetAxis("move_left", "move_right");

    // Analog strength (0.0 to 1.0 — works for triggers and sticks)
    float accelerate = Input.GetActionStrength("accelerate");

    // Digital checks
    if (Input.IsActionPressed("fire"))
        ContinuousFire();

    if (Input.IsActionJustPressed("jump"))
        Jump();

    if (Input.IsActionJustReleased("charge"))
        ReleaseCharge();
}
```

### Event-Driven (Discrete)

```csharp
public override void _UnhandledInput(InputEvent @event)
{
    // Action-based (preferred)
    if (@event.IsActionPressed("interact"))
    {
        Interact();
        GetViewport().SetInputAsHandled();
    }

    // Type-specific
    if (@event is InputEventMouseButton mb && mb.Pressed)
    {
        switch (mb.ButtonIndex)
        {
            case MouseButton.Left:
                OnLeftClick(mb.Position);
                break;
            case MouseButton.WheelUp:
                ZoomIn();
                break;
            case MouseButton.WheelDown:
                ZoomOut();
                break;
        }
    }

    if (@event is InputEventMouseMotion motion)
        OnMouseMove(motion.Relative);

    if (@event is InputEventKey key && key.Pressed)
    {
        if (key.Keycode == Key.Escape)
            TogglePause();

        // With modifiers
        if (key.Keycode == Key.S && key.CtrlPressed)
            SaveGame();
    }
}
```

---

## InputEvent Subclass Reference

| Type | Key Properties |
|------|---------------|
| `InputEventKey` | `Keycode`, `Unicode`, `Pressed`, `Echo`, `ShiftPressed`, `CtrlPressed`, `AltPressed` |
| `InputEventMouseButton` | `ButtonIndex`, `Pressed`, `DoubleClick`, `Position` |
| `InputEventMouseMotion` | `Position`, `Relative`, `Velocity` |
| `InputEventJoypadMotion` | `Axis`, `AxisValue` |
| `InputEventJoypadButton` | `ButtonIndex`, `Pressed` |
| `InputEventScreenTouch` | `Index`, `Position`, `Pressed` |
| `InputEventScreenDrag` | `Index`, `Position`, `Relative`, `Velocity` |
| `InputEventAction` | `Action`, `Pressed`, `Strength` |

---

## Gamepad / Controller Support

### Analog-Aware Methods

| Method | Returns | Use Case |
|--------|---------|----------|
| `Input.GetVector(neg_x, pos_x, neg_y, pos_y)` | Vector2 | Joystick movement (circular deadzone) |
| `Input.GetAxis(negative, positive)` | float | Single axis (triggers) |
| `Input.GetActionStrength(action)` | float | Analog pressure (0.0-1.0) |
| `Input.IsActionPressed(action)` | bool | Digital state |

```csharp
// Best practice: always use GetVector for movement — handles deadzone automatically
Vector2 moveInput = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
```

### Deadzone

Analog sticks never truly rest at 0.0. Godot applies a **deadzone filter** (default: 0.5):
- Ignores input noise below threshold
- `GetVector()` applies **circular deadzone** (prevents unwanted diagonal input)
- Configure per-action in Input Map settings

### Vibration / Haptic

```csharp
// Gamepad vibration
Input.StartJoyVibration(0, 0.5f, 0.5f, 0.3f); // device, weak, strong, duration
Input.StopJoyVibration(0);

// Mobile device vibration (requires Android VIBRATE permission)
Input.VibrateHandheld(200); // milliseconds
```

**Always provide an option to disable vibration.**

### Controller Gotchas

- **No echo events**: Controller buttons don't repeat when held (unlike keyboard). Implement manually if needed.
- **Window focus**: Controller input reaches ALL windows system-wide, including unfocused ones. Check focus before processing.
- **Windows limit**: Maximum **4 simultaneous controllers** (XInput API limit).
- **Screen sleep**: Controller input does NOT prevent screen dimming. Enable **Keep Screen On** in Project Settings.
- **Keyboard ghosting**: Low-end keyboards can't register 3+ simultaneous keys.

### Focus-Safe Controller Input

```csharp
// Autoload to prevent controller input when game is unfocused
public partial class FocusGuard : Node
{
    public static FocusGuard Instance { get; private set; }
    public bool IsFocused { get; private set; } = true;

    public override void _Ready() => Instance = this;

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) IsFocused = false;
        if (what == NotificationApplicationFocusIn) IsFocused = true;
    }

    public bool IsActionPressed(StringName action)
        => IsFocused && Input.IsActionPressed(action);
}
```

---

## Mouse Mode

```csharp
// Capture mouse (FPS games)
Input.MouseMode = Input.MouseModeEnum.Captured;

// Show mouse (menus)
Input.MouseMode = Input.MouseModeEnum.Visible;

// Hidden but not captured (custom cursor)
Input.MouseMode = Input.MouseModeEnum.Hidden;

// Confined to window
Input.MouseMode = Input.MouseModeEnum.Confined;
Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
```

---

## Programmatic Input Events

```csharp
// Fire an action programmatically (useful for tutorials, AI, testing)
var ev = new InputEventAction();
ev.Action = "jump";
ev.Pressed = true;
Input.ParseInputEvent(ev);
```

---

## Runtime Keybind Remapping

```csharp
public void RemapAction(string actionName, InputEvent newEvent)
{
    // Remove existing events
    InputMap.ActionEraseEvents(actionName);

    // Add new event
    InputMap.ActionAddEvent(actionName, newEvent);
}

// Note: InputMap state is NOT saved automatically.
// You must serialize/deserialize keybinds to a config file.
```

---

## Input Composition Pattern

```
Player (CharacterBody3D)
├── ... components ...
└── (input handled in Player.cs _UnhandledInput)

PauseMenu (CanvasLayer, ProcessMode=Always)
└── (captures "pause" action in _UnhandledInput)

HUD (CanvasLayer)
├── Buttons (consume clicks via _GuiInput, stopping propagation)
└── InventorySlots (consume clicks)
```

Input flows: Window → HUD buttons → PauseMenu → Player.
If a button consumes the click, Player never sees it. This is the correct layering.
