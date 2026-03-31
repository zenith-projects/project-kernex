# Scene Composition Guidelines

Sourced from official Godot 4.6 documentation and community best practices.

---

## Core Official Principle: "Design Scenes With No Dependencies"

From the official Godot docs: **"If at all possible, you should design scenes to have no dependencies."** Scenes should keep everything they need within themselves. When they must interact with external context, use **Dependency Injection** -- have the parent/high-level code provide the dependencies.

### The 5 Official Dependency Injection Methods

When a scene MUST interact with its environment, use these patterns (from official docs):

**1. Connect to a signal** (respond to behavior):
```csharp
// Parent wires child's signal
GetNode<HealthComponent>("HealthComponent").Died += OnDied;
// Child emits signal without knowing who listens
EmitSignal(SignalName.Died);
```

**2. Call a method** (start behavior):
```csharp
// Parent tells child what method to call
child.Set("MethodName", "Execute");
// Child calls it without knowing the parent
Call(MethodName);
```

**3. Initialize a Callable property** (flexible delegation):
```csharp
// Parent sets a callable on the child
child.OnAction = Callable.From(parentObject.HandleAction);
// Child invokes it without knowing the source
OnAction.Call();
```

**4. Initialize a Node reference** (direct but loose):
```csharp
// Parent provides a reference
child.Target = this;
// Child uses it without hard-coding paths
GD.Print(Target);
```

**5. Initialize a NodePath** (deferred resolution):
```csharp
// Parent provides a path
child.TargetPath = new NodePath("..");
// Child resolves it at runtime
GetNode(TargetPath);
```

### Self-Documenting Scenes

Use `_GetConfigurationWarnings()` to show editor warnings when dependencies are missing:
```csharp
public string[] _GetConfigurationWarnings()
{
    if (TargetScene == null)
        return ["Must set 'TargetScene' property."];
    return [];
}
```
This creates the same yellow warning icon as built-in nodes (e.g., Area2D without CollisionShape).

### Key Official Rules

- **Siblings should NOT know about each other** -- an ancestor mediates their communication
- **Signal names should be past-tense verbs**: "entered", "skill_activated", "item_collected"
- **Signals respond to behavior, method calls start behavior**
- **Follow SOLID, DRY, KISS, YAGNI principles**
- **Consider the SceneTree in relational terms, not spatial terms** -- a child should only be a child if removing the parent means removing the child too
- **Godot's nodes are NOT components** (official docs) -- they use aggregation, not ECS. But composition patterns work excellently with scenes-as-components.

---

## Fundamental Principle: Composition Over Inheritance

Godot's node system naturally supports composition. Instead of creating deep class hierarchies, build complex game objects by assembling smaller, self-contained scenes.

### Bad: Deep Inheritance
```
Entity (base class)
  └── LivingEntity
       └── MovableEntity
            └── DamageableEntity
                 └── Player
```

### Good: Composition
```
Player (CharacterBody2D)
├── HealthComponent.tscn      ← handles health, damage, death
├── MovementComponent.tscn    ← handles velocity, acceleration
├── HitboxComponent.tscn      ← deals damage on contact
├── HurtboxComponent.tscn     ← receives damage on contact
├── Sprite2D                  ← visuals
├── CollisionShape2D          ← physics collision
└── AnimationPlayer           ← animations
```

Now `HealthComponent` can be reused on Player, Enemy, Destructible Crate, Boss — any entity that has health.

---

## The Golden Rule: "Call Down, Signal Up"

This is the most important communication pattern in Godot:

```
Parent ──[method call]──→ Child        ✅ OK: parent calls child's method
Child  ──[signal]──────→ Parent        ✅ OK: child emits signal, parent listens
Sibling ←──[via parent]──→ Sibling     ✅ OK: parent mediates between siblings
Child  ──[GetNode("../../")]→ Parent   ❌ BAD: child reaches up the tree
```

### C# Implementation:

```csharp
// HealthComponent.cs — emits signals UP
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();

    [Export] public int MaxHealth { get; set; } = 100;
    private int _currentHealth;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
        if (_currentHealth <= 0)
            EmitSignal(SignalName.Died);
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
        EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
    }
}

// Player.cs — calls DOWN to children, listens to their signals
public partial class Player : CharacterBody2D
{
    private HealthComponent _health;
    private MovementComponent _movement;

    public override void _Ready()
    {
        _health = GetNode<HealthComponent>("HealthComponent");
        _movement = GetNode<MovementComponent>("MovementComponent");

        // Listen to child signals (signal UP)
        _health.Died += OnDied;
        _health.HealthChanged += OnHealthChanged;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Call DOWN to children
        var direction = Input.GetVector("left", "right", "up", "down");
        _movement.Move(direction);
    }

    private void OnDied()
    {
        GD.Print("Player died!");
        QueueFree();
    }

    private void OnHealthChanged(int current, int max)
    {
        GD.Print($"Health: {current}/{max}");
    }
}
```

