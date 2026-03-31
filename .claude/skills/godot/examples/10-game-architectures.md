# Example 10: Complete Game Architecture Patterns

Here is a comprehensive collection of 9 game architecture patterns for Godot 4 with C#. Every pattern includes complete code, scene trees, integration notes, and gotchas.

---

# 1. Platformer Architecture -- Complete Scene/Script Structure

## Recommended Project Layout

```
res://
├── Autoloads/
│   ├── GameManager.cs          # Global state, score, lives
│   ├── AudioManager.cs         # Sound effects & music
│   └── SceneTransitioner.cs    # Fade transitions (see pattern #9)
├── Scenes/
│   ├── Levels/
│   │   ├── Level01.tscn
│   │   ├── Level02.tscn
│   │   └── TileSet.tres
│   ├── Entities/
│   │   ├── Player/
│   │   │   ├── Player.tscn
│   │   │   ├── Player.cs
│   │   │   └── PlayerStateMachine.cs
│   │   └── Enemies/
│   │       ├── Slime.tscn
│   │       ├── Slime.cs
│   │       └── EnemyBase.cs
│   └── UI/
│       ├── HUD.tscn
│       ├── HUD.cs
│       ├── PauseMenu.tscn
│       └── MainMenu.tscn
└── Resources/
    ├── Sprites/
    ├── Audio/
    └── Themes/
```

## Scene Tree -- Level01.tscn

```
Level01 (Node2D)
├── TileMapLayer                  # Ground, walls, platforms
├── ParallaxBackground
│   └── ParallaxLayer
│       └── Sprite2D              # Background art
├── Player (CharacterBody2D)      # Instance of Player.tscn
│   ├── CollisionShape2D          # CapsuleShape2D
│   ├── AnimatedSprite2D
│   ├── CoyoteTimer (Timer)       # Coyote time for jumps
│   ├── JumpBufferTimer (Timer)   # Jump buffering
│   └── Camera2D
│       └── RemoteTransform2D     # Optional: decouple camera
├── Enemies (Node2D)
│   ├── Slime
│   └── Slime2
├── Collectibles (Node2D)
│   ├── Coin
│   └── Coin2
├── Hazards (Node2D)
│   └── Spikes (Area2D)
└── CanvasLayer
    └── HUD                       # Instance of HUD.tscn
```

## Player.cs -- State Machine Based

```csharp
using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 200f;
    [Export] public float JumpVelocity = -350f;
    [Export] public float Gravity = 980f;
    [Export] public float CoyoteTime = 0.1f;
    [Export] public float JumpBufferTime = 0.1f;
    [Export] public float Acceleration = 1500f;
    [Export] public float Friction = 1200f;

    private AnimatedSprite2D _sprite;
    private Timer _coyoteTimer;
    private Timer _jumpBufferTimer;
    private bool _wasOnFloor;
    private bool _coyoteAvailable;
    private bool _jumpBuffered;

    private enum State { Idle, Run, Jump, Fall }
    private State _currentState = State.Idle;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _coyoteTimer = GetNode<Timer>("CoyoteTimer");
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");

        _coyoteTimer.WaitTime = CoyoteTime;
        _coyoteTimer.OneShot = true;
        _coyoteTimer.Timeout += () => _coyoteAvailable = false;

        _jumpBufferTimer.WaitTime = JumpBufferTime;
        _jumpBufferTimer.OneShot = true;
        _jumpBufferTimer.Timeout += () => _jumpBuffered = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector2 vel = Velocity;

        // Gravity
        if (!IsOnFloor())
            vel.Y += Gravity * dt;

        // Coyote time: was on floor last frame, now falling
        if (_wasOnFloor && !IsOnFloor() && vel.Y >= 0)
        {
            _coyoteAvailable = true;
            _coyoteTimer.Start();
        }

        // Jump buffer: pressed jump while in air
        if (Input.IsActionJustPressed("jump") && !IsOnFloor())
        {
            _jumpBuffered = true;
            _jumpBufferTimer.Start();
        }

        // Jump
        bool canJump = IsOnFloor() || _coyoteAvailable;
        bool wantsJump = Input.IsActionJustPressed("jump") || _jumpBuffered;
        if (wantsJump && canJump)
        {
            vel.Y = JumpVelocity;
            _coyoteAvailable = false;
            _jumpBuffered = false;
        }

        // Variable jump height: release early = lower jump
        if (Input.IsActionJustReleased("jump") && vel.Y < 0)
            vel.Y *= 0.5f;

        // Horizontal movement
        float direction = Input.GetAxis("move_left", "move_right");
        if (direction != 0)
        {
            vel.X = Mathf.MoveToward(vel.X, direction * Speed, Acceleration * dt);
            _sprite.FlipH = direction < 0;
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, Friction * dt);
        }

        Velocity = vel;
        MoveAndSlide();

        _wasOnFloor = IsOnFloor();
        UpdateState();
    }

    private void UpdateState()
    {
        State newState;
        if (!IsOnFloor())
            newState = Velocity.Y < 0 ? State.Jump : State.Fall;
        else if (Mathf.Abs(Velocity.X) > 10f)
            newState = State.Run;
        else
            newState = State.Idle;

        if (newState != _currentState)
        {
            _currentState = newState;
            _sprite.Play(_currentState.ToString().ToLower());
        }
    }

    public void TakeDamage()
    {
        GameManager.Instance.LoseLife();
        // Knockback, invincibility frames, etc.
    }
}
```

## GameManager.cs (Autoload)

```csharp
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Lives { get; private set; } = 3;

    [Signal] public delegate void ScoreChangedEventHandler(int newScore);
    [Signal] public delegate void LivesChangedEventHandler(int newLives);
    [Signal] public delegate void GameOverEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        Score += amount;
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    public void LoseLife()
    {
        Lives--;
        EmitSignal(SignalName.LivesChanged, Lives);
        if (Lives <= 0)
            EmitSignal(SignalName.GameOver);
    }

    public void ResetGame()
    {
        Score = 0;
        Lives = 3;
    }
}
```

## Gotchas

- **Input Map**: You must define `jump`, `move_left`, `move_right` in Project > Project Settings > Input Map. The code will silently fail otherwise.
- **TileMapLayer vs TileMap**: Godot 4.3+ deprecated the old `TileMap` node. Use `TileMapLayer` instead.
- **Physics frame independence**: Always multiply by `delta` for gravity and acceleration. `MoveAndSlide()` already uses the physics delta internally for velocity.
- **Coyote time direction**: Only grant coyote time when falling (vel.Y >= 0), not when jumping off an edge.

---

# 2. Top-Down RPG Architecture

## Project Layout

```
res://
├── Autoloads/
│   ├── GameManager.cs
│   ├── DialogueManager.cs      # See pattern #7
│   ├── InventoryManager.cs
│   └── QuestManager.cs
├── Data/
│   ├── Items/
│   │   ├── Sword.tres           # Custom Resource
│   │   └── HealthPotion.tres
│   ├── Enemies/
│   │   └── SlimeData.tres
│   └── Quests/
│       └── Quest01.tres
├── Scenes/
│   ├── World/
│   │   ├── Overworld.tscn
│   │   ├── DungeonFloor1.tscn
│   │   └── Town.tscn
│   ├── Entities/
│   │   ├── Player/
│   │   │   ├── Player.tscn
│   │   │   └── Player.cs
│   │   ├── NPC/
│   │   │   ├── NPC.tscn
│   │   │   └── NPC.cs
│   │   └── Enemies/
│   │       ├── EnemyBase.tscn
│   │       └── EnemyBase.cs
│   └── UI/
│       ├── InventoryUI.tscn
│       ├── DialogueBox.tscn
│       └── QuestLog.tscn
└── Resources/
```

