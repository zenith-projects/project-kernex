# GDScript to C# Conversion Cheat Sheet

Quick reference for converting GDScript code from tutorials/forums to Godot C#.

---

## Syntax Mapping

| GDScript | C# |
|---|---|
| `extends Node` | `public partial class MyClass : Node` |
| `func _ready():` | `public override void _Ready()` |
| `func _process(delta):` | `public override void _Process(double delta)` |
| `func _physics_process(delta):` | `public override void _PhysicsProcess(double delta)` |
| `func _input(event):` | `public override void _Input(InputEvent @event)` |
| `func _unhandled_input(event):` | `public override void _UnhandledInput(InputEvent @event)` |
| `func _enter_tree():` | `public override void _EnterTree()` |
| `func _exit_tree():` | `public override void _ExitTree()` |
| `func my_func(a: int) -> String:` | `public string MyFunc(int a)` |
| `var x = 5` | `var x = 5;` or `int x = 5;` |
| `var x: float = 5.0` | `float x = 5.0f;` |
| `const MAX = 100` | `private const int Max = 100;` |
| `pass` | `{ }` (empty block) |

---

## Variables & Properties

| GDScript | C# |
|---|---|
| `@export var speed = 100` | `[Export] public int Speed { get; set; } = 100;` |
| `@export var speed: float = 100.0` | `[Export] public float Speed { get; set; } = 100f;` |
| `@export_range(0, 100) var hp` | `[Export(PropertyHint.Range, "0,100")] public int Hp { get; set; }` |
| `@export_file("*.tscn") var path` | `[Export(PropertyHint.File, "*.tscn")] public string Path { get; set; }` |
| `@export_enum("A", "B") var x` | `[Export(PropertyHint.Enum, "A,B")] public int X { get; set; }` |
| `@export_group("Combat")` | `[ExportGroup("Combat")]` |
| `@export_subgroup("Melee")` | `[ExportSubgroup("Melee")]` |
| `@onready var sprite = $Sprite2D` | Cache in `_Ready()`: `_sprite = GetNode<Sprite2D>("Sprite2D");` |

---

## Node Access

| GDScript | C# |
|---|---|
| `$Sprite2D` | `GetNode<Sprite2D>("Sprite2D")` |
| `$"Arm/Hand/Weapon"` | `GetNode<Node2D>("Arm/Hand/Weapon")` |
| `%UniqueNode` | `GetNode<Node>("%UniqueNode")` |
| `get_node("../Sibling")` | `GetNode<Node>("../Sibling")` |
| `get_parent()` | `GetParent()` or `GetParent<SpecificType>()` |
| `get_children()` | `GetChildren()` |
| `get_tree()` | `GetTree()` |
| `get_tree().current_scene` | `GetTree().CurrentScene` |
| `owner` | `Owner` |

---

## Signals

| GDScript | C# |
|---|---|
| `signal my_signal` | `[Signal] public delegate void MySignalEventHandler();` |
| `signal damaged(amount: int)` | `[Signal] public delegate void DamagedEventHandler(int amount);` |
| `my_signal.emit()` | `EmitSignal(SignalName.MySignal);` |
| `damaged.emit(10)` | `EmitSignal(SignalName.Damaged, 10);` |
| `node.my_signal.connect(func)` | `node.MySignal += OnMySignal;` |
| `node.my_signal.disconnect(func)` | `node.MySignal -= OnMySignal;` |
| `await signal` | `await ToSignal(obj, ClassName.SignalName.Signal);` |
| `await get_tree().create_timer(1.0).timeout` | `await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);` |

---

## Resources & Scenes