---

## Scene Design Principles

### 1. Self-Contained Scenes
A scene should have NO external dependencies. Everything it needs should be inside it or passed via `[Export]` properties.

```csharp
// GOOD: Scene configurable via exports
public partial class Projectile : Area2D
{
    [Export] public float Speed { get; set; } = 400f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public float Lifetime { get; set; } = 3f;
}

// BAD: Scene depends on external node paths
public partial class Projectile : Area2D
{
    public override void _Ready()
    {
        var player = GetNode("/root/Main/Player"); // NEVER DO THIS
    }
}
```

### 2. One Responsibility Per Scene
Each scene should do ONE thing well.

| Scene | Responsibility |
|-------|---------------|
| `HealthComponent.tscn` | Track health, emit damage/death signals |
| `HitboxComponent.tscn` | Detect when it overlaps a hurtbox, deal damage |
| `HurtboxComponent.tscn` | Receive damage from hitboxes, forward to HealthComponent |
| `MovementComponent.tscn` | Apply velocity, handle acceleration/deceleration |
| `StateMachine.tscn` | Manage state transitions, delegate to current state |

### 3. Configure Via Exports, Not Hard-Coding

```csharp
// GOOD: All configurable
public partial class Enemy : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 100f;
    [Export] public float DetectionRadius { get; set; } = 200f;
    [Export] public PackedScene DeathEffect { get; set; }
    [Export] public AudioStream HitSound { get; set; }
}

// BAD: Hard-coded values buried in logic
public partial class Enemy : CharacterBody2D
{
    public override void _PhysicsProcess(double delta)
    {
        Velocity = direction * 100f; // Magic number
    }
}
```

---

## Composition Patterns

### Pattern 1: Component Scene

Create a scene that encapsulates a single behavior:

**HealthComponent.tscn:**
```ini
[gd_scene load_steps=2 format=3]

[ext_resource type="Script" path="res://scripts/components/HealthComponent.cs" id="1_script"]

[node name="HealthComponent" type="Node"]
script = ExtResource("1_script")
MaxHealth = 100
```

**Usage in Player.tscn:**
```ini
[ext_resource type="PackedScene" path="res://scenes/components/HealthComponent.tscn" id="health"]

[node name="Player" type="CharacterBody2D"]

[node name="HealthComponent" parent="." instance=ExtResource("health")]
MaxHealth = 150
```

### Pattern 2: Hitbox/Hurtbox System

This is the standard damage system in Godot games:

```
Hitbox (Area2D)              Hurtbox (Area2D)
├── CollisionShape2D         ├── CollisionShape2D
└── HitboxComponent.cs       └── HurtboxComponent.cs

Hitbox collision_layer = 4 (player projectiles)
Hurtbox collision_mask = 4 (detects player projectiles)
```

```csharp
// HitboxComponent.cs
public partial class HitboxComponent : Area2D
{
    [Export] public int Damage { get; set; } = 10;
}

// HurtboxComponent.cs
public partial class HurtboxComponent : Area2D
{
    [Signal] public delegate void HurtEventHandler(int damage);

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is HitboxComponent hitbox)
        {
            EmitSignal(SignalName.Hurt, hitbox.Damage);
        }
    }
}
```

### Pattern 3: State Machine

State machines are essential for characters with multiple behaviors:

```
StateMachine (Node)
├── IdleState (Node)
├── RunState (Node)
├── JumpState (Node)
└── AttackState (Node)
```

```csharp
// State.cs — base class for all states
public partial class State : Node
{
    [Signal] public delegate void TransitionedEventHandler(string newStateName);

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double delta) { }
    public virtual void PhysicsUpdate(double delta) { }
    public virtual void HandleInput(InputEvent @event) { }
}

// StateMachine.cs
public partial class StateMachine : Node
{
    [Export] public NodePath InitialStatePath { get; set; }
    private State _currentState;

    public override void _Ready()
    {
        _currentState = GetNode<State>(InitialStatePath);
        foreach (var child in GetChildren())
        {
            if (child is State state)
            {
                state.Transitioned += OnChildTransitioned;
            }
        }
        _currentState.Enter();
    }

    public override void _Process(double delta)
    {
        _currentState.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState.PhysicsUpdate(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _currentState.HandleInput(@event);
    }

    private void OnChildTransitioned(string newStateName)
    {
        var newState = GetNode<State>(newStateName);
        if (newState == null || newState == _currentState)
            return;

        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}
```

