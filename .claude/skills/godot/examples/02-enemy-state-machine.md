# Example 02: 3D Enemy with State Machine (Composition)

## User Input
> "Create a 3D enemy with patrol, chase, and attack states using composition"

## Expected Output

### Scene Tree (Heavily Composed)
```
Enemy (CharacterBody3D)
├── CollisionShape3D
├── MeshInstance3D (or imported model)
├── NavigationAgent3D
├── HealthComponent.tscn          ← reusable
├── HitboxComponent (Area3D)      ← reusable
│   └── CollisionShape3D
├── HurtboxComponent (Area3D)     ← reusable
│   └── CollisionShape3D
├── DetectionArea (Area3D)
│   └── CollisionShape3D (sphere)
├── AttackArea (Area3D)
│   └── CollisionShape3D
├── StateMachine (Node)            ← reusable
│   ├── PatrolState (Node)
│   ├── ChaseState (Node)
│   └── AttackState (Node)
├── AnimationPlayer
└── AttackCooldown (Timer)
```

Every component is a separate scene or follows the component pattern. The StateMachine, HealthComponent, HitboxComponent, and HurtboxComponent are all reusable across any entity.

### Enemy.tscn
```ini
[gd_scene load_steps=10 format=3 uid="uid://enemy3d"]

[ext_resource type="Script" path="res://scripts/characters/enemies/Enemy.cs" id="1_script"]
[ext_resource type="PackedScene" path="res://scenes/components/HealthComponent.tscn" id="2_health"]
[ext_resource type="PackedScene" path="res://scenes/components/HitboxComponent.tscn" id="3_hitbox"]
[ext_resource type="PackedScene" path="res://scenes/components/HurtboxComponent.tscn" id="4_hurtbox"]
[ext_resource type="Script" path="res://scripts/components/StateMachine.cs" id="5_sm"]
[ext_resource type="Script" path="res://scripts/characters/enemies/states/PatrolState.cs" id="6_patrol"]
[ext_resource type="Script" path="res://scripts/characters/enemies/states/ChaseState.cs" id="7_chase"]
[ext_resource type="Script" path="res://scripts/characters/enemies/states/AttackState.cs" id="8_attack"]

[sub_resource type="CapsuleShape3D" id="CapsuleShape3D_body"]
radius = 0.4
height = 1.8

[sub_resource type="CapsuleMesh" id="CapsuleMesh_visual"]
radius = 0.4
height = 1.8

[sub_resource type="SphereShape3D" id="SphereShape3D_detect"]
radius = 10.0

[sub_resource type="SphereShape3D" id="SphereShape3D_attack"]
radius = 2.0

[node name="Enemy" type="CharacterBody3D" groups=["enemies", "damageable"]]
collision_layer = 4
collision_mask = 1
script = ExtResource("1_script")
MoveSpeed = 3.5
RotationSpeed = 5.0

[node name="CollisionShape3D" type="CollisionShape3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.9, 0)
shape = SubResource("CapsuleShape3D_body")

[node name="MeshInstance3D" type="MeshInstance3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.9, 0)
mesh = SubResource("CapsuleMesh_visual")

[node name="NavigationAgent3D" type="NavigationAgent3D" parent="."]
path_desired_distance = 0.5
target_desired_distance = 1.0

[node name="HealthComponent" parent="." instance=ExtResource("2_health")]
MaxHealth = 80

[node name="HitboxComponent" parent="." instance=ExtResource("3_hitbox")]
Damage = 15

[node name="HurtboxComponent" parent="." instance=ExtResource("4_hurtbox")]

[node name="DetectionArea" type="Area3D" parent="."]
collision_layer = 0
collision_mask = 2

[node name="CollisionShape3D" type="CollisionShape3D" parent="DetectionArea"]
shape = SubResource("SphereShape3D_detect")

[node name="AttackArea" type="Area3D" parent="."]
collision_layer = 0
collision_mask = 2

[node name="CollisionShape3D" type="CollisionShape3D" parent="AttackArea"]
shape = SubResource("SphereShape3D_attack")

[node name="StateMachine" type="Node" parent="."]
script = ExtResource("5_sm")
InitialStatePath = NodePath("PatrolState")

[node name="PatrolState" type="Node" parent="StateMachine"]
script = ExtResource("6_patrol")

[node name="ChaseState" type="Node" parent="StateMachine"]
script = ExtResource("7_chase")

[node name="AttackState" type="Node" parent="StateMachine"]
script = ExtResource("8_attack")

[node name="AnimationPlayer" type="AnimationPlayer" parent="."]

[node name="AttackCooldown" type="Timer" parent="."]
wait_time = 1.5
one_shot = true

[connection signal="Died" from="HealthComponent" to="." method="OnDied"]
[connection signal="Hurt" from="HurtboxComponent" to="." method="OnHurt"]
[connection signal="body_entered" from="DetectionArea" to="StateMachine/PatrolState" method="OnDetectionBodyEntered"]
[connection signal="body_exited" from="DetectionArea" to="StateMachine/ChaseState" method="OnDetectionBodyExited"]
[connection signal="body_entered" from="AttackArea" to="StateMachine/ChaseState" method="OnAttackRangeEntered"]
```

