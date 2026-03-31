# Animation System Reference

Complete reference for Godot 4.6 animation: AnimationPlayer, AnimationTree, BlendSpaces, and StateMachines.

---

## AnimationPlayer

The core node for creating and managing animations. It's a data container holding multiple animations with auto-transition support.

**WARNING**: AnimationPlayer inherits from `Node` (bare), NOT Node2D/Node3D. Child nodes will NOT inherit transforms. Never add transform-dependent nodes as children of AnimationPlayer.

### Track Types

| Track Type | Purpose |
|------------|---------|
| Property | Animate any property (position, modulate, custom exports) |
| Position 3D / Rotation 3D / Scale 3D | Specialized 3D transform tracks |
| Blend Shape | Mesh deformation |
| Call Method | Execute code at specific times |
| Bezier Curve | Smooth interpolated single values |
| Audio Playback | Synchronize sound with animation |
| Animation Playback | Layer animations together |

### Update Modes

| Mode | Behavior |
|------|----------|
| Continuous | Update every frame (default) |
| Discrete | Only update on keyframes |
| Capture | Blends from current value to first keyframe (great for smooth transitions from any state) |

### Interpolation Modes

| Mode | Behavior |
|------|----------|
| Nearest | Snap to nearest keyframe value |
| Linear | Linear interpolation between keyframes |
| Cubic | Cubic interpolation (slower at keyframes, faster between — natural feel) |
| Linear Angle | Linear with shortest-path rotation |
| Cubic Angle | Cubic with shortest-path rotation |

### RESET Animation (Important!)

Create a special animation named `RESET` (case-sensitive) to define default values:
- One keyframe at time 0 for each track
- Used as **reference values for blending** in AnimationTree
- If `Reset On Save` is true, scene saves with RESET applied
- Without RESET, missing tracks in a blend default to zero (e.g., `Vector3.Zero` for position)

```csharp
// Play animation from C#
var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
animPlayer.Play("run");
animPlayer.PlayBackwards("run");
animPlayer.Stop();
animPlayer.Queue("idle"); // Play after current finishes

// Check state
bool isPlaying = animPlayer.IsPlaying();
string current = animPlayer.CurrentAnimation;
float position = (float)animPlayer.CurrentAnimationPosition;

// Seek
animPlayer.Seek(0.5); // Jump to 0.5 seconds

// Speed
animPlayer.SpeedScale = 2.0f; // 2x speed
```

### Animation Markers

Markers let you play sub-sections of an animation by name:

```csharp
// Play from marker "attack_start" to marker "attack_end"
animPlayer.PlaySectionWithMarkers("combat_anim", "attack_start", "attack_end");
```

---

## AnimationTree

AnimationTree does NOT contain animations — it uses animations from an AnimationPlayer. Create animations in AnimationPlayer, then use AnimationTree for blending and state transitions.

### Setup

1. Add AnimationTree node to your scene
2. Set `Anim Player` property to point to your AnimationPlayer
3. Set `Active` to true
4. Choose a root node type

### Root Node Types

| Type | Use Case |
|------|----------|
| `AnimationNodeAnimation` | Play one animation (rarely used as root) |
| `AnimationNodeBlendTree` | Graph of blend nodes (mix, oneshot, etc.) |
| `AnimationNodeBlendSpace1D` | Blend along one axis (walk speed) |
| `AnimationNodeBlendSpace2D` | Blend in 2D space (movement direction + speed) |
| `AnimationNodeStateMachine` | State graph with transitions |

---

## BlendSpace2D (Most Common for 3D Characters)

Blend between animations based on a 2D position. The classic use: movement direction (X) and speed (Y).

```
Setup:
1. AnimationTree root = AnimationNodeBlendSpace2D
2. Place animations at positions:
   - Idle at (0, 0)
   - Walk Forward at (0, 1)
   - Walk Back at (0, -1)
   - Walk Left at (-1, 0)
   - Walk Right at (1, 0)
   - Run Forward at (0, 2)
3. Triangulation auto-generated
```

```csharp
// Control blend position from code
public partial class Player : CharacterBody3D
{
    private AnimationTree _animTree;

    public override void _Ready()
    {
        _animTree = GetNode<AnimationTree>("AnimationTree");
    }

    public override void _PhysicsProcess(double delta)
    {
        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var velocity = new Vector2(input.X, input.Y) * Speed;

        // Set blend position — AnimationTree handles the blending
        _animTree.Set("parameters/BlendSpace2D/blend_position", velocity);
    }
}
```

### Blend Modes

| Mode | Use |
|------|-----|
| Continuous | Smooth interpolation inside triangles (default, best for 3D) |
| Discrete | Snap between animations (for 2D frame-by-frame) |
| Carry | Like Discrete but maintains playback position |

---

## AnimationTree StateMachine

State graph with named states and transitions. Most flexible for complex character animation.

### Setup in Editor

1. Set AnimationTree root to `AnimationNodeStateMachine`
2. Right-click to add states (each state = an animation, BlendSpace, BlendTree, or nested StateMachine)
3. Connect states with transition arrows
4. Set transition properties (type, crossfade, conditions)

### Transition Types

| Type | Behavior |
|------|----------|
| Immediate | Switch instantly |
| Sync | Switch instantly, seek new state to old state's position |
| At End | Wait for current state to finish, then switch |

