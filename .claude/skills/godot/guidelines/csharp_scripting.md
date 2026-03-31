# C# Scripting Reference for Godot 4.6

Complete reference for C# game development in Godot 4.6, sourced from official documentation.

---

## Prerequisites

- .NET SDK 8 or later is required (exporting to Android requires .NET 9+)
- Install the .NET-enabled version of Godot
- All classes must be `partial` and match their file name (case-sensitive)
- The `using Godot;` namespace provides access to all engine types
- Rebuild the project (Build button top-right) whenever you change exports, signals, or tool scripts

---

## C# vs GDScript Key Differences

| Feature | GDScript | C# |
|---------|----------|-----|
| Naming | `snake_case` | `PascalCase` for methods/properties |
| Properties | `@export var speed = 100` | `[Export] public float Speed { get; set; } = 100f;` |
| Signals | `signal health_changed` | `[Signal] public delegate void HealthChangedEventHandler();` |
| Preloading | `preload("res://scene.tscn")` | Not available. Use `GD.Load<T>()` or `ResourceLoader.Load<T>()` |
| Await | `await signal` | `await ToSignal(obj, SignalName.Signal)` |
| Null check | `if node:` | `if (node != null)` or `node?.Method()` |
| Type cast | `node as Type` | `node as Type` or `(Type)node` |
| Print | `print("text")` | `GD.Print("text")` |
| Range | `range(10)` | `Enumerable.Range(0, 10)` or `for (int i = 0; i < 10; i++)` |
| @onready | `@onready var x = $Child` | Cache in `_Ready()`: `_x = GetNode<T>("Child")` |
| Singletons | `Input.is_action_pressed()` | `Input.IsActionPressed()` (static class) |

**Critical C# differences:**
- No `preload()` -- use `GD.Load<T>()` or `ResourceLoader.Load<T>()` or cache in `_Ready()`
- No `$NodeName` syntax -- use `GetNode<T>("NodeName")`
- Signal delegate names MUST end with `EventHandler`
- Rebuild the project (Build button) for exports/signals to appear in editor
- Class name MUST match file name: class `MyNode` must be in `MyNode.cs`
- All scripts must use the `partial` keyword
- Methods that use Godot's `snake_case` API internally (like `Get()`, `Set()`, `Call()`, `CallDeferred()`, `Connect()`) expect snake_case string names. Use `PropertyName`, `MethodName`, `SignalName` nested classes to avoid this.

---

## Naming Conventions (PascalCase API)

C# uses `PascalCase` instead of GDScript's `snake_case`. Where possible, getters/setters are properties:

```csharp
// GDScript: x.set_name("Friend")
// C#:
x.Name = "Friend";

// GDScript: get_parent().set("visible", false)
// C#:
GetParent().Set("visible", false);  // Note: Set() uses snake_case strings internally

// Using StringName classes avoids snake_case issues:
EmitSignal(SignalName.MySignal);
CallDeferred(MethodName.DoSomething);
```

