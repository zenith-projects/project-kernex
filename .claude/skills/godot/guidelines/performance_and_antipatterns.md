# Performance & Anti-Patterns Guide

How to write performant Godot 4 C# code and avoid common mistakes.

---

## C# Garbage Collection in Godot

The biggest performance concern with C# in Godot is the garbage collector (GC). GC pauses can cause frame drops.

### The Problem
```csharp
// BAD: Creates garbage every frame
public override void _Process(double delta)
{
    var enemies = GetTree().GetNodesInGroup("enemies"); // New array every frame
    var text = $"Score: {score}";                        // New string every frame
    var direction = new Vector2(1, 0).Rotated(angle);    // Struct, OK actually
}
```

### The Solution: Minimize Allocations

```csharp
// GOOD: Cache and reuse
private Godot.Collections.Array<Node> _enemiesCache;
private Label _scoreLabel;

public override void _Ready()
{
    _scoreLabel = GetNode<Label>("ScoreLabel");
}

public override void _Process(double delta)
{
    // Only query when needed, not every frame
    // Vector2 is a struct — no GC pressure
}

public void UpdateScore(int score)
{
    // Only update text when score changes
    _scoreLabel.Text = $"Score: {score}";
}
```

### Object Pooling Pattern

Essential for frequently spawned/despawned objects (bullets, particles, enemies):

```csharp
public partial class ObjectPool<T> : Node where T : Node, new()
{
    [Export] public PackedScene Scene { get; set; }
    [Export] public int InitialSize { get; set; } = 20;

    private Queue<T> _pool = new();

    public override void _Ready()
    {
        for (int i = 0; i < InitialSize; i++)
        {
            var obj = Scene.Instantiate<T>();
            obj.ProcessMode = ProcessModeEnum.Disabled;
            obj.Visible = false;
            AddChild(obj);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Scene.Instantiate<T>();
            AddChild(obj);
        }
        obj.ProcessMode = ProcessModeEnum.Inherit;
        obj.Visible = true;
        return obj;
    }

    public void Return(T obj)
    {
        obj.ProcessMode = ProcessModeEnum.Disabled;
        obj.Visible = false;
        _pool.Enqueue(obj);
    }
}
```

**Usage:**
```csharp
// In BulletManager or weapon script
private ObjectPool<Bullet> _bulletPool;

public override void _Ready()
{
    _bulletPool = GetNode<ObjectPool<Bullet>>("BulletPool");
}

public void Fire(Vector2 pos, float rotation)
{
    var bullet = _bulletPool.Get();
    bullet.GlobalPosition = pos;
    bullet.Rotation = rotation;
    bullet.Initialize(); // Reset bullet state
}

// In Bullet.cs
public void OnLifetimeExpired()
{
    GetParent<ObjectPool<Bullet>>().Return(this); // Return, don't QueueFree
}
```

---

## Performance Best Practices

### 1. Use _PhysicsProcess Only for Physics

```csharp
// GOOD: Visual in _Process, physics in _PhysicsProcess
public override void _Process(double delta)
{
    UpdateAnimation();       // Visual, every frame
    UpdateUI();              // Visual, every frame
}

public override void _PhysicsProcess(double delta)
{
    ApplyMovement(delta);    // Physics, fixed timestep
    MoveAndSlide();          // Physics
}
```

### 2. Disable Processing When Not Needed

```csharp
public override void _Ready()
{
    SetProcess(false);        // Disable _Process
    SetPhysicsProcess(false); // Disable _PhysicsProcess
}

public void Activate()
{
    SetProcess(true);
    SetPhysicsProcess(true);
}

public void Deactivate()
{
    SetProcess(false);
    SetPhysicsProcess(false);
}
```

### 3. Use Visibility Notifiers

