# Example 04: Deep Scene Composition (3D Game)

## User Input
> "Show me how to architect a 3D action RPG character using heavy composition"

## Expected Output

### Architecture: Character Built from 11 Composed Scenes

```
PlayerCharacter (CharacterBody3D)              ← Player.tscn
├── Model (Node3D)                             ← visual root
│   ├── MeshInstance3D / imported .glb
│   └── AnimationPlayer
├── CollisionShape3D                           ← physics body
│
├── HealthComponent.tscn                       ← COMPONENT: health tracking
├── StaminaComponent.tscn                      ← COMPONENT: stamina for actions
├── MovementComponent3D.tscn                   ← COMPONENT: 3D movement + gravity
├── CameraRig.tscn                             ← COMPONENT: 3rd person camera
│   ├── SpringArm3D
│   └── Camera3D
├── HitboxComponent3D.tscn                     ← COMPONENT: deal damage
│   └── CollisionShape3D
├── HurtboxComponent3D.tscn                    ← COMPONENT: receive damage
│   └── CollisionShape3D
├── InteractionComponent.tscn                  ← COMPONENT: interact with objects
│   └── RayCast3D
├── InventoryComponent.tscn                    ← COMPONENT: item management
├── StateMachine.tscn                          ← COMPONENT: state management
│   ├── IdleState
│   ├── RunState
│   ├── DodgeState
│   ├── AttackState
│   └── DeathState
└── AudioComponent.tscn                        ← COMPONENT: footsteps, sfx
    ├── AudioStreamPlayer3D (footsteps)
    ├── AudioStreamPlayer3D (voice)
    └── AudioStreamPlayer3D (weapon)
```

**11 composed scenes, each with a single responsibility.**

---

### Core Pattern: Each Component Scene

#### HealthComponent.tscn (reusable by ANY entity)
```ini
[gd_scene load_steps=2 format=3 uid="uid://health_comp"]

[ext_resource type="Script" path="res://scripts/components/HealthComponent.cs" id="1"]

[node name="HealthComponent" type="Node"]
script = ExtResource("1")
MaxHealth = 100
```

```csharp
// scripts/components/HealthComponent.cs
using Godot;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void DamageTakenEventHandler(int amount, Node source);

    [Export] public int MaxHealth { get; set; } = 100;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public float HealthRatio => (float)CurrentHealth / MaxHealth;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount, Node source = null)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EmitSignal(SignalName.DamageTaken, amount, source);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        if (CurrentHealth <= 0)
            EmitSignal(SignalName.Died);
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }

    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        MaxHealth = newMax;
        if (healToFull)
            CurrentHealth = MaxHealth;
        else
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }
}
```

#### StaminaComponent.tscn
```csharp
// scripts/components/StaminaComponent.cs
using Godot;

[GlobalClass]
public partial class StaminaComponent : Node
{
    [Signal] public delegate void StaminaChangedEventHandler(float current, float max);
    [Signal] public delegate void StaminaDepletedEventHandler();

    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float RegenRate { get; set; } = 20f;
    [Export] public float RegenDelay { get; set; } = 1.0f;

    public float CurrentStamina { get; private set; }
    public bool CanUse(float cost) => CurrentStamina >= cost;

    private float _regenTimer;

    public override void _Ready()
    {
        CurrentStamina = MaxStamina;
    }

    public override void _Process(double delta)
    {
        if (_regenTimer > 0)
        {
            _regenTimer -= (float)delta;
            return;
        }

        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + RegenRate * (float)delta);
            EmitSignal(SignalName.StaminaChanged, CurrentStamina, MaxStamina);
        }
    }

    public bool TryUse(float cost)
    {
        if (CurrentStamina < cost) return false;
        CurrentStamina -= cost;
        _regenTimer = RegenDelay;
        EmitSignal(SignalName.StaminaChanged, CurrentStamina, MaxStamina);
        if (CurrentStamina <= 0)
            EmitSignal(SignalName.StaminaDepleted);
        return true;
    }
}
```

