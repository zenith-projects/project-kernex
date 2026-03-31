# Shaders, i18n, Instancing with Signals, and Ray-casting in C#

Source: Godot stable documentation (8 pages), all GDScript converted to C#.

---

## 1. SHADER FUNDAMENTALS

### Shader Types (first line of every .gdshader file)

```glsl
shader_type spatial;       // 3D rendering
shader_type canvas_item;   // 2D rendering
shader_type particles;     // particle systems
shader_type sky;           // sky rendering
shader_type fog;           // FogVolume rendering
```

### Processor Functions

| Function     | Runs on                  | Available in                      |
|-------------|--------------------------|-----------------------------------|
| `vertex()`  | Every vertex             | spatial, canvas_item              |
| `fragment()`| Every pixel              | spatial, canvas_item              |
| `light()`   | Every pixel * every light| spatial, canvas_item              |
| `start()`   | Particle spawn (once)    | particles                         |
| `process()` | Particle every frame     | particles                         |
| `sky()`     | Every sky pixel          | sky                               |
| `fog()`     | Every fog froxel         | fog                               |

### Render Modes

```glsl
shader_type spatial;
render_mode unshaded, cull_disabled;
```

### How to Attach a Shader

In the editor: Node Inspector > Material > New ShaderMaterial > Shader > New Shader.

From C# code:

```csharp
// Create and assign a ShaderMaterial programmatically
var shaderMaterial = new ShaderMaterial();
var shader = GD.Load<Shader>("res://my_shader.gdshader");
shaderMaterial.Shader = shader;

// For a Sprite2D / any CanvasItem:
mySprite.Material = shaderMaterial;

// For a MeshInstance3D (on the mesh resource):
var planeMesh = new PlaneMesh();
planeMesh.Material = shaderMaterial;
meshInstance.Mesh = planeMesh;

// For a MeshInstance3D (override per-instance):
meshInstance.MaterialOverride = shaderMaterial;
```

---

## 2. YOUR FIRST 2D SHADER (canvas_item)

### Scene Setup
- Add a Sprite2D node, set its Texture to `icon.svg`
- In Inspector > CanvasItem > Material > New ShaderMaterial
- In the ShaderMaterial, Shader > New Shader

### Minimal Fragment Shader (solid color)

```glsl
shader_type canvas_item;

void fragment() {
    COLOR = vec4(0.4, 0.6, 0.9, 1.0);
}
```

### Using UV Coordinates

```glsl
shader_type canvas_item;

void fragment() {
    COLOR = vec4(UV, 0.5, 1.0);
}
```

### Reading from the Sprite Texture + Tinting

```glsl
shader_type canvas_item;

void fragment() {
    COLOR = texture(TEXTURE, UV); // read from sprite texture
    COLOR.b = 1.0;               // force blue channel to 1.0
}
```

### Uniforms (passed from C#)

```glsl
shader_type canvas_item;

uniform float blue = 1.0;

void fragment() {
    COLOR = texture(TEXTURE, UV);
    COLOR.b = blue;
}
```

### Setting Uniforms from C#

```csharp
// On a Sprite2D (or any CanvasItem) that has a ShaderMaterial assigned:
float blueValue = 1.0f;
((ShaderMaterial)Material).SetShaderParameter("blue", blueValue);
```

### Vertex Function -- Offset and Animation

```glsl
shader_type canvas_item;

void vertex() {
    VERTEX += vec2(10.0, 0.0);
}
```

```glsl
shader_type canvas_item;

void vertex() {
    // Animate sprite moving in a circle
    VERTEX += vec2(cos(TIME) * 100.0, sin(TIME) * 100.0);
}
```

---

## 3. YOUR FIRST 3D SHADER (spatial)

### Scene Setup
1. Add MeshInstance3D node
2. Set its Mesh to a new PlaneMesh
3. Set PlaneMesh Subdivide Width and Subdivide Depth to 32
4. Set PlaneMesh Material to a new ShaderMaterial
5. Set the ShaderMaterial's Shader to a new Shader

### Basic Vertex Displacement

```glsl
shader_type spatial;

void vertex() {
    VERTEX.y += cos(VERTEX.x) * sin(VERTEX.z);
}
```

### Scaled Vertex Displacement