## Scene Tree -- Overworld.tscn

```
Overworld (Node2D)
├── TileMapLayer                  # Ground tiles
├── YSortGroup (Node2D)           # Everything that needs Y-sorting
│   ├── Player (CharacterBody2D)
│   │   ├── CollisionShape2D
│   │   ├── AnimatedSprite2D
│   │   ├── InteractionArea (Area2D)
│   │   │   └── CollisionShape2D  # Interaction range
│   │   ├── HurtBox (Area2D)
│   │   │   └── CollisionShape2D
│   │   └── Camera2D
│   ├── NPCs (Node2D)
│   │   └── NPC_Villager (CharacterBody2D)
│   │       ├── CollisionShape2D
│   │       ├── AnimatedSprite2D
│   │       └── InteractionArea (Area2D)
│   └── Enemies (Node2D)
│       └── Slime (CharacterBody2D)
├── Doors (Node2D)                # Area2D triggers for zone transitions
│   └── DungeonEntrance (Area2D)
└── CanvasLayer
    ├── HUD
    └── DialogueBox
```

## Data-Driven Items with Custom Resources

```csharp
// ItemData.cs
using Godot;

[GlobalClass]  // Makes it visible in the Godot editor
public partial class ItemData : Resource
{
    [Export] public string ItemName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }
    [Export] public int MaxStack { get; set; } = 99;
    [Export] public ItemType Type { get; set; } = ItemType.Consumable;
    [Export] public int Value { get; set; } = 0;

    // For equipment
    [Export] public int AttackBonus { get; set; } = 0;
    [Export] public int DefenseBonus { get; set; } = 0;
}

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    KeyItem
}
```

## Inventory System

```csharp
// InventoryManager.cs (Autoload)
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; }

    [Signal] public delegate void InventoryChangedEventHandler();

    private readonly List<InventorySlot> _slots = new();
    public int MaxSlots { get; set; } = 20;

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public override void _Ready()
    {
        Instance = this;
        // Initialize empty slots
        for (int i = 0; i < MaxSlots; i++)
            _slots.Add(new InventorySlot());
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        // Try stacking first
        var existing = _slots.FirstOrDefault(
            s => s.Item == item && s.Quantity < item.MaxStack);

        if (existing != null)
        {
            int canAdd = Mathf.Min(quantity, item.MaxStack - existing.Quantity);
            existing.Quantity += canAdd;
            quantity -= canAdd;
        }

        // Remaining goes into empty slots
        while (quantity > 0)
        {
            var empty = _slots.FirstOrDefault(s => s.Item == null);
            if (empty == null)
                return false;  // Inventory full

            empty.Item = item;
            empty.Quantity = Mathf.Min(quantity, item.MaxStack);
            quantity -= empty.Quantity;
        }

        EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    public void RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;
        var slot = _slots[slotIndex];
        if (slot.Item == null) return;

        slot.Quantity -= quantity;
        if (slot.Quantity <= 0)
        {
            slot.Item = null;
            slot.Quantity = 0;
        }
        EmitSignal(SignalName.InventoryChanged);
    }
}

public class InventorySlot
{
    public ItemData Item { get; set; }
    public int Quantity { get; set; }
}
```

## Top-Down Player with 4-Direction Animation

```csharp
// Player.cs
using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 120f;

    private AnimatedSprite2D _sprite;
    private Area2D _interactionArea;
    private Vector2 _facingDirection = Vector2.Down;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _interactionArea = GetNode<Area2D>("InteractionArea");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector(
            "move_left", "move_right", "move_up", "move_down");

        Velocity = input * Speed;
        MoveAndSlide();

        UpdateAnimation(input);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            TryInteract();
            GetViewport().SetInputAsHandled();
        }
    }

    private void UpdateAnimation(Vector2 input)
    {
        if (input == Vector2.Zero)
        {
            _sprite.Play("idle_" + DirectionName(_facingDirection));
            return;
        }

        // Update facing direction based on dominant axis
        if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
            _facingDirection = input.X > 0 ? Vector2.Right : Vector2.Left;
        else
            _facingDirection = input.Y > 0 ? Vector2.Down : Vector2.Up;

        _sprite.Play("walk_" + DirectionName(_facingDirection));
    }

    private string DirectionName(Vector2 dir)
    {
        if (dir == Vector2.Down) return "down";
        if (dir == Vector2.Up) return "up";
        if (dir == Vector2.Left) return "left";
        return "right";
    }

    private void TryInteract()
    {
        var bodies = _interactionArea.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is IInteractable interactable)
            {
                interactable.Interact(this);
                return;
            }
        }
    }
}

public interface IInteractable
{
    void Interact(Player player);
}
```

## NPC.cs

```csharp
using Godot;

public partial class NPC : CharacterBody2D, IInteractable
{
    [Export] public string[] DialogueLines { get; set; } = System.Array.Empty<string>();
    [Export] public string NpcName { get; set; } = "Villager";

    public void Interact(Player player)
    {
        DialogueManager.Instance.StartDialogue(NpcName, DialogueLines);
    }
}
```

## Gotchas

- **Y-Sorting**: Enable `Y Sort Enabled` on the parent Node2D that contains all entities. Do NOT set it on each individual entity; set it on their shared parent container.
- **[GlobalClass]**: Required for custom resources to appear in Godot's "New Resource" dropdown. Without it, you can only create them in code.
- **Resource sharing**: Resources are shared by reference by default. If you modify a `tres` resource at runtime, every node referencing it sees the change. Use `resource.Duplicate()` if you need a unique copy.
- **Area2D interaction range**: Position the InteractionArea slightly in front of the player, offset in the facing direction. A circle shape with radius ~16-24px works well for pixel-art games.

---

# 3. FPS Game Architecture

## Scene Tree -- FPSLevel.tscn

```
FPSLevel (Node3D)
├── WorldEnvironment
│   └── Environment (sky, fog, tonemap)
├── DirectionalLight3D
├── NavigationRegion3D
│   └── LevelGeometry (Node3D)
│       ├── MeshInstance3D (floors, walls)
│       └── StaticBody3D + CollisionShape3D
├── Player (CharacterBody3D)
│   ├── CollisionShape3D            # CapsuleShape3D, height 2m
│   ├── CameraMount (Node3D)        # Y offset ~1.6m, used for head bob
│   │   └── Camera3D
│   │       ├── RayCast3D            # For hitscan weapons
│   │       ├── WeaponHolder (Node3D)
│   │       │   └── WeaponModel (MeshInstance3D)
│   │       └── CrosshairCenter (CenterContainer)
│   └── StepRayCast (RayCast3D)     # Stair stepping (optional)
├── Enemies (Node3D)
│   └── Enemy01 (CharacterBody3D)
└── CanvasLayer
    └── HUD
        ├── Crosshair
        ├── HealthBar
        └── AmmoCounter
```

## Player.cs -- FPS Controller

