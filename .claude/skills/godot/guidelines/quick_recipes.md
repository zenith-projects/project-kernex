# Quick Recipes — C# Snippets for Godot 4.6

Copy-paste ready snippets for the most common operations. All C#.

---

## Table of Contents

- [Scene Management](#scene-management)
- [Node Access](#node-access)
- [Spawning & Instancing](#spawning--instancing)
- [Movement](#movement)
- [Input](#input)
- [Signals](#signals)
- [Timers & Delays](#timers--delays)
- [Tweens & Animation](#tweens--animation)
- [Audio](#audio)
- [Physics & Collision](#physics--collision)
- [UI](#ui)
- [Save & Load](#save--load)
- [Math & Utility](#math--utility)
- [Camera](#camera)
- [Resource Management](#resource-management)
- [Debug](#debug)

---

## Scene Management

```csharp
// Change scene by file
GetTree().ChangeSceneToFile("res://scenes/levels/Level2.tscn");

// Change scene by packed scene
var scene = GD.Load<PackedScene>("res://scenes/Level2.tscn");
GetTree().ChangeSceneToPacked(scene);

// Reload current scene
GetTree().ReloadCurrentScene();

// Quit game
GetTree().Quit();

// Pause / unpause
GetTree().Paused = true;
GetTree().Paused = false;

// Make node immune to pause
ProcessMode = ProcessModeEnum.Always;
```

---

## Node Access

```csharp
// Get child node (typed)
var sprite = GetNode<Sprite2D>("Sprite2D");

// Null-safe
var player = GetNodeOrNull<Player>("/root/Main/Player");
player?.TakeDamage(10);

// Scene unique node (marked with % in editor)
var label = GetNode<Label>("%ScoreLabel");

// Get parent
var parent = GetParent<Node2D>();

// Get all children
foreach (Node child in GetChildren())
    GD.Print(child.Name);

// Find first node in group
var player = GetTree().GetFirstNodeInGroup("player") as Player;

// Get all nodes in group
var enemies = GetTree().GetNodesInGroup("enemies");
```

---

## Spawning & Instancing

```csharp
// Load and instance a scene
var scene = GD.Load<PackedScene>("res://scenes/Bullet.tscn");
var bullet = scene.Instantiate<Bullet>();
bullet.GlobalPosition = _muzzle.GlobalPosition;
bullet.Rotation = _muzzle.GlobalRotation;
GetTree().CurrentScene.AddChild(bullet);

// Instance from exported PackedScene
[Export] public PackedScene EnemyScene { get; set; }
var enemy = EnemyScene.Instantiate<Enemy>();
AddChild(enemy);

// Remove node
node.QueueFree();

// Reparent node (preserving global transform)
node.Reparent(newParent);
```

---

## Movement

```csharp
// 2D Platformer
var velocity = Velocity;
if (!IsOnFloor()) velocity.Y += Gravity * (float)delta;
if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpForce;
velocity.X = Input.GetAxis("move_left", "move_right") * Speed;
Velocity = velocity;
MoveAndSlide();

// 2D Top-down
var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
Velocity = direction * Speed;
MoveAndSlide();

// 3D Movement (camera-relative)
var input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
var camBasis = _camera.GlobalTransform.Basis;
var forward = -camBasis.Z; forward.Y = 0; forward = forward.Normalized();
var right = camBasis.X; right.Y = 0; right = right.Normalized();
var direction = (forward * input.Y + right * input.X).Normalized();
Velocity = new Vector3(direction.X * Speed, Velocity.Y - Gravity * (float)delta, direction.Z * Speed);
MoveAndSlide();

// Smooth rotation toward direction
var targetAngle = Mathf.Atan2(direction.X, direction.Z);
Rotation = new Vector3(0, Mathf.LerpAngle(Rotation.Y, targetAngle, 10f * (float)delta), 0);

// Look at target (2D)
LookAt(target.GlobalPosition);

// Look at target (3D, ignoring Y)
var lookTarget = new Vector3(target.GlobalPosition.X, GlobalPosition.Y, target.GlobalPosition.Z);
LookAt(lookTarget);
```

---

## Input

```csharp
// Movement vector (with circular deadzone)
Vector2 dir = Input.GetVector("left", "right", "up", "down");

// Single axis
float horizontal = Input.GetAxis("left", "right");

// Just pressed / held / just released
if (Input.IsActionJustPressed("jump")) { }
if (Input.IsActionPressed("fire")) { }
if (Input.IsActionJustReleased("charge")) { }

// Analog strength (0.0 to 1.0)
float trigger = Input.GetActionStrength("accelerate");

// Mouse position
Vector2 mousePos = GetGlobalMousePosition(); // 2D
Vector2 screenPos = GetViewport().GetMousePosition(); // screen coords

// Capture/release mouse
Input.MouseMode = Input.MouseModeEnum.Captured;
Input.MouseMode = Input.MouseModeEnum.Visible;

// Mouse motion (in _UnhandledInput)
if (@event is InputEventMouseMotion motion)
    _cameraRotation += motion.Relative * Sensitivity;
```

---

## Signals

```csharp
// Declare
[Signal] public delegate void DiedEventHandler();
[Signal] public delegate void HealthChangedEventHandler(int current, int max);

// Emit
EmitSignal(SignalName.Died);
EmitSignal(SignalName.HealthChanged, _hp, _maxHp);

// Connect (C# event syntax)
health.Died += OnDied;
health.HealthChanged += (cur, max) => _bar.Value = cur;

// Connect built-in
button.Pressed += () => GD.Print("Clicked!");
area.BodyEntered += OnBodyEntered;
timer.Timeout += OnTimeout;
anim.AnimationFinished += (name) => GD.Print($"{name} done");

// Disconnect
health.Died -= OnDied;

// One-shot
button.Connect(Button.SignalName.Pressed,
    Callable.From(OnPressed), (uint)GodotObject.ConnectFlags.OneShot);

// Await signal
await ToSignal(GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);
await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // next frame
```

---

## Timers & Delays

```csharp
// One-shot delay (no Timer node needed)
await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
GD.Print("2 seconds later");

// Using Timer node
var timer = GetNode<Timer>("CooldownTimer");
timer.WaitTime = 0.5;
timer.OneShot = true;
timer.Timeout += () => _canFire = true;
timer.Start();

// Create timer programmatically
var t = new Timer();
t.WaitTime = 1.0;
t.OneShot = true;
t.Timeout += OnTimeout;
AddChild(t);
t.Start();
```

---

## Tweens & Animation

```csharp
// Move to position
CreateTween().TweenProperty(this, "position", new Vector2(300, 200), 0.5);

// Fade out
CreateTween().TweenProperty(this, "modulate:a", 0.0f, 0.3);

// Fade in
Modulate = new Color(1, 1, 1, 0);
CreateTween().TweenProperty(this, "modulate:a", 1.0f, 0.3);

// Scale bounce
var t = CreateTween();
t.TweenProperty(this, "scale", new Vector2(1.3f, 1.3f), 0.1);
t.TweenProperty(this, "scale", Vector2.One, 0.15).SetTrans(Tween.TransitionType.Bounce);

// Sequential + parallel
var t = CreateTween();
t.TweenProperty(this, "position:x", 100f, 0.5);
t.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.5); // same time
t.TweenCallback(Callable.From(QueueFree)); // after both

// Smooth health bar
CreateTween().TweenProperty(healthBar, "value", (double)newHealth, 0.3)
    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

// Easing options: Linear, Sine, Quad, Cubic, Quart, Quint, Expo, Circ, Elastic, Back, Bounce
// Ease types: In, Out, InOut

// Loop
CreateTween().SetLoops(0) // forever
    .TweenProperty(this, "rotation", Mathf.Tau, 2.0);

// AnimationPlayer
_animPlayer.Play("run");
_animPlayer.PlayBackwards("run");
_animPlayer.Queue("idle"); // play after current
_animPlayer.Stop();
```

---

## Audio

```csharp
// Play sound (global)
var player = GetNode<AudioStreamPlayer>("SFXPlayer");
player.Stream = GD.Load<AudioStream>("res://audio/sfx/hit.wav");
player.Play();

// Play with pitch variation
player.PitchScale = (float)GD.RandRange(0.9, 1.1);
player.Play();

// Set bus volume (0.0 = silent, 1.0 = full)
int busIdx = AudioServer.GetBusIndex("SFX");
AudioServer.SetBusVolumeDb(busIdx, Mathf.LinearToDb(0.8f));

// Mute bus
AudioServer.SetBusMute(busIdx, true);

// Crossfade music
var tween = CreateTween();
tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, 1.0);
tween.TweenCallback(Callable.From(() => {
    _musicPlayer.Stream = newTrack;
    _musicPlayer.Play();
}));
tween.TweenProperty(_musicPlayer, "volume_db", 0.0f, 1.0);
```

---

## Physics & Collision

```csharp
// Raycast from code (2D)
var spaceState = GetWorld2D().DirectSpaceState;
var query = PhysicsRayQueryParameters2D.Create(from, to, collisionMask);
query.Exclude = new Godot.Collections.Array<Rid> { GetRid() }; // exclude self
var result = spaceState.IntersectRay(query);
if (result.Count > 0)
{
    var collider = (Node2D)result["collider"];
    var point = (Vector2)result["position"];
    var normal = (Vector2)result["normal"];
}

// Raycast from code (3D)
var spaceState = GetWorld3D().DirectSpaceState;
var query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask);
var result = spaceState.IntersectRay(query);

// Mouse to ray (3D)
var camera = GetViewport().GetCamera3D();
var from = camera.ProjectRayOrigin(mousePos);
var to = from + camera.ProjectRayNormal(mousePos) * 1000f;

// Set collision layer/mask by code
SetCollisionLayerValue(2, true);  // I'm on layer 2
SetCollisionMaskValue(3, true);   // I detect layer 3

// Get collision info after MoveAndSlide
for (int i = 0; i < GetSlideCollisionCount(); i++)
{
    var col = GetSlideCollision(i);
    GD.Print(col.GetCollider().Name, col.GetNormal());
}

// Apply impulse to RigidBody
rigidBody.ApplyCentralImpulse(direction * force);
rigidBody.ApplyImpulse(impulse, contactPoint - rigidBody.GlobalPosition);
```

---

## UI

```csharp
// Button connection
GetNode<Button>("Button").Pressed += () => GD.Print("Click");

// Update label
GetNode<Label>("ScoreLabel").Text = $"Score: {score}";

// Progress bar
var bar = GetNode<ProgressBar>("HealthBar");
bar.MaxValue = maxHealth;
bar.Value = currentHealth;

// Show/hide
control.Visible = true;
control.Show(); control.Hide();

// Grab focus (for gamepad)
GetNode<Button>("FirstButton").GrabFocus();

// Full-rect Control (code)
AnchorLeft = 0; AnchorTop = 0; AnchorRight = 1; AnchorBottom = 1;

// Theme override
label.AddThemeFontSizeOverride("font_size", 24);
button.AddThemeColorOverride("font_color", Colors.Red);

// RichTextLabel BBCode
richText.BbcodeEnabled = true;
richText.Text = "[b]Bold[/b] [color=red]Red[/color] [wave]Wavy[/wave]";
```

---

## Save & Load

```csharp
// Simple config file
var config = new ConfigFile();
config.SetValue("audio", "master_volume", 0.8f);
config.SetValue("video", "fullscreen", true);
config.Save("user://settings.cfg");

// Load config
var config = new ConfigFile();
if (config.Load("user://settings.cfg") == Error.Ok)
{
    float vol = (float)config.GetValue("audio", "master_volume", 1.0f);
    bool fs = (bool)config.GetValue("video", "fullscreen", false);
}

// Save JSON data
var data = new Godot.Collections.Dictionary<string, Variant>
{
    { "level", 5 }, { "score", 12500 }, { "name", "Player1" }
};
using var file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Write);
file.StoreString(Json.Stringify(data));

// Load JSON data
using var file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Read);
var json = new Json();
json.Parse(file.GetAsText());
var data = (Godot.Collections.Dictionary)json.Data;

// Check if file exists
bool exists = FileAccess.FileExists("user://save.json");
```

---

## Math & Utility

```csharp
// Lerp
float result = Mathf.Lerp(a, b, t);
Vector2 pos = currentPos.Lerp(targetPos, 0.1f);

// Smooth exponential decay (framerate-independent)
float smoothed = Mathf.Lerp(current, target, 1.0f - Mathf.Exp(-speed * (float)delta));

// Clamp
int hp = Mathf.Clamp(value, 0, maxHp);

// Move toward (constant speed)
float val = Mathf.MoveToward(current, target, speed * (float)delta);
Vector2 pos = Position.MoveToward(target, speed * (float)delta);

// Distance
float dist = GlobalPosition.DistanceTo(target.GlobalPosition);

// Direction
Vector2 dir = GlobalPosition.DirectionTo(target.GlobalPosition);

// Angle between two points
float angle = GlobalPosition.AngleTo(target.GlobalPosition);

// Random
float r = GD.Randf();                    // 0.0 to 1.0
float r = (float)GD.RandRange(5.0, 10.0); // range
int i = (int)GD.Randi() % 100;            // 0 to 99
GD.Randomize();                            // seed from time

// Random from array
var items = new string[] { "sword", "shield", "potion" };
var pick = items[GD.Randi() % items.Length];

// Deg/Rad
float rad = Mathf.DegToRad(90f);
float deg = Mathf.RadToDeg(Mathf.Pi);

// Snapped (grid snapping)
Vector2 snapped = position.Snapped(new Vector2(16, 16));
```

---

## Camera

```csharp
// Camera shake (2D)
public async void Shake(float intensity = 5f, float duration = 0.2f)
{
    var cam = GetNode<Camera2D>("Camera2D");
    var originalOffset = cam.Offset;
    float elapsed = 0;
    while (elapsed < duration)
    {
        cam.Offset = originalOffset + new Vector2(
            (float)GD.RandRange(-intensity, intensity),
            (float)GD.RandRange(-intensity, intensity));
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        elapsed += (float)GetProcessDeltaTime();
    }
    cam.Offset = originalOffset;
}

// Camera shake (tween version)
public void ShakeTween(Camera2D cam, float intensity = 10f, float duration = 0.3f)
{
    var tween = CreateTween();
    int steps = (int)(duration / 0.05f);
    for (int i = 0; i < steps; i++)
    {
        tween.TweenProperty(cam, "offset",
            new Vector2((float)GD.RandRange(-1,1), (float)GD.RandRange(-1,1)) * intensity, 0.05f);
    }
    tween.TweenProperty(cam, "offset", Vector2.Zero, 0.05f);
}

// Screen flash
public void FlashScreen(Color color, float duration = 0.1f)
{
    var flash = GetNode<ColorRect>("%ScreenFlash");
    flash.Color = color;
    flash.Visible = true;
    var tween = CreateTween();
    tween.TweenProperty(flash, "modulate:a", 0.0f, duration);
    tween.TweenCallback(Callable.From(() => flash.Visible = false));
}
```

---

## Resource Management

```csharp
// Load resource (cached by engine)
var tex = GD.Load<Texture2D>("res://sprites/player.png");
var scene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");

// Background loading
ResourceLoader.LoadThreadedRequest("res://scenes/HeavyLevel.tscn");
// Later:
var status = ResourceLoader.LoadThreadedGetStatus("res://scenes/HeavyLevel.tscn");
if (status == ResourceLoader.ThreadLoadStatus.Loaded)
    var res = ResourceLoader.LoadThreadedGet("res://scenes/HeavyLevel.tscn");

// Custom resource
[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public int Value { get; set; }
    [Export] public Texture2D Icon { get; set; }
}

// Save resource
ResourceSaver.Save(myResource, "res://data/sword.tres");
```

---

## Debug

```csharp
// Print
GD.Print("hello");
GD.Print($"Position: {Position}");
GD.PrintErr("Error!");
GD.PushWarning("Warning!");
GD.PushError("Error in log!");

// Timing
ulong start = Time.GetTicksMsec();
ExpensiveOperation();
GD.Print($"Took {Time.GetTicksMsec() - start}ms");

// Draw debug (override _Draw in Node2D)
public override void _Draw()
{
    DrawCircle(Vector2.Zero, 50f, Colors.Red);
    DrawLine(Vector2.Zero, new Vector2(100, 0), Colors.Green, 2f);
    DrawRect(new Rect2(-25, -25, 50, 50), Colors.Blue, false, 2f);
}
// Call QueueRedraw() to trigger _Draw again

// Assert
System.Diagnostics.Debug.Assert(health > 0, "Health must be positive");

// Is in editor?
if (Engine.IsEditorHint()) { /* editor only */ }

// FPS
GD.Print($"FPS: {Engine.GetFramesPerSecond()}");
```