```glsl
shader_type spatial;

void vertex() {
    VERTEX.y += cos(VERTEX.x * 4.0) * sin(VERTEX.z * 4.0);
}
```

### Noise Heightmap with Uniform

```glsl
shader_type spatial;

uniform float height_scale = 0.5;
uniform sampler2D noise;

void vertex() {
    float height = texture(noise, VERTEX.xz / 2.0 + 0.5).x;
    VERTEX.y += height * height_scale;
}
```

Assign a NoiseTexture2D (with FastNoiseLite) to the "noise" shader parameter in the inspector.

### Setting Shader Parameters from C#

```csharp
// On the MeshInstance3D:
var mesh = GetNode<MeshInstance3D>("MeshInstance3D").Mesh as PlaneMesh;
((ShaderMaterial)mesh.Material).SetShaderParameter("height_scale", 0.5f);

// Or if using MaterialOverride:
var mat = (ShaderMaterial)meshInstance.MaterialOverride;
mat.SetShaderParameter("height_scale", 0.5f);
```

### Full 3D Shader with Normal Map

```glsl
shader_type spatial;

uniform float height_scale = 0.5;
uniform sampler2D noise;
uniform sampler2D normalmap;

varying vec2 tex_position;

void vertex() {
    tex_position = VERTEX.xz / 2.0 + 0.5;
    float height = texture(noise, tex_position).x;
    VERTEX.y += height * height_scale;
}

void fragment() {
    NORMAL_MAP = texture(normalmap, tex_position).xyz;
}
```

Assign a second NoiseTexture2D with "As Normal Map" checked to the "normalmap" parameter.

### Programmatic Setup from C# (complete)

```csharp
using Godot;

public partial class TerrainSetup : Node3D
{
    public override void _Ready()
    {
        var meshInstance = new MeshInstance3D();
        AddChild(meshInstance);

        var planeMesh = new PlaneMesh();
        planeMesh.SubdivideWidth = 32;
        planeMesh.SubdivideDepth = 32;

        var shaderMat = new ShaderMaterial();
        shaderMat.Shader = GD.Load<Shader>("res://terrain.gdshader");

        // Create noise texture for heightmap
        var noiseTexture = new NoiseTexture2D();
        noiseTexture.Noise = new FastNoiseLite();
        shaderMat.SetShaderParameter("noise", noiseTexture);
        shaderMat.SetShaderParameter("height_scale", 0.5f);

        // Create normal map noise texture
        var normalTexture = new NoiseTexture2D();
        normalTexture.Noise = new FastNoiseLite();
        normalTexture.AsNormalMap = true;
        shaderMat.SetShaderParameter("normalmap", normalTexture);

        planeMesh.Material = shaderMat;
        meshInstance.Mesh = planeMesh;
    }
}
```

---

## 4. SCREEN-READING SHADERS

### Reading the Screen Texture (invisible pass-through)

```glsl
shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
    COLOR = textureLod(screen_texture, SCREEN_UV, 0.0);
}
```

Key points:
- `hint_screen_texture` tells the shader to sample from the already-rendered screen
- Use `textureLod` with LOD `0.0` for unblurred screen
- Increase LOD and change filter to `filter_nearest_mipmap` for a blurred read
- `SCREEN_UV` gives the current fragment's screen-space UV

### Brightness / Contrast / Saturation Shader

```glsl
shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

void fragment() {
    vec3 c = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;

    c.rgb = mix(vec3(0.0), c.rgb, brightness);
    c.rgb = mix(vec3(0.5), c.rgb, contrast);
    c.rgb = mix(vec3(dot(vec3(1.0), c.rgb) * 0.33333), c.rgb, saturation);

    COLOR.rgb = c;
}
```

### BackBufferCopy Node (2D overlapping screen-read shaders)
When multiple 2D nodes use `hint_screen_texture`, only the first triggers a screen copy.
Insert a `BackBufferCopy` node between them to force a fresh copy so they composite correctly.

### Depth Texture (3D only)

```glsl
uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;

void fragment() {
    float depth = textureLod(depth_texture, SCREEN_UV, 0.0).r;
    vec4 upos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth, 1.0);
    vec3 pixel_position = upos.xyz / upos.w;
}
```

### Normal-Roughness Texture (Forward+ only)

