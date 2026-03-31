# Example 08: Complete Component Pattern (3D)

## User Input
> "Create a full set of reusable components for a 3D game: health, hitbox, hurtbox, movement, and show how they compose together for different entities"

## Expected Output

### The Component Library

All components live in `scenes/components/` and `scripts/components/`. Each is a self-contained scene with a single responsibility.

```
scenes/components/
├── HealthComponent.tscn
├── HitboxComponent3D.tscn
├── HurtboxComponent3D.tscn
├── MovementComponent3D.tscn
├── KnockbackComponent.tscn
└── DeathComponent.tscn
```

---

### 1. HealthComponent (Node — no physics needed)

```ini
[gd_scene load_steps=2 format=3 uid="uid://health"]
[ext_resource type="Script" path="res://scripts/components/HealthComponent.cs" id="1"]
[node name="HealthComponent" type="Node"]
script = ExtResource("1")
MaxHealth = 100
InvincibilityDuration = 0.0
```

```csharp
using Godot;

public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void DamageTakenEventHandler(int amount, Node source);
    [Signal] public delegate void InvincibilityStartedEventHandler();
    [Signal] public delegate void InvincibilityEndedEventHandler();

    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float InvincibilityDuration { get; set; } = 0f;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvincible { get; private set; }

    public override void _Ready() => CurrentHealth = MaxHealth;

    public void TakeDamage(int amount, Node source = null)
    {
        if (!IsAlive || IsInvincible) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EmitSignal(SignalName.DamageTaken, amount, source);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        if (InvincibilityDuration > 0)
            StartInvincibility();

        if (CurrentHealth <= 0)
            EmitSignal(SignalName.Died);
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }

    private async void StartInvincibility()
    {
        IsInvincible = true;
        EmitSignal(SignalName.InvincibilityStarted);
        await ToSignal(GetTree().CreateTimer(InvincibilityDuration), SceneTreeTimer.SignalName.Timeout);
        IsInvincible = false;
        EmitSignal(SignalName.InvincibilityEnded);
    }
}
```

### 2. HitboxComponent3D (Area3D — deals damage)

```ini
[gd_scene load_steps=3 format=3 uid="uid://hitbox3d"]
[ext_resource type="Script" path="res://scripts/components/HitboxComponent3D.cs" id="1"]
[sub_resource type="BoxShape3D" id="BoxShape3D_hit"]
size = Vector3(1, 1, 1)

[node name="HitboxComponent3D" type="Area3D"]
collision_layer = 0
collision_mask = 0
monitorable = true
monitoring = false
script = ExtResource("1")
Damage = 10

[node name="CollisionShape3D" type="CollisionShape3D" parent="."]
shape = SubResource("BoxShape3D_hit")
disabled = true
```

```csharp
using Godot;

public partial class HitboxComponent3D : Area3D
{
    [Export] public int Damage { get; set; } = 10;
    [Export] public float KnockbackForce { get; set; } = 5f;
    [Export] public Vector3 KnockbackDirection { get; set; } = Vector3.Zero;

    private CollisionShape3D _shape;

    public override void _Ready()
    {
        _shape = GetNode<CollisionShape3D>("CollisionShape3D");
    }

    /// <summary>Enable the hitbox for a duration (attack window)</summary>
    public async void Activate(float duration)
    {
        _shape.Disabled = false;
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        _shape.Disabled = true;
    }

    /// <summary>Enable/disable manually</summary>
    public void SetActive(bool active) => _shape.Disabled = !active;

    /// <summary>Get knockback direction from this hitbox's position toward target</summary>
    public Vector3 GetKnockbackToward(Vector3 targetPosition)
    {
        if (KnockbackDirection != Vector3.Zero)
            return KnockbackDirection.Normalized() * KnockbackForce;

        var dir = (targetPosition - GlobalPosition).Normalized();
        dir.Y = 0.3f; // Slight upward arc
        return dir.Normalized() * KnockbackForce;
    }
}
```

### 3. HurtboxComponent3D (Area3D — receives damage)

```ini
[gd_scene load_steps=3 format=3 uid="uid://hurtbox3d"]
[ext_resource type="Script" path="res://scripts/components/HurtboxComponent3D.cs" id="1"]
[sub_resource type="BoxShape3D" id="BoxShape3D_hurt"]
size = Vector3(0.8, 1.6, 0.8)

[node name="HurtboxComponent3D" type="Area3D"]
collision_layer = 0
collision_mask = 0
monitorable = false
monitoring = true
script = ExtResource("1")

[node name="CollisionShape3D" type="CollisionShape3D" parent="."]
shape = SubResource("BoxShape3D_hurt")
```

```csharp
using Godot;

public partial class HurtboxComponent3D : Area3D
{
    [Signal] public delegate void HurtEventHandler(HitboxComponent3D hitbox);

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area3D area)
    {
        if (area is HitboxComponent3D hitbox)
        {
            EmitSignal(SignalName.Hurt, hitbox);
        }
    }
}
```

### 4. KnockbackComponent (Node — applies knockback to parent body)

```csharp
using Godot;

public partial class KnockbackComponent : Node
{
    [Export] public float DecayRate { get; set; } = 10f;

    private CharacterBody3D _body;
    private Vector3 _knockbackVelocity;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_knockbackVelocity.LengthSquared() > 0.1f)
        {
            _knockbackVelocity = _knockbackVelocity.MoveToward(Vector3.Zero, DecayRate * (float)delta);
            var vel = _body.Velocity;
            vel.X += _knockbackVelocity.X;
            vel.Z += _knockbackVelocity.Z;
            _body.Velocity = vel;
        }
    }

    public void Apply(Vector3 force)
    {
        _knockbackVelocity = force;
    }
}
```

