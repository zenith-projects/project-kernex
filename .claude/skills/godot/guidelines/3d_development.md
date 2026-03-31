# 3D Game Development Reference

Complete reference for Godot 4.6 3D game development. Sourced from official documentation.

---

## Coordinate System

Godot uses the **metric system**: 1 unit = 1 meter.

**Right-handed, Y-up coordinate system:**
- **X axis** (Red): Sides (left/right)
- **Y axis** (Green): Up/down (vertical)
- **Z axis** (Blue): Depth (front/back) -- forward direction for lights and cameras

`Node3D` is the base node for all 3D objects. Each has a local transform relative to its parent.

---

## Transforms (Transform3D)

### Structure
```
Transform3D:
├── basis: Basis (3x3 rotation/scale matrix)
│   ├── basis.X = (1, 0, 0)  → Right direction
│   ├── basis.Y = (0, 1, 0)  → Up direction
│   └── basis.Z = (0, 0, 1)  → Forward direction
└── origin: Vector3           → Position relative to parent
```

### Euler Angle Problems (Official Warning)

From official docs: **"You should NOT use the `rotation` property of Node3D nodes for games."**

Problems:
- **Axis order dependency**: X-then-Y-then-Z produces different results than Y-then-Z-then-X
- **Gimbal lock**: Certain rotations cause loss of rotational freedom
- **Interpolation**: Interpolating angles doesn't take the shortest path

### Working Without Angles (C#)

```csharp
// Fire in facing direction
var bulletDirection = -GlobalTransform.Basis.Z;

// Check if enemy faces player
var facingDot = GlobalTransform.Basis.Z.Dot(
    (player.GlobalPosition - GlobalPosition).Normalized());
// dot > 0 = facing toward, dot < 0 = facing away

// Move left relative to self
var leftDir = -GlobalTransform.Basis.X;

// Smooth rotation with quaternion slerp
var currentQuat = new Quaternion(GlobalTransform.Basis);
var targetQuat = new Quaternion(targetTransform.Basis);
var interpolated = currentQuat.Slerp(targetQuat, 5f * (float)delta);
GlobalTransform = new Transform3D(new Basis(interpolated), GlobalPosition);
```

### FPS Camera Pattern (C#)

```csharp
// Store accumulated angles, rebuild every frame to avoid precision drift
private float _rotX;
private float _rotY;

public override void _UnhandledInput(InputEvent @event)
{
    if (@event is InputEventMouseMotion motion)
    {
        _rotX -= motion.Relative.X * MouseSensitivity;
        _rotY -= motion.Relative.Y * MouseSensitivity;
        _rotY = Mathf.Clamp(_rotY, Mathf.DegToRad(-89), Mathf.DegToRad(89));

        // Reset and rebuild every frame
        Transform = new Transform3D(Basis.Identity, Position);
        RotateObjectLocal(Vector3.Up, _rotX);    // Y rotation first
        RotateObjectLocal(Vector3.Right, _rotY);  // X rotation second
    }
}
```

### Precision Management

```csharp
// Fix floating-point drift after many rotations
Transform = Transform.Orthonormalized();

// If scale must be preserved
var scale = Scale;
Transform = Transform.Orthonormalized();
Scale = scale;
```

---

## Physics Body Types

### StaticBody3D
- NOT moved by physics engine. Participates in collision but doesn't move.
- Properties: `constant_linear_velocity`, `constant_angular_velocity` (for conveyors)
- **Use for**: Walls, floors, platforms, terrain, conveyor belts

### RigidBody3D
- Fully simulated by physics. Apply forces, don't set position directly.
- Properties: `mass`, `friction`, `bounce`
- Bodies enter sleep state when inactive (saves CPU)
- **Use for**: Crates, balls, ragdolls, any physics-driven object

```csharp
// Apply force in _IntegrateForces (NOT _PhysicsProcess)
public override void _IntegrateForces(PhysicsDirectBodyState3D state)
{
    if (Input.IsActionPressed("push"))
    {
        var thrust = -Transform.Basis.Z * 250f;
        state.ApplyForce(thrust);
    }
}
```

### CharacterBody3D
- Collision detection WITHOUT physics simulation
- ALL movement must be coded manually
- **Use for**: Player, enemies, NPCs -- anything needing precise movement control

### Area3D
- Detection and influence node (triggers, pickups, damage zones)
- Can override physics parameters (gravity, damping) in a region
- **Use for**: Trigger zones, damage areas, gravity fields, item pickups

### Collision Shapes