### Enemy.cs (Orchestrator)
```csharp
using Godot;

public partial class Enemy : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 3.5f;
    [Export] public float RotationSpeed { get; set; } = 5.0f;
    [Export] public float Gravity { get; set; } = 9.8f;

    private HealthComponent _health;
    private NavigationAgent3D _navAgent;

    public override void _Ready()
    {
        _health = GetNode<HealthComponent>("HealthComponent");
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

    public void MoveTo(Vector3 target, double delta)
    {
        _navAgent.TargetPosition = target;
        if (_navAgent.IsNavigationFinished())
            return;

        var nextPos = _navAgent.GetNextPathPosition();
        var direction = (nextPos - GlobalPosition).Normalized();
        direction.Y = 0;

        // Smooth rotation toward movement direction
        if (direction.LengthSquared() > 0.01f)
        {
            var targetRotation = Mathf.Atan2(direction.X, direction.Z);
            Rotation = new Vector3(0,
                Mathf.LerpAngle(Rotation.Y, targetRotation, RotationSpeed * (float)delta),
                0);
        }

        var velocity = Velocity;
        velocity.X = direction.X * MoveSpeed;
        velocity.Z = direction.Z * MoveSpeed;

        if (!IsOnFloor())
            velocity.Y -= Gravity * (float)delta;

        Velocity = velocity;
        MoveAndSlide();
    }

    public void StopMoving()
    {
        Velocity = new Vector3(0, Velocity.Y, 0);
        MoveAndSlide();
    }

    private void OnDied()
    {
        // Play death animation, drop loot, then free
        QueueFree();
    }

    private void OnHurt(int damage)
    {
        _health.TakeDamage(damage);
    }
}
```

### PatrolState.cs
```csharp
using Godot;

public partial class PatrolState : State
{
    [Export] public Vector3[] PatrolPoints { get; set; }
    [Export] public float WaitTime { get; set; } = 2f;

    private int _currentPoint;
    private float _waitTimer;
    private Enemy _enemy;

    public override void Enter()
    {
        _enemy = GetParent().GetParent<Enemy>();
        _currentPoint = 0;
        _waitTimer = 0;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0)
            return;

        var target = _enemy.GlobalPosition + PatrolPoints[_currentPoint];
        var distance = _enemy.GlobalPosition.DistanceTo(target);

        if (distance < 1.0f)
        {
            _waitTimer += (float)delta;
            _enemy.StopMoving();
            if (_waitTimer >= WaitTime)
            {
                _currentPoint = (_currentPoint + 1) % PatrolPoints.Length;
                _waitTimer = 0;
            }
        }
        else
        {
            _enemy.MoveTo(target, delta);
        }
    }

    public void OnDetectionBodyEntered(Node3D body)
    {
        if (body.IsInGroup("player"))
        {
            EmitSignal(SignalName.Transitioned, "ChaseState");
        }
    }
}
```

### ChaseState.cs
```csharp
using Godot;

public partial class ChaseState : State
{
    private Enemy _enemy;
    private Node3D _target;

    public override void Enter()
    {
        _enemy = GetParent().GetParent<Enemy>();
        // Find player in detection area
        var detectionArea = _enemy.GetNode<Area3D>("DetectionArea");
        foreach (var body in detectionArea.GetOverlappingBodies())
        {
            if (body.IsInGroup("player"))
            {
                _target = body;
                break;
            }
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        if (_target == null || !IsInstanceValid(_target))
        {
            EmitSignal(SignalName.Transitioned, "PatrolState");
            return;
        }

        _enemy.MoveTo(_target.GlobalPosition, delta);
    }

    public void OnDetectionBodyExited(Node3D body)
    {
        if (body == _target)
        {
            _target = null;
            EmitSignal(SignalName.Transitioned, "PatrolState");
        }
    }

    public void OnAttackRangeEntered(Node3D body)
    {
        if (body == _target)
        {
            EmitSignal(SignalName.Transitioned, "AttackState");
        }
    }
}
```

### AttackState.cs
```csharp
using Godot;

public partial class AttackState : State
{
    private Enemy _enemy;
    private Timer _cooldown;
    private bool _canAttack = true;

    public override void Enter()
    {
        _enemy = GetParent().GetParent<Enemy>();
        _cooldown = _enemy.GetNode<Timer>("AttackCooldown");
        _cooldown.Timeout += () => _canAttack = true;

        _enemy.StopMoving();
        TryAttack();
    }

    public override void PhysicsUpdate(double delta)
    {
        // Face the player
        var attackArea = _enemy.GetNode<Area3D>("AttackArea");
        var bodies = attackArea.GetOverlappingBodies();
        bool playerInRange = false;

        foreach (var body in bodies)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                var lookDir = (body.GlobalPosition - _enemy.GlobalPosition).Normalized();
                lookDir.Y = 0;
                if (lookDir.LengthSquared() > 0.01f)
                {
                    var targetRot = Mathf.Atan2(lookDir.X, lookDir.Z);
                    _enemy.Rotation = new Vector3(0,
                        Mathf.LerpAngle(_enemy.Rotation.Y, targetRot, 10f * (float)delta), 0);
                }
                break;
            }
        }

        if (!playerInRange)
        {
            EmitSignal(SignalName.Transitioned, "ChaseState");
            return;
        }

        if (_canAttack)
            TryAttack();
    }

    private void TryAttack()
    {
        _canAttack = false;
        _cooldown.Start();
        // Trigger attack animation, hitbox activation, etc.
        GD.Print("Enemy attacks!");
    }

    public override void Exit()
    {
        _cooldown.Timeout -= () => _canAttack = true;
    }
}
```

## Key Takeaways
- **6 reusable components**: HealthComponent, HitboxComponent, HurtboxComponent, StateMachine, DetectionArea, AttackArea
- **3D navigation** via NavigationAgent3D
- **State machine is a composition**: Each state is a child Node with its own script
- **Enemy.cs is thin** (~60 lines) — it just orchestrates components
- **Signal flow**: Detection areas signal states, states signal StateMachine via `Transitioned`
