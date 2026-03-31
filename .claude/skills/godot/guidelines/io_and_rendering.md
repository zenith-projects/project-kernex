# I/O, Rendering & Scene Management Reference

Complete reference for Godot 4.6 file I/O, rendering, viewports, and scene management. All C#.

---

## File Paths

### Path Prefixes

| Prefix | Use | Writable |
|--------|-----|----------|
| `res://` | Project files (assets, scenes, scripts) | Only in editor |
| `user://` | Save data, settings, logs | Always |

Use UNIX forward slashes on ALL platforms: `res://scenes/Player.tscn`, not backslashes.

### user:// Locations

| OS | Default Path |
|----|-------------|
| Windows | `%APPDATA%\Godot\app_userdata\[project_name]` |
| macOS | `~/Library/Application Support/Godot/app_userdata/[project_name]` |
| Linux | `~/.local/share/godot/app_userdata/[project_name]` |

Enable `application/config/use_custom_user_dir` in Project Settings for cleaner paths.

### Path Conversion

```csharp
// res:// or user:// → absolute OS path
string absPath = ProjectSettings.GlobalizePath("user://saves/game.save");

// Absolute OS path → res:// or user://
string localPath = ProjectSettings.LocalizePath(absPath);
```

---

## Saving Games

### Architecture: Group + Serialize + JSON

```csharp
// 1. Tag persistent nodes with "Persist" group
AddToGroup("Persist");

// 2. Each persistent node exposes a Save() method
public Godot.Collections.Dictionary<string, Variant> Save()
{
    return new()
    {
        { "Filename", SceneFilePath },
        { "Parent", GetParent().GetPath() },
        { "PosX", Position.X },
        { "PosY", Position.Y },
        { "Health", _health },
        { "Score", _score },
    };
}

// 3. Save to file
public void SaveGame()
{
    using var file = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
    foreach (Node node in GetTree().GetNodesInGroup("Persist"))
    {
        if (string.IsNullOrEmpty(node.SceneFilePath) || !node.HasMethod("Save"))
            continue;
        var data = node.Call("Save");
        file.StoreLine(Json.Stringify(data));
    }
}

// 4. Load from file
public void LoadGame()
{
    if (!FileAccess.FileExists("user://savegame.save")) return;

    // Delete existing persistent nodes
    foreach (Node node in GetTree().GetNodesInGroup("Persist"))
        node.QueueFree();

    using var file = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
    while (file.GetPosition() < file.GetLength())
    {
        var json = new Json();
        if (json.Parse(file.GetLine()) != Error.Ok) continue;
        var data = new Godot.Collections.Dictionary<string, Variant>(
            (Godot.Collections.Dictionary)json.Data);

        var newNode = GD.Load<PackedScene>(data["Filename"].ToString()).Instantiate();
        GetNode(data["Parent"].ToString()).AddChild(newNode);
        newNode.Set(Node2D.PropertyName.Position,
            new Vector2((float)data["PosX"], (float)data["PosY"]));

        foreach (var (key, value) in data)
        {
            if (key is "Filename" or "Parent" or "PosX" or "PosY") continue;
            newNode.Set(key, value);
        }
    }
}
```

**JSON limitation**: Cannot represent Vector2, Vector3, Color — split manually. For binary: use `FileAccess.StoreVar()`/`GetVar()`.

---

## Background Loading

```csharp
// 1. Queue the request (non-blocking)
ResourceLoader.LoadThreadedRequest("res://scenes/HeavyLevel.tscn");

// 2. Check progress in _Process
public override void _Process(double delta)
{
    var status = ResourceLoader.LoadThreadedGetStatus(
        "res://scenes/HeavyLevel.tscn", out var progress);

    if (status == ResourceLoader.ThreadLoadStatus.InProgress)
        _progressBar.Value = (float)progress[0] * 100;
    else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
    {
        var scene = ResourceLoader.LoadThreadedGet("res://scenes/HeavyLevel.tscn") as PackedScene;
        GetTree().ChangeSceneToPacked(scene);
    }
}
```

---

## Multiple Resolutions

### Recommended Settings

**Desktop (non-pixel-art):**
- Base: 1920x1080
- Stretch Mode: `canvas_items`
- Stretch Aspect: `expand`