**NEVER scale collision shapes via the Node3D scale property** -- only use the shape's own size handles/properties.

Common 3D shapes:
- `BoxShape3D`, `SphereShape3D`, `CapsuleShape3D`, `CylinderShape3D`
- `ConvexPolygonShape3D`, `ConcavePolygonShape3D`
- `WorldBoundaryShape3D`

---

## CharacterBody3D Movement (C#)

### 3D Platformer

```csharp
public partial class Player3D : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public float JumpSpeed { get; set; } = 4.5f;

    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= _gravity * (float)delta;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpSpeed;

        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
```

### Key CharacterBody3D Properties

| Property | Default | Description |
|----------|---------|-------------|
| `floor_max_angle` | 0.785 (45deg) | Angle threshold: floor vs wall |
| `floor_snap_length` | 0.1 | Distance to keep body on slopes |
| `floor_stop_on_slope` | true | Prevents sliding when stationary |
| `floor_constant_speed` | false | Maintains constant speed on slopes |
| `motion_mode` | GROUNDED | GROUNDED (platformer) or FLOATING (space) |
| `max_slides` | 6 | Max direction changes per move_and_slide |
| `up_direction` | (0,1,0) | Defines "up" for floor/wall/ceiling |

### move_and_slide vs move_and_collide

| Method | Use Case | Notes |
|--------|----------|-------|
| `MoveAndSlide()` | Player, NPC movement | Auto-slides along obstacles. **Does NOT need delta multiplication** -- it's built in. |
| `MoveAndCollide(vel * delta)` | Bullets, custom physics | Stops on collision. Returns `KinematicCollision3D`. You handle response. |

### Iterating Collisions

```csharp
MoveAndSlide();
for (int i = 0; i < GetSlideCollisionCount(); i++)
{
    var collision = GetSlideCollision(i);
    var collider = collision.GetCollider();
    if (collider is RigidBody3D rigidBody)
    {
        rigidBody.ApplyCentralImpulse(-collision.GetNormal() * 5f);
    }
}
```

---

## Navigation System (3D)

### Setup

1. Add `NavigationRegion3D` to scene
2. Attach `NavigationMesh` resource
3. Create `MeshInstance3D` child with floor geometry
4. Bake navmesh via editor button
5. Add `NavigationAgent3D` as child of the moving entity

### CRITICAL: Await First Physics Frame

**The NavigationServer has NOT synced on the first frame.** Path queries return empty until after the first physics frame:

```csharp
public override void _Ready()
{
    _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    _navigationAgent.PathDesiredDistance = 0.5f;
    _navigationAgent.TargetDesiredDistance = 0.5f;
    Callable.From(ActorSetup).CallDeferred();
}

private async void ActorSetup()
{
    await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    _navigationAgent.TargetPosition = _targetPosition;
}
```

### Basic Navigation Movement (C#)

```csharp
public override void _PhysicsProcess(double delta)
{
    if (_navigationAgent.IsNavigationFinished())
        return;

    var nextPos = _navigationAgent.GetNextPathPosition();
    var direction = GlobalPosition.DirectionTo(nextPos);
    Velocity = direction * MovementSpeed;
    MoveAndSlide();
}
```

### Navigation with Avoidance (C#)

```csharp
public override void _Ready()
{
    _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    _navigationAgent.VelocityComputed += OnVelocityComputed;
}

public override void _PhysicsProcess(double delta)
{
    if (NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        return; // Server not synced yet
    if (_navigationAgent.IsNavigationFinished())
        return;

    var nextPos = _navigationAgent.GetNextPathPosition();
    var newVelocity = GlobalPosition.DirectionTo(nextPos) * MovementSpeed;

    if (_navigationAgent.AvoidanceEnabled)
        _navigationAgent.Velocity = newVelocity; // Triggers VelocityComputed
    else
        OnVelocityComputed(newVelocity);
}

private void OnVelocityComputed(Vector3 safeVelocity)
{
    Velocity = safeVelocity;
    MoveAndSlide();
}
```

### Common Navigation Problems
- **Empty path**: Queried before map sync -- use `CallDeferred` or `await PhysicsFrame`
- **Agent dancing**: `PathMaxDistance` too short, causing constant recalculation
- **Agent backtracking**: Speed too high, overshoots `PathDesiredDistance` check
- **Agent facing backward**: Precision issues when agent sits on mesh edges

---

## Collision Layers and Masks

32 available physics layers. Configure names in Project Settings > Layer Names > 3D Physics.

- **collision_layer**: Which layers this object EXISTS ON (what it IS)
- **collision_mask**: Which layers this object SCANS FOR (what it interacts with)