#### MovementComponent3D.tscn
```csharp
// scripts/components/MovementComponent3D.cs
using Godot;

[GlobalClass]
public partial class MovementComponent3D : Node
{
    [Signal] public delegate void MovedEventHandler(Vector3 velocity);
    [Signal] public delegate void StoppedEventHandler();

    [ExportGroup("Speed")]
    [Export] public float WalkSpeed { get; set; } = 4.0f;
    [Export] public float RunSpeed { get; set; } = 7.0f;
    [Export] public float Acceleration { get; set; } = 20f;
    [Export] public float Friction { get; set; } = 15f;

    [ExportGroup("Gravity")]
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float JumpForce { get; set; } = 8f;

    [ExportGroup("Rotation")]
    [Export] public float RotationSpeed { get; set; } = 10f;

    private CharacterBody3D _body;
    private bool _isRunning;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody3D>();
    }

    public void Move(Vector3 direction, double delta, bool running = false)
    {
        _isRunning = running;
        float speed = running ? RunSpeed : WalkSpeed;
        float dt = (float)delta;

        var velocity = _body.Velocity;

        // Horizontal movement
        if (direction.LengthSquared() > 0.01f)
        {
            direction = direction.Normalized();
            velocity.X = Mathf.MoveToward(velocity.X, direction.X * speed, Acceleration * dt);
            velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * speed, Acceleration * dt);

            // Rotate body toward movement
            var targetAngle = Mathf.Atan2(direction.X, direction.Z);
            _body.Rotation = new Vector3(0,
                Mathf.LerpAngle(_body.Rotation.Y, targetAngle, RotationSpeed * dt), 0);

            EmitSignal(SignalName.Moved, velocity);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * dt);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Friction * dt);

            if (Mathf.Abs(velocity.X) < 0.1f && Mathf.Abs(velocity.Z) < 0.1f)
                EmitSignal(SignalName.Stopped);
        }

        // Gravity
        if (!_body.IsOnFloor())
            velocity.Y -= Gravity * dt;

        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }

    public void Jump()
    {
        if (_body.IsOnFloor())
        {
            var velocity = _body.Velocity;
            velocity.Y = JumpForce;
            _body.Velocity = velocity;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        var velocity = _body.Velocity;
        velocity += direction.Normalized() * force;
        _body.Velocity = velocity;
    }
}
```

#### CameraRig.tscn
```ini
[gd_scene load_steps=2 format=3 uid="uid://camera_rig"]

[ext_resource type="Script" path="res://scripts/components/CameraRig.cs" id="1"]

[node name="CameraRig" type="Node3D"]
script = ExtResource("1")
MouseSensitivity = 0.003
MinPitch = -60.0
MaxPitch = 40.0

[node name="SpringArm3D" type="SpringArm3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1.5, 0)
spring_length = 5.0
collision_mask = 1

[node name="Camera3D" type="Camera3D" parent="SpringArm3D"]
current = true
```

```csharp
using Godot;

public partial class CameraRig : Node3D
{
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    [Export] public float MinPitch { get; set; } = -60f;
    [Export] public float MaxPitch { get; set; } = 40f;

    private SpringArm3D _springArm;

    public override void _Ready()
    {
        _springArm = GetNode<SpringArm3D>("SpringArm3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            _springArm.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            _springArm.Rotation = new Vector3(
                Mathf.Clamp(_springArm.Rotation.X, Mathf.DegToRad(MinPitch), Mathf.DegToRad(MaxPitch)),
                _springArm.Rotation.Y,
                _springArm.Rotation.Z
            );
        }
    }

    /// <summary>
    /// Returns camera-relative movement direction from input.
    /// </summary>
    public Vector3 GetInputDirection()
    {
        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var forward = -GlobalTransform.Basis.Z;
        var right = GlobalTransform.Basis.X;
        forward.Y = 0;
        right.Y = 0;
        return (forward.Normalized() * input.Y + right.Normalized() * input.X).Normalized();
    }
}
```