```glsl
uniform sampler2D normal_roughness_texture : hint_normal_roughness_texture, repeat_disable, filter_nearest;

void fragment() {
    float screen_roughness = texture(normal_roughness_texture, SCREEN_UV).w;
    vec3 screen_normal = texture(normal_roughness_texture, SCREEN_UV).xyz;
    screen_normal = screen_normal * 2.0 - 1.0;
}
```

### Controlling Screen-Read Shaders from C#

```csharp
using Godot;

public partial class ScreenEffectController : Node
{
    private ShaderMaterial _effectMaterial;

    public override void _Ready()
    {
        var colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
        _effectMaterial = (ShaderMaterial)colorRect.Material;
    }

    public void SetBrightness(float value)
    {
        _effectMaterial.SetShaderParameter("brightness", value);
    }

    public void SetContrast(float value)
    {
        _effectMaterial.SetShaderParameter("contrast", value);
    }

    public void SetSaturation(float value)
    {
        _effectMaterial.SetShaderParameter("saturation", value);
    }
}
```

---

## 5. CUSTOM POST-PROCESSING

### Single-Pass Setup

Scene tree:
```
Main Scene
  CanvasLayer
    ColorRect  (Anchor Preset: Full Rect, with ShaderMaterial)
```

1. Create a CanvasLayer
2. Add a ColorRect child
3. Set the ColorRect's anchor preset to Full Rect (covers entire screen)
4. Assign a ShaderMaterial with your post-processing shader

### Hex Pixelization Post-Process Shader

```glsl
shader_type canvas_item;

uniform vec2 size = vec2(32.0, 28.0);
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
    vec2 norm_size = size * SCREEN_PIXEL_SIZE;
    bool less_than_half = mod(SCREEN_UV.y / 2.0, norm_size.y) / norm_size.y < 0.5;
    vec2 uv = SCREEN_UV + vec2(norm_size.x * 0.5 * float(less_than_half), 0.0);
    vec2 center_uv = floor(uv / norm_size) * norm_size;
    vec2 norm_uv = mod(uv, norm_size) / norm_size;
    center_uv += mix(vec2(0.0, 0.0),
                     mix(mix(vec2(norm_size.x, -norm_size.y),
                             vec2(0.0, -norm_size.y),
                             float(norm_uv.x < 0.5)),
                         mix(vec2(0.0, -norm_size.y),
                             vec2(-norm_size.x, -norm_size.y),
                             float(norm_uv.x < 0.5)),
                         float(less_than_half)),
                     float(norm_uv.y < 0.3333333) * float(norm_uv.y / 0.3333333 < (abs(norm_uv.x - 0.5) * 2.0)));

    COLOR = textureLod(screen_texture, center_uv, 0.0);
}
```

### Multi-Pass Post-Processing (Gaussian Blur)

Scene tree:
```
Main Scene
  CanvasLayer        (layer 1 -- horizontal blur)
    ColorRect        (Full Rect, ShaderMaterial with X-blur)
  CanvasLayer2       (layer 2 -- vertical blur)
    ColorRect        (Full Rect, ShaderMaterial with Y-blur)
```

**Pass 1: Horizontal Blur**

```glsl
shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Blurs the screen in the X-direction.
void fragment() {
    vec3 col = texture(screen_texture, SCREEN_UV).xyz * 0.16;
    col += texture(screen_texture, SCREEN_UV + vec2(SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.15;
    col += texture(screen_texture, SCREEN_UV + vec2(-SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.15;
    col += texture(screen_texture, SCREEN_UV + vec2(2.0 * SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.12;
    col += texture(screen_texture, SCREEN_UV + vec2(2.0 * -SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.12;
    col += texture(screen_texture, SCREEN_UV + vec2(3.0 * SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.09;
    col += texture(screen_texture, SCREEN_UV + vec2(3.0 * -SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.09;
    col += texture(screen_texture, SCREEN_UV + vec2(4.0 * SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.05;
    col += texture(screen_texture, SCREEN_UV + vec2(4.0 * -SCREEN_PIXEL_SIZE.x, 0.0)).xyz * 0.05;
    COLOR.xyz = col;
}
```

**Pass 2: Vertical Blur**