```csharp
using Godot;

public partial class FPSPlayer : CharacterBody3D
{
    [Export] public float Speed = 5.0f;
    [Export] public float SprintMultiplier = 1.6f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float HeadBobFrequency = 2.4f;
    [Export] public float HeadBobAmplitude = 0.08f;

    private Camera3D _camera;
    private Node3D _cameraMount;
    private float _gravity;
    private double _headBobTime;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("CameraMount/Camera3D");
        _cameraMount = GetNode<Node3D>("CameraMount");
        _gravity = (float)ProjectSettings.GetSetting(
            "physics/3d/default_gravity");

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            // Horizontal rotation: rotate the whole body
            RotateY(-motion.Relative.X * MouseSensitivity);

            // Vertical rotation: rotate only the camera mount
            _cameraMount.RotateX(-motion.Relative.Y * MouseSensitivity);

            // Clamp vertical look to prevent flipping
            Vector3 rot = _cameraMount.Rotation;
            rot.X = Mathf.Clamp(rot.X,
                Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
            _cameraMount.Rotation = rot;
        }

        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector3 vel = Velocity;

        // Gravity
        if (!IsOnFloor())
            vel.Y -= _gravity * dt;

        // Jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            vel.Y = JumpVelocity;

        // Movement
        Vector2 inputDir = Input.GetVector(
            "move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis *
            new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        float currentSpeed = Speed;
        if (Input.IsActionPressed("sprint"))
            currentSpeed *= SprintMultiplier;

        if (direction != Vector3.Zero)
        {
            vel.X = direction.X * currentSpeed;
            vel.Z = direction.Z * currentSpeed;
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, currentSpeed);
            vel.Z = Mathf.MoveToward(vel.Z, 0, currentSpeed);
        }

        Velocity = vel;
        MoveAndSlide();

        // Head bob
        if (IsOnFloor() && direction != Vector3.Zero)
        {
            _headBobTime += delta * HeadBobFrequency;
            Vector3 mountPos = _cameraMount.Position;
            mountPos.Y = 1.6f + Mathf.Sin(
                (float)_headBobTime * Mathf.Tau) * HeadBobAmplitude;
            _cameraMount.Position = mountPos;
        }
        else
        {
            _headBobTime = 0;
            Vector3 mountPos = _cameraMount.Position;
            mountPos.Y = Mathf.Lerp(mountPos.Y, 1.6f, dt * 10f);
            _cameraMount.Position = mountPos;
        }
    }
}
```

## Hitscan Weapon

```csharp
using Godot;

public partial class HitscanWeapon : Node3D
{
    [Export] public int Damage = 25;
    [Export] public float Range = 100f;
    [Export] public float FireRate = 0.1f;  // Seconds between shots
    [Export] public int MaxAmmo = 30;

    private RayCast3D _ray;
    private Timer _fireTimer;
    private int _currentAmmo;
    private bool _canFire = true;

    [Signal] public delegate void AmmoChangedEventHandler(
        int current, int max);

    public override void _Ready()
    {
        _ray = GetNode<RayCast3D>("../RayCast3D");  // Sibling under Camera3D
        _ray.TargetPosition = new Vector3(0, 0, -Range);
        _currentAmmo = MaxAmmo;

        _fireTimer = new Timer();
        AddChild(_fireTimer);
        _fireTimer.WaitTime = FireRate;
        _fireTimer.OneShot = true;
        _fireTimer.Timeout += () => _canFire = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("fire") && _canFire && _currentAmmo > 0)
            Fire();
        if (@event.IsActionPressed("reload"))
            Reload();
    }

    private void Fire()
    {
        _canFire = false;
        _currentAmmo--;
        _fireTimer.Start();
        EmitSignal(SignalName.AmmoChanged, _currentAmmo, MaxAmmo);

        // Force raycast update for this frame
        _ray.ForceRaycastUpdate();

        if (_ray.IsColliding())
        {
            GodotObject hit = _ray.GetCollider();
            Vector3 hitPoint = _ray.GetCollisionPoint();
            Vector3 hitNormal = _ray.GetCollisionNormal();

            if (hit is IDamageable damageable)
                damageable.TakeDamage(Damage, hitPoint, hitNormal);

            // Spawn impact effect at hitPoint (use object pool, pattern #8)
        }
    }

    private void Reload()
    {
        _currentAmmo = MaxAmmo;
        EmitSignal(SignalName.AmmoChanged, _currentAmmo, MaxAmmo);
    }
}

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 point, Vector3 normal);
}
```

## Gotchas