```csharp
// Only process when visible on screen
public partial class Enemy : CharacterBody2D
{
    public override void _Ready()
    {
        var notifier = GetNode<VisibleOnScreenNotifier2D>("Notifier");
        notifier.ScreenEntered += () => SetPhysicsProcess(true);
        notifier.ScreenExited += () => SetPhysicsProcess(false);
    }
}
```

### 4. Batch Operations

```csharp
// BAD: Modifying tree in a loop triggers layout recalculation each time
foreach (var item in items)
{
    var node = CreateItemNode(item);
    container.AddChild(node);
}

// GOOD: Defer additions
foreach (var item in items)
{
    var node = CreateItemNode(item);
    container.CallDeferred("add_child", node);
}

// BEST: Add to hidden container, then show
container.Visible = false;
foreach (var item in items)
{
    container.AddChild(CreateItemNode(item));
}
container.Visible = true;
```

### 5. Use Typed Queries

```csharp
// BAD: Untyped, requires casting
var children = GetChildren();
foreach (Node child in children)
{
    if (child is Enemy enemy)
        enemy.TakeDamage(10);
}

// GOOD: Use groups for targeted queries
GetTree().CallGroup("enemies", "TakeDamage", 10);

// GOOD: Cache results when possible
private List<Enemy> _activeEnemies = new();
```

### 6. Avoid String Operations in Hot Paths

```csharp
// BAD: String concatenation every frame
public override void _Process(double delta)
{
    debugLabel.Text = "Pos: " + Position.ToString() + " Vel: " + Velocity.ToString();
}

// GOOD: Only update when changed, use StringName for repeated lookups
private StringName _moveLeftAction = "move_left";
private Vector2 _lastPosition;

public override void _Process(double delta)
{
    if (Position != _lastPosition)
    {
        debugLabel.Text = $"Pos: {Position}";
        _lastPosition = Position;
    }
}
```

### 7. Signal vs Polling

```csharp
// BAD: Polling every frame
public override void _Process(double delta)
{
    if (health.CurrentHealth <= 0)
        Die();
}

// GOOD: React to signals
public override void _Ready()
{
    _health.Died += OnDied;
}
```

---

## Common Anti-Patterns

### 1. God Node / Manager Bloat

**Problem:** One massive script that handles everything.

```csharp
// BAD
public partial class GameManager : Node
{
    // Handles: input, audio, UI, physics, AI, saves, networking, inventory...
    // 3000+ lines
}
```

**Fix:** Split into focused systems:
```
Systems (Node)
├── AudioManager
├── SaveManager
├── CombatSystem
├── InventorySystem
└── QuestSystem
```

### 2. Reaching Up the Tree

**Problem:** Child nodes depend on parent/sibling structure.

```csharp
// BAD: Fragile, breaks if hierarchy changes
var player = GetNode("../../Player");
var healthBar = GetNode("/root/Main/UI/HUD/HealthBar");
```

**Fix:** Use signals, exports, or groups:
```csharp
[Signal] public delegate void DamageTakenEventHandler(int amount);
// Parent connects to this signal

// Or use groups
var players = GetTree().GetNodesInGroup("player");
```

### 3. Autoload Overuse

**Problem:** Everything is a singleton.

```csharp
// BAD: 15+ autoloads
// GameManager, AudioManager, UIManager, InputManager, CombatManager,
// InventoryManager, QuestManager, DialogueManager, ...
```

**Fix:** Only 2-4 autoloads for truly global state. Everything else is a component or scene-local system.

### 4. Not Setting Owner for Programmatic Scenes

**Problem:** Dynamically created nodes don't save.

```csharp
// BAD: Child won't be saved in the scene
var child = new Node2D();
root.AddChild(child);

// GOOD: Set owner for scene saving
var child = new Node2D();
root.AddChild(child);
child.Owner = root;
```

### 5. Circular Dependencies

**Problem:** Two scenes depend on each other.

```csharp
// BAD: Player and Enemy directly reference each other
public partial class Player : CharacterBody2D
{
    public Enemy CurrentTarget; // Direct dependency
}

public partial class Enemy : CharacterBody2D
{
    public Player TargetPlayer; // Circular!
}
```