```glsl
shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Blurs the screen in the Y-direction.
void fragment() {
    vec3 col = texture(screen_texture, SCREEN_UV).xyz * 0.16;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, SCREEN_PIXEL_SIZE.y)).xyz * 0.15;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, -SCREEN_PIXEL_SIZE.y)).xyz * 0.15;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 2.0 * SCREEN_PIXEL_SIZE.y)).xyz * 0.12;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 2.0 * -SCREEN_PIXEL_SIZE.y)).xyz * 0.12;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 3.0 * SCREEN_PIXEL_SIZE.y)).xyz * 0.09;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 3.0 * -SCREEN_PIXEL_SIZE.y)).xyz * 0.09;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 4.0 * SCREEN_PIXEL_SIZE.y)).xyz * 0.05;
    col += texture(screen_texture, SCREEN_UV + vec2(0.0, 4.0 * -SCREEN_PIXEL_SIZE.y)).xyz * 0.05;
    COLOR.xyz = col;
}
```

### Post-Processing Setup from C# (complete)

```csharp
using Godot;

public partial class PostProcessSetup : Node
{
    public override void _Ready()
    {
        // Single-pass post-processing
        var canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);

        var colorRect = new ColorRect();
        colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        canvasLayer.AddChild(colorRect);

        var shaderMat = new ShaderMaterial();
        shaderMat.Shader = GD.Load<Shader>("res://post_process.gdshader");
        colorRect.Material = shaderMat;

        // Adjust parameters at runtime
        shaderMat.SetShaderParameter("brightness", 1.2f);
        shaderMat.SetShaderParameter("contrast", 1.1f);
        shaderMat.SetShaderParameter("saturation", 0.8f);
    }
}
```

### Multi-Pass from C#

```csharp
using Godot;

public partial class MultiPassBlur : Node
{
    public override void _Ready()
    {
        // Pass 1: Horizontal blur
        var layer1 = new CanvasLayer();
        layer1.Layer = 1;
        AddChild(layer1);

        var rect1 = new ColorRect();
        rect1.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer1.AddChild(rect1);

        var mat1 = new ShaderMaterial();
        mat1.Shader = GD.Load<Shader>("res://blur_horizontal.gdshader");
        rect1.Material = mat1;

        // Pass 2: Vertical blur
        var layer2 = new CanvasLayer();
        layer2.Layer = 2;
        AddChild(layer2);

        var rect2 = new ColorRect();
        rect2.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer2.AddChild(rect2);

        var mat2 = new ShaderMaterial();
        mat2.Shader = GD.Load<Shader>("res://blur_vertical.gdshader");
        rect2.Material = mat2;
    }
}
```

---

## 6. INTERNATIONALIZATION (i18n)

### Setup Steps
1. Create translation files (CSV or PO format)
2. Import them in Project > Project Settings > Localization > Translations
3. Add each .csv or .po file to the translation list

### Tr() -- Basic Translation Lookup

```csharp
// Basic key lookup
GetNode<Label>("Level").Text = Tr("LEVEL_5_NAME");

// With string interpolation for dynamic keys
GetNode<Label>("Status").Text = Tr($"GAME_STATUS_{statusIndex}");
```

### Translation Contexts (disambiguating identical source strings)

```csharp
// "Close" as a verb (action)
GetNode<Button>("Button").Text = Tr("Close", "Actions");

// "Close" as adjective (distance)
GetNode<Label>("Distance").Text = Tr("Close", "Distance");
```

### TrN() -- Pluralization

```csharp
int numApples = 5;
GetNode<Label>("Label").Text = string.Format(
    TrN("There is {0} apple", "There are {0} apples", numApples), numApples);
```

### TrN() with Context

```csharp
int numJobs = 1;
GetNode<Label>("Label").Text = string.Format(
    TrN("{0} job", "{0} jobs", numJobs, "Task Manager"), numJobs);
```

### Placeholders with Named Arguments

```csharp
// Using C# string interpolation with Tr():
string character = "Ogre";
string weapon = "Sword";
// Option 1: positional
GetNode<Label>("Message").Text = string.Format(Tr("{0} picked up the {1}"), character, weapon);

// Option 2: using Godot's string Format for named placeholders
// In GDScript this uses str.format({key: value}), in C# use string.Replace or interpolation
string template = Tr("{character} picked up the {weapon}");
GetNode<Label>("Message").Text = template
    .Replace("{character}", character)
    .Replace("{weapon}", weapon);
```