Collision occurs only when A's mask includes B's layer (or vice versa).

```csharp
// Method-based approach (preferred for clarity)
SetCollisionLayerValue(1, true);   // Exists on layer 1
SetCollisionMaskValue(2, true);    // Detects layer 2
SetCollisionMaskValue(3, true);    // Detects layer 3
```

---

## Lighting and Environment

### Light Types
| Light | Use For |
|-------|---------|
| `DirectionalLight3D` | Sun/moon, outdoor scenes (infinite distance) |
| `OmniLight3D` | Point light in all directions (lamps, torches) |
| `SpotLight3D` | Cone-shaped directed light (flashlights) |

### Environment Setup

Every 3D scene should have:
```
WorldEnvironment (Node)     ← holds Environment resource
DirectionalLight3D          ← sun/main light
```

**CameraAttributes** (separated from Environment in Godot 4):
- `CameraAttributesPractical`: Arbitrary units, suitable for most games
- `CameraAttributesPhysical`: Real-world camera units for photorealism

### Tonemapping

| Algorithm | Characteristics |
|-----------|----------------|
| Linear | Fastest, clips bright values |
| Reinhard | Simple curve, may look dull |
| Filmic | Better contrast than Reinhard |
| ACES | High-contrast, realistic desaturation |
| AgX | Best hue preservation, slowest |

### Global Illumination Options
- **VoxelGI**: Good for interiors
- **SDFGI**: Good for large open scenes (Forward+ only)
- **LightmapGI**: Baked lightmaps for static scenes

### Camera Systems

```
Third-person camera composition:
CameraRig (Node3D)
├── SpringArm3D           ← collision-aware arm, shortens near walls
│   └── Camera3D          ← actual camera
```

**Near/Far planes**: Keep near as large as possible and far as small as possible to maximize depth buffer precision.

---

## 3D Scene Composition

### Typical 3D Level Structure
```
Level (Node3D)
├── WorldEnvironment
├── DirectionalLight3D
├── NavigationRegion3D
│   └── MeshInstance3D (terrain/floor)
├── Player.tscn (CharacterBody3D)
├── Enemies (Node3D)
│   ├── Enemy1.tscn
│   └── Enemy2.tscn
├── Props (Node3D)
│   ├── Torch.tscn
│   └── Chest.tscn
└── UI (CanvasLayer)
    └── HUD.tscn
```

### 3D Character Composition
```
Player (CharacterBody3D)
├── CollisionShape3D (CapsuleShape3D)
├── Model (Node3D)
│   ├── MeshInstance3D / imported .glb
│   └── AnimationPlayer
├── CameraRig.tscn
│   ├── SpringArm3D
│   └── Camera3D
├── HealthComponent.tscn
├── MovementComponent3D.tscn
├── HitboxComponent3D.tscn (Area3D)
│   └── CollisionShape3D
├── HurtboxComponent3D.tscn (Area3D)
│   └── CollisionShape3D
├── NavigationAgent3D
├── InteractionRay (RayCast3D)
└── StateMachine.tscn
```

### Optimization Techniques

| Technique | Use Case |
|-----------|----------|
| `MultiMeshInstance3D` | Thousands of identical objects (grass, trees) |
| Mesh LOD | Auto quality scaling by distance |
| Visibility ranges (HLOD) | Hierarchical level-of-detail |
| Occlusion culling | Skip rendering hidden geometry |
| Resolution scaling | Dynamic resolution for performance |

### Level Design Tools
- **CSG** (Constructive Solid Geometry): Boolean operations for prototyping
- **GridMaps**: Grid-based level construction using mesh libraries

### Asset Import Formats
- **glTF 2.0** (recommended), `.blend` files (direct), DAE, OBJ, FBX

---

## 3D Rendering Gotchas

### Z-Fighting
When two surfaces overlap at the same depth buffer position, textures flicker. Fix:
- Increase Camera `Near` property (0.1 or higher)
- Decrease Camera `Far` property
- Offset surfaces slightly

### Transparency Sorting
Transparent materials sort by Node3D origin position, NOT by vertex. Solutions:
- **Alpha Scissor**: For mostly opaque/transparent textures (binary alpha)
- **Alpha Hash**: Dithered transparency
- **Depth Pre-Pass**: Writes to depth buffer first

### Texture Size Limits
- Desktop GPUs: up to 8192x8192 (older hardware may be less)
- Mobile devices: typically 4096x4096 max
- For cross-platform safety: keep textures <= 4096x4096