**Fix:** Use signals, groups, or a mediator:
```csharp
// Enemy emits signal when it spots player
[Signal] public delegate void PlayerSpottedEventHandler(Node2D player);

// Common parent or system handles the interaction
```

### 6. Magic Numbers

**Problem:** Hard-coded values scattered through code.

```csharp
// BAD
Velocity = direction * 200f;
if (health < 30)
    FlashRed();
```

**Fix:** Use exports or constants:
```csharp
[Export] public float Speed { get; set; } = 200f;
[Export] public int LowHealthThreshold { get; set; } = 30;
```

### 7. Mixing Concerns

**Problem:** One script handles rendering, physics, audio, and UI.

```csharp
// BAD: Player.cs handles everything
public partial class Player : CharacterBody2D
{
    // Movement code (200 lines)
    // Combat code (150 lines)
    // Animation code (100 lines)
    // Audio code (80 lines)
    // UI interaction code (100 lines)
}
```

**Fix:** Extract into components:
```csharp
// Player.cs — only orchestration (50 lines)
// MovementComponent.cs — movement (60 lines)
// CombatComponent.cs — combat (80 lines)
// Animation driven by AnimationTree
// Audio driven by AudioStreamPlayer nodes
```

### 8. Deep Inheritance Chains

**Problem:** Fragile, hard to modify base classes.

```
Node → Entity → LivingEntity → MovableEntity → DamageableEntity → Player
```

**Fix:** Flat hierarchy with composition:
```
CharacterBody2D (Player)
├── HealthComponent
├── MovementComponent
└── CombatComponent
```

### 9. Instantiating in _Process

**Problem:** Spawning objects every frame.

```csharp
// BAD
public override void _Process(double delta)
{
    var particle = particleScene.Instantiate(); // Every frame!
    AddChild(particle);
}
```

**Fix:** Use timers, signals, or object pools.

### 10. Not Using Deferred Calls for Tree Modifications

**Problem:** Modifying the scene tree during physics processing.

```csharp
// BAD: Can crash during physics step
private void OnBodyEntered(Node2D body)
{
    body.QueueFree(); // Generally OK (deferred already)
    AddChild(newNode); // Potentially dangerous
}

// GOOD: Explicitly defer
private void OnBodyEntered(Node2D body)
{
    body.QueueFree();
    CallDeferred("add_child", newNode);
}
```

---

## Profiling Tips

1. **Use Godot's built-in profiler**: Debugger → Profiler tab
2. **Monitor frame time**: `Engine.GetFramesPerSecond()`
3. **Check physics**: Debugger → Monitors → Physics
4. **Visual profiler**: Viewport → Debug Draw settings
5. **C# specific**: Check GC collections in Monitors
6. **Print timing**:
```csharp
var start = Time.GetTicksMsec();
ExpensiveOperation();
GD.Print($"Operation took: {Time.GetTicksMsec() - start}ms");
```

---

## Multithreading in Godot (from official docs)

Threads allow simultaneous execution of code, off-loading work from the main thread.

> **Note**: If using C#, it may be easier to use the threading classes C# supports natively.

### Creating Threads (C#)

```csharp
private GodotThread _thread;

public override void _Ready()
{
    _thread = new GodotThread();
    _thread.Start(Callable.From(() => ThreadFunction("Wafflecopter")));
}

private void ThreadFunction(string userdata)
{
    GD.Print($"I'm a thread! Userdata is: {userdata}");
}

public override void _ExitTree()
{
    _thread.WaitToFinish();
}
```

> **Better approach for C#**: Use `System.Threading.Tasks.Task.Run()` with `CallDeferred()` to post results back to the main thread. Godot's Thread class is GDScript-oriented.