### Automatic Language Detection and Runtime Switching

```csharp
using Godot;

public partial class LanguageManager : Node
{
    public override void _Ready()
    {
        string language = "automatic"; // load from user settings
        if (language == "automatic")
        {
            string preferredLanguage = OS.GetLocaleLanguage();
            TranslationServer.SetLocale(preferredLanguage);
        }
        else
        {
            TranslationServer.SetLocale(language);
        }
    }

    // Call this from a language selection UI
    public void ChangeLanguage(string localeCode)
    {
        TranslationServer.SetLocale(localeCode);
        // UI nodes with Auto Translate enabled will update automatically
    }
}
```

### Auto Translation on Nodes
- Label, Button, etc. auto-translate if their text matches a translation key
- To disable: set node's Auto Translate > Mode to "Disabled" in the inspector
- Or from code:

```csharp
// Disable auto-translation for a node (e.g., player name label)
GetNode<Label>("PlayerName").AutoTranslateMode = Node.AutoTranslateModeEnum.Disabled;
```

---

## 7. INSTANCING WITH SIGNALS

### The Problem
Adding bullets (or any spawned object) as children of the player makes them rotate/move with the player. Adding them via `GetParent().AddChild()` couples the player to the scene tree structure.

### The Solution: Emit a Signal with the PackedScene

**Bullet.cs (Area2D)**

```csharp
using Godot;

public partial class Bullet : Area2D
{
    public Vector2 Velocity { get; set; } = Vector2.Right;

    public override void _PhysicsProcess(double delta)
    {
        Position += Velocity * (float)delta;
    }
}
```

**Player.cs (Sprite2D)**

```csharp
using Godot;

public partial class Player : Sprite2D
{
    [Signal]
    public delegate void ShootEventHandler(PackedScene bullet, float direction, Vector2 location);

    private PackedScene _bullet = GD.Load<PackedScene>("res://Bullet.tscn");

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                EmitSignal(SignalName.Shoot, _bullet, Rotation, Position);
            }
        }
    }

    public override void _Process(double delta)
    {
        LookAt(GetGlobalMousePosition());
    }
}
```

**Main.cs (receives the signal, spawns the bullet)**

```csharp
using Godot;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        // Connect in code (or connect via editor Node tab)
        GetNode<Player>("Player").Shoot += OnPlayerShoot;
    }

    private void OnPlayerShoot(PackedScene bullet, float direction, Vector2 location)
    {
        var spawnedBullet = bullet.Instantiate<Bullet>();
        AddChild(spawnedBullet);
        spawnedBullet.Rotation = direction;
        spawnedBullet.Position = location;
        spawnedBullet.Velocity = spawnedBullet.Velocity.Rotated(direction);
    }
}
```

Key pattern: The player knows nothing about where bullets go. The main scene (or any listener) receives the signal and decides where to place them in the tree.

---

## 8. RAY-CASTING FROM C#

### Accessing Physics Space State

```csharp
// 2D -- in _PhysicsProcess only
public override void _PhysicsProcess(double delta)
{
    var spaceState = GetWorld2D().DirectSpaceState;
}

// 3D
public override void _PhysicsProcess(double delta)
{
    var spaceState = GetWorld3D().DirectSpaceState;
}
```

**Important:** Raycasts must run in `_PhysicsProcess()`. During `_Input()` the physics space may be locked.

### 2D Raycast

```csharp
public override void _PhysicsProcess(double delta)
{
    var spaceState = GetWorld2D().DirectSpaceState;
    // Use global coordinates, not local
    var query = PhysicsRayQueryParameters2D.Create(Vector2.Zero, new Vector2(50, 100));
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0)
    {
        GD.Print("Hit at point: ", result["position"]);
        // result["position"]  -- Vector2 hit position
        // result["normal"]    -- Vector2 surface normal
        // result["collider"]  -- Object that was hit
        // result["rid"]       -- RID of the collider
        // result["shape"]     -- shape index
    }
}
```

### 3D Raycast from Camera/Mouse