### Pattern 4: Scene Inheritance

Use scene inheritance for variants of the same entity:

1. Create `BaseEnemy.tscn` with shared nodes (Sprite, Collision, HealthComponent, etc.)
2. Create inherited scenes: `Goblin.tscn`, `Orc.tscn`, `Dragon.tscn`
3. Override specific properties (speed, health, sprite) in each variant

In the inherited scene TSCN:
```ini
[gd_scene load_steps=2 format=3]

[ext_resource type="PackedScene" path="res://scenes/characters/BaseEnemy.tscn" id="base"]

[node name="Goblin" instance=ExtResource("base")]
MoveSpeed = 120.0
MaxHealth = 50

[node name="Sprite2D" parent="." index="0"]
texture = ExtResource("goblin_texture")
```

### Pattern 5: Factory Pattern for Dynamic Spawning

```csharp
public partial class EnemySpawner : Node
{
    [Export] public PackedScene[] EnemyScenes { get; set; }
    [Export] public float SpawnInterval { get; set; } = 2f;

    private Timer _timer;
    private Random _rng = new();

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = SpawnInterval;
        _timer.Timeout += OnTimerTimeout;
        _timer.Start();
    }

    private void OnTimerTimeout()
    {
        if (EnemyScenes.Length == 0) return;

        var scene = EnemyScenes[_rng.Next(EnemyScenes.Length)];
        var enemy = scene.Instantiate<Node2D>();
        enemy.GlobalPosition = GetSpawnPosition();
        GetParent().AddChild(enemy);
    }

    private Vector2 GetSpawnPosition()
    {
        // Implement spawn logic
        return Vector2.Zero;
    }
}
```

---

## Scene Tree Organization

### Functional Hierarchy (Recommended)
Organize by functionality, not spatial grouping:

```
Game (Node)
├── World (Node2D)
│   ├── TileMapLayer
│   ├── NavigationRegion2D
│   └── Props (Node2D)
├── Entities (Node2D)
│   ├── Player.tscn
│   ├── Enemies (Node2D)
│   └── NPCs (Node2D)
├── Systems (Node)
│   ├── AudioManager
│   ├── ParticlePool
│   └── DialogueManager
└── UI (CanvasLayer)
    ├── HUD.tscn
    └── PauseMenu.tscn
```

### Groups for Cross-Cutting Concerns

Use groups to tag nodes across the tree:

```csharp
// In C#
AddToGroup("enemies");
AddToGroup("damageable");
AddToGroup("saveable");

// Query groups
var enemies = GetTree().GetNodesInGroup("enemies");
GetTree().CallGroup("enemies", "AlertToPlayer");
```

Common groups:
- `enemies` — all enemy nodes
- `damageable` — anything that can take damage
- `interactable` — objects the player can interact with
- `persistent` — nodes that persist between scenes
- `saveable` — nodes whose state should be saved

---

## Dynamic Scene Instantiation (C#)

### Loading and Instancing
```csharp
// Load once, instance many times
private PackedScene _bulletScene;

public override void _Ready()
{
    _bulletScene = ResourceLoader.Load<PackedScene>("res://scenes/Bullet.tscn");
}

public void Fire()
{
    var bullet = _bulletScene.Instantiate<Bullet>();
    bullet.GlobalPosition = _muzzle.GlobalPosition;
    bullet.Rotation = _muzzle.GlobalRotation;
    GetTree().CurrentScene.AddChild(bullet);
}
```

### Creating Scenes Programmatically
```csharp
public static PackedScene CreateSceneProgrammatically()
{
    // 1. Create nodes
    var root = new CharacterBody2D();
    root.Name = "Player";

    var sprite = new Sprite2D();
    sprite.Name = "Sprite2D";
    root.AddChild(sprite);
    sprite.Owner = root; // CRITICAL: set owner for packing

    var collision = new CollisionShape2D();
    collision.Name = "CollisionShape2D";
    collision.Shape = new CircleShape2D { Radius = 16f };
    root.AddChild(collision);
    collision.Owner = root;

    // 2. Pack into scene
    var scene = new PackedScene();
    var result = scene.Pack(root);

    if (result == Error.Ok)
    {
        ResourceSaver.Save(scene, "res://scenes/Player.tscn");
    }

    root.QueueFree(); // Clean up the temporary nodes
    return scene;
}
```