**Critical rules:**
- Threads MUST be disposed (joined) via `wait_to_finish()` for portability
- **Creating threads is SLOW, especially on Windows** -- create threads during loading, NOT just-in-time
- Before using a built-in class in a thread, check the Thread Safe APIs documentation

### Mutexes -- Protecting Shared Data

Always use a Mutex when accessing data from different threads. Without it, you get synchronization problems (data not always updated between CPU cores when modified).

When a thread calls `lock()`, all other threads that try to lock the same mutex are blocked (suspended) until the first thread calls `unlock()`.

```csharp
private int _counter;
private Mutex _mutex;
private GodotThread _thread;

public override void _Ready()
{
    _mutex = new Mutex();
    _thread = new GodotThread();
    _thread.Start(Callable.From(ThreadFunction));

    _mutex.Lock();
    _counter += 1;
    _mutex.Unlock();
}

private void ThreadFunction()
{
    _mutex.Lock();
    _counter += 1;
    _mutex.Unlock();
}

public override void _ExitTree()
{
    _thread.WaitToFinish();
    GD.Print($"Counter is: {_counter}"); // Will be 2
}
```

> **C# alternative**: Use `lock` keyword with `System.Object` for simpler syntax:
> ```csharp
> private readonly object _lock = new();
> lock (_lock) { _counter++; }
> ```

**Mutex warnings:**
- Locking/unlocking mutexes is expensive -- avoid locking too often or for too long
- Never access shared data from different threads without a Mutex

### Semaphores -- On-Demand Thread Execution

Semaphores let threads work "on demand". A thread calls `wait()` to suspend, and another thread calls `post()` to wake it.

```csharp
private int _counter;
private Mutex _mutex;
private Semaphore _semaphore;
private GodotThread _thread;
private bool _exitThread;

public override void _Ready()
{
    _mutex = new Mutex();
    _semaphore = new Semaphore();
    _exitThread = false;
    _thread = new GodotThread();
    _thread.Start(Callable.From(ThreadFunction));
}

private void ThreadFunction()
{
    while (true)
    {
        _semaphore.Wait(); // Suspend until Post() is called

        _mutex.Lock();
        bool shouldExit = _exitThread;
        _mutex.Unlock();

        if (shouldExit) break;

        _mutex.Lock();
        _counter++;
        _mutex.Unlock();
    }
}

public void IncrementCounter() => _semaphore.Post();

public int GetCounter()
{
    _mutex.Lock();
    int value = _counter;
    _mutex.Unlock();
    return value;
}

public override void _ExitTree()
{
    _mutex.Lock();
    _exitThread = true;
    _mutex.Unlock();

    _semaphore.Post(); // Unblock thread so it can exit
    _thread.WaitToFinish();
    GD.Print($"Counter is: {_counter}");
}
```

> **C# native alternative** (preferred): Use `SemaphoreSlim` + `Task.Run()`:
> ```csharp
> private readonly SemaphoreSlim _sem = new(0);
> private readonly CancellationTokenSource _cts = new();
> Task.Run(async () => { await _sem.WaitAsync(_cts.Token); /* work */ });
> ```

### Thread Safety Rules Summary

1. Accessing objects/data from multiple threads causes unexpected behaviors or crashes if not supported
2. Always use a Mutex when accessing shared data from different threads
3. Create threads during loading phases, not just-in-time (especially on Windows)
4. Always call `wait_to_finish()` to properly dispose threads
5. Check Thread Safe APIs docs before using engine classes in threads
6. Mutex locking is expensive -- minimize lock scope and frequency

---

## Performance Documentation Index (from official Godot docs)

### Available Performance Topics

**Common:**
- General optimization
- Using servers (direct server API access for performance)

**CPU:**
- CPU optimization

**GPU:**
- GPU optimization
- Using MultiMesh (batched rendering for many instances)
- Pipeline compilations

**3D:**
- Optimizing 3D performance
- Vertex animation

**Threads:**
- Using multiple threads (covered above)
- Thread safe APIs