### Transition Properties

| Property | Description |
|----------|-------------|
| Xfade Time | Cross-fade duration in seconds |
| Xfade Curve | Curve resource for non-linear crossfade |
| Reset | `true` = play from start, `false` = continue |
| Priority | Lower = preferred path for `Travel()` |
| Advance Mode | `Auto` (use condition), `Enabled` (only during travel), `Disabled` |

### Advance Expressions (Godot 4)

Evaluate any boolean expression to trigger transitions automatically:

```
is_walking
velocity.Length() > 0.1
is_on_floor && !is_jumping
health <= 0
```

**CRITICAL**: The **Advance Expression Base Node** must point to the node with your script variables. Set it in the AnimationTree inspector. Expression property names are case-sensitive and must match your script (PascalCase for C#).

### Controlling from C#

```csharp
public partial class Player : CharacterBody3D
{
    private AnimationTree _animTree;
    private AnimationNodeStateMachinePlayback _stateMachine;

    // Properties read by Advance Expressions
    public bool IsWalking { get; set; }
    public bool IsJumping { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsOnFloor => base.IsOnFloor();
    public float Speed => Velocity.Length();

    public override void _Ready()
    {
        _animTree = GetNode<AnimationTree>("AnimationTree");
        _stateMachine = (AnimationNodeStateMachinePlayback)
            _animTree.Get("parameters/playback");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Update state variables — AnimationTree reads them via Advance Expressions
        IsWalking = Velocity.LengthSquared() > 0.01f;

        // Or manually trigger transitions
        if (Input.IsActionJustPressed("attack"))
        {
            _stateMachine.Travel("Attack");
        }
    }

    // Travel uses A* to find path through intermediate states
    public void PlayAnimation(string stateName)
    {
        _stateMachine.Travel(stateName);
    }

    // Get current state
    public string GetCurrentState()
    {
        return _stateMachine.GetCurrentNode();
    }
}
```

**GOTCHA**: StateMachine must be running before `Travel()` works. Either connect a state to the `Start` node, or call `Start("StateName")`.

---

## Blend Tree Nodes

### OneShot (Play Once, Return)

Perfect for attack animations layered over movement:

```csharp
// Trigger one-shot animation
_animTree.Set("parameters/OneShot/request",
    (int)AnimationNodeOneShot.OneShotRequest.Fire);

// Abort one-shot
_animTree.Set("parameters/OneShot/request",
    (int)AnimationNodeOneShot.OneShotRequest.Abort);

// Check if active
bool isActive = (bool)_animTree.Get("parameters/OneShot/active");
```

### TimeSeek

```csharp
// Seek to specific time in seconds
_animTree.Set("parameters/TimeSeek/seek_request", 0.0); // Start
_animTree.Set("parameters/TimeSeek/seek_request", 12.0); // 12 seconds in
```

### TimeScale

```csharp
// Speed multiplier (0 = pause, negative = backwards)
_animTree.Set("parameters/TimeScale/scale", 2.0); // 2x speed
```

### Transition (Simplified StateMachine)

```csharp
// Switch to state by name
_animTree.Set("parameters/Transition/transition_request", "state_2");

// Read current state
string currentState = (string)_animTree.Get("parameters/Transition/current_state");
int currentIndex = (int)_animTree.Get("parameters/Transition/current_index");
```

---

## Root Motion

Extract motion from the root bone for physics-based character movement:

```csharp
public override void _PhysicsProcess(double delta)
{
    // Get motion delta from animation
    Vector3 rootMotion = _animTree.GetRootMotionPosition();
    Quaternion rootRotation = _animTree.GetRootMotionRotation();

    // Apply to CharacterBody3D
    Velocity = rootMotion / (float)delta;
    MoveAndSlide();
}
```

Use `RootMotionView` node for a debug floor visualization.

---

## Blending Gotchas

1. **Missing tracks blend to zero**: If animation A has a Position track but B doesn't, B treats position as `Vector3.Zero`. Fix by adding all tracks to the `RESET` animation.

2. **Rotation limit**: Rotation 3D tracks with Angle interpolation prevent >180 degree rotation from initial value (prevents bone penetration). Import humanoid models in **T-pose** for best range.

3. **Shared resources**: Animation nodes are resources shared between ALL instances. Set values through `AnimationTree.Set("parameters/...")`, NOT by modifying the node resource directly.

---

## Common Animation Setup (3D Character)

```
Player (CharacterBody3D)
├── Model (Node3D)
│   ├── Imported3DScene.glb
│   └── AnimationPlayer (auto-created by import)
├── AnimationTree
│   └── Root: StateMachine
│       ├── Idle (Animation)
│       ├── Locomotion (BlendSpace2D)
│       │   ├── Idle at (0,0)
│       │   ├── Walk at (0,1)
│       │   ├── Run at (0,2)
│       │   ├── StrafeLeft at (-1,0)
│       │   └── StrafeRight at (1,0)
│       ├── Jump (Animation)
│       ├── Fall (Animation)
│       ├── Attack (OneShot over Locomotion)
│       └── Death (Animation)
├── HealthComponent.tscn
├── MovementComponent3D.tscn
└── CollisionShape3D
```