### 5. DeathComponent (Node — handles death sequence)

```csharp
using Godot;

public partial class DeathComponent : Node
{
    [Export] public PackedScene DeathEffect { get; set; }
    [Export] public float FadeOutDuration { get; set; } = 0.5f;
    [Export] public bool DropLoot { get; set; } = true;

    public async void Execute()
    {
        var parent = GetParent<Node3D>();

        // Spawn death effect
        if (DeathEffect != null)
        {
            var effect = DeathEffect.Instantiate<Node3D>();
            effect.GlobalPosition = parent.GlobalPosition;
            parent.GetTree().CurrentScene.AddChild(effect);
        }

        // Fade out
        if (parent is CharacterBody3D body)
        {
            body.SetPhysicsProcess(false);
            body.SetProcess(false);
        }

        // Tween fade
        var tween = parent.CreateTween();
        tween.TweenProperty(parent, "modulate:a", 0.0f, FadeOutDuration);
        await parent.ToSignal(tween, Tween.SignalName.Finished);

        parent.QueueFree();
    }
}
```

---

### Composing: Player

```
Player (CharacterBody3D)
├── MeshInstance3D + CollisionShape3D
├── HealthComponent          MaxHealth=150, InvincibilityDuration=0.5
├── HitboxComponent3D        Damage=20 (collision_layer=8, for player attacks)
├── HurtboxComponent3D       (collision_mask=16, detects enemy attacks)
├── KnockbackComponent       DecayRate=15
├── DeathComponent           DeathEffect=player_death.tscn
├── MovementComponent3D      WalkSpeed=5, RunSpeed=8
├── CameraRig.tscn
└── StateMachine.tscn
```

```csharp
public partial class Player : CharacterBody3D
{
    private HealthComponent _health;
    private HurtboxComponent3D _hurtbox;
    private KnockbackComponent _knockback;
    private DeathComponent _death;

    public override void _Ready()
    {
        _health = GetNode<HealthComponent>("HealthComponent");
        _hurtbox = GetNode<HurtboxComponent3D>("HurtboxComponent3D");
        _knockback = GetNode<KnockbackComponent>("KnockbackComponent");
        _death = GetNode<DeathComponent>("DeathComponent");

        // Wire signals
        _hurtbox.Hurt += OnHurt;
        _health.Died += OnDied;
    }

    private void OnHurt(HitboxComponent3D hitbox)
    {
        _health.TakeDamage(hitbox.Damage);
        _knockback.Apply(hitbox.GetKnockbackToward(GlobalPosition));
    }

    private void OnDied() => _death.Execute();
}
```

### Composing: Enemy (Same Components, Different Config)

```
Enemy (CharacterBody3D)
├── MeshInstance3D + CollisionShape3D
├── HealthComponent          MaxHealth=80
├── HitboxComponent3D        Damage=15 (collision_layer=16, for enemy attacks)
├── HurtboxComponent3D       (collision_mask=8, detects player attacks)
├── KnockbackComponent       DecayRate=8
├── DeathComponent           DeathEffect=enemy_death.tscn, DropLoot=true
├── NavigationAgent3D
└── StateMachine.tscn
```

### Composing: Destructible Barrel (Subset of Components)

```
Barrel (StaticBody3D)
├── MeshInstance3D + CollisionShape3D
├── HealthComponent          MaxHealth=30
├── HurtboxComponent3D       (collision_mask=8|16, both player and enemy can break it)
└── DeathComponent           DeathEffect=barrel_break.tscn, DropLoot=true
```

Only 3 components! No movement, no hitbox, no knockback. Just health, hurtbox, and death.

### Composing: Healing Fountain (Even Simpler)

```
Fountain (Area3D)
├── MeshInstance3D
├── CollisionShape3D (trigger)
└── Script: OnBodyEntered → heal player
```

No components needed — it's simple enough.

---

## Collision Layer Setup

| Layer | Purpose | Who sets layer | Who sets mask |
|-------|---------|---------------|--------------|
| 1 | World geometry | StaticBody3D | Everything |
| 2 | Player body | Player CharacterBody3D | Enemies |
| 4 | Enemy body | Enemy CharacterBody3D | Player |
| 8 | Player attacks | Player HitboxComponent3D | Enemy HurtboxComponent3D |
| 16 | Enemy attacks | Enemy HitboxComponent3D | Player HurtboxComponent3D |
| 32 | Pickups | Item Area3D | Player |

```
Player hitbox:   layer=8,  mask=0 (doesn't detect, only IS detected)
Player hurtbox:  layer=0,  mask=16 (detects enemy attacks)
Enemy hitbox:    layer=16, mask=0
Enemy hurtbox:   layer=0,  mask=8
Barrel hurtbox:  layer=0,  mask=24 (8+16, both can break it)
```

## Key Takeaways

1. **5 components compose into ANY entity** — from Player (7 components) to Barrel (3 components)
2. **Hitbox/Hurtbox is the core combat pattern** — hitboxes deal, hurtboxes receive
3. **Collision layers separate interaction types** — player attacks vs enemy attacks
4. **Components don't know about each other** — the parent wires them via signals
5. **Same component, different config** — HealthComponent(150) for player, HealthComponent(30) for barrel
6. **Death is a component** — it handles effects, loot, fade, and cleanup