#### InteractionComponent.tscn
```csharp
using Godot;

public partial class InteractionComponent : Node3D
{
    [Signal] public delegate void InteractableFoundEventHandler(Node3D target);
    [Signal] public delegate void InteractableLostEventHandler();

    [Export] public float InteractDistance { get; set; } = 3.0f;

    private RayCast3D _ray;
    private Node3D _currentTarget;

    public override void _Ready()
    {
        _ray = GetNode<RayCast3D>("RayCast3D");
        _ray.TargetPosition = new Vector3(0, 0, -InteractDistance);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ray.IsColliding())
        {
            var collider = _ray.GetCollider() as Node3D;
            if (collider != null && collider.IsInGroup("interactable"))
            {
                if (collider != _currentTarget)
                {
                    _currentTarget = collider;
                    EmitSignal(SignalName.InteractableFound, _currentTarget);
                }
            }
            else if (_currentTarget != null)
            {
                _currentTarget = null;
                EmitSignal(SignalName.InteractableLost);
            }
        }
        else if (_currentTarget != null)
        {
            _currentTarget = null;
            EmitSignal(SignalName.InteractableLost);
        }
    }

    public void TryInteract()
    {
        if (_currentTarget != null && _currentTarget.HasMethod("Interact"))
        {
            _currentTarget.Call("Interact");
        }
    }
}
```

---

### Player.cs — The Orchestrator (Thin!)

```csharp
using Godot;

public partial class Player : CharacterBody3D
{
    // Components — all composed scenes
    private HealthComponent _health;
    private StaminaComponent _stamina;
    private MovementComponent3D _movement;
    private CameraRig _camera;
    private InteractionComponent _interaction;

    public override void _Ready()
    {
        // Get references to composed components
        _health = GetNode<HealthComponent>("HealthComponent");
        _stamina = GetNode<StaminaComponent>("StaminaComponent");
        _movement = GetNode<MovementComponent3D>("MovementComponent3D");
        _camera = GetNode<CameraRig>("CameraRig");
        _interaction = GetNode<InteractionComponent>("InteractionComponent");

        // Connect signals (signal UP)
        _health.Died += OnDied;
        _health.DamageTaken += OnDamageTaken;
        _interaction.InteractableFound += OnInteractableFound;
        _interaction.InteractableLost += OnInteractableLost;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Get camera-relative input direction
        var direction = _camera.GetInputDirection();
        bool running = Input.IsActionPressed("run") && _stamina.CanUse(10f * (float)delta);

        // Call DOWN to movement component
        _movement.Move(direction, delta, running);

        if (running && direction.LengthSquared() > 0.01f)
            _stamina.TryUse(10f * (float)delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("jump"))
            _movement.Jump();

        if (@event.IsActionPressed("interact"))
            _interaction.TryInteract();

        if (@event.IsActionPressed("dodge") && _stamina.TryUse(25f))
            Dodge();
    }

    private void Dodge()
    {
        var direction = _camera.GetInputDirection();
        if (direction == Vector3.Zero)
            direction = -GlobalTransform.Basis.Z; // Dodge forward
        _movement.ApplyKnockback(direction, 12f);
    }

    private void OnDied() => GD.Print("Player died");
    private void OnDamageTaken(int amount, Node source) => GD.Print($"Took {amount} damage");
    private void OnInteractableFound(Node3D target) => GD.Print($"Can interact with {target.Name}");
    private void OnInteractableLost() => GD.Print("Nothing to interact with");
}
```

## Key Takeaways

1. **Player.cs is ~60 lines** — all logic lives in components
2. **Every component is reusable**: HealthComponent works on Player, Enemy, NPC, destructible barrel
3. **Signal flow is strict**: Components emit UP, Player calls DOWN
4. **No component knows about Player**: Components only know about their own data
5. **CameraRig provides camera-relative directions**: Movement doesn't know about the camera
6. **StaminaComponent gates actions**: Run, dodge — all check stamina
7. **New behaviors = new component scene**: Add `StealthComponent.tscn`, compose it in