**Struct identity constructors** (parameterless constructors initialize to zero/default in C#):
```csharp
// GDScript: Basis()  ->  C#: Basis.Identity
// GDScript: Transform2D()  ->  C#: Transform2D.Identity
// GDScript: Transform3D()  ->  C#: Transform3D.Identity
// GDScript: Quaternion()  ->  C#: Quaternion.Identity
// GDScript: Projection()  ->  C#: Projection.Identity
// GDScript: Color()  ->  C#: Colors.Black  (new Color() is transparent black)
```

**Type naming differences:**
- `AABB` -> `Aabb`
- `RID` -> `Rid`
- `Rect2i` -> `Rect2I`

---

## Node Lifecycle Methods

```csharp
public partial class MyNode : Node
{
    // Called when node enters the scene tree (BEFORE _Ready)
    public override void _EnterTree()
    {
        // Use for: early initialization, connecting to tree signals
    }

    // Called after node AND all children have entered the tree
    // Children's _Ready() fires BEFORE parent's _Ready()
    public override void _Ready()
    {
        // Use for: initialization, getting node references, signal connections
    }

    // Called every frame (variable timestep)
    // delta = time since last frame in seconds
    public override void _Process(double delta)
    {
        // Use for: visual updates, UI, non-physics logic
    }

    // Called at fixed interval (default 60 FPS, configurable in Project Settings)
    public override void _PhysicsProcess(double delta)
    {
        // Use for: physics, movement, collision response
    }

    // Called for every input event
    public override void _Input(InputEvent @event)
    {
        // Use for: global input handling (pause, screenshot)
        // Call GetViewport().SetInputAsHandled() to consume the event
    }

    // Called for input events not handled by _Input or UI
    public override void _UnhandledInput(InputEvent @event)
    {
        // Use for: game input (movement, attack)
    }

    // Called when node is about to leave the scene tree
    public override void _ExitTree()
    {
        // Use for: cleanup, disconnecting signals, freeing resources
    }

    // Called by the notification system
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // Object is about to be freed
        }
    }
}
```

**Execution order:**
```
_EnterTree()  (parent first, then children)
_Ready()      (children first, then parent)
_Process()    (parent first, then children)
_ExitTree()   (children first, then parent)
```

---

## Signal System

### Declaring Signals

```csharp
public partial class HealthComponent : Node
{
    // Parameterless signal
    [Signal] public delegate void DiedEventHandler();

    // Signal with parameters
    [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    // Signal with complex types
    [Signal] public delegate void DamageTakenEventHandler(int amount, Vector2 knockbackDirection);
}
```

**Rules:**
- Delegate name MUST end with `EventHandler`
- The event name (and signal name) is the delegate name minus `EventHandler` (e.g., `HealthChanged`)
- Parameters must be Variant-compatible types
- Signal arguments of custom types must inherit from `GodotObject` or a subclass:
  ```csharp
  public partial class DataObject : GodotObject
  {
      public string MyFirstString { get; set; }
      public string MySecondString { get; set; }
  }
  ```
- Rebuild project after adding signals
- You CANNOT use `Invoke` to raise events tied to Godot signals; you must use `EmitSignal`

### Emitting Signals

```csharp
// Using SignalName (type-safe, preferred)
EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
EmitSignal(SignalName.Died);

// Using string (legacy, avoid)
EmitSignal("HealthChanged", _currentHealth, MaxHealth);
```

### Connecting Signals in C#

```csharp
public override void _Ready()
{
    // Method 1: C# event syntax (preferred, type-safe)
    var health = GetNode<HealthComponent>("HealthComponent");
    health.HealthChanged += OnHealthChanged;
    health.Died += OnDied;

    // Method 2: Lambda (for simple cases)
    var button = GetNode<Button>("Button");
    button.Pressed += () => GD.Print("Button pressed!");

    // Method 3: Connect with flags (e.g., OneShot)
    button.Connect(Button.SignalName.Pressed,
        Callable.From(OnButtonPressed),
        (uint)GodotObject.ConnectFlags.OneShot);

    // Method 4: Connect built-in signals
    var area = GetNode<Area2D>("DetectionArea");
    area.BodyEntered += OnBodyEnteredDetection;

    // Method 5: Bound values via lambda
    Button plusButton = GetNode<Button>("PlusButton");
    plusButton.Pressed += () => ModifyValue(1);
    Button minusButton = GetNode<Button>("MinusButton");
    minusButton.Pressed += () => ModifyValue(-1);
}
```

### Disconnecting Signals

```csharp
public override void _ExitTree()
{
    var health = GetNode<HealthComponent>("HealthComponent");
    health.HealthChanged -= OnHealthChanged;
    health.Died -= OnDied;
}
```

**Automatic disconnection caveats:**
- Godot auto-disconnects when any `GodotObject` is freed -- for BOTH emitter and receiver sides.
- Automatic disconnection does NOT work when:
  1. The signal is connected to a lambda that captures a variable (the `Delegate.Target` points to a generated closure type, not the node)
  2. The signal is a custom signal (declared with `[Signal]`) connected via `+=`
- For custom signals, either disconnect manually in `_ExitTree` / `Dispose`, or use `Connect()` method which does auto-disconnect:
  ```csharp
  [Export] public MyClass Target { get; set; }

  public override void _EnterTree()
  {
      // Using Connect() auto-disconnects even for custom signals
      Target.Connect(MyClass.SignalName.MySignal, Callable.From(OnMySignal));
  }
  ```

### Signal creation at runtime

```csharp
public override void _Ready()
{
    AddUserSignal("MyCustomSignal");
    EmitSignal("MyCustomSignal");
}
```
Runtime signals are NOT visible via `SignalName` and must use string names.

### Awaiting Signals (async/await)

```csharp
public async Task SomeFunction()
{
    await ToSignal(timer, Timer.SignalName.Timeout);
    GD.Print("After timeout");
}

// Await next frame
await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

// Await timer
await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);

// Await animation
_animPlayer.Play("attack");
await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
```

The `Signal` type implements the awaitable pattern and can be used with `await`.
C#'s `await` works with any type that has `GetAwaiter()` returning a type implementing `INotifyCompletion` with `IsCompleted` and `GetResult()`.

---

## Export Properties

### Basic Exports

Exporting works with both fields and properties, with any access modifier.
Only Variant-compatible types can be exported.

```csharp
// Fields
[Export] private int _number;

// Properties (preferred)
[Export] public int Number { get; set; }

// With defaults
[Export] public int Number { get; set; } = 5;
[Export] public string Text { get; set; } = "Hello World";

// Simple types
[Export] public float Speed { get; set; } = 200f;
[Export] public bool IsActive { get; set; } = true;

// Vectors
[Export] public Vector2 SpawnOffset { get; set; } = Vector2.Zero;
[Export] public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);

// Colors
[Export] public Color TintColor { get; set; } = Colors.White;
[Export(PropertyHint.ColorNoAlpha)] public Color SolidColor { get; set; }

// Resources (shows drag-and-drop in inspector)
[Export] public PackedScene BulletScene { get; set; }
[Export] public Texture2D Icon { get; set; }
[Export] public AudioStream HitSound { get; set; }
[Export] public AnimationNode AnimNode { get; set; }

// Nodes (directly, without NodePath)
[Export] public Node Node { get; set; }
[Export] public Sprite2D Sprite2D { get; set; }  // Filtered to Sprite2D types

// Node paths (legacy style)
[Export] public NodePath NodePath { get; set; }
```

**Properties with backing fields** use the backing field's default:
```csharp
private int _number = 2;

[Export]
public int NumberWithBackingField
{
    get => _number;
    set => _number = value;
}
```

> Note: Godot analyzes C# source code to determine defaults (not executing the getter). Complex expressions in getters won't be understood by the analyzer.

### Export Groups and Categories

```csharp
// Group (indented section in inspector)
[ExportGroup("Movement")]
[Export] public float Speed { get; set; } = 200f;
[Export] public float Acceleration { get; set; } = 1000f;

// Subgroup (nested inside group)
[ExportSubgroup("Knockback")]
[Export] public float KnockbackForce { get; set; } = 200f;
[Export] public float KnockbackDuration { get; set; } = 0.2f;

// End group
[ExportGroup("")]

// Category (top-level section, like a new class header)
[ExportCategory("Main Category")]
[Export] public int Number { get; set; } = 3;
[Export] public string Text { get; set; } = "";

[ExportCategory("Extra Category")]
[Export] public bool Flag { get; set; } = false;
```

The second argument of `[ExportGroup]` can filter by prefix.
Groups cannot be nested -- use `[ExportSubgroup]` for nesting within a group.

### Strings as Paths

```csharp
[Export(PropertyHint.File)] public string GameFile { get; set; }
[Export(PropertyHint.Dir)] public string GameDirectory { get; set; }
[Export(PropertyHint.File, "*.txt,")] public string TextFile { get; set; }
[Export(PropertyHint.File, "*.png,*.jpg")] public string ImageFile { get; set; }
[Export(PropertyHint.GlobalFile, "*.png")] public string ToolImage { get; set; }  // Tool mode only
[Export(PropertyHint.GlobalDir)] public string ToolDir { get; set; }  // Tool mode only
[Export(PropertyHint.MultilineText)] public string Description { get; set; }
```

### Numeric Ranges

```csharp
[Export(PropertyHint.Range, "0,20,")] public int Number { get; set; }
[Export(PropertyHint.Range, "-10,20,")] public int SignedNumber { get; set; }
[Export(PropertyHint.Range, "-10,20,0.2")] public float FloatNumber { get; set; }
[Export(PropertyHint.Range, "0,100,1,or_greater,or_less")] public int Flexible { get; set; }
```

### Easing and Suffix Hints

```csharp
[Export(PropertyHint.ExpEasing)] public float TransitionSpeed { get; set; }

[Export(PropertyHint.None, "suffix:m/s\u00b2")] public float GravityAccel { get; set; } = 9.8f;
[Export(PropertyHint.None, "suffix:m/s")] public Vector3 Velocity { get; set; }
```

### Exporting Enums

```csharp
// Enum type (automatic dropdown)
public enum MyEnum { Thing1, Thing2, AnotherThing = -1 }

[Export] public MyEnum MyEnumValue { get; set; }

// String hint enum (value stored as int index)
[Export(PropertyHint.Enum, "Warrior,Magician,Thief")]
public int CharacterClass { get; set; }

// With explicit values
[Export(PropertyHint.Enum, "Slow:30,Average:60,Very Fast:200")]
public int CharacterSpeed { get; set; }

// String enum (value stored as string)
[Export(PropertyHint.Enum, "Rebecca,Mary,Leah")]
public string CharacterName { get; set; } = "Rebecca";
```

### Exporting Bit Flags

```csharp
// Using [Flags] enum (preferred)
[Flags]
public enum SpellElements
{
    Fire = 1,
    Water = 2,
    Earth = 4,
    Wind = 8,
}

[Export] public SpellElements Elements { get; set; }

// Using string hint
[Export(PropertyHint.Flags, "Fire,Water,Earth,Wind")]
public int SpellElements { get; set; } = 0;

// With explicit values
[Export(PropertyHint.Flags, "Self:4,Allies:8,Foes:16")]
public int SpellTargets { get; set; } = 0;

// Combination flags
[Export(PropertyHint.Flags, "Self:4,Allies:8,Self and Allies:12,Foes:16")]
public int Targets { get; set; } = 0;

// Physics/Render layers
[Export(PropertyHint.Layers2DPhysics)] public uint Layers2DPhysics { get; set; }
[Export(PropertyHint.Layers2DRender)] public uint Layers2DRender { get; set; }
[Export(PropertyHint.Layers3DPhysics)] public uint Layers3DPhysics { get; set; }
[Export(PropertyHint.Layers3DRender)] public uint Layers3DRender { get; set; }
```

### Exporting Inspector Buttons ([ExportToolButton])

Requires `[Tool]` attribute on the class. Creates a clickable button in the inspector:

```csharp
[Tool]
public partial class MyNode : Node
{
    [ExportToolButton("Click me!")]
    public Callable ClickMeButton => Callable.From(ClickMe);

    // With icon from EditorIcons
    [ExportToolButton("Click me!", Icon = "CharacterBody2D")]
    public Callable IconButton => Callable.From(ClickMe);

    public void ClickMe()
    {
        GD.Print("Hello world!");
    }
}
```

### Exporting Collections

```csharp
// Untyped Godot array
[Export] public Godot.Collections.Array Array { get; set; }

// Typed Godot array (inspector restricts element type)
[Export] public Godot.Collections.Array<int> IntArray { get; set; }

// Array with default values
[Export] public Godot.Collections.Array<string> Names { get; set; } =
[
    "Rebecca",
    "Mary",
    "Leah",
];

// Resource arrays (drag-and-drop from FileSystem dock)
[Export] public Godot.Collections.Array<Texture2D> Textures { get; set; }
[Export] public Godot.Collections.Array<PackedScene> Scenes { get; set; }

// Untyped dictionary
[Export] public Godot.Collections.Dictionary Dictionary { get; set; }

// Typed dictionary
[Export] public Godot.Collections.Dictionary<string, int> Stats { get; set; }

// Dictionary with defaults
[Export] public Godot.Collections.Dictionary<string, int> CharacterLives { get; set; } =
    new Godot.Collections.Dictionary<string, int>
    {
        ["Rebecca"] = 10,
        ["Mary"] = 42,
        ["Leah"] = 0,
    };

// C# arrays (element type must be Variant-compatible)
[Export] public Vector3[] Vectors { get; set; }
[Export] public NodePath[] NodePaths { get; set; }

// C# array with defaults
[Export] public Vector3[] Positions { get; set; } =
[
    new Vector3(1, 2, 3),
    new Vector3(3, 2, 1),
];
```

Default value of Godot arrays, dictionaries, and C# arrays is `null`.

### Tool Script Exports

When changing an exported variable from a `[Tool]` script, call `NotifyPropertyListChanged()` to update the inspector.

### Advanced Exports

For complex export logic, implement `_Set()`, `_Get()`, and `_GetPropertyList()`. The script must be in `[Tool]` mode for these to work in the editor.

---

## [GlobalClass] Attribute

Registers a type in Godot's editor for the Add Node / Create Resource dialogs.

```csharp
using Godot;

[GlobalClass]
public partial class MyNode : Node
{
}
```

**Rules:**
- File name must match class name in case-sensitive fashion (`MyNode.cs`)
- Without `[GlobalClass]`, inspector assignments fall back to the base Godot type

**With [Icon] attribute** (shows custom icon in editor):
```csharp
[GlobalClass, Icon("res://Stats/StatsIcon.svg")]
public partial class Stats : Resource
{
    [Export] public int Strength { get; set; }
    [Export] public int Defense { get; set; }
    [Export] public int Speed { get; set; }
}
```

**Exporting GlobalClass properties** filters assignments in the inspector:
```csharp
public partial class Main : Node
{
    [Export] public MyNode MyNode { get; set; }  // Only MyNode or derived types
    [Export] public Stats EnemyStats { get; set; }  // Only Stats resources
}
```

> Warning: Classes with names starting with "Editor" are hidden in Create New Node/Scene dialogs.

---

## [Tool] Attribute

Makes a script run in the editor:

```csharp
[Tool]
public partial class MyEditorNode : Node
{
    public override void _Process(double delta)
    {
        // This runs in the editor!
    }
}
```

Check if running in editor:
```csharp
if (Engine.IsEditorHint())
{
    // Editor-only logic
}
```

---

## Variant Type System

### Variant-Compatible Types

| Variant.Type | C# Type |
|---|---|
| Nil | null |
| Bool | bool |
| Int | long (64-bit) |
| Float | double (64-bit) |
| String | string |
| Vector2 | Godot.Vector2 |
| Vector2I | Godot.Vector2I |
| Rect2 | Godot.Rect2 |
| Rect2I | Godot.Rect2I |
| Vector3 | Godot.Vector3 |
| Vector3I | Godot.Vector3I |
| Transform2D | Godot.Transform2D |
| Vector4 | Godot.Vector4 |
| Vector4I | Godot.Vector4I |
| Plane | Godot.Plane |
| Quaternion | Godot.Quaternion |
| Aabb | Godot.Aabb |
| Basis | Godot.Basis |
| Transform3D | Godot.Transform3D |
| Projection | Godot.Projection |
| Color | Godot.Color |
| StringName | Godot.StringName |
| NodePath | Godot.NodePath |
| Rid | Godot.Rid |
| Object | Godot.GodotObject (or any derived type) |
| Callable | Godot.Callable |
| Signal | Godot.Signal |
| Dictionary | Godot.Collections.Dictionary |
| Array | Godot.Collections.Array |
| PackedByteArray | byte[] |
| PackedInt32Array | int[] |
| PackedInt64Array | long[] |
| PackedFloat32Array | float[] |
| PackedFloat64Array | double[] |
| PackedStringArray | string[] |
| PackedVector2Array | Vector2[] |
| PackedVector3Array | Vector3[] |
| PackedVector4Array | Vector4[] |
| PackedColorArray | Color[] |

**All built-in value types except `decimal`, `nint`, `nuint` are compatible.**
Smaller types (`int`, `short`, `float`) are supported but stored as 64-bit internally -- potential precision loss.

### Variant Conversion

```csharp
// To Variant (implicit conversion)
int x = 42;
Variant numberVariant = x;
Variant helloVariant = "Hello, World!";

// Explicit creation
Variant v1 = Variant.CreateFrom(x);
Variant v2 = Variant.From(x);

// From Variant (explicit conversion)
int number = (int)numberVariant;
string hello = (string)helloVariant;

// Generic conversion
int n2 = numberVariant.As<int>();
int n3 = numberVariant.AsInt32();

// Null variant
Variant nullVar = default;

// Enum conversion (no implicit conversion)
enum MyEnum { A, B, C }
Variant ev1 = (int)MyEnum.A;
MyEnum e1 = (MyEnum)(int)ev1;
// Or generic:
Variant ev2 = Variant.From(MyEnum.A);
MyEnum e2 = ev2.As<MyEnum>();

// Variant.Obj for unknown types (boxes value types)
object obj = someVariant.Obj;
```

### [MustBeVariant] Attribute

Constrains generic types to Variant-compatible:
```csharp
public void Method<[MustBeVariant] T>(T value)
{
    Variant variant = Variant.From(value);
}
```

---

## Global Scope Equivalents

### Key Mappings

| GDScript | C# |
|---|---|
| `print(x)` | `GD.Print(x)` |
| `printerr(x)` | `GD.PrintErr(x)` |
| `push_error(x)` | `GD.PushError(x)` |
| `push_warning(x)` | `GD.PushWarning(x)` |
| `abs(x)` | `Mathf.Abs(x)` |
| `lerp(a,b,t)` | `Mathf.Lerp(a,b,t)` |
| `clamp(x,a,b)` | `Mathf.Clamp(x,a,b)` |
| `deg_to_rad(x)` | `Mathf.DegToRad(x)` |
| `rad_to_deg(x)` | `Mathf.RadToDeg(x)` |
| `move_toward(a,b,d)` | `Mathf.MoveToward(a,b,d)` |
| `smoothstep(a,b,x)` | `Mathf.SmoothStep(a,b,x)` |
| `snapped(x,s)` | `Mathf.Snapped(x,s)` |
| `PI` | `Mathf.Pi` |
| `TAU` | `Mathf.Tau` |
| `randf()` | `GD.Randf()` |
| `randi()` | `GD.Randi()` |
| `randf_range(a,b)` | `GD.RandRange(a,b)` |
| `randomize()` | `GD.Randomize()` |
| `seed(s)` | `GD.Seed(s)` |
| `hash(x)` | `GD.Hash(x)` |
| `load(path)` | `GD.Load(path)` or `GD.Load<T>(path)` |
| `preload(path)` | N/A (use `GD.Load<T>()`) |
| `var_to_str(x)` | `GD.VarToStr(x)` |
| `str_to_var(x)` | `GD.StrToVar(x)` |
| `bytes_to_var(x)` | `GD.BytesToVar(x)` |
| `var_to_bytes(x)` | `GD.VarToBytes(x)` |
| `assert(x)` | `System.Diagnostics.Debug.Assert(x)` |
| `range(n)` | `GD.Range(n)` or `Enumerable.Range(0,n)` |
| `len(x)` | N/A (use `.Length` / `.Count`) |
| `weakref(obj)` | `GodotObject.WeakRef(obj)` |
| `instance_from_id(id)` | `GodotObject.InstanceFromId(id)` |
| `is_instance_valid(obj)` | `GodotObject.IsInstanceValid(obj)` |
| `type_convert(x,t)` | `Variant.As<T>()` or `GD.Convert()` |
| `typeof(x)` | `Variant.VariantType` |

### `using static` Shortcut

```csharp
using static Godot.GD;

public class Test
{
    static Test()
    {
        Print("Hello"); // Instead of GD.Print("Hello");
    }
}
```

---

## Singletons

Godot singletons are static classes in C#:
```csharp
Input.IsActionPressed("ui_down")
```

For rare cases needing the base `GodotObject` API (like signal connection):
```csharp
Input.Singleton.JoyConnectionChanged += Input_JoyConnectionChanged;
```

`EditorInterface` is NOT static in C#:
```csharp
EditorInterface.Singleton  // Use this to access it
```

---

## Callable

```csharp
// Create from method
Callable callable = Callable.From(MyMethod);
Callable callable = Callable.From(() => SayHello(name));

// bind/unbind are not implemented -- use lambdas instead:
string name = "John Doe";
Callable callable = Callable.From(() => SayHello(name));
```

Limitations:
- Custom Callables (with bind/unbind, or from GDExtension) are unsupported in C#
- Use lambdas for capturing/binding values

---

## Node Access Patterns

```csharp
// Direct typed access
var player = GetNode<Player>("Player");
var sprite = GetNode<Sprite2D>("Sprite2D");

// Null-safe access
var player = GetNodeOrNull<Player>("/root/Main/Player");
player?.TakeDamage(10);

// Cache in _Ready (equivalent of @onready)
private Label _myLabel;
public override void _Ready()
{
    _myLabel = GetNode<Label>("MyLabel");
}

// Export for editor assignment (fastest, survives node moves)
[Export] public Node MyNode { get; set; }
[Export] public Sprite2D MySprite { get; set; }

// Autoload access
var globals = GetNode<MyGlobals>("/root/Globals");

// Duck-typed access (for cross-language scripting)
GetParent().Set("visible", false);
GetParent().Call("MyMethod", arg1, arg2);
```

---

## Godot Interfaces Pattern

Godot uses duck-typing. For type-safe access in C#:

```csharp
// Cast check
CanvasItem ci = GetParent() as CanvasItem;
if (ci != null)
{
    ci.Visible = false;
    ci.ShowOnTop = true;
}

// Pattern matching
if (body is Player player)
{
    player.TakeDamage(10);
}

// Method existence check
if (child.HasMethod("SetVisible"))
{
    child.Call("SetVisible", false);
}

// Group-based interface
if (child.IsInGroup("Offer"))
{
    child.Call("Accept");
}

// Callable delegation pattern
public partial class Child : Node
{
    public Callable? Callable { get; set; }

    public void MyMethod()
    {
        Callable?.Call();
    }
}

public partial class Parent : Node
{
    private Child _child;

    public override void _Ready()
    {
        _child = GetNode<Child>("Child");
        _child.Callable = Callable.From(PrintMe);
    }

    public void PrintMe() => GD.Print(Name);
}
```

---

## Common Pitfalls

### Struct Copy-on-Assignment

```csharp
// ERROR: Cannot modify return value
Position.X = 100.0f;  // CS1612

// FIX 1: Reassign entire struct
var newPosition = Position;
newPosition.X = 100.0f;
Position = newPosition;

// FIX 2: With expression (C# 10+, preferred)
Position = Position with { X = 100.0f };
```

### Performance: Native Interop Costs

Properties on GodotObject-derived types require native interop calls. Cache values locally:

```csharp
private void ExpensiveReposition()
{
    // BAD: Each Position access is a native call
    for (var i = 0; i < 10000; i++)
    {
        Position = new Vector3(Position.X + 1, Position.Y, Position.Z);
    }

    // GOOD: Cache locally, assign once
    var pos = Position;
    for (var i = 0; i < 10000; i++)
    {
        pos.X += 1;
    }
    Position = pos;
}
```

String-to-`NodePath`/`StringName` implicit conversions also incur marshalling costs.
Passing raw `byte[]` or `string` to Godot APIs requires marshalling.

---

## Resource Loading

### Synchronous Loading

```csharp
var scene = ResourceLoader.Load<PackedScene>("res://scenes/Bullet.tscn");
var texture = ResourceLoader.Load<Texture2D>("res://sprites/player.png");
var sound = ResourceLoader.Load<AudioStream>("res://audio/sfx/hit.wav");
var theme = ResourceLoader.Load<Theme>("res://resources/themes/main_theme.tres");

// GD.Load shorthand
var typedResource = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
```

### Caching Pattern

```csharp
public partial class ProjectileManager : Node
{
    private PackedScene _bulletScene;
    private PackedScene _rocketScene;

    public override void _Ready()
    {
        _bulletScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/Bullet.tscn");
        _rocketScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/Rocket.tscn");
    }

    public Node2D SpawnBullet(Vector2 position, float rotation)
    {
        var bullet = _bulletScene.Instantiate<Node2D>();
        bullet.GlobalPosition = position;
        bullet.Rotation = rotation;
        AddChild(bullet);
        return bullet;
    }
}
```

### Background Loading

```csharp
public partial class SceneLoader : Node
{
    [Signal] public delegate void LoadProgressEventHandler(float progress);
    [Signal] public delegate void LoadCompleteEventHandler();

    private string _loadingPath;

    public void LoadSceneAsync(string path)
    {
        _loadingPath = path;
        ResourceLoader.LoadThreadedRequest(path, "PackedScene", true);
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        var status = ResourceLoader.LoadThreadedGetStatus(_loadingPath, out Godot.Collections.Array progress);

        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                EmitSignal(SignalName.LoadProgress, (float)progress[0]);
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                var scene = ResourceLoader.LoadThreadedGet(_loadingPath) as PackedScene;
                GetTree().ChangeSceneToPacked(scene);
                EmitSignal(SignalName.LoadComplete);
                SetProcess(false);
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                GD.PrintErr($"Failed to load: {_loadingPath}");
                SetProcess(false);
                break;
        }
    }
}
```

---

## Scene Instancing

```csharp
var enemyScene = ResourceLoader.Load<PackedScene>("res://scenes/Enemy.tscn");
var enemy = enemyScene.Instantiate<Enemy>();

// Configure before adding to tree
enemy.GlobalPosition = spawnPoint;
enemy.MaxHealth = 200;

// Add to scene tree
GetTree().CurrentScene.AddChild(enemy);

// Or add to specific parent
GetNode("Enemies").AddChild(enemy);
```

---

## Input Handling

### Action-Based Input (Preferred)

```csharp
public override void _PhysicsProcess(double delta)
{
    float horizontalInput = Input.GetAxis("move_left", "move_right");
    Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

    if (Input.IsActionJustPressed("jump")) Jump();
    if (Input.IsActionPressed("fire")) Fire();
    if (Input.IsActionJustReleased("charge")) ReleaseCharge();
    float triggerStrength = Input.GetActionStrength("accelerate");
}
```

### Event-Based Input

```csharp
public override void _UnhandledInput(InputEvent @event)
{
    if (@event.IsActionPressed("interact"))
    {
        Interact();
        GetViewport().SetInputAsHandled();
    }

    if (@event is InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            OnLeftClick(mouseButton.Position);
    }

    if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        TogglePause();

    if (@event is InputEventMouseMotion mouseMotion)
        OnMouseMove(mouseMotion.Relative);
}
```

---

## Tweens and Animation

```csharp
var tween = CreateTween();

// Property tween
tween.TweenProperty(this, "position", new Vector2(100, 200), 1.0);
tween.TweenProperty(this, "modulate:a", 0.0f, 0.5);

// Callback
tween.TweenCallback(Callable.From(() => GD.Print("Done!")));

// Interval (delay)
tween.TweenInterval(0.5);

// Easing
tween.SetTrans(Tween.TransitionType.Bounce);
tween.SetEase(Tween.EaseType.Out);

// Parallel (runs same time as previous)
tween.TweenProperty(this, "position:x", 100f, 0.5);
tween.Parallel().TweenProperty(this, "position:y", 200f, 0.5);

// Looping
tween.SetLoops(3);   // Loop 3 times
tween.SetLoops(0);   // Loop forever

// Implicit Variant conversion in tween arguments
tween.TweenProperty(GetNode("Sprite"), "modulate", Colors.Red, 1.0f);
```

---

## Autoloads / Singletons

```csharp
// GameManager.cs -- register as Autoload in Project Settings
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public int Score { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}

// Usage:
GameManager.Instance.AddScore(100);
```

---

## Custom Resources

```csharp
[GlobalClass, Icon("res://icons/enemy.svg")]
public partial class EnemyData : Resource
{
    [Export] public string EnemyName { get; set; } = "Enemy";
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float MoveSpeed { get; set; } = 100f;
    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public Texture2D Sprite { get; set; }
    [Export] public PackedScene DeathEffect { get; set; }
}

// Usage
public partial class Enemy : CharacterBody2D
{
    [Export] public EnemyData Data { get; set; }

    public override void _Ready()
    {
        GetNode<HealthComponent>("HealthComponent").MaxHealth = Data.MaxHealth;
    }
}

// Save programmatically
var data = new EnemyData { EnemyName = "Goblin", MaxHealth = 50 };
ResourceSaver.Save(data, "res://resources/data/goblin_data.tres");
```

---

## Physics and Collision

### CharacterBody2D Platformer

```csharp
public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 200f;
    [Export] public float JumpForce { get; set; } = -400f;
    [Export] public float Gravity { get; set; } = 800f;

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpForce;

        float direction = Input.GetAxis("move_left", "move_right");
        velocity.X = direction * Speed;

        Velocity = velocity;
        MoveAndSlide();
    }
}
```

### Top-Down Movement

```csharp
public partial class TopDownPlayer : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 200f;
    [Export] public float Acceleration { get; set; } = 1500f;
    [Export] public float Friction { get; set; } = 1200f;

    public override void _PhysicsProcess(double delta)
    {
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (input != Vector2.Zero)
            Velocity = Velocity.MoveToward(input * Speed, Acceleration * (float)delta);
        else
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

        MoveAndSlide();
    }
}
```

### Area2D Detection

```csharp
public partial class DetectionZone : Area2D
{
    [Signal] public delegate void PlayerEnteredEventHandler(Player player);
    [Signal] public delegate void PlayerExitedEventHandler(Player player);

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
            EmitSignal(SignalName.PlayerEntered, player);
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
            EmitSignal(SignalName.PlayerExited, player);
    }
}
```

---

## Timer Patterns

```csharp
// Using Timer node
private Timer _cooldownTimer;
private bool _canFire = true;

public override void _Ready()
{
    _cooldownTimer = GetNode<Timer>("CooldownTimer");
    _cooldownTimer.WaitTime = Cooldown;
    _cooldownTimer.OneShot = true;
    _cooldownTimer.Timeout += () => _canFire = true;
}

// Using SceneTree timer (no node needed)
public async void DelayedExplosion()
{
    await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
    Explode();
}
```

---

## Common Patterns

### Scene Management
```csharp
GetTree().ChangeSceneToFile("res://scenes/levels/Level2.tscn");
GetTree().ChangeSceneToPacked(nextLevel);
GetTree().ReloadCurrentScene();
```

### Pause System
```csharp
GetTree().Paused = true;
ProcessMode = ProcessModeEnum.Always;  // Immune to pause
```

### Deferred Calls
```csharp
CallDeferred(MethodName.DoSomething);
Callable.From(() => AddChild(newNode)).CallDeferred();
```

### Configuration Warnings
```csharp
public string[] _GetConfigurationWarnings()
{
    if (EnemyScn == null)
        return ["Must initialize property 'EnemyScn'."];
    return [];
}
```

---

## String Methods Reference

Key mappings for common string operations:

| GDScript | C# |
|---|---|
| `begins_with` | `string.StartsWith` |
| `ends_with` | `string.EndsWith` |
| `contains` | `string.Contains` |
| `find` | `string.IndexOf` or `StringExtensions.Find` |
| `replace` | `string.Replace` |
| `split` | `string.Split` or `StringExtensions.Split` |
| `strip_edges` | `string.Trim` |
| `to_lower` | `string.ToLower` |
| `to_upper` | `string.ToUpper` |
| `length` | `string.Length` |
| `format` | `$"string {interpolation}"` |
| `is_empty` | `string.IsNullOrEmpty` |
| `join` | `string.Join` |
| `lpad` | `string.PadLeft` |
| `rpad` | `string.PadRight` |

Note: C# strings are UTF-16, Godot Strings are UTF-32.
`System.IO.Path` methods only work with native OS paths, not `res://` or `user://` paths.
Use `StringExtensions` methods (e.g., `GetBaseDir()`, `GetExtension()`, `PathJoin()`) for Godot paths.

---

## NuGet Packages

Add to `.csproj`:
```xml
<ItemGroup>
    <PackageReference Include="PackageName" Version="X.Y.Z" />
</ItemGroup>
```
Godot auto-downloads on next build.

---

## Official C# Style Guide (from Godot docs)

These conventions are followed by Godot engine developers and contributors. Godot currently uses **C# version 12.0** (supported by .NET 8.0). Do not use C# 13.0+ features until migration occurs.

### Formatting Rules

- **Line endings**: LF (line feed), not CRLF or CR
- **Encoding**: UTF-8 without byte order mark
- **Indentation**: 4 spaces, not tabs
- **Line length**: Consider breaking lines longer than 100 characters
- **Brace style**: Allman style (braces on new lines)

```csharp
// Correct: Allman style
if (x > 0)
{
    DoSomething();
}

// Wrong: K&R style
if (x > 0) {
    DoSomething();
}
```

**Omit braces on new lines for:**
- Simple property accessors: `public int Value { get; set; }`
- Simple object/array/collection initializers
- Abstract auto properties, indexers, or events

**Blank line rules:**
- Insert blank lines: after `using` statements, between methods/properties/inner types, at end of file
- Do NOT insert blank lines: after `{`, before `}`, after comments, adjacent to other blank lines

```csharp
using System;
using Godot;
                                          // Blank line after using list.
public class MyClass
{                                         // No blank line after {.
    public enum MyEnum
    {
        Value,
        AnotherValue                      // No blank line before }.
    }
                                          // Blank line around inner types.
    public const int SomeConstant = 1;
    public const int AnotherConstant = 2;

    private Vector3 _x;                  // Related fields can be grouped.
    private Vector3 _y;

    private float _width;
    private float _height;

    public int MyProperty { get; set; }
                                          // Blank line around properties.
    public void MyMethod()
    {
        // Some comment.
        AnotherMethod();                  // No blank line after a comment.
    }
                                          // Blank line around methods.
    public void AnotherMethod()
    {
    }
}
```

### Spacing Rules

**Insert spaces:**
- Around binary and ternary operators
- Between `if`/`for`/`foreach`/`catch`/`while`/`lock`/`using` and opening parenthesis
- Before and within single-line accessor blocks
- After commas (not at line end), after semicolons in `for`
- Around colons in type declarations, around lambda arrows
- After `//` in comments
- After opening / before closing braces in single-line initializers

**Do NOT insert spaces:** after type cast parentheses

```csharp
public class MyClass<A, B> : Parent<A, B>
{
    public float MyProperty { get; set; }

    public float AnotherProperty
    {
        get { return MyProperty; }
    }

    public void MyMethod()
    {
        int[] values = { 1, 2, 3, 4 };  // Spaces in initializer
        int sum = 0;

        for (int i = 0; i < values.Length; i++)
        {
            switch (i)
            {
                case 3: return;
                default:
                    sum += i > 2 ? 0 : 1;  // Spaces around ternary
                    break;
            }
        }

        i += (int)MyProperty;  // No space after type cast
    }
}
```

### Naming Conventions (Official Style)

**PascalCase** for: namespaces, type names, methods, properties, constants, events, and all member-level identifiers EXCEPT private fields.

**camelCase** for: local variables and method arguments.

**_camelCase** (underscore prefix) for: private fields.

```csharp
namespace ExampleProject
{
    public class PlayerCharacter
    {
        public const float DefaultSpeed = 10f;
        public float CurrentSpeed { get; set; }
        protected int HitPoints;

        private Vector3 _aimingAt;  // Underscore prefix for private fields

        private void CalculateWeaponDamage() { }

        private void Attack(float attackStrength)
        {
            Enemy targetFound = FindTarget(_aimingAt);
            targetFound?.Hit(attackStrength);
        }
    }
}
```

**Acronym rules:**
- Two-letter acronyms like "UI" stay uppercase in PascalCase: `UIManager`
- "id" is NOT an acronym, follows normal casing: `Id` (PascalCase), `id` (camelCase)
- Interfaces get uppercase "I" prefix: `IInventoryHolder`, `IDamageable`
- Do NOT use Hungarian notation (`strText`, `fPower`) -- exception: interface "I" prefix

**Prefer descriptive names:**
```csharp
// Good
FindNearbyEnemy()?.Damage(weaponDamage);

// Bad
FindNode()?.Change(wpnDmg);
```

### Member Variable and Local Variable Rules

- Do NOT declare member variables for local-only use; declare as local variables instead
- Declare local variables as close as possible to first use
- Use `var` only when the type is evident from the right side:

```csharp
// Good: type is evident
var direction = new Vector2(1, 0);
var value = (int)speed;
var text = "Some value";
for (var i = 0; i < 10; i++) { }

// Bad: type is unclear or numeric ambiguity
var value = GetValue();        // What type?
var velocity = direction * 1.5; // real_t alias issues
var value = 1.5;               // double? float?
```

### Other Style Conventions

- Use **explicit access modifiers** always
- Use **properties** instead of non-private fields
- Apply modifiers in this order: `public`/`protected`/`private`/`internal`/`virtual`/`override`/`abstract`/`new`/`static`/`readonly`
- Avoid fully-qualified names or `this.` prefix when unnecessary
- Remove unused `using` statements and parentheses
- Consider omitting default initial values (e.g., `= 0`, `= null`, `= false`)
- Use null-conditional operators for compact code: `target?.Hit(damage)`
- Use safe cast (`as`) when type is uncertain; direct cast when certain

---

## Known Gotchas (Godot 4.6)

- Web platform export not supported for C# projects
- Android/iOS support is experimental (since 4.2)
- Editor plugins in C# are possible but convoluted
- State is NOT saved/restored on hot-reload (except exports)
- Class name must match file name
- `Get()`/`Set()`/`Call()`/`Connect()` use snake_case API names internally; use `PropertyName`/`MethodName`/`SignalName` nested classes instead
- Writing editor plugins requires tool mode
