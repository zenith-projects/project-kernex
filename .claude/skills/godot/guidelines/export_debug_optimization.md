# Export, Debugging, Optimization & Editor Patterns (Godot 4 / C#)

Comprehensive reference extracted from official Godot documentation covering project export, feature tags, tool scripts, debugging, profiling, optimization (CPU/GPU), notifications, and autoloads.

---

## 1. Exporting Projects

### Why Export?

Distributing a game requires packaging the Godot binary with project data. Export produces compiled binaries that are smaller and exclude editor tools. Mobile platforms additionally require platform-specific SDKs and texture compression formats (ETC1, ETC2).

### Export Workflow

1. Open the **Export** dialog via the toolbar Export button
2. Click **Add...** to create an export preset for your target platform
3. The system warns if platform SDKs or export templates are missing
4. Configure resources, features, and platform-specific settings
5. Choose one of three export actions:
   - **Export All** -- builds for all defined presets
   - **Export Project** -- builds for the selected preset only
   - **Export PCK/ZIP** -- exports only project data without an executable

### Export Templates

Export templates are TPZ files (renamed ZIP archives) downloaded from the Godot website. Install via **Editor > Manage Export Templates > Install Export Templates**.

### Resource Export Modes

Five modes control which resources are included:

1. **Export all resources in the project**
2. **Export selected scenes (and dependencies)**
3. **Export selected resources (and dependencies)**
4. **Export all resources except those checked below**
5. **Export as dedicated server** -- removes visual resources and replaces with placeholders

Files beginning with a period (e.g., `.git/`) are never exported.

### Configuration Files

Two files are created in the project directory:

| File | Purpose | Version Control |
|------|---------|-----------------|
| `export_presets.cfg` | Preset configuration | Safe to commit |
| `.godot/export_credentials.cfg` | Sensitive data (keys, passwords) | NEVER commit |

### Command-Line Export

```bash
# Export release build
godot --export-release "Preset Name" output.exe

# Export debug build
godot --export-debug "Preset Name" output.exe

# Export PCK only (no executable)
godot --export-pack "Preset Name" output.pck
```

Extension varies by platform: `.exe` (Windows), `.app` (macOS), `.apk` (Android), `.x86_64` (Linux).

### PCK vs ZIP Format

| Format | Compression | Speed | OS-Readable |
|--------|-------------|-------|-------------|
| **PCK** | Uncompressed | Faster loading | No |
| **ZIP** | Compressed | Smaller size | Yes, but needs launcher scripts |

### Gotchas

- Export templates must match the exact Godot version used
- Files starting with `.` are auto-excluded
- Never commit `.godot/export_credentials.cfg` to version control
- The dedicated server export mode strips all visual assets

---

## 2. Feature Tags

### How Feature Tags Work

Godot's feature tag system identifies platform capabilities and build configurations at runtime. Tags are **case-sensitive** and mostly **immutable** at runtime.

### Checking Feature Tags in Code

```csharp
// C# -- check any feature tag at runtime
if (OS.HasFeature("windows"))
{
    GD.Print("Running on Windows");
}

if (OS.HasFeature("debug"))
{
    GD.Print("Debug build");
}

if (OS.HasFeature("mobile"))
{
    GD.Print("Mobile device");
}
```

### Complete Feature Tag Reference

**Platform Tags:**

| Tag | Platform |
|-----|----------|
| `windows` | Windows |
| `macos` | macOS |
| `linux` | Linux |
| `bsd` | BSD |
| `linuxbsd` | Linux or any BSD |
| `android` | Android |
| `ios` | iOS |
| `visionos` | visionOS |

**Build Type Tags:**

| Tag | Meaning |
|-----|---------|
| `debug` | Debug build (includes editor) |
| `release` | Release build |
| `editor` | Running as editor |
| `template` | Non-editor export (release or debug template) |
| `editor_hint` | Editor build, inside the editor |
| `editor_runtime` | Editor build, project running from editor |

**Device Type Tags:**

| Tag | Meaning |
|-----|---------|
| `mobile` | Mobile device (Android, iOS, visionOS) |
| `pc` | Desktop platform (Windows, macOS, Linux, BSD) |
| `web` | Web platform |

**Architecture Tags:**

| Tag | Architecture |
|-----|-------------|
| `64` | 64-bit |
| `32` | 32-bit |
| `x86_64` | x86 64-bit |
| `x86_32` | x86 32-bit |
| `x86` | Any x86 |
| `arm64` | ARM 64-bit |
| `arm32` | ARM 32-bit |
| `arm` | Any ARM |
| `rv64` | RISC-V 64-bit |
| `riscv` | Any RISC-V |
| `ppc64` | PowerPC 64-bit |
| `ppc32` | PowerPC 32-bit |
| `ppc` | Any PowerPC |
| `wasm64` | WebAssembly 64-bit |
| `wasm32` | WebAssembly 32-bit |
| `wasm` | Any WebAssembly |