**Critical: The Owner Property**
- Only nodes whose `Owner` is set to the root are included when packing
- The root node does NOT set its own Owner
- Forgetting to set Owner = root is the #1 mistake when creating scenes programmatically

---

## Anti-Patterns to Avoid

### 1. God Nodes
```csharp
// BAD: One script does everything
public partial class GameManager : Node
{
    // Input, audio, UI, physics, AI, networking...
    // 2000+ lines of code
}
```
**Fix:** Split into focused systems (AudioManager, UIManager, etc.)

### 2. Reaching Up the Tree
```csharp
// BAD
var player = GetNode("/root/Main/Player");
var health = GetNode("../../HealthBar");
```
**Fix:** Use signals, exports, or groups.

### 3. Autoload Everything
```csharp
// BAD: 15 autoloads for every system
// GameManager, AudioManager, UIManager, InputManager, etc.
```
**Fix:** Use autoloads only for truly global state (save data, settings). Use composition for everything else.

### 4. Circular Dependencies
```csharp
// BAD: Player depends on Enemy, Enemy depends on Player
public partial class Player : CharacterBody2D
{
    public Enemy NearestEnemy; // Direct reference
}
```
**Fix:** Use signals or a mediator pattern through the common parent.

---

## Advanced Composition Patterns

### Pattern 6: Mediator (Parent Wires Siblings)

The parent scene orchestrates communication between sibling components that don't know about each other:

```csharp
// Level.cs — mediator between Player, Enemies, and HUD
public partial class Level : Node
{
    private Player _player;
    private HUD _hud;
    private EnemyManager _enemyManager;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _hud = GetNode<HUD>("UI/HUD");
        _enemyManager = GetNode<EnemyManager>("Enemies");

        // Player → HUD (health display)
        var playerHealth = _player.GetNode<HealthComponent>("HealthComponent");
        playerHealth.HealthChanged += (current, max) => _hud.UpdateHealthBar(current, max);
        playerHealth.Died += () => _hud.ShowGameOver();

        // Player → EnemyManager (enemies react to player position)
        // Instead of enemies knowing about player, level provides position
        _player.PositionUpdated += (pos) => _enemyManager.UpdatePlayerPosition(pos);

        // EnemyManager → HUD (kill count)
        _enemyManager.EnemyKilled += (enemyType, pos) =>
        {
            _hud.IncrementKillCount();
            _hud.ShowDamageNumber(pos, "KILL!");
        };

        // EnemyManager → Player (dropped items)
        _enemyManager.ItemDropped += (itemData, pos) =>
        {
            var item = SpawnItem(itemData, pos);
            item.PickedUp += () => _player.GetNode<InventoryComponent>("Inventory").AddItem(itemData);
        };
    }
}
```

**Key insight**: Player, EnemyManager, and HUD have ZERO direct references to each other. The Level script is the only place that knows about all three. You can swap any one of them without touching the others.

### Pattern 7: Signal Relay (Forwarding Signals Through Layers)

When a deeply nested component needs to communicate with a distant ancestor:

```csharp
// WeaponComponent.cs (deeply nested: Player > WeaponHolder > Weapon > WeaponComponent)
public partial class WeaponComponent : Node
{
    [Signal] public delegate void FiredEventHandler(int ammoRemaining);
    [Signal] public delegate void ReloadedEventHandler();
    [Signal] public delegate void AmmoDepletedEventHandler();

    public void Fire()
    {
        _ammo--;
        EmitSignal(SignalName.Fired, _ammo);
        if (_ammo <= 0)
            EmitSignal(SignalName.AmmoDepleted);
    }
}

// Player.cs — relays weapon signals up to anyone who cares
public partial class Player : CharacterBody3D
{
    // Re-declare signals that need to be visible at Player level
    [Signal] public delegate void WeaponFiredEventHandler(int ammoRemaining);
    [Signal] public delegate void WeaponAmmoDepletedEventHandler();

    public override void _Ready()
    {
        var weapon = GetNode<WeaponComponent>("WeaponHolder/Weapon/WeaponComponent");

        // Relay signals: component emits → player re-emits at higher level
        weapon.Fired += (ammo) => EmitSignal(SignalName.WeaponFired, ammo);
        weapon.AmmoDepleted += () => EmitSignal(SignalName.WeaponAmmoDepleted);
    }
}

// HUD.cs — listens to Player, doesn't know about WeaponComponent
public override void _Ready()
{
    var player = GetNode<Player>("%Player");
    player.WeaponFired += (ammo) => UpdateAmmoDisplay(ammo);
    player.WeaponAmmoDepleted += ShowReloadPrompt;
}
```