- **Mouse capture**: You must call `Input.MouseMode = Input.MouseModeEnum.Captured` or mouse look will not work. Always provide a way to un-capture (Escape key).
- **Transform.Basis for direction**: Always use the body's `Transform.Basis` to convert input to world-space direction. Using `_camera.GlobalTransform.Basis` will tilt movement when looking up/down.
- **RayCast3D.ForceRaycastUpdate()**: Raycasts only update during the physics step by default. Call `ForceRaycastUpdate()` before checking `IsColliding()` if you fire from `_UnhandledInput`.
- **Head bob reset**: Lerp back to the default Y position when stopping; otherwise the camera can freeze mid-bob.
- **useSubThreads deadlock**: If you later add async loading to this (pattern #5), note that `ResourceLoader.LoadThreadedRequest` with `useSubThreads: true` can deadlock with C# scripts. Keep it `false` unless you test thoroughly.

---

# 4. Settings Menu with Save/Load Persistence

## Scene Tree -- SettingsMenu.tscn

```
SettingsMenu (Control)
├── VBoxContainer
│   ├── Label ("Settings")
│   ├── TabContainer
│   │   ├── Audio (VBoxContainer)
│   │   │   ├── HBoxContainer
│   │   │   │   ├── Label ("Master Volume")
│   │   │   │   └── MasterSlider (HSlider)
│   │   │   ├── HBoxContainer
│   │   │   │   ├── Label ("Music Volume")
│   │   │   │   └── MusicSlider (HSlider)
│   │   │   └── HBoxContainer
│   │   │       ├── Label ("SFX Volume")
│   │   │       └── SfxSlider (HSlider)
│   │   ├── Video (VBoxContainer)
│   │   │   ├── HBoxContainer
│   │   │   │   ├── Label ("Fullscreen")
│   │   │   │   └── FullscreenCheck (CheckBox)
│   │   │   ├── HBoxContainer
│   │   │   │   ├── Label ("VSync")
│   │   │   │   └── VSyncCheck (CheckBox)
│   │   │   └── HBoxContainer
│   │   │       ├── Label ("Resolution")
│   │   │       └── ResolutionOption (OptionButton)
│   │   └── Controls (VBoxContainer)
│   │       └── HBoxContainer
│   │           ├── Label ("Mouse Sensitivity")
│   │           └── SensitivitySlider (HSlider)
│   ├── HBoxContainer
│   │   ├── ApplyButton (Button)
│   │   └── ResetButton (Button)
│   └── BackButton (Button)
```

## SettingsData.cs -- The data model

```csharp
using Godot;

public partial class SettingsData : Node
{
    public static SettingsData Instance { get; private set; }

    // Audio
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 1.0f;

    // Video
    public bool Fullscreen { get; set; } = false;
    public bool VSync { get; set; } = true;
    public Vector2I Resolution { get; set; } = new(1920, 1080);

    // Controls
    public float MouseSensitivity { get; set; } = 0.5f;

    private const string SettingsPath = "user://settings.cfg";

    public override void _Ready()
    {
        Instance = this;
        Load();
        Apply();
    }

    public void Save()
    {
        var config = new ConfigFile();

        config.SetValue("Audio", "master_volume", MasterVolume);
        config.SetValue("Audio", "music_volume", MusicVolume);
        config.SetValue("Audio", "sfx_volume", SfxVolume);

        config.SetValue("Video", "fullscreen", Fullscreen);
        config.SetValue("Video", "vsync", VSync);
        config.SetValue("Video", "resolution_x", Resolution.X);
        config.SetValue("Video", "resolution_y", Resolution.Y);

        config.SetValue("Controls", "mouse_sensitivity", MouseSensitivity);

        Error err = config.Save(SettingsPath);
        if (err != Error.Ok)
            GD.PrintErr($"Failed to save settings: {err}");
    }

    public void Load()
    {
        var config = new ConfigFile();
        Error err = config.Load(SettingsPath);

        if (err != Error.Ok)
        {
            GD.Print("No settings file found, using defaults.");
            return;
        }

        MasterVolume = (float)config.GetValue(
            "Audio", "master_volume", MasterVolume);
        MusicVolume = (float)config.GetValue(
            "Audio", "music_volume", MusicVolume);
        SfxVolume = (float)config.GetValue(
            "Audio", "sfx_volume", SfxVolume);

        Fullscreen = (bool)config.GetValue(
            "Video", "fullscreen", Fullscreen);
        VSync = (bool)config.GetValue(
            "Video", "vsync", VSync);
        int resX = (int)config.GetValue(
            "Video", "resolution_x", Resolution.X);
        int resY = (int)config.GetValue(
            "Video", "resolution_y", Resolution.Y);
        Resolution = new Vector2I(resX, resY);

        MouseSensitivity = (float)config.GetValue(
            "Controls", "mouse_sensitivity", MouseSensitivity);
    }

    public void Apply()
    {
        // Audio buses: "Master", "Music", "SFX" must exist in your AudioBusLayout
        SetBusVolume("Master", MasterVolume);
        SetBusVolume("Music", MusicVolume);
        SetBusVolume("SFX", SfxVolume);

        // Video
        if (Fullscreen)
        {
            DisplayServer.WindowSetMode(
                DisplayServer.WindowMode.ExclusiveFullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetSize(Resolution);
            // Center the window
            var screenSize = DisplayServer.ScreenGetSize();
            DisplayServer.WindowSetPosition(
                (screenSize - Resolution) / 2);
        }

        DisplayServer.WindowSetVsyncMode(
            VSync
                ? DisplayServer.VSyncMode.Enabled
                : DisplayServer.VSyncMode.Disabled);
    }

    private void SetBusVolume(string busName, float linear)
    {
        int busIdx = AudioServer.GetBusIndex(busName);
        if (busIdx == -1)
        {
            GD.PrintErr($"Audio bus '{busName}' not found!");
            return;
        }
        // Convert linear [0..1] to decibels. Mute at 0.
        if (linear <= 0.001f)
            AudioServer.SetBusMute(busIdx, true);
        else
        {
            AudioServer.SetBusMute(busIdx, false);
            AudioServer.SetBusVolumeDb(busIdx,
                Mathf.LinearToDb(linear));
        }
    }

    public void ResetToDefaults()
    {
        MasterVolume = 1.0f;
        MusicVolume = 0.8f;
        SfxVolume = 1.0f;
        Fullscreen = false;
        VSync = true;
        Resolution = new Vector2I(1920, 1080);
        MouseSensitivity = 0.5f;
    }
}
```

## SettingsMenu.cs -- UI binding

```csharp
using Godot;

public partial class SettingsMenu : Control
{
    private HSlider _masterSlider;
    private HSlider _musicSlider;
    private HSlider _sfxSlider;
    private CheckBox _fullscreenCheck;
    private CheckBox _vsyncCheck;
    private OptionButton _resolutionOption;
    private HSlider _sensitivitySlider;

    private readonly Vector2I[] _resolutions = new[]
    {
        new Vector2I(1280, 720),
        new Vector2I(1600, 900),
        new Vector2I(1920, 1080),
        new Vector2I(2560, 1440),
        new Vector2I(3840, 2160)
    };

    public override void _Ready()
    {
        _masterSlider = GetNode<HSlider>(
            "VBoxContainer/TabContainer/Audio/HBoxContainer/MasterSlider");
        _musicSlider = GetNode<HSlider>(
            "VBoxContainer/TabContainer/Audio/HBoxContainer2/MusicSlider");
        _sfxSlider = GetNode<HSlider>(
            "VBoxContainer/TabContainer/Audio/HBoxContainer3/SfxSlider");
        _fullscreenCheck = GetNode<CheckBox>(
            "VBoxContainer/TabContainer/Video/HBoxContainer/FullscreenCheck");
        _vsyncCheck = GetNode<CheckBox>(
            "VBoxContainer/TabContainer/Video/HBoxContainer2/VSyncCheck");
        _resolutionOption = GetNode<OptionButton>(
            "VBoxContainer/TabContainer/Video/HBoxContainer3/ResolutionOption");
        _sensitivitySlider = GetNode<HSlider>(
            "VBoxContainer/TabContainer/Controls/HBoxContainer/SensitivitySlider");

        // Populate resolutions
        for (int i = 0; i < _resolutions.Length; i++)
        {
            var r = _resolutions[i];
            _resolutionOption.AddItem($"{r.X} x {r.Y}", i);
        }

        // Configure sliders
        ConfigureSlider(_masterSlider, 0, 1, 0.01f);
        ConfigureSlider(_musicSlider, 0, 1, 0.01f);
        ConfigureSlider(_sfxSlider, 0, 1, 0.01f);
        ConfigureSlider(_sensitivitySlider, 0.05f, 2.0f, 0.05f);

        LoadFromSettings();

        // Connect buttons
        GetNode<Button>("VBoxContainer/HBoxContainer/ApplyButton")
            .Pressed += OnApply;
        GetNode<Button>("VBoxContainer/HBoxContainer/ResetButton")
            .Pressed += OnReset;
        GetNode<Button>("VBoxContainer/BackButton")
            .Pressed += OnBack;
    }

    private void ConfigureSlider(HSlider s, float min, float max, float step)
    {
        s.MinValue = min;
        s.MaxValue = max;
        s.Step = step;
    }

    private void LoadFromSettings()
    {
        var s = SettingsData.Instance;
        _masterSlider.Value = s.MasterVolume;
        _musicSlider.Value = s.MusicVolume;
        _sfxSlider.Value = s.SfxVolume;
        _fullscreenCheck.ButtonPressed = s.Fullscreen;
        _vsyncCheck.ButtonPressed = s.VSync;
        _sensitivitySlider.Value = s.MouseSensitivity;

        // Find matching resolution index
        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i] == s.Resolution)
            {
                _resolutionOption.Selected = i;
                break;
            }
        }
    }

    private void OnApply()
    {
        var s = SettingsData.Instance;
        s.MasterVolume = (float)_masterSlider.Value;
        s.MusicVolume = (float)_musicSlider.Value;
        s.SfxVolume = (float)_sfxSlider.Value;
        s.Fullscreen = _fullscreenCheck.ButtonPressed;
        s.VSync = _vsyncCheck.ButtonPressed;
        s.MouseSensitivity = (float)_sensitivitySlider.Value;
        s.Resolution = _resolutions[_resolutionOption.Selected];

        s.Apply();
        s.Save();
    }

    private void OnReset()
    {
        SettingsData.Instance.ResetToDefaults();
        LoadFromSettings();
    }

    private void OnBack()
    {
        Hide();
        // Or: GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
```

## Gotchas

- **Audio bus names**: You must create buses named "Master", "Music", and "SFX" in the Audio tab at the bottom of the editor (default layout file at `res://default_bus_layout.tres`). Route the Music and SFX buses to Master.
- **ConfigFile type casting in C#**: `GetValue()` returns `Variant`. Cast to `float`, not `double`. If the stored type does not match, it throws. Always provide the third parameter (default value).
- **Fullscreen on macOS**: Use `ExclusiveFullscreen` rather than `Fullscreen` for consistent behavior; `Fullscreen` may behave like a maximized window on some platforms.
- **user:// path**: On Windows this resolves to `%APPDATA%/Godot/app_userdata/<project_name>/`. On Linux: `~/.local/share/godot/app_userdata/<project_name>/`. The file is human-readable INI format.
- **Register as Autoload**: Add `SettingsData.cs` as an Autoload singleton in Project Settings > Globals so it loads before any scene.

---

# 5. Loading Screen with Progress Bar

## Scene Tree -- LoadingScreen.tscn

```
LoadingScreen (Control)                    # Full Rect layout
├── ColorRect                              # Background (full rect, dark color)
├── VBoxContainer (centered)
│   ├── Label ("Loading...")               # LoadingLabel
│   ├── ProgressBar                        # 0-100 range
│   └── Label                              # PercentLabel ("0%")
└── AnimationPlayer                        # Optional: spinner/tip rotation
```

## LoadingScreen.cs

```csharp
using Godot;
using Godot.Collections;

public partial class LoadingScreen : Control
{
    [Export] public string NextScenePath { get; set; }

    private ProgressBar _progressBar;
    private Label _percentLabel;
    private bool _loading;

    public override void _Ready()
    {
        _progressBar = GetNode<ProgressBar>(
            "VBoxContainer/ProgressBar");
        _percentLabel = GetNode<Label>(
            "VBoxContainer/Label2");

        _progressBar.MinValue = 0;
        _progressBar.MaxValue = 100;

        if (!string.IsNullOrEmpty(NextScenePath))
            StartLoading(NextScenePath);
    }

    public void StartLoading(string scenePath)
    {
        NextScenePath = scenePath;

        Error err = ResourceLoader.LoadThreadedRequest(
            scenePath,
            typeHint: "",
            useSubThreads: false  // true can deadlock with C#
        );

        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to start loading: {err}");
            // Fallback to synchronous load
            GetTree().ChangeSceneToFile(scenePath);
            return;
        }

        _loading = true;
    }

    public override void _Process(double delta)
    {
        if (!_loading)
            return;

        var progress = new Array();
        var status = ResourceLoader.LoadThreadedGetStatus(
            NextScenePath, progress);

        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                float percent = (float)progress[0] * 100f;
                _progressBar.Value = percent;
                _percentLabel.Text = $"{percent:F0}%";
                break;

            case ResourceLoader.ThreadLoadStatus.Loaded:
                _progressBar.Value = 100;
                _percentLabel.Text = "100%";
                _loading = false;

                var scene = ResourceLoader.LoadThreadedGet(
                    NextScenePath) as PackedScene;
                GetTree().ChangeSceneToPacked(scene);
                break;

            case ResourceLoader.ThreadLoadStatus.Failed:
                GD.PrintErr($"Failed to load: {NextScenePath}");
                _loading = false;
                break;

            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                GD.PrintErr($"Invalid resource: {NextScenePath}");
                _loading = false;
                break;
        }
    }
}
```

## SceneLoader.cs (Autoload) -- Global scene changer that shows the loading screen

```csharp
using Godot;

public partial class SceneLoader : Node
{
    public static SceneLoader Instance { get; private set; }

    private const string LoadingScreenPath =
        "res://Scenes/UI/LoadingScreen.tscn";

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Call this from anywhere to change scenes with a loading screen.
    /// For small scenes, it flashes by quickly. For big scenes, it shows real progress.
    /// </summary>
    public void LoadScene(string scenePath)
    {
        // Load the loading screen itself (it is small, synchronous is fine)
        var loadingScene = GD.Load<PackedScene>(LoadingScreenPath);
        var loadingInstance = loadingScene.Instantiate<LoadingScreen>();
        loadingInstance.NextScenePath = scenePath;

        // Replace the current scene tree
        GetTree().Root.AddChild(loadingInstance);

        // Remove the old scene
        var currentScene = GetTree().CurrentScene;
        if (currentScene != null)
        {
            GetTree().CurrentScene = null;
            currentScene.QueueFree();
        }

        GetTree().CurrentScene = loadingInstance;
    }
}
```

## Usage from anywhere

```csharp
// Instead of GetTree().ChangeSceneToFile(...)
SceneLoader.Instance.LoadScene("res://Scenes/Levels/Level02.tscn");
```

## Gotchas

- **useSubThreads: false**: As of Godot 4.3/4.4, setting `useSubThreads` to `true` with C# scripts can cause a deadlock when calling `LoadThreadedGet()`. Keep it `false` for C# projects. See [godot#103674](https://github.com/godotengine/godot/issues/103674).
- **progress array**: `LoadThreadedGetStatus` requires a `Godot.Collections.Array` (not `System.Collections.Generic.List`). The array receives a single element with the ratio between 0.0 and 1.0.
- **Progress stuck at 0.5**: If you have very few sub-resources, the progress may jump from 0 to 0.5 to 1.0 with nothing in between. Consider animating the progress bar with a tween for visual smoothness.
- **Web exports**: Background loading does not work on web exports as of Godot 4.4. Fall back to `ResourceLoader.Load()` for web builds.
- **Minimum display time**: You may want to add a minimum display time (e.g., 0.5 seconds) so the loading screen does not flash imperceptibly on fast loads.

---

# 6. Camera Shake Effect

## Scene Tree

For 2D, replace your existing Camera2D with this scene. For 3D, attach to a Node3D parent of your Camera3D.

```
ShakeCamera2D (Camera2D)
    # No children needed; script does everything
```

## ShakeCamera2D.cs -- Noise-based (smooth)

```csharp
using Godot;

public partial class ShakeCamera2D : Camera2D
{
    /// <summary>How fast the shake fades out. Higher = faster decay.</summary>
    [Export(PropertyHint.Range, "0.1,10,0.1")]
    public float DecayRate { get; set; } = 5.0f;

    /// <summary>Maximum pixel displacement in X and Y.</summary>
    [Export] public Vector2 MaxOffset { get; set; } = new(100, 75);

    /// <summary>Maximum rotation in radians during shake.</summary>
    [Export] public float MaxRoll { get; set; } = 0.1f;

    /// <summary>
    /// Exponent applied to trauma. 2 = quadratic (smooth),
    /// 3 = cubic (snappier).
    /// </summary>
    [Export(PropertyHint.Range, "2,3,1")]
    public int TraumaPower { get; set; } = 2;

    private float _trauma;
    private FastNoiseLite _noise;
    private int _noiseY;

    public override void _Ready()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = (int)GD.Randi();
        _noise.Frequency = 0.5f;
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
    }

    /// <summary>
    /// Add shake intensity. Values typically 0.2 (light) to 1.0 (heavy).
    /// Clamps at 1.0.
    /// </summary>
    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Min(_trauma + amount, 1.0f);
    }

    public override void _Process(double delta)
    {
        if (_trauma <= 0)
        {
            Offset = Vector2.Zero;
            Rotation = 0;
            return;
        }

        _trauma = Mathf.Max(_trauma - DecayRate * (float)delta, 0);
        _noiseY++;

        float amount = Mathf.Pow(_trauma, TraumaPower);

        Rotation = MaxRoll * amount *
            _noise.GetNoise2D(_noise.Seed, _noiseY);
        Offset = new Vector2(
            MaxOffset.X * amount *
                _noise.GetNoise2D(_noise.Seed * 2, _noiseY),
            MaxOffset.Y * amount *
                _noise.GetNoise2D(_noise.Seed * 3, _noiseY)
        );
    }
}
```

## ShakeCamera3D.cs -- For 3D games

```csharp
using Godot;

/// <summary>
/// Attach this to a Node3D that is the parent of your Camera3D.
/// The Camera3D should be at position (0, 0, 0) relative to this node.
/// </summary>
public partial class ShakeCamera3D : Node3D
{
    [Export] public float DecayRate { get; set; } = 5.0f;
    [Export] public float MaxOffsetX { get; set; } = 0.2f;
    [Export] public float MaxOffsetY { get; set; } = 0.15f;
    [Export] public float MaxRoll { get; set; } = 0.05f;
    [Export] public int TraumaPower { get; set; } = 2;

    private float _trauma;
    private FastNoiseLite _noise;
    private int _noiseY;
    private Vector3 _basePosition;
    private Vector3 _baseRotation;

    public override void _Ready()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = (int)GD.Randi();
        _noise.Frequency = 0.5f;
        _basePosition = Position;
        _baseRotation = Rotation;
    }

    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Min(_trauma + amount, 1.0f);
    }

    public override void _Process(double delta)
    {
        if (_trauma <= 0)
        {
            Position = _basePosition;
            Rotation = _baseRotation;
            return;
        }

        _trauma = Mathf.Max(_trauma - DecayRate * (float)delta, 0);
        _noiseY++;
        float amount = Mathf.Pow(_trauma, TraumaPower);

        Position = _basePosition + new Vector3(
            MaxOffsetX * amount *
                _noise.GetNoise2D(_noise.Seed * 2, _noiseY),
            MaxOffsetY * amount *
                _noise.GetNoise2D(_noise.Seed * 3, _noiseY),
            0
        );

        Vector3 rot = _baseRotation;
        rot.Z += MaxRoll * amount *
            _noise.GetNoise2D(_noise.Seed, _noiseY);
        Rotation = rot;
    }
}
```

## Usage

```csharp
// From Player.cs when taking damage:
GetNode<ShakeCamera2D>("Camera2D").AddTrauma(0.5f);

// From a weapon on fire:
GetNode<ShakeCamera2D>("/root/Level/Player/Camera2D").AddTrauma(0.3f);

// Or use a signal:
[Signal] public delegate void ScreenShakeRequestedEventHandler(float amount);
// ...in GameManager, connect to the camera's AddTrauma
```

## Gotchas

- **Noise vs Random**: `randf_range(-1, 1)` creates jittery, violent shake. `FastNoiseLite` produces smooth, organic shake. Always use noise for camera shake.
- **TraumaPower**: Use 2 for smooth, cinematic shake. Use 3 for snappier, more impactful shake. The power is applied to the trauma value, so low trauma is barely noticeable (which is desirable).
- **Offset vs Position**: For Camera2D, use `Offset` (not `Position`) for the shake displacement. `Position` is where the camera is in the world; `Offset` is a visual displacement from that position. If you use Position, the camera will drift permanently.
- **_Process vs _PhysicsProcess**: Use `_Process` for shake so it updates every render frame, not just physics ticks. This gives smoother visual results.
- **3D setup**: For 3D, do NOT shake the Camera3D directly. Shake its parent Node3D. This way the camera's local transform stays clean and you can compose shake with other camera effects.

---

# 7. Simple Dialogue System

## Scene Tree -- DialogueBox.tscn

```
DialogueBox (CanvasLayer)
├── PanelContainer (anchored bottom, full width)
│   ├── MarginContainer
│   │   └── VBoxContainer
│   │       ├── NameLabel (Label)           # Speaker name
│   │       ├── TextLabel (RichTextLabel)   # Dialogue text with BBCode
│   │       └── ContinueIndicator (Label)   # "Press Enter ▼"
```

## DialogueData -- Simple array-based

```csharp
// DialogueLine.cs
using Godot;

[GlobalClass]
public partial class DialogueLine : Resource
{
    [Export] public string Speaker { get; set; } = "";
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "";
}
```

## DialogueManager.cs (Autoload)

```csharp
using Godot;

public partial class DialogueManager : Node
{
    public static DialogueManager Instance { get; private set; }

    [Signal] public delegate void DialogueStartedEventHandler();
    [Signal] public delegate void DialogueEndedEventHandler();

    private DialogueBox _dialogueBox;
    private string[] _lines;
    private string _speaker;
    private int _currentIndex;
    private bool _isActive;

    public bool IsActive => _isActive;

    public override void _Ready()
    {
        Instance = this;

        // Load and instantiate the dialogue box scene
        var scene = GD.Load<PackedScene>(
            "res://Scenes/UI/DialogueBox.tscn");
        _dialogueBox = scene.Instantiate<DialogueBox>();
        AddChild(_dialogueBox);
        _dialogueBox.Hide();
    }

    public void StartDialogue(string speaker, string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        _speaker = speaker;
        _lines = lines;
        _currentIndex = 0;
        _isActive = true;

        _dialogueBox.Show();
        ShowCurrentLine();
        EmitSignal(SignalName.DialogueStarted);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_isActive) return;

        if (@event.IsActionPressed("interact") ||
            @event.IsActionPressed("ui_accept"))
        {
            if (_dialogueBox.IsTyping)
            {
                // Skip typewriter, show full text immediately
                _dialogueBox.SkipTypewriter();
            }
            else
            {
                Advance();
            }
            GetViewport().SetInputAsHandled();
        }
    }

    private void Advance()
    {
        _currentIndex++;
        if (_currentIndex >= _lines.Length)
        {
            EndDialogue();
            return;
        }
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        _dialogueBox.DisplayLine(_speaker, _lines[_currentIndex]);
    }

    private void EndDialogue()
    {
        _isActive = false;
        _dialogueBox.Hide();
        _lines = null;
        EmitSignal(SignalName.DialogueEnded);
    }
}
```

## DialogueBox.cs -- With typewriter effect

```csharp
using Godot;

public partial class DialogueBox : CanvasLayer
{
    private Label _nameLabel;
    private RichTextLabel _textLabel;
    private Label _continueIndicator;
    private Tween _typewriterTween;

    public bool IsTyping { get; private set; }

    // Characters per second for typewriter effect
    [Export] public float TypewriterSpeed { get; set; } = 40f;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>(
            "PanelContainer/MarginContainer/VBoxContainer/NameLabel");
        _textLabel = GetNode<RichTextLabel>(
            "PanelContainer/MarginContainer/VBoxContainer/TextLabel");
        _continueIndicator = GetNode<Label>(
            "PanelContainer/MarginContainer/VBoxContainer/ContinueIndicator");

        _textLabel.BbcodeEnabled = true;
        _continueIndicator.Text = "▼";
        _continueIndicator.Hide();
    }

    public void DisplayLine(string speaker, string text)
    {
        _nameLabel.Text = speaker;
        _textLabel.Text = text;
        _textLabel.VisibleCharacters = 0;
        _continueIndicator.Hide();

        // Kill any ongoing tween
        _typewriterTween?.Kill();

        float duration = text.Length / TypewriterSpeed;
        _typewriterTween = CreateTween();
        _typewriterTween.TweenProperty(
            _textLabel, "visible_characters", text.Length, duration);
        _typewriterTween.TweenCallback(Callable.From(OnTypewriterFinished));

        IsTyping = true;
    }

    public void SkipTypewriter()
    {
        _typewriterTween?.Kill();
        _textLabel.VisibleCharacters = -1;  // -1 shows all
        OnTypewriterFinished();
    }

    private void OnTypewriterFinished()
    {
        IsTyping = false;
        _continueIndicator.Show();
    }
}
```

## Usage from NPC

```csharp
public partial class ShopKeeper : CharacterBody2D, IInteractable
{
    public void Interact(Player player)
    {
        DialogueManager.Instance.StartDialogue("Shopkeeper", new[]
        {
            "Welcome to my shop, traveler!",
            "I have potions, swords, and shields.",
            "Take a look around."
        });
    }
}
```

## Integration with Player (disable movement during dialogue)

```csharp
// In Player.cs _PhysicsProcess:
public override void _PhysicsProcess(double delta)
{
    if (DialogueManager.Instance.IsActive)
    {
        Velocity = Vector2.Zero;
        MoveAndSlide();
        return;  // Skip all input processing
    }
    // ... normal movement code
}
```

## Gotchas

- **Input priority**: Use `_UnhandledInput` in the DialogueManager and call `GetViewport().SetInputAsHandled()`. This prevents the interact button from simultaneously advancing dialogue AND triggering a new interaction.
- **CanvasLayer**: The DialogueBox must be a CanvasLayer (or be a child of one) so it renders on top of the game world regardless of camera position.
- **RichTextLabel vs Label**: Use `RichTextLabel` for the text body. It supports BBCode (`[b]bold[/b]`, `[color=red]red[/color]`, `[wave]wavy[/wave]`) and the `visible_characters` property for typewriter effects. Regular `Label` also supports `visible_characters` but lacks BBCode.
- **Tween cleanup**: Always call `_typewriterTween?.Kill()` before creating a new tween. Otherwise old tweens keep running and fight the new one, causing visual glitches.
- **Localization**: For production, store dialogue in JSON or CSV files loaded at runtime rather than hardcoding strings. Godot's built-in `TranslationServer` supports CSV-based localization.

---

# 8. Generic Object Pool

## ObjectPool.cs

```csharp
using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Generic object pool for any Godot Node type.
/// Register as autoload or instantiate per-system.
/// </summary>
public partial class ObjectPool<T> where T : Node
{
    private readonly Stack<T> _available = new();
    private readonly Func<T> _factory;
    private readonly Node _parent;
    private readonly int _maxSize;

    /// <summary>
    /// Creates a new object pool.
    /// </summary>
    /// <param name="factory">Function to create a new instance
    ///   (e.g., () => scene.Instantiate&lt;T&gt;())</param>
    /// <param name="parent">Node to add pooled objects as children of</param>
    /// <param name="initialSize">Number of objects to pre-create</param>
    /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
    public ObjectPool(
        Func<T> factory, Node parent,
        int initialSize = 0, int maxSize = 0)
    {
        _factory = factory;
        _parent = parent;
        _maxSize = maxSize;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateNew();
            Deactivate(obj);
            _available.Push(obj);
        }
    }

    public int CountAvailable => _available.Count;

    /// <summary>
    /// Get an object from the pool. Creates a new one if pool is empty.
    /// </summary>
    public T Get()
    {
        T obj;
        if (_available.Count > 0)
        {
            obj = _available.Pop();
        }
        else
        {
            obj = CreateNew();
        }

        Activate(obj);
        return obj;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    public void Return(T obj)
    {
        if (_maxSize > 0 && _available.Count >= _maxSize)
        {
            // Pool is full, just destroy
            obj.QueueFree();
            return;
        }

        Deactivate(obj);
        _available.Push(obj);
    }

    /// <summary>
    /// Return an object after a delay (useful for particles, effects).
    /// </summary>
    public async void ReturnDelayed(T obj, float seconds)
    {
        var timer = obj.GetTree().CreateTimer(seconds);
        await obj.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

        if (GodotObject.IsInstanceValid(obj))
            Return(obj);
    }

    /// <summary>
    /// Destroy all pooled objects.
    /// </summary>
    public void Clear()
    {
        while (_available.Count > 0)
        {
            T obj = _available.Pop();
            obj.QueueFree();
        }
    }

    private T CreateNew()
    {
        T obj = _factory();
        _parent.AddChild(obj);
        return obj;
    }

    private void Activate(T obj)
    {
        obj.ProcessMode = Node.ProcessModeEnum.Inherit;
        if (obj is Node2D node2D)
            node2D.Visible = true;
        else if (obj is Node3D node3D)
            node3D.Visible = true;
        else if (obj is CanvasItem canvasItem)
            canvasItem.Visible = true;
    }

    private void Deactivate(T obj)
    {
        obj.ProcessMode = Node.ProcessModeEnum.Disabled;
        if (obj is Node2D node2D)
            node2D.Visible = false;
        else if (obj is Node3D node3D)
            node3D.Visible = false;
        else if (obj is CanvasItem canvasItem)
            canvasItem.Visible = false;
    }
}
```

## IPoolable Interface (optional reset contract)

```csharp
/// <summary>
/// Implement on pooled objects so the pool can reset them
/// before reuse.
/// </summary>
public interface IPoolable
{
    void OnGetFromPool();
    void OnReturnToPool();
}
```

## Enhanced Get/Return with IPoolable support

```csharp
// Add to ObjectPool<T>:
public T Get(Vector2 position)
{
    T obj = Get();
    if (obj is Node2D n2d)
        n2d.GlobalPosition = position;
    if (obj is IPoolable poolable)
        poolable.OnGetFromPool();
    return obj;
}

public T Get(Vector3 position)
{
    T obj = Get();
    if (obj is Node3D n3d)
        n3d.GlobalPosition = position;
    if (obj is IPoolable poolable)
        poolable.OnGetFromPool();
    return obj;
}

// Updated Return:
public new void Return(T obj)  // 'new' to shadow base if needed
{
    if (obj is IPoolable poolable)
        poolable.OnReturnToPool();

    if (_maxSize > 0 && _available.Count >= _maxSize)
    {
        obj.QueueFree();
        return;
    }
    Deactivate(obj);
    _available.Push(obj);
}
```

## Example: Bullet Pool in an FPS

```csharp
public partial class WeaponManager : Node3D
{
    [Export] public PackedScene BulletHoleScene { get; set; }
    private ObjectPool<BulletHole> _bulletHolePool;

    public override void _Ready()
    {
        _bulletHolePool = new ObjectPool<BulletHole>(
            factory: () => BulletHoleScene.Instantiate<BulletHole>(),
            parent: this,
            initialSize: 50,
            maxSize: 200
        );
    }

    public void SpawnBulletHole(Vector3 position, Vector3 normal)
    {
        var hole = _bulletHolePool.Get(position);
        hole.LookAt(position + normal);

        // Auto-return after 5 seconds
        _bulletHolePool.ReturnDelayed(hole, 5.0f);
    }
}
```

## Example: Bullet Poolable

```csharp
public partial class BulletHole : Node3D, IPoolable
{
    private GpuParticles3D _particles;

    public override void _Ready()
    {
        _particles = GetNode<GpuParticles3D>("Particles");
    }

    public void OnGetFromPool()
    {
        _particles.Restart();
        _particles.Emitting = true;
    }

    public void OnReturnToPool()
    {
        _particles.Emitting = false;
    }
}
```

## Gotchas

- **ProcessMode, not RemoveChild**: Do NOT use `RemoveChild`/`AddChild` for pooling. That is expensive. Instead toggle `ProcessMode` to `Disabled` and set `Visible = false`. This keeps the node in the tree but dormant.
- **IsInstanceValid**: Always check `GodotObject.IsInstanceValid(obj)` before operating on a pooled object that might have been freed externally (e.g., scene change).
- **Area2D/CollisionShape2D**: Disabling `ProcessMode` does NOT disable collision shapes. If your pooled object has an Area2D, you must also toggle `CollisionShape2D.Disabled` in Activate/Deactivate, or the invisible object will still trigger collisions.
- **Generic class and Godot**: `ObjectPool<T>` cannot extend `Node` because Godot does not support generic Node subclasses. That is why it is a plain C# class. Hold a reference to it in a Node-based script.
- **Thread safety**: This pool is not thread-safe. Only call Get/Return from the main thread.

---

# 9. Scene Transition with Fade

## Scene Tree -- SceneTransitioner.tscn (Autoload)

```
SceneTransitioner (CanvasLayer)       # Layer set to 100 (above everything)
├── ColorRect                         # Full Rect, Color = Black
│   └── (Mouse Filter = Ignore)
└── AnimationPlayer
```

## AnimationPlayer Setup (do this in the editor)

Create two animations:

**"fade_out"** (0.5s duration):
- Track: `ColorRect:modulate:a`
- Key at 0.0s: alpha = 0.0 (transparent)
- Key at 0.5s: alpha = 1.0 (opaque black)

**"fade_in"** (0.5s duration):
- Track: `ColorRect:modulate:a`
- Key at 0.0s: alpha = 1.0 (opaque black)
- Key at 0.5s: alpha = 0.0 (transparent)

## SceneTransitioner.cs

```csharp
using Godot;

public partial class SceneTransitioner : CanvasLayer
{
    public static SceneTransitioner Instance { get; private set; }

    private AnimationPlayer _animPlayer;
    private ColorRect _colorRect;
    private bool _transitioning;

    public override void _Ready()
    {
        Instance = this;
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _colorRect = GetNode<ColorRect>("ColorRect");

        // Start fully transparent
        _colorRect.Modulate = new Color(1, 1, 1, 0);

        // Ensure this layer renders above everything
        Layer = 100;

        // Don't block input when not transitioning
        _colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    /// <summary>
    /// Transition to a new scene with a fade effect.
    /// </summary>
    public async void TransitionToScene(string scenePath)
    {
        if (_transitioning) return;
        _transitioning = true;

        // Block input during transition
        _colorRect.MouseFilter = Control.MouseFilterEnum.Stop;

        // Fade to black
        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        // Change scene
        GetTree().ChangeSceneToFile(scenePath);

        // Wait one frame for the new scene to initialize
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Fade from black
        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        _colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        _transitioning = false;
    }

    /// <summary>
    /// Transition to a packed scene with a fade effect.
    /// </summary>
    public async void TransitionToPacked(PackedScene scene)
    {
        if (_transitioning) return;
        _transitioning = true;

        _colorRect.MouseFilter = Control.MouseFilterEnum.Stop;

        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        GetTree().ChangeSceneToPacked(scene);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        _colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        _transitioning = false;
    }

    /// <summary>
    /// Just fade out (useful before loading screens).
    /// </summary>
    public async void FadeOut()
    {
        _animPlayer.Play("fade_out");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    /// <summary>
    /// Just fade in (useful after loading screens).
    /// </summary>
    public async void FadeIn()
    {
        _animPlayer.Play("fade_in");
        await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }
}
```

## Alternative: Code-Only (No AnimationPlayer)

If you prefer not to set up animations in the editor:

```csharp
using Godot;

public partial class SceneTransitionerCodeOnly : CanvasLayer
{
    public static SceneTransitionerCodeOnly Instance { get; private set; }

    private ColorRect _overlay;
    private bool _transitioning;

    [Export] public float FadeDuration { get; set; } = 0.4f;

    public override void _Ready()
    {
        Instance = this;
        Layer = 100;

        _overlay = new ColorRect();
        _overlay.Color = Colors.Black;
        _overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_overlay);

        // Full screen coverage
        _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        // Start transparent
        _overlay.Modulate = new Color(1, 1, 1, 0);
    }

    public async void TransitionToScene(string scenePath)
    {
        if (_transitioning) return;
        _transitioning = true;
        _overlay.MouseFilter = Control.MouseFilterEnum.Stop;

        // Fade out
        var tween = CreateTween();
        tween.TweenProperty(_overlay, "modulate:a", 1.0f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);

        // Change scene
        GetTree().ChangeSceneToFile(scenePath);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Fade in
        tween = CreateTween();
        tween.TweenProperty(_overlay, "modulate:a", 0.0f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);

        _overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        _transitioning = false;
    }
}
```

## Usage from anywhere

```csharp
// From a button, a door trigger, a level-end zone, etc.
SceneTransitioner.Instance.TransitionToScene(
    "res://Scenes/Levels/Level02.tscn");
```

## Combining with Loading Screen (pattern #5)

```csharp
// For large scenes, combine fade + loading screen:
public async void TransitionWithLoading(string scenePath)
{
    if (_transitioning) return;
    _transitioning = true;

    _animPlayer.Play("fade_out");
    await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

    // Switch to loading screen, which handles its own fade-in
    SceneLoader.Instance.LoadScene(scenePath);

    _transitioning = false;
}
```

## Gotchas

- **CanvasLayer.Layer = 100**: Set this high so the fade renders above all other CanvasLayers (HUD, dialogue, pause menu). The default is 1.
- **Register as Autoload**: The SceneTransitioner must be an autoload (Project Settings > Globals). Autoloads persist across scene changes, which is the entire point.
- **MouseFilter**: Set to `Ignore` normally so the black rectangle does not swallow clicks. Switch to `Stop` during transitions to prevent the player from clicking through the fade.
- **await + async void**: Godot C# `await ToSignal(...)` works in `async void` methods. These are fire-and-forget. If you need error handling, wrap in try/catch. Do not use `async Task` with Godot signals -- it will not work correctly.
- **ProcessFrame wait**: After `ChangeSceneToFile`, wait one frame (`ProcessFrame` signal) before fading in. Without this, the fade-in may start before the new scene's `_Ready` has run, causing a flash of un-initialized content.
- **ColorRect color vs modulate**: Set the ColorRect's `Color` to black (or whatever your fade color is) and animate `modulate:a` for opacity. Do not animate `Color.A` directly -- `modulate` is the standard approach and works with all node types.

---

# How Everything Connects

Here is how these 9 patterns compose into a full game:

```
project.godot Autoloads:
  1. GameManager       (pattern #1/#2 -- global state)
  2. SettingsData       (pattern #4 -- persisted settings)
  3. SceneTransitioner  (pattern #9 -- fade transitions)
  4. SceneLoader        (pattern #5 -- loading screen)
  5. DialogueManager    (pattern #7 -- dialogue system)
  6. InventoryManager   (pattern #2 -- inventory)

Game flow:
  MainMenu
    ──[Start]──> SceneTransitioner.TransitionToScene()
                   ──> SceneLoader.LoadScene() if big level
                       ──> LoadingScreen (ProgressBar updates)
                           ──> Level loads
                               ──> Player uses ObjectPool (#8)
                                   for bullets, effects
                               ──> Camera uses ShakeCamera (#6)
                                   on explosions, damage
                               ──> NPC interaction triggers
                                   DialogueManager (#7)
    ──[Settings]──> SettingsMenu reads/writes SettingsData (#4)
    ──[Quit]──> GetTree().Quit()
```

Sources:
- [Chickensoft: Enjoyable Game Architecture](https://chickensoft.games/blog/game-architecture)
- [Chickensoft: Using Godot with C# in 2024](https://chickensoft.games/blog/godot-csharp-2024)
- [Godot Architecture Organization Advice](https://github.com/abmarnie/godot-architecture-organization-advice)
- [Kids Can Code: Screen Shake Recipe](https://kidscancode.org/godot_recipes/4.x/2d/screen_shake/index.html)
- [Kids Can Code: Basic FPS Character](https://kidscancode.org/godot_recipes/4.x/3d/basic_fps/index.html)
- [GDQuest: Scene Transitions](https://www.gdquest.com/tutorial/godot/2d/scene-transition-rect/)
- [JetBrains: Singletons and Autoloads with Godot and C#](https://www.jetbrains.com/guide/gamedev/tutorials/singletons-autoloads-godot-csharp/)
- [Godot FPS Template (C#)](https://github.com/Stablest/godot-fps)
- [EasyPool: C# Object Pool for Godot](https://github.com/SrdanJokic/easyPool)
- [Godot Object Pooling Example (C# + GDScript)](https://github.com/anasrar/godot-object-pooling)
- [Setting up JRPG Dialogues in Godot 4/C# (Medium)](https://medium.com/codex/setting-up-basic-jrpg-like-dialogues-godot-4-c-1574eb28e548)
- [Godot Docs: Background Loading](https://docs.godotengine.org/en/stable/tutorials/io/background_loading.html)
- [Godot Docs: ConfigFile API](https://docs.godotengine.org/en/stable/classes/class_configfile.html)
- [Godot C# API: ResourceLoader](https://straydragon.github.io/godot-csharp-api-doc/4.4-stable/main/Godot.ResourceLoader.html)
- [DwarfImpulse Camera Shake Plugin](https://godotengine.org/asset-library/asset/2488)