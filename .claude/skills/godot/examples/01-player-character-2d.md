# Example 01: 2D Platformer Player Character

## User Input
> "Create a 2D platformer player with movement, jump, and double jump"

## Expected Output

### Scene Tree (Composition-Based)
```
Player (CharacterBody2D)
├── Sprite2D
├── CollisionShape2D
├── HealthComponent.tscn         ← reusable component
├── MovementComponent.tscn       ← reusable component
├── HurtboxComponent (Area2D)    ← reusable component
│   └── CollisionShape2D
├── AnimationPlayer
├── Camera2D
├── CoyoteTimer (Timer)          ← coyote time window
└── JumpBufferTimer (Timer)      ← jump buffering
```

### Player.tscn
```ini
[gd_scene load_steps=6 format=3 uid="uid://player2d"]

[ext_resource type="Script" path="res://scripts/characters/Player.cs" id="1_script"]
[ext_resource type="Texture2D" path="res://assets/sprites/characters/player.png" id="2_tex"]
[ext_resource type="PackedScene" path="res://scenes/components/HealthComponent.tscn" id="3_health"]
[ext_resource type="PackedScene" path="res://scenes/components/HurtboxComponent.tscn" id="4_hurtbox"]

[sub_resource type="CapsuleShape2D" id="CapsuleShape2D_abc"]
radius = 8.0
height = 24.0

[node name="Player" type="CharacterBody2D" groups=["player"]]
collision_layer = 2
collision_mask = 1
script = ExtResource("1_script")
Speed = 200.0
JumpForce = -350.0
Gravity = 900.0
MaxJumps = 2

[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("2_tex")
offset = Vector2(0, -12)

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
position = Vector2(0, -12)
shape = SubResource("CapsuleShape2D_abc")

[node name="HealthComponent" parent="." instance=ExtResource("3_health")]
MaxHealth = 100

[node name="HurtboxComponent" parent="." instance=ExtResource("4_hurtbox")]

[node name="AnimationPlayer" type="AnimationPlayer" parent="."]

[node name="Camera2D" type="Camera2D" parent="."]
position_smoothing_enabled = true
position_smoothing_speed = 8.0

[node name="CoyoteTimer" type="Timer" parent="."]
wait_time = 0.12
one_shot = true

[node name="JumpBufferTimer" type="Timer" parent="."]
wait_time = 0.1
one_shot = true

[connection signal="Died" from="HealthComponent" to="." method="OnDied"]
[connection signal="Hurt" from="HurtboxComponent" to="." method="OnHurt"]
```

### Player.cs
```csharp
using Godot;

public partial class Player : CharacterBody2D
{
    [ExportGroup("Movement")]
    [Export] public float Speed { get; set; } = 200f;
    [Export] public float Acceleration { get; set; } = 1200f;
    [Export] public float Friction { get; set; } = 1000f;

    [ExportGroup("Jumping")]
    [Export] public float JumpForce { get; set; } = -350f;
    [Export] public float Gravity { get; set; } = 900f;
    [Export] public int MaxJumps { get; set; } = 2;

    private int _jumpsRemaining;
    private bool _wasOnFloor;
    private Timer _coyoteTimer;
    private Timer _jumpBufferTimer;
    private HealthComponent _health;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _coyoteTimer = GetNode<Timer>("CoyoteTimer");
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
        _health = GetNode<HealthComponent>("HealthComponent");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _jumpsRemaining = MaxJumps;
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;
        float dt = (float)delta;

        // Gravity
        if (!IsOnFloor())
            velocity.Y += Gravity * dt;

        // Coyote time: was on floor last frame, now isn't
        if (_wasOnFloor && !IsOnFloor() && _jumpsRemaining == MaxJumps)
        {
            _coyoteTimer.Start();
        }

        // Reset jumps on landing
        if (IsOnFloor())
        {
            _jumpsRemaining = MaxJumps;
            // Check jump buffer
            if (!_jumpBufferTimer.IsStopped())
            {
                velocity.Y = JumpForce;
                _jumpsRemaining--;
                _jumpBufferTimer.Stop();
            }
        }

        // Jump input
        if (Input.IsActionJustPressed("jump"))
        {
            bool canCoyote = !_coyoteTimer.IsStopped();
            if (IsOnFloor() || canCoyote)
            {
                velocity.Y = JumpForce;
                _jumpsRemaining = MaxJumps - 1;
                _coyoteTimer.Stop();
            }
            else if (_jumpsRemaining > 0)
            {
                velocity.Y = JumpForce * 0.85f; // Weaker double jump
                _jumpsRemaining--;
            }
            else
            {
                _jumpBufferTimer.Start();
            }
        }

        // Variable jump height
        if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
            velocity.Y *= 0.5f;

        // Horizontal movement
        float direction = Input.GetAxis("move_left", "move_right");
        if (direction != 0)
        {
            velocity.X = Mathf.MoveToward(velocity.X, direction * Speed, Acceleration * dt);
            GetNode<Sprite2D>("Sprite2D").FlipH = direction < 0;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * dt);
        }

        _wasOnFloor = IsOnFloor();
        Velocity = velocity;
        MoveAndSlide();
    }

    private void OnDied()
    {
        // Handle death (restart, respawn, etc.)
        GetTree().ReloadCurrentScene();
    }

    private void OnHurt(int damage)
    {
        _health.TakeDamage(damage);
    }
}
```

## Key Takeaways
- **Composition**: HealthComponent and HurtboxComponent are separate reusable scenes
- **Coyote time** and **jump buffer** use Timer nodes (no manual deltaTime tracking)
- **Signals flow up**: HealthComponent.Died → Player.OnDied
- **Exports** make all values tunable in the editor