**Precision Tags:**

| Tag | Meaning |
|-----|---------|
| `double` | Double-precision build |
| `single` | Single-precision build |

**Texture Compression Tags:**

| Tag | Format |
|-----|--------|
| `etc` | ETC1 compression |
| `etc2` | ETC2 compression |
| `s3tc` | S3TC/DXT compression |

**Threading Tags:**

| Tag | Meaning |
|-----|---------|
| `threads` | Threading enabled |
| `nothreads` | Threading disabled |

**Web Browser Detection Tags:**

| Tag | Meaning |
|-----|---------|
| `web_android` | Web on Android browser |
| `web_ios` | Web on iOS browser |
| `web_linuxbsd` | Web on Linux/BSD browser |
| `web_macos` | Web on macOS browser |
| `web_windows` | Web on Windows browser |

**Special Tags:**

| Tag | Meaning |
|-----|---------|
| `movie` | Movie Maker mode active |
| `shader_baker` | Shader baking enabled in exports |
| `dedicated_server` | Exported as dedicated server |

### Custom Feature Tags

Custom tags can be added via export preset configurations, but they only apply to exported projects -- NOT to editor runs.

### Project Settings Override with Feature Tags

Feature tags can override project settings. Use `ProjectSettings.GetSettingWithOverride()` to respect feature tag overrides:

```csharp
// Reads the setting with any feature-tag-based override applied
var value = ProjectSettings.GetSettingWithOverride("my_section/my_setting");
```

### Practical Pattern: Platform-Specific Behavior

```csharp
public partial class PlatformManager : Node
{
    public override void _Ready()
    {
        if (OS.HasFeature("mobile"))
        {
            // Enable touch controls, reduce quality
            SetupMobileControls();
            RenderingServer.ViewportSetMsaa3D(GetViewport().GetViewportRid(), RenderingServer.ViewportMsaa.Disabled);
        }
        else if (OS.HasFeature("pc"))
        {
            // Enable keyboard/mouse controls
            SetupDesktopControls();
        }

        if (OS.HasFeature("dedicated_server"))
        {
            // Strip visual processing for server
            SetProcess(false);
        }

        if (OS.HasFeature("debug"))
        {
            // Enable debug overlay
            GetNode<CanvasLayer>("DebugOverlay").Visible = true;
        }
    }
}
```

---

## 3. Running Code in the Editor (@tool Scripts)

### The [Tool] Attribute