| GDScript | C# |
|---|---|
| `preload("res://scene.tscn")` | `GD.Load<PackedScene>("res://scene.tscn")` (no preload in C#) |
| `load("res://scene.tscn")` | `ResourceLoader.Load<PackedScene>("res://scene.tscn")` |
| `scene.instantiate()` | `scene.Instantiate()` or `scene.Instantiate<MyType>()` |
| `add_child(node)` | `AddChild(node)` |
| `remove_child(node)` | `RemoveChild(node)` |
| `queue_free()` | `QueueFree()` |
| `node.reparent(new_parent)` | `node.Reparent(newParent)` |

---

## Input

| GDScript | C# |
|---|---|
| `Input.is_action_pressed("jump")` | `Input.IsActionPressed("jump")` |
| `Input.is_action_just_pressed("jump")` | `Input.IsActionJustPressed("jump")` |
| `Input.is_action_just_released("jump")` | `Input.IsActionJustReleased("jump")` |
| `Input.get_axis("left", "right")` | `Input.GetAxis("left", "right")` |
| `Input.get_vector("l","r","u","d")` | `Input.GetVector("l","r","u","d")` |
| `Input.get_action_strength("accel")` | `Input.GetActionStrength("accel")` |
| `event is InputEventKey` | `@event is InputEventKey keyEvent` |
| `event.keycode == KEY_ESCAPE` | `keyEvent.Keycode == Key.Escape` |
| `event is InputEventMouseButton` | `@event is InputEventMouseButton mb` |

---

## Control Flow

| GDScript | C# |
|---|---|
| `if x:` | `if (x)` |
| `elif x:` | `else if (x)` |
| `for i in range(10):` | `for (int i = 0; i < 10; i++)` |
| `for item in array:` | `foreach (var item in array)` |
| `while x:` | `while (x)` |
| `match x:` | `switch (x)` |
| `x if cond else y` | `cond ? x : y` |
| `x is Type` | `x is Type` |
| `x as Type` | `x as Type` |

---

## Types & Casting

| GDScript | C# |
|---|---|
| `int(x)` | `(int)x` or `Mathf.RoundToInt(x)` |
| `float(x)` | `(float)x` |
| `str(x)` | `x.ToString()` or `$"{x}"` |
| `typeof(x)` | `x.GetType()` (C# type) or `variant.VariantType` (Godot type) |
| `x is Node2D` | `x is Node2D` |
| `x as Node2D` | `x as Node2D` |

---

## Common Built-ins

| GDScript | C# |
|---|---|
| `print(x)` | `GD.Print(x)` |
| `printerr(x)` | `GD.PrintErr(x)` |
| `push_error(x)` | `GD.PushError(x)` |
| `push_warning(x)` | `GD.PushWarning(x)` |
| `abs(x)` | `Mathf.Abs(x)` |
| `clamp(x, min, max)` | `Mathf.Clamp(x, min, max)` |
| `lerp(a, b, t)` | `Mathf.Lerp(a, b, t)` |
| `move_toward(a, b, d)` | `Mathf.MoveToward(a, b, d)` |
| `deg_to_rad(x)` | `Mathf.DegToRad(x)` |
| `rad_to_deg(x)` | `Mathf.RadToDeg(x)` |
| `randf()` | `GD.Randf()` |
| `randi()` | `GD.Randi()` |
| `randf_range(a, b)` | `(float)GD.RandRange(a, b)` |
| `randomize()` | `GD.Randomize()` |
| `snapped(x, step)` | `Mathf.Snapped(x, step)` |
| `PI` | `Mathf.Pi` |
| `TAU` | `Mathf.Tau` |
| `INF` | `Mathf.Inf` |
| `NAN` | `float.NaN` |

---

## Collections

| GDScript | C# |
|---|---|
| `var arr = [1, 2, 3]` | `var arr = new Godot.Collections.Array<int> { 1, 2, 3 };` |
| `var dict = {"a": 1}` | `var dict = new Godot.Collections.Dictionary<string, int> { { "a", 1 } };` |
| `arr.append(x)` | `arr.Add(x)` |
| `arr.size()` | `arr.Count` |
| `arr.remove_at(i)` | `arr.RemoveAt(i)` |
| `arr.has(x)` | `arr.Contains(x)` |
| `arr.find(x)` | `arr.IndexOf(x)` |
| `dict.has(key)` | `dict.ContainsKey(key)` |
| `dict.keys()` | `dict.Keys` |
| `dict.values()` | `dict.Values` |
| `dict.erase(key)` | `dict.Remove(key)` |
| `PackedStringArray` | `string[]` |
| `PackedVector2Array` | `Vector2[]` |

---

## Groups

| GDScript | C# |
|---|---|
| `add_to_group("enemies")` | `AddToGroup("enemies")` |
| `is_in_group("enemies")` | `IsInGroup("enemies")` |
| `get_tree().get_nodes_in_group("x")` | `GetTree().GetNodesInGroup("x")` |
| `get_tree().call_group("x", "m")` | `GetTree().CallGroup("x", "m")` |

---

## Scene Tree

| GDScript | C# |
|---|---|
| `get_tree().change_scene_to_file(path)` | `GetTree().ChangeSceneToFile(path)` |
| `get_tree().reload_current_scene()` | `GetTree().ReloadCurrentScene()` |
| `get_tree().quit()` | `GetTree().Quit()` |
| `get_tree().paused = true` | `GetTree().Paused = true` |
| `get_tree().create_timer(1.0)` | `GetTree().CreateTimer(1.0)` |
| `get_tree().create_tween()` | `GetTree().CreateTween()` or `CreateTween()` |

---

## Struct Gotcha

```csharp
// GDScript: position.x = 100 (works)
// C#: Position.X = 100; // ERROR! Structs are copied

// Fix:
Position = Position with { X = 100f };
// or:
var pos = Position;
pos.X = 100f;
Position = pos;
```

---

## String Naming

| GDScript | C# | When to use |
|---|---|---|
| `snake_case` | `PascalCase` | Methods, properties, signals |
| `_private_var` | `_camelCase` | Private fields |
| `local_var` | `camelCase` | Local variables |
| `CONSTANT` | `PascalCase` | Constants |
| `signal_name` | `SignalNameEventHandler` | Signal delegates |

**Internal API strings** (`Get()`, `Set()`, `Call()`, `Connect()`) still use **snake_case**. Use `PropertyName`, `MethodName`, `SignalName` nested classes to avoid this.