### Pattern 8: Observer via Groups (Many-to-Many)

When you need to notify many unrelated nodes at once:

```csharp
// DayNightCycle.cs — notifies all "light_sensitive" nodes
public partial class DayNightCycle : Node
{
    [Signal] public delegate void TimeChangedEventHandler(float normalizedTime, bool isNight);

    private float _time;

    public override void _Process(double delta)
    {
        _time = (_time + (float)delta * 0.01f) % 1.0f;
        bool isNight = _time > 0.5f;

        // Notify all nodes in group — they don't need to know about DayNightCycle
        GetTree().CallGroup("light_sensitive", "OnTimeChanged", _time, isNight);
    }
}

// StreetLamp.cs — adds itself to group, receives calls
public partial class StreetLamp : Node3D
{
    private OmniLight3D _light;

    public override void _Ready()
    {
        _light = GetNode<OmniLight3D>("Light");
        AddToGroup("light_sensitive");
    }

    // Called by DayNightCycle via CallGroup
    public void OnTimeChanged(float time, bool isNight)
    {
        _light.LightEnergy = isNight ? 2.0f : 0.0f;
    }
}

// NPC.cs — also in group, different behavior
public partial class NPC : CharacterBody3D
{
    public override void _Ready()
    {
        AddToGroup("light_sensitive");
    }

    public void OnTimeChanged(float time, bool isNight)
    {
        if (isNight) GoHome();
        else GoToWork();
    }
}
```

### Pattern 9: Exported Node References (Godot 4+ Direct Wiring)

In Godot 4, you can export direct node references (not just NodePaths). This is the simplest form of dependency injection:

```csharp
public partial class HealthBar : ProgressBar
{
    // Drag and drop in the editor — no code to resolve paths
    [Export] public HealthComponent Target { get; set; }

    public override void _Ready()
    {
        if (Target == null)
        {
            GD.PrintErr("HealthBar: No Target assigned!");
            return;
        }

        Target.HealthChanged += OnHealthChanged;
        MaxValue = Target.MaxHealth;
        Value = Target.CurrentHealth;
    }

    private void OnHealthChanged(int current, int max)
    {
        MaxValue = max;
        var tween = CreateTween();
        tween.TweenProperty(this, "value", (double)current, 0.3);
    }

    public override void _ExitTree()
    {
        if (Target != null)
            Target.HealthChanged -= OnHealthChanged;
    }
}
```

**Advantage over GetNode**: Works regardless of hierarchy. The HealthBar can be anywhere in the scene tree — on the HUD, floating above the enemy, in a different CanvasLayer. Just drag the HealthComponent in the inspector.

### Pattern 10: Component With Required Dependencies

Pattern for components that require other sibling components to function:

```csharp
public partial class CombatComponent : Node
{
    [Signal] public delegate void AttackedEventHandler(int damage);
    [Signal] public delegate void KilledTargetEventHandler(Node target);

    // Dependencies — set by parent or by export
    [Export] public HealthComponent Health { get; set; }
    [Export] public HitboxComponent3D Hitbox { get; set; }
    [Export] public HurtboxComponent3D Hurtbox { get; set; }

    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public float AttackCooldown { get; set; } = 0.5f;

    private bool _canAttack = true;

    public override void _Ready()
    {
        // Validate dependencies
        if (Health == null || Hitbox == null || Hurtbox == null)
        {
            GD.PrintErr($"{GetParent().Name}/CombatComponent: Missing required dependencies!");
            SetProcess(false);
            return;
        }

        // Wire internal signal flow
        Hurtbox.Hurt += OnHurt;
        Hitbox.Damage = AttackDamage;
    }

    private void OnHurt(HitboxComponent3D hitbox)
    {
        Health.TakeDamage(hitbox.Damage, hitbox.GetParent());
    }

    public void Attack()
    {
        if (!_canAttack) return;
        _canAttack = false;
        Hitbox.Activate(0.2f);
        EmitSignal(SignalName.Attacked, AttackDamage);

        // Cooldown
        GetTree().CreateTimer(AttackCooldown).Timeout += () => _canAttack = true;
    }

    // Self-documenting in editor
    public string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();
        if (Health == null) warnings.Add("CombatComponent requires a HealthComponent reference.");
        if (Hitbox == null) warnings.Add("CombatComponent requires a HitboxComponent3D reference.");
        if (Hurtbox == null) warnings.Add("CombatComponent requires a HurtboxComponent3D reference.");
        return warnings.ToArray();
    }
}
```