**Desktop (pixel-art):**
- Base: 640x360 (scales to 720p/1080p/1440p/4K)
- Stretch Mode: `viewport`
- Stretch Aspect: `keep` or `expand`
- Stretch Scale Mode: `integer`

**Mobile landscape:** Base 1280x720, Mode `canvas_items`, Aspect `expand`

**Mobile portrait:** Base 720x1280, Mode `canvas_items`, Aspect `expand`, Orientation `portrait`

### Stretch Modes

| Mode | Behavior |
|------|----------|
| `disabled` | No stretching, 1 unit = 1 pixel |
| `canvas_items` | 2D scales to screen, 3D unaffected |
| `viewport` | Renders at base size, then scales output |

### Stretch Aspect

| Aspect | Black Bars | Best For |
|--------|-----------|----------|
| `ignore` | None (distorts) | Never |
| `keep` | Letterbox + pillarbox | Single aspect ratio |
| `keep_width` | Pillarbox only | Bottom-anchored UI |
| `keep_height` | Letterbox only | Platformers, runners |
| `expand` | None (extends view) | Most games |

---

## Viewports

### SubViewport Uses
- 3D objects in 2D game (character preview, minimap)
- 2D in 3D (screen textures)
- Dynamic textures (security cameras, mirrors)
- Split-screen multiplayer

### Screenshot Capture

```csharp
// Must wait for rendering to complete
await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
var img = GetViewport().GetTexture().GetImage();
img.SavePng("user://screenshot.png");
```

---

## Scene Unique Nodes

Access nodes without fragile paths using `%` prefix:

```csharp
// Mark node as unique in editor (right-click → "Access as Unique Name")
// Then access from any script in the SAME scene:
var button = GetNode<Button>("%RedButton");
var label = GetNode<Label>("%ScoreLabel");
```

**Same-scene limitation**: `%Name` only resolves within the scene that owns the node.

**Cross-scene**: `GetNode("%Player/%Weapon")` chains through instanced scenes.

---

## Groups

```csharp
// Add to group
AddToGroup("enemies");

// Query group
var enemies = GetTree().GetNodesInGroup("enemies");

// Call method on all group members
GetTree().CallGroup("enemies", "AlertToPlayer");

// First node in group
var player = GetTree().GetFirstNodeInGroup("player");
```

---

## Pausing

```csharp
GetTree().Paused = true;

// Process modes
ProcessMode = ProcessModeEnum.Always;     // Ignores pause (pause menu)
ProcessMode = ProcessModeEnum.WhenPaused; // Only runs when paused
ProcessMode = ProcessModeEnum.Pausable;   // Stops when paused (default via Inherit)
ProcessMode = ProcessModeEnum.Disabled;   // Never runs
```

**Gotcha**: Physics servers stop when paused. Even `Always` nodes can't use physics unless you call `SetActive` on physics servers.

---

## Scene Transitions

### Three Strategies

| Strategy | Memory | Processing | Best For |
|----------|--------|-----------|----------|
| **Delete** (`QueueFree`) | Freed | Stopped | Clean level transitions |
| **Hide** (`Visible = false`) | Kept | Continues | Background worlds |
| **Remove** (`RemoveChild`) | Kept | Stopped | Quick toggle (best of both) |

```csharp
// Strategy 3: Remove and re-add
private Node _cachedLevel;

public void SwapLevel(PackedScene newLevel)
{
    _cachedLevel = GetNode("CurrentLevel");
    RemoveChild(_cachedLevel); // Keeps in memory, stops processing

    var next = newLevel.Instantiate();
    AddChild(next);
}

public void RestoreLevel()
{
    GetNode("CurrentLevel").QueueFree();
    AddChild(_cachedLevel); // Restored with all state intact
}
```

---

## Jitter, Stutter, and Input Lag

### Physics Interpolation
Enable in Project Settings to smooth physics-to-render jitter. Slightly increases input lag.

### Reducing Input Lag
```csharp
// Disable input buffering (more CPU, less lag)
Input.SetUseAccumulatedInput(false);

// Increase physics tick rate (reduces lag for physics-driven movement)
Engine.PhysicsTicksPerSecond = 120;

// Cap FPS slightly below refresh for variable refresh rate monitors
Engine.MaxFps = 141; // on 144Hz monitor
```

### Windows-Specific
- Use **Exclusive Fullscreen** (not Fullscreen) for best performance
- 1000+ Hz mice: use fully updated Windows 11