The `[Tool]` attribute (C# equivalent of GDScript `@tool`) enables script execution within the editor. It allows you to decide which parts of the script execute in the editor, in game, or both.

**Common use cases:**
- Visualizing physics trajectories (e.g., cannonball arcs)
- Displaying jump pad height indicators
- Rendering code-based player graphics during editing
- Custom gizmos and handles

### Basic Tool Script Pattern (C#)

```csharp
using Godot;

[Tool]
public partial class EditorVisualization : Node2D
{
    private float _radius = 50f;

    [Export]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            QueueRedraw(); // Trigger redraw when property changes in editor
        }
    }

    public override void _Draw()
    {
        // This runs in BOTH editor and game
        DrawCircle(Vector2.Zero, _radius, Colors.Red);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            // Editor-only logic
            QueueRedraw();
            return;
        }

        // Game-only logic
        ApplyGameBehavior(delta);
    }

    private void ApplyGameBehavior(double delta)
    {
        // This only runs in game
    }
}
```

### Engine.IsEditorHint() -- Context Detection

```csharp
[Tool]
public partial class DualModeScript : Node2D
{
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            // Runs ONLY in the editor
            EditorUpdate(delta);
        }
        else
        {
            // Runs ONLY in game
            GameUpdate(delta);
        }
    }

    // Code outside conditionals runs in BOTH contexts
    public override void _Ready()
    {
        // This runs in both editor and game
        GD.Print("Ready in both contexts");
    }
}
```

### Critical Warnings for Tool Scripts

1. **Crash risk with QueueFree**: Calling `Node.QueueFree()` carelessly can crash the editor if the editor is running logic involving that node. Always guard destructive operations.

2. **Transitive @tool requirement**: Any script that your tool script uses must ALSO be a tool script. Exception: static methods, constants, and enums CAN reference non-tool scripts.

3. **Inheritance does NOT propagate [Tool]**: Extending a `[Tool]` script does NOT automatically make the child script a tool. You must explicitly add `[Tool]` to each derived class.

4. **Scene persistence**: Nodes added programmatically in `[Tool]` scripts need explicit owner assignment to appear in the Scene tree and persist to disk:

```csharp
[Tool]
public partial class EditorNodeCreator : Node2D
{
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            var marker = new Sprite2D();
            AddChild(marker);
            // CRITICAL: Set owner for editor visibility and disk persistence
            marker.Owner = GetTree().EditedSceneRoot;
        }
    }
}
```

### EditorScript -- One-Off Automation

For one-off automation tasks (not persistent tool scripts), extend `EditorScript`:

```csharp
using Godot;

[Tool]
public partial class MyEditorScript : EditorScript
{
    public override void _Run()
    {
        // Access the editor scene tree
        var root = GetScene();
        GD.Print($"Root node: {root.Name}");

        // Perform automation
        foreach (var child in root.GetChildren())
        {
            GD.Print($"  Child: {child.Name}");
        }
    }
}
```

**Execution methods:**
- File > Run (in script editor)
- Ctrl+Shift+X shortcut
- Right-click > Run in FileSystem dock
- Command palette via class_name

**Critical**: EditorScripts have NO undo/redo. Save your scene before running one.

---

## 4. Debugging Tools Overview

### Debugger Panel

The Debugger panel (bottom of the editor) houses most debugging tools. It contains tabs for Debugger, Errors, Profiler, Monitors, Video RAM, and more.

### Debug Menu Toggles

| Option | Purpose |
|--------|---------|
| **Deploy with Remote Debug** | Connects executable to computer's IP for remote debugging (mobile) |
| **Small Deploy with Network Filesystem** | Builds minimal executable; editor serves files over network |
| **Visible Collision Shapes** | Shows collision shapes and raycast nodes at runtime |
| **Visible Paths** | Displays curve resources used by path nodes |
| **Visible Navigation** | Shows navigation meshes and polygons |
| **Visible Avoidance** | Reveals avoidance object shapes, radiuses, velocities |
| **Debug CanvasItem Redraws** | Flashes 2D objects when they request redraws |
| **Synchronize Scene Changes** | Replicates editor scene modifications in the running project |
| **Synchronize Script Changes** | Reloads script changes in the running project |
| **Keep Debug Server Open** | Maintains debug server for external sessions |

### Multiple Instance Debugging

Essential for multiplayer debugging. Configuration:

- **Enable Multiple Instances** checkbox with instance count selector
- **Main Run Args** applied to all instances (unless overridden)
- **Main Feature Tags** applied to all instances (unless overridden)
- Per-instance Launch Arguments and Feature Tags with override capabilities

```csharp
// Access command-line arguments in code
var allArgs = OS.GetCmdlineArgs();       // All arguments
var userArgs = OS.GetCmdlineUserArgs();  // User-defined arguments only
```

### Breakpoints

- Click the **left gutter** (left of line numbers) in the script editor to place breakpoints
- Breakpoints persist across editor restarts even if the script wasn't saved
- GDScript also supports the `breakpoint` keyword for version-control-persistent breakpoints

**Debugger controls when paused:**
- **Break** -- pause execution like a breakpoint
- **Continue** -- resume after pause
- **Step Over** -- advance to next line
- **Step Into** -- enter function calls when possible

### Remote Scene Inspection

When running from the editor, the Scene dock shows **Remote** and **Local** tabs. The Remote tab allows inspecting and modifying node parameters in the running project in real time.

### Project Settings Debug Category

Under Advanced Settings:

| Subcategory | Controls |
|-------------|----------|
| **Settings** | General debugging (FPS printing, profiling limits) |
| **File Logging** | Console output and error message file logging |
| **GDScript** | Toggle specific warnings (unused variables, etc.) |
| **Shader Language** | Shader-specific warning toggles |
| **Canvas Items** | Canvas item redraw debugging settings |
| **Shapes** | Debugging shape colors (collision, navigation) |

### Print Functions (C# Equivalents)

```csharp
// Standard print
GD.Print("Hello World");
GD.Print("Value: ", someValue);

// Rich text print (supports BBCode)
GD.PrintRich("[color=green]Success[/color]");

// Error and warning
GD.PrintErr("This is an error");
GD.PushError("Error pushed to Errors tab");
GD.PushWarning("Warning pushed to Errors tab");

// Debug-only print (stripped in release builds)
System.Diagnostics.Debug.WriteLine("Debug only");
```

---

## 5. The Profiler

### Activation

The profiler is in the **Debugger** panel's **Profiler** tab. It does NOT run automatically because profiling itself is performance-intensive.

1. Run your game
2. Click **Start** in the Profiler tab
3. The **Autostart** option enables automatic profiling on next launch (not preserved between sessions)

### Primary Metrics

| Metric | Meaning |
|--------|---------|
| **Frame Time** | Total time for one complete frame cycle |
| **Physics Frame** | Allocated time between physics updates (default 16.66ms for 60 FPS) |
| **Idle Time** | Non-physics logic duration |
| **Physics Time** | Physics-specific processing duration |

### Interface

- **Left panel**: Lists functions with their timing data
- **Right panel**: Performance graph over time
- Click the graph to examine specific frames
- Frame counter in top-right allows precise frame selection

### Measurement Modes

Via the **Measure** dropdown:

| Mode | Shows |
|------|-------|
| **Milliseconds** | Absolute time in ms |
| **Percentage (Frame)** | Relative to total frame time |
| **Percentage (Physics)** | Relative to physics time |

**Scope types:**
- **Inclusive** -- includes time spent in nested/called functions
- **Self** -- measures only the function body, excluding called functions

### Manual Timing in C#

```csharp
// Microsecond precision
ulong start = Time.GetTicksUsec();
ExpensiveOperation();
ulong elapsed = Time.GetTicksUsec() - start;
GD.Print($"Operation took {elapsed} microseconds");

// Millisecond precision
ulong startMs = Time.GetTicksMsec();
AnotherOperation();
GD.Print($"Operation took {Time.GetTicksMsec() - startMs}ms");
```

### Critical Limitation

**The Godot profiler does not currently support C# scripts.** For C# profiling:
- Use manual timing with `Time.GetTicksUsec()` / `Time.GetTicksMsec()`
- Use external .NET profilers (dotTrace, dotnet-trace, PerfView)
- Use Godot's Monitor tab for high-level metrics
- Use `Engine.GetFramesPerSecond()` for FPS monitoring

---

## 6. General Optimization Principles

### The Fundamental Truth

Software performance is about compromise. Two core approaches: **work faster** and **work smarter** -- ideally combined.

### Smoke and Mirrors

Games can give players the illusion of complexity far beyond what actually exists. Programmers should learn existing tricks and develop new ones rather than brute-forcing realistic simulations.

### Three Types of Slowness

1. **Continuous slow processes** every frame -- causes persistently low frame rates
2. **Intermittent slow processes** -- causes spikes and stalls
3. **Slow processes outside gameplay** -- e.g., level loading

### Measurement Tools

| Tool | Use Case |
|------|----------|
| Godot built-in profiler | Function-level timing |
| External CPU profilers | Deep engine-level analysis |
| GPU profilers (NVIDIA Nsight, Radeon GPU Profiler, PIX, Xcode, Arm Performance Studio) | GPU bottleneck analysis |
| RivaTuner Statistics Server / Special K / MangoHud | Frame rate monitoring (disable V-Sync first) |

**Always measure on multiple hardware configurations**, especially for mobile targets.

### The Optimization Cycle

```
1. Profile / Identify the bottleneck
2. Optimize the bottleneck
3. Re-profile to verify improvement
4. Return to step 1
```

### Donald Knuth's Principle

"Premature optimization is the root of all evil" -- but this does NOT mean ignore performance until the end. **Performant software results from performant design.** Design-stage decisions matter far more than late-stage polishing. An inefficient design cannot be made truly fast through optimization alone.

### Key Guidelines

1. **Developer time is limited** -- concentrate on what truly matters
2. **Optimization makes code harder to read/debug** -- limit to proven bottlenecks
3. **Design for performance first** -- good design often runs many times faster than bad design with micro-optimizations
4. **Incremental design** -- first designs are rarely optimal; iterate before committing to details
5. **Data-oriented design** -- modern CPUs are almost always limited by memory bandwidth; emphasize cache locality and linear access

### Algorithm and Data Structure First

Prioritize in this order:
1. Algorithm choice (Big-O complexity)
2. Data structure choice (cache-friendly layout)
3. Data access patterns (sequential over random)
4. Precalculation (at load time, from files, or as script constants)
5. Loop optimization (move calculations outside loops, flatten nested loops)

### Bottleneck Math

CPU and GPU run independently; total frame time equals the **slower** component:

```
CPU: 9ms    GPU: 50ms   -> Frame time: 50ms (GPU-bound)
CPU: 1ms    GPU: 50ms   -> Frame time: 50ms (CPU optimization had NO effect)
```

When 90% of time is in function A:
```
Before: A=9ms, Other=1ms, Total=10ms
After:  A=1ms, Other=1ms, Total=2ms   (5x improvement!)
```

When function A is fast but other bottlenecks exist:
```
Before: A=9ms, Other=50ms, Total=59ms
After:  A=1ms, Other=50ms, Total=51ms  (only 14% improvement)
```

**Always identify whether you are CPU-bound or GPU-bound before optimizing.**

---

## 7. CPU Optimization

### Profiling First

The Godot IDE profiler must be manually started/stopped. Recording timing measurements can slow your project significantly.

For deeper analysis, use external profilers (Valgrind/Callgrind, dotnet-trace for C#) to see where CPU time goes: graphics drivers, rendering, physics, etc.

### Manual Timing (C#)

```csharp
ulong timeStart = Time.GetTicksUsec();
UpdateEnemies();
ulong timeEnd = Time.GetTicksUsec();
GD.Print($"UpdateEnemies() took {timeEnd - timeStart} microseconds");
```

Run functions 1000+ times for accurate measurements -- timers have limited accuracy and CPU scheduling varies between runs.

### CPU Cache Awareness

Data not in CPU cache requires slow main memory access (**cache miss**). The first run of a function may be slow because data isn't cached yet.

**Optimization strategy**: Access memory sequentially rather than randomly. Godot's Server APIs are already optimized for cache-friendly access patterns.

### Language Performance Comparison

| Language | Speed | Ease | Deployment | GC Pauses |
|----------|-------|------|------------|-----------|
| **GDScript** | Slowest | Easiest | Built-in | No |
| **C#** | Fast | Good balance | Requires .NET | Yes -- watch for GC pauses |
| **C++ (GDExtension)** | Fastest | Hardest | Complex | No |
| **Rust (third-party)** | Fast | Hard | Complex | No |

### C# Garbage Collection Mitigation

```csharp
// BAD: Allocates every frame
public override void _Process(double delta)
{
    var enemies = GetTree().GetNodesInGroup("enemies");  // New array
    var text = $"Score: {_score}";                        // New string
}

// GOOD: Cache and reuse, update only when changed
private Godot.Collections.Array<Node> _cachedEnemies;
private int _lastScore = -1;

public override void _Process(double delta)
{
    // Only allocate when needed
}

public void RefreshEnemies()
{
    _cachedEnemies = GetTree().GetNodesInGroup("enemies");
}

public void UpdateScore(int score)
{
    if (score != _lastScore)
    {
        _lastScore = score;
        _scoreLabel.Text = $"Score: {score}";
    }
}
```

### SceneTree Optimization

Nodes have processing overhead. Key strategies:

1. **Fewer nodes with more logic each** can outperform many small nodes
2. **Remove from SceneTree** (without deleting) rather than pausing/hiding for better performance gains
3. **Disable processing** when not needed:

```csharp
public override void _Ready()
{
    // Start with processing disabled
    SetProcess(false);
    SetPhysicsProcess(false);
}

public void OnBecameActive()
{
    SetProcess(true);
    SetPhysicsProcess(true);
}

public void OnBecameInactive()
{
    SetProcess(false);
    SetPhysicsProcess(false);
}
```

### Physics Optimization

1. **Use simplified collision geometry** -- fewer vertices = faster collision detection
2. **Remove/reuse physics objects** outside the viewport
3. **Reduce physics tick rate** when acceptable (30 or even 20 Hz instead of default 60):
   - Set via `Engine.PhysicsTicksPerSecond`
   - Risk: jitter and input lag at lower rates
4. **Use fixed timestep interpolation** for smooth visuals at lower tick rates

### Threading

Multiple cores enable parallel work, but require synchronization to avoid race conditions.

```csharp
// C# threading (preferred over Godot Thread class in C#)
using System.Threading.Tasks;

public partial class AsyncLoader : Node
{
    private Task _loadTask;

    public override void _Ready()
    {
        _loadTask = Task.Run(() => HeavyComputation());
    }

    private void HeavyComputation()
    {
        // Do NOT access Godot API from background threads
        // Only process pure data here
        var result = ProcessData();

        // Use CallDeferred to interact with the scene tree
        CallDeferred(nameof(ApplyResult), result);
    }

    private void ApplyResult(int result)
    {
        // Safe to access Godot API here (runs on main thread)
        GD.Print($"Result: {result}");
    }
}
```

**Threading rules:**
- Creating threads is SLOW, especially on Windows -- create during loading, not just-in-time
- Always use locks/mutexes for shared data
- Never access Godot API from background threads (use `CallDeferred`)
- Check Thread Safe APIs documentation before using engine classes in threads

---

## 8. GPU Optimization

### Draw Calls and State Changes

The primary goal is to minimize GPU instructions and group similar objects together.

**2D Batching**: Godot automatically batches similar 2D items into single draw calls, minimizing state and texture changes.

**3D Batching**: Join meshes ahead of time (static relative to each other) rather than combining dynamically. Caveat: batched objects cannot be individually culled, so placement and visibility matter.

### Material and Shader Reuse

Godot automatically reuses shaders between `StandardMaterial3D` instances with identical configurations, even if parameters differ.

**Warning**: 20,000 objects with 20,000 different materials each will render slowly. Reuse materials wherever possible.

### Polygon Count by Platform

| Platform | Vertex Handling | Guidance |
|----------|----------------|----------|
| **Desktop/Console** | Efficient GPU processing | Polycount less critical |
| **Mobile** | Tile-based rendering | Avoid concentrated geometry in small screen areas; avoid triangles smaller than individual pixels |

### Fill Rate Optimization

Fill rate is expensive at higher resolutions. To test if you're fill-rate limited:
1. Disable V-Sync
2. Compare FPS in large window vs. small window
3. If FPS scales with window size, you're fill-rate bound

**Solutions:**
- Simplify shaders
- Reduce texture count per material
- Use vertex shading for particles instead of fragment shading
- Reduce transparent/overlapping objects

### Texture Management

- **VRAM compression**: Godot compresses 3D textures by default for bandwidth reduction
- **Pixel art exception**: Disable compression for pixel art textures (causes artifacts)
- **Shader texture reads**: Use algorithms requiring as few texture reads as possible

### Transparency Performance

Transparent objects must render back-to-front ("painter's order") and cannot use Z-buffer optimizations. Multiple overlapping transparent objects are especially expensive.

**Guideline**: Rendering more complex opaque geometry can be FASTER than using transparency.

### Shadow Optimization

- Reduce shadowmap size to increase performance (both writing and reading)
- Disable shadows on smaller or distant lights
- Use fewer shadow-casting lights overall

### Post-Processing

Be cautious with post-processing on mobile. Tile-based renderers struggle when tiles depend on results from other tiles.

### Multi-Platform Strategy

1. **Test early and often** on all target platforms, especially mobile
2. **Design for the lowest common denominator** first
3. **Add optional enhancements** for more powerful hardware using feature tags:

```csharp
[Tool]
public partial class QualitySettings : Node
{
    public override void _Ready()
    {
        if (OS.HasFeature("mobile"))
        {
            // Lower quality for mobile
            ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", 0);
            RenderingServer.EnvironmentSetSsaoQuality(
                RenderingServer.EnvironmentSsaoQuality.Low, true, 0.5f, 2, 50f, 300f);
        }
    }
}
```

### GPU Optimization Checklist

- [ ] Reuse materials and shaders across objects
- [ ] Merge static meshes where appropriate
- [ ] Use LOD (Level of Detail) for distant objects
- [ ] Enable occlusion culling for complex 3D scenes
- [ ] Minimize transparent overlapping surfaces
- [ ] Reduce shadowmap resolution where possible
- [ ] Test fill rate by comparing FPS at different resolutions
- [ ] Profile with GPU-specific tools (Nsight, RGP, PIX)
- [ ] Test on target mobile hardware early

---

## 9. Godot Notifications System

### Core Concept

Every `GodotObject` implements `_Notification(int what)` allowing it to respond to engine-level callbacks. When the engine tells a `CanvasItem` to draw, it calls `_Notification(NotificationDraw)`.

### Notification-to-Virtual-Method Mapping

| Notification | Virtual Method | When Called |
|-------------|---------------|-------------|
| `NOTIFICATION_READY` | `_Ready()` | Node and all children have entered the tree |
| `NOTIFICATION_ENTER_TREE` | `_EnterTree()` | Node enters the scene tree |
| `NOTIFICATION_EXIT_TREE` | `_ExitTree()` | Node exits the scene tree |
| `NOTIFICATION_PROCESS` | `_Process(delta)` | Every visual frame |
| `NOTIFICATION_PHYSICS_PROCESS` | `_PhysicsProcess(delta)` | Every physics tick |
| `NOTIFICATION_DRAW` | `_Draw()` | CanvasItem needs to redraw |
| `NOTIFICATION_PARENTED` | (none) | Node gets a parent |
| `NOTIFICATION_UNPARENTED` | (none) | Node loses its parent |
| `NOTIFICATION_POSTINITIALIZE` | (none) | Object initialization (not accessible to scripts) |
| `NOTIFICATION_PREDELETE` | (none) | Before engine deletes the Object (destructor) |

### Using _Notification Directly (C#)

```csharp
public partial class NotificationExample : Node2D
{
    public override void _Notification(int what)
    {
        switch (what)
        {
            case NotificationReady:
                GD.Print("Ready!");
                break;
            case NotificationEnterTree:
                GD.Print("Entered tree");
                break;
            case NotificationExitTree:
                GD.Print("Exited tree");
                break;
            case NotificationParented:
                GD.Print("Got a parent -- fires even outside SceneTree");
                break;
            case NotificationUnparented:
                GD.Print("Lost parent");
                break;
            case NotificationPredelete:
                GD.Print("About to be deleted -- cleanup here");
                break;
        }
    }
}
```

### _Process vs _PhysicsProcess vs Input Callbacks

| Method | Delta Type | Use For |
|--------|-----------|---------|
| `_Process(delta)` | Framerate-dependent | Visual updates, UI, caching, non-physics recurring logic |
| `_PhysicsProcess(delta)` | Framerate-independent (fixed) | Kinematic movement, transform operations, physics |
| `_Input(event)` / `_UnhandledInput(event)` | Event-driven | Input handling (only fires when input occurs) |

**Key insight**: Use input callbacks instead of checking input in `_Process` -- they trigger only on frames where input was actually detected, improving performance.

```csharp
// GOOD: Event-driven input
public override void _UnhandledInput(InputEvent @event)
{
    if (@event.IsActionPressed("jump"))
    {
        Jump();
    }
}

// LESS EFFICIENT: Polling in _Process
public override void _Process(double delta)
{
    if (Input.IsActionJustPressed("jump"))  // Checked every single frame
    {
        Jump();
    }
}
```

### _init / Constructor vs _Ready vs _EnterTree

**Property initialization sequence in C#:**

1. **Field initializers** -- run during object construction, setters NOT triggered
2. **Constructor** -- C# constructor runs (equivalent to GDScript `_init()`), setters ARE triggered
3. **_EnterTree()** -- fires when node enters the scene tree (cascades top-down)
4. **_Ready()** -- fires after the node AND all its children have entered the tree (cascades bottom-up)
5. **Exported property values** (from Inspector) -- applied between construction and tree entry, triggers setters

```csharp
public partial class InitOrder : Node
{
    // Step 1: Field initializer (no setter triggered)
    private int _health = 100;

    [Export]
    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            GD.Print($"Health setter: {value}");
            // Step 2 (constructor) and Step 5 (Inspector values) trigger this
        }
    }

    // Constructor (equivalent to GDScript _init)
    public InitOrder()
    {
        Health = 50;  // Triggers setter
        GD.Print("Constructor");
    }

    public override void _EnterTree()
    {
        GD.Print("EnterTree -- top-down cascade");
    }

    public override void _Ready()
    {
        GD.Print("Ready -- bottom-up cascade (children are ready)");
    }
}
```

### Tree Building Order

When instantiating a scene:
1. Godot builds the tree **downward** from root to leaves
2. `_EnterTree()` cascades **top-down** (parent before children)
3. `_Ready()` cascades **bottom-up** (leaf nodes first, then parents)

**NOTIFICATION_PARENTED** fires when a node gets a parent, regardless of whether the SceneTree is involved. Use this when you need behavior to trigger even for standalone node operations outside the scene tree.

---

## 10. Autoloads vs Regular Nodes

### What Are Autoloads?

Autoloads automatically load nodes at the root of your project, allowing global access. They persist across scene changes made via `SceneTree.ChangeSceneToFile()`.

### The Problem: Global State Anti-Pattern

The "cutting audio issue" demonstrates why global managers cause problems:

1. **Single point of failure**: One object responsible for all objects' data. If the Sound class has errors or runs out of AudioStreamPlayers, all callers break.
2. **Debugging difficulty**: Any object can call `Sound.Play(path)` from anywhere, making it hard to trace issues.
3. **Unpredictable resources**: Either too few players (bugs) or too many (wasted memory).

**Better approach**: Each scene manages its own AudioStreamPlayer nodes.

### Alternatives to Autoloads

**For shared functionality:**

```csharp
// Alternative 1: Custom node types with class_name
// Create reusable node types that scenes instantiate themselves
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();

    [Export] public int MaxHealth { get; set; } = 100;
    public int CurrentHealth { get; private set; }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        if (CurrentHealth <= 0)
            EmitSignal(SignalName.Died);
    }
}
```

```csharp
// Alternative 2: Static helper functions (no instance needed)
public static class MathHelpers
{
    public static float EaseOutBounce(float x)
    {
        // Pure function, no global state
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (x < 1f / d1)
            return n1 * x * x;
        else if (x < 2f / d1)
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        else if (x < 2.5f / d1)
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        else
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
    }
}
```

```csharp
// Alternative 3: Static variables for shared state across instances (Godot 4.1+)
// In C#, just use regular static fields
public partial class EnemyTracker : Node
{
    public static int TotalEnemiesAlive { get; private set; } = 0;

    public override void _EnterTree()
    {
        TotalEnemiesAlive++;
    }

    public override void _ExitTree()
    {
        TotalEnemiesAlive--;
    }
}
```

**For shared data:**

```csharp
// Use Resources for shared data
[GlobalClass]
public partial class GameConfig : Resource
{
    [Export] public float PlayerSpeed { get; set; } = 300f;
    [Export] public int StartingHealth { get; set; } = 100;
    [Export] public float GravityScale { get; set; } = 1.0f;
}

// Any node can load and use it
public partial class Player : CharacterBody2D
{
    [Export] public GameConfig Config { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        // Uses shared config without autoload
        Velocity = direction * Config.PlayerSpeed;
    }
}
```

### When Autoloads ARE Appropriate

Use autoloads for **systems with wide scope** where the autoload manages its own information and does NOT invade other objects' data. Good candidates:

- **Quest systems** -- track global quest state
- **Dialogue systems** -- manage conversation flow across scenes
- **Save/Load systems** -- coordinate serialization
- **Scene transition manager** -- handle scene changes with effects

```csharp
// GOOD autoload: Manages its own data, doesn't control other nodes
public partial class QuestSystem : Node
{
    private Dictionary<string, QuestState> _quests = new();

    public void StartQuest(string questId)
    {
        _quests[questId] = new QuestState { Status = QuestStatus.Active };
    }

    public bool IsQuestComplete(string questId)
    {
        return _quests.TryGetValue(questId, out var state)
            && state.Status == QuestStatus.Complete;
    }
}
```

### Key Clarification

An autoload is NOT necessarily a singleton. Nothing prevents instantiating additional copies of an autoloaded node. Access autoloads via:

```csharp
// Access autoload by path
var questSystem = GetNode<QuestSystem>("/root/QuestSystem");

// Or use the generated singleton accessor (if using class_name in GDScript)
// In C#, typically just use GetNode with the autoload name
```

### Decision Guide

| Scenario | Use Autoload? | Alternative |
|----------|--------------|-------------|
| Audio that survives scene transitions | Maybe | Scene-local AudioStreamPlayers |
| Global game configuration | No | Resource files with [Export] |
| Helper/utility functions | No | Static classes |
| Shared counters/state across instances | No | Static variables |
| Quest/dialogue system spanning scenes | Yes | -- |
| Save/load coordination | Yes | -- |
| Scene transition effects | Yes | -- |
| Input remapping persistence | Yes | -- |
| Per-scene UI management | No | Scene-local nodes |
| Per-scene enemy management | No | Scene-local nodes or groups |

**Rule of thumb**: If you have more than 3-4 autoloads, you are probably overusing them. Move scene-specific logic into scene-local nodes and use signals, groups, or exported references for communication.

---

## Quick Reference: Key C# API Equivalents

| GDScript | C# |
|----------|-----|
| `OS.has_feature("tag")` | `OS.HasFeature("tag")` |
| `Engine.is_editor_hint()` | `Engine.IsEditorHint()` |
| `Time.get_ticks_usec()` | `Time.GetTicksUsec()` |
| `Time.get_ticks_msec()` | `Time.GetTicksMsec()` |
| `Engine.get_frames_per_second()` | `Engine.GetFramesPerSecond()` |
| `get_tree().edited_scene_root` | `GetTree().EditedSceneRoot` |
| `@tool` | `[Tool]` attribute |
| `@export` | `[Export]` attribute |
| `breakpoint` keyword | Set breakpoint in IDE |
| `OS.get_cmdline_args()` | `OS.GetCmdlineArgs()` |
| `OS.get_cmdline_user_args()` | `OS.GetCmdlineUserArgs()` |
| `ProjectSettings.get_setting_with_override()` | `ProjectSettings.GetSettingWithOverride()` |
| `Node.queue_free()` | `Node.QueueFree()` |
| `set_process(false)` | `SetProcess(false)` |
| `set_physics_process(false)` | `SetPhysicsProcess(false)` |