---

## Decision Tree: When to Use Each Communication Pattern

```
Need to communicate?
│
├── Parent → Child?
│   └── Direct method call ✅
│       child.DoSomething();
│
├── Child → Parent?
│   └── Signal ✅
│       EmitSignal(SignalName.SomethingHappened);
│
├── Sibling → Sibling?
│   └── Parent mediates ✅
│       // In parent: childA.EventX += (args) => childB.ReactToX(args);
│
├── Distant nodes in same scene?
│   ├── Use [Export] node reference ✅ (simplest)
│   ├── Signal relay through parent chain ✅ (cleanest)
│   └── Groups + CallGroup ✅ (many receivers)
│
├── Cross-scene communication?
│   ├── Few listeners → Signal on shared parent ✅
│   ├── Many listeners → Event Bus singleton ⚠️ (use sparingly)
│   └── Persistent data → Autoload with signals ✅
│
└── Truly global event?
    └── Event Bus ⚠️
        // Only when: score changed, player died, level completed
        // Never when: component-level details
```

---

## Real-World Composition Example: Complete Weapon System

```
Player (CharacterBody3D)
├── HealthComponent.tscn
├── MovementComponent3D.tscn
├── WeaponManager (Node)                    ← manages weapon switching
│   ├── Sword.tscn (current weapon)
│   │   ├── MeshInstance3D                  ← visual
│   │   ├── HitboxComponent3D.tscn         ← deals damage
│   │   ├── AnimationPlayer                ← swing animation
│   │   └── AudioStreamPlayer3D            ← swing/hit sounds
│   ├── Bow.tscn (in inventory)
│   │   ├── MeshInstance3D
│   │   ├── ArrowSpawnPoint (Marker3D)
│   │   └── AnimationPlayer
│   └── Shield.tscn (in inventory)
│       ├── MeshInstance3D
│       ├── BlockArea (Area3D)
│       └── AnimationPlayer
├── HurtboxComponent3D.tscn
├── CameraRig.tscn
└── StateMachine.tscn
    ├── IdleState
    ├── RunState
    ├── AttackState
    └── BlockState
```

```csharp
// WeaponManager.cs — orchestrates weapons
public partial class WeaponManager : Node
{
    [Signal] public delegate void WeaponSwitchedEventHandler(string weaponName);
    [Signal] public delegate void AttackStartedEventHandler();
    [Signal] public delegate void AttackEndedEventHandler();

    [Export] public Godot.Collections.Array<PackedScene> WeaponScenes { get; set; }

    private IWeapon _currentWeapon;
    private int _currentIndex;

    public interface IWeapon
    {
        void Attack();
        void StopAttack();
        string WeaponName { get; }
    }

    public void SwitchWeapon(int index)
    {
        if (index == _currentIndex) return;

        // Remove current
        if (_currentWeapon is Node currentNode)
        {
            currentNode.QueueFree();
        }

        // Instantiate new
        var weaponNode = WeaponScenes[index].Instantiate();
        AddChild(weaponNode);
        _currentWeapon = weaponNode as IWeapon;
        _currentIndex = index;

        EmitSignal(SignalName.WeaponSwitched, _currentWeapon?.WeaponName ?? "Unknown");
    }

    public void Attack()
    {
        if (_currentWeapon == null) return;
        EmitSignal(SignalName.AttackStarted);
        _currentWeapon.Attack();
    }
}
```

---

## Composition Checklist

When designing a new game entity, ask:

1. **What behaviors does it need?** → Each behavior = potential component scene
2. **Are any of these reusable?** → If yes, extract to `scenes/components/`
3. **How do components communicate?** → Signals up, method calls down
4. **Does it have variants?** → Consider scene inheritance (BaseEnemy → Goblin, Orc)
5. **What data drives it?** → Custom Resources for configuration
6. **Is any behavior complex/multi-state?** → Add a StateMachine component
7. **Does it interact with the world?** → Hitbox/Hurtbox components
8. **Does it have health?** → HealthComponent
9. **Does it move?** → MovementComponent (2D or 3D)
10. **Does it make sound?** → AudioComponent with AudioStreamPlayer children