```csharp
private const int RayLength = 1000;

public override void _PhysicsProcess(double delta)
{
    var spaceState = GetWorld3D().DirectSpaceState;
    var cam = GetNode<Camera3D>("Camera3D");
    var mousePos = GetViewport().GetMousePosition();

    var origin = cam.ProjectRayOrigin(mousePos);
    var end = origin + cam.ProjectRayNormal(mousePos) * RayLength;
    var query = PhysicsRayQueryParameters3D.Create(origin, end);
    query.CollideWithAreas = true;

    var result = spaceState.IntersectRay(query);

    if (result.Count > 0)
    {
        var hitPosition = (Vector3)result["position"];
        var hitNormal = (Vector3)result["normal"];
        var collider = (GodotObject)result["collider"];
        GD.Print($"Hit {collider.GetClass()} at {hitPosition}");
    }
}
```

### Getting Ray from Mouse Click (store for _PhysicsProcess)

```csharp
private const float RayLength = 1000.0f;
private Vector3 _rayFrom;
private Vector3 _rayTo;
private bool _shouldCast;

public override void _Input(InputEvent @event)
{
    if (@event is InputEventMouseButton eventMouseButton
        && eventMouseButton.Pressed
        && eventMouseButton.ButtonIndex == MouseButton.Left)
    {
        var camera3D = GetNode<Camera3D>("Camera3D");
        _rayFrom = camera3D.ProjectRayOrigin(eventMouseButton.Position);
        _rayTo = _rayFrom + camera3D.ProjectRayNormal(eventMouseButton.Position) * RayLength;
        _shouldCast = true;
    }
}

public override void _PhysicsProcess(double delta)
{
    if (!_shouldCast) return;
    _shouldCast = false;

    var spaceState = GetWorld3D().DirectSpaceState;
    var query = PhysicsRayQueryParameters3D.Create(_rayFrom, _rayTo);
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0)
    {
        GD.Print("Clicked on: ", ((Node)result["collider"]).Name);
    }
}
```

### Excluding Self from Raycast

```csharp
using Godot;

public partial class MyCharacterBody2D : CharacterBody2D
{
    public override void _PhysicsProcess(double delta)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, _targetPosition);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = spaceState.IntersectRay(query);
    }
}
```

### Using Collision Mask

```csharp
using Godot;

public partial class MyCharacterBody2D : CharacterBody2D
{
    public override void _PhysicsProcess(double delta)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            _targetPosition,
            CollisionMask,                                    // use same mask as this body
            new Godot.Collections.Array<Rid> { GetRid() }    // exclude self
        );
        var result = spaceState.IntersectRay(query);
    }
}
```

### RayCast3D / RayCast2D Nodes (alternative to code-based raycasting)

For simple cases, add a RayCast3D or RayCast2D node as a child. Configure target_position, collision mask, etc. in the inspector. Query results in code:

```csharp
using Godot;

public partial class RayCastExample : Node3D
{
    private RayCast3D _ray;

    public override void _Ready()
    {
        _ray = GetNode<RayCast3D>("RayCast3D");
        _ray.Enabled = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ray.IsColliding())
        {
            var collider = _ray.GetCollider();
            var point = _ray.GetCollisionPoint();
            var normal = _ray.GetCollisionNormal();
            GD.Print($"Ray hit {((Node)collider).Name} at {point}");
        }
    }
}
```

---

## QUICK REFERENCE: Uniform Hint Types for Shaders

```glsl
uniform sampler2D my_tex : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D my_depth : hint_depth_texture, repeat_disable, filter_nearest;
uniform sampler2D my_normals : hint_normal_roughness_texture, repeat_disable, filter_nearest;
uniform sampler2D my_noise;                    // regular texture, set from inspector or C#
uniform float my_float = 1.0;
uniform vec2 my_vec2 = vec2(1.0, 1.0);
uniform vec4 my_color : source_color = vec4(1.0);   // shows color picker in inspector
```

Setting all of these from C#:

```csharp
var mat = (ShaderMaterial)someNode.Material;
mat.SetShaderParameter("my_float", 1.5f);
mat.SetShaderParameter("my_vec2", new Vector2(2.0f, 3.0f));
mat.SetShaderParameter("my_color", new Color(1, 0, 0, 1));
mat.SetShaderParameter("my_noise", GD.Load<Texture2D>("res://noise.png"));
```
