# Godot 4.6 — 2D Development Guide (C# Only)

Comprehensive reference covering movement, sprite animation, TileSets, TileMaps, parallax, lights/shadows, canvas layers, and particle systems.

---

## 1. 2D Movement Overview

### Concepts

All movement patterns use **CharacterBody2D** as the parent node. The scene tree for any movement style requires:
- `CharacterBody2D` (root)
  - `Sprite2D`
  - `CollisionShape2D`

Input actions (`left`, `right`, `up`, `down`, `click`) must be configured in **Project Settings > Input Map** before use.

The key method is `MoveAndSlide()`, which handles collision response automatically. Movement code goes in `_PhysicsProcess()` (not `_Process()`), ensuring frame-rate-independent physics.

### 8-Way Movement

The simplest pattern. Uses `Input.GetVector()` to get a normalized direction from four input actions, then multiplies by speed.

```csharp
using Godot;

public partial class Movement : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400;

    public void GetInput()
    {
        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        Velocity = inputDirection * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput();
        MoveAndSlide();
    }
}
```

**Key detail:** `Input.GetVector()` returns a normalized vector automatically, so diagonal movement is not faster than cardinal movement.

### Rotation + Movement (Asteroids-Style)

Left/right keys rotate the character; up/down move forward/backward relative to the character's facing direction. `Transform.X` gives the local forward direction.

```csharp
using Godot;

public partial class Movement : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400;

    [Export]
    public float RotationSpeed { get; set; } = 1.5f;

    private float _rotationDirection;

    public void GetInput()
    {
        _rotationDirection = Input.GetAxis("left", "right");
        Velocity = Transform.X * Input.GetAxis("down", "up") * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput();
        Rotation += _rotationDirection * RotationSpeed * (float)delta;
        MoveAndSlide();
    }
}
```

### Rotation + Movement (Mouse)

Character always faces the mouse pointer. Forward/backward is still keyboard-driven. `LookAt()` handles rotation.

```csharp
using Godot;

public partial class Movement : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400;

    public void GetInput()
    {
        LookAt(GetGlobalMousePosition());
        Velocity = Transform.X * Input.GetAxis("down", "up") * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput();
        MoveAndSlide();
    }
}
```

**Alternative angle calculation** (manual rotation instead of `LookAt()`):

```csharp
var rotation = GetGlobalMousePosition().AngleToPoint(Position);
```

### Click-and-Move

Click a point on screen; character moves there. Includes a distance threshold (`> 10`) to stop jittering at the destination.

```csharp
using Godot;

public partial class Movement : CharacterBody2D
{
    [Export]
    public int Speed { get; set; } = 400;

    private Vector2 _target;

    public override void _Input(InputEvent @event)
    {
        // Use IsActionPressed to only accept single taps, not mouse drags.
        if (@event.IsActionPressed("click"))
        {
            _target = GetGlobalMousePosition();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = Position.DirectionTo(_target) * Speed;
        // LookAt(_target);
        if (Position.DistanceTo(_target) > 10)
        {
            MoveAndSlide();
        }
    }
}
```

**Gotcha:** `_target` defaults to `Vector2.Zero` (world origin). Initialize it to `Position` in `_Ready()` if you don't want the character to immediately fly to the origin on startup.

---

## 2. 2D Sprite Animation

### Concepts

Two primary approaches:
1. **AnimatedSprite2D** — simplest, uses `SpriteFrames` resource. Good for straightforward frame-by-frame animation.
2. **AnimationPlayer + Sprite2D** — more powerful, can animate any property (position, scale, modulate, etc.) alongside frame changes.

### Method 1: Individual Images with AnimatedSprite2D

**Scene tree:**
- `CharacterBody2D` (or `Area2D` / `RigidBody2D`)
  - `AnimatedSprite2D`
  - `CollisionShape2D`

**Setup steps:**
1. Select `AnimatedSprite2D`, in its **SpriteFrames** property choose "New SpriteFrames."
2. A new panel appears at the bottom of the editor.
3. Drag individual frame images from the FileSystem dock into the center of the SpriteFrames panel.
4. Rename the animation from "default" to your animation name (e.g., "run").
5. Set **Speed (FPS)** to your desired value (e.g., 10).
6. Use the Play buttons in the panel to preview.
7. Add more animations via the "Add Animation" button.

**Note:** The root node can be `CharacterBody2D`, `Area2D`, or `RigidBody2D`. The animation technique is the same regardless. Assign a shape to `CollisionShape2D` separately.

**Controlling the animation via code:**

```csharp
using Godot;

public partial class Character : CharacterBody2D
{
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("ui_right"))
        {
            _animatedSprite.Play("run");
        }
        else
        {
            _animatedSprite.Stop();
        }
    }
}
```

### Method 2: Sprite Sheet with AnimatedSprite2D

Same scene tree as above. Instead of dragging individual images:

1. Select `AnimatedSprite2D`, create a new `SpriteFrames`.
2. In the SpriteFrames panel, click **"Add frames from a Sprite Sheet"**.
3. Select your sprite sheet file.
4. In the dialog, set the number of horizontal and vertical frames (e.g., 4 columns, 2 rows).
5. Select the frames you want, then click **"Add N frames"**.
6. Rename the animation (e.g., "jump").
7. Press Play to preview.

### Method 3: Sprite Sheet with AnimationPlayer

**Scene tree:**
- `CharacterBody2D`
  - `Sprite2D`
  - `AnimationPlayer`
  - `CollisionShape2D`

**Setup steps:**
1. Drag the sprite sheet into the Sprite2D's **Texture** property.
2. In the Sprite2D Inspector, expand **Animation** and set **Hframes** (horizontal frames) and **Vframes** (vertical frames). For a 6-frame horizontal sheet: `Hframes = 6`.
3. The **Frame** property (0 to N-1) controls which frame is displayed. This is the property you animate.
4. Select `AnimationPlayer`, click **Animation > New**, name it (e.g., "walk").
5. Set the animation length (e.g., `0.6` seconds) and enable **Loop**.
6. Select the `Sprite2D`, click the key icon to add a keyframe track for the `Frame` property.
7. Add a keyframe at each time step (default `0.1s`), incrementing the frame from 0 to 5.
8. Press Play to preview.

**Controlling AnimationPlayer via code:**

```csharp
using Godot;

public partial class Character : CharacterBody2D
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("ui_right"))
        {
            _animationPlayer.Play("walk");
        }
        else
        {
            _animationPlayer.Stop();
        }
    }
}
```

### Gotchas

- **`Play()` is not applied instantly.** It takes effect the next time the AnimationPlayer is processed (potentially the next frame). If you update a property (like `FlipH`) and call `Play()` in the same frame, you may get a "glitch frame" where the property changed but the animation hasn't started. **Fix:** Call `Advance(0)` immediately after `Play()` to force an immediate update.
- **AnimationPlayer is more complex but more powerful** than AnimatedSprite2D because it can animate any property (position, scale, modulate, etc.). AnimationPlayer can also be used together with AnimatedSprite2D.

---

## 3. Using TileSets

### Concepts

A **TileSet** is a collection of tiles used to create game layouts. Benefits of using `TileMapLayer` nodes:
- Much faster than placing individual `Sprite2D` nodes
- Optimized for rendering large numbers of tiles
- Support collision, occlusion, and navigation shapes per tile

Tiles can be: **square**, **isometric**, **hexagonal**, or **half-offset square**.

### Node Types

- **TileMapLayer** — the node that displays tiles on a grid
- **TileSet** — the resource containing tile definitions (assigned to TileMapLayer)

### Creating a TileSet from a Tilesheet

1. Create a `TileMapLayer` node and select it.
2. In the inspector, create a **New TileSet** resource.
3. Click the TileSet value to expand it. Set the **Tile Shape** (Square, Isometric, Hexagonal, Half-Offset Square).
4. Set the **Tile Size** to match your tilesheet (e.g., 64x64). **Must be set before creating the atlas.**
5. Optionally enable **Rendering > UV Clipping** to prevent tiles drawing outside their allocated area.
6. Open the **TileSet** panel at the bottom of the editor.
7. Drag your tilesheet image into the panel. Answer **Yes** when asked to create tiles automatically.
8. Fully transparent parts of the tilesheet will not generate tiles.
9. Use the **Eraser** tool (or right-click > Delete) to remove unwanted tiles.

**Atlas Properties (adjustable in the middle column):**
- **ID:** Unique identifier within the TileSet (used for sorting)
- **Name:** Descriptive name (e.g., "terrain", "decoration")
- **Margins:** Edge margins on the image in pixels (useful for tilesheets with attribution borders)
- **Separation:** Pixel gap between tiles (for tilesheets with guide lines)
- **Texture Region Size:** Per-tile size on the atlas; usually matches the TileMapLayer tile size
- **Use Texture Padding:** Adds 1px transparent border per tile to prevent texture bleeding. Recommended to leave enabled.

**Gotcha:** Changing margin, separation, or region size may cause tiles to be lost. Regenerate via the three-dot menu > **Create Tiles in Non-Transparent Texture Regions**.

**Tip:** You can pan the TileSet panel with middle/right mouse buttons and zoom with the mouse wheel.

### Using Scene Tiles

You can place entire scenes as tiles — useful for interactive elements (shops), ambient sounds (AudioStreamPlayer2D), particle effects, etc.

1. Create a scene (e.g., with a CPUParticles2D root). Save it.
2. In the TileSet editor, create a **Scenes Collection** in the left column.
3. Name the collection, then create a new scene slot.
4. Use Quick Load / Load to assign your scene file.

**Warning:** Scene tiles have significantly higher performance overhead than atlas tiles because every placed tile is instanced individually. Only use when necessary.

### Merging Atlases

If you have multiple atlases and want to consolidate:
1. Open the three-dot menu at the bottom of the atlas list.
2. Choose **Open Atlas Merging Tool**.
3. Select multiple atlases (Shift/Ctrl+click), then click **Merge**.
4. Original tilesheet images are preserved on disk. Unmerged atlases are removed from the TileSet.
5. Use **Merge (Keep Original Atlases)** to preserve unmerged entries.

**Tip:** Tile proxies (a mapping table) are automatically set up during merging, allowing TileMaps to remap old tile IDs to new ones. You can also manually manage proxies via the three-dot menu > **Manage Tile Proxies**.

### Adding Collision, Navigation, and Occlusion

To define physics/navigation/occlusion per tile, you must first create the corresponding layers on the TileSet resource:

1. Select the TileMapLayer node, click the TileSet property to edit it.
2. Expand **Physics Layers** > **Add Element** (for collision).
3. Expand **Navigation Layers** > **Add Element** (for pathfinding).
4. Expand **Occlusion Layers** > **Add Element** (for light occlusion / shadow casting).

**Creating collision polygons:**
1. In the TileSet atlas inspector, select a tile and find the **Physics Layer** section.
2. Press **F** to auto-create a rectangle collision shape (make sure the polygon editor is focused).
3. Edit the polygon:
   - Create new points by clicking/dragging between existing points
   - Remove points by right-clicking
   - Pan with middle/right mouse buttons
   - Use the toolbar for additional operations (rotate, flip)
4. Remove a corner from the rectangle to make a triangle, or add points for complex shapes.

**Note:** Occlusion polygons are in the **Rendering** subsection — make sure to unfold it.

**Tip:** For large tilesets, use "assign properties to multiple tiles at once" to speed up collision setup.

### Custom Metadata (Custom Data Layers)

Assign per-tile custom data (damage values, destructibility flags, etc.):

1. In the TileSet inspector, expand **Custom Data Layers** > **Add Element**.
2. Define the layer name and type (e.g., `damage_per_second` as `int`, `destructible` as `bool`).
3. In the TileSet editor (Select mode), click a tile and edit its custom data in the middle column.
4. You can reorder custom data layers without breaking existing metadata.

All instances of a placed tile share the same custom data. For variants, create **alternative tiles**.

### Terrain Sets (Autotiling)

Terrains auto-select tile variants based on neighbors. They replace Godot 3.x autotiles with more power (tiles can define multiple terrains).

**Setup:**
1. In the TileSet inspector, create a **Terrain Set** and choose a mode:
   - **Match Corners and Sides** (equivalent to 3x3 bitmask)
   - **Match Corners** (equivalent to 2x2 bitmask)
   - **Match Sides** (equivalent to 3x3 minimal bitmask)
2. Create one or more **Terrains** within the terrain set.
3. In the TileSet editor (Select mode), click a tile:
   - Set **Terrain Set** to `0` (or higher).
   - Set **Terrain** to `0` (or higher). `-1` means "no terrain."
4. Configure **Terrain Peering Bits**: these determine which tile variant appears based on neighboring tiles.
   - All bits set to `0+` = tile appears only when all 8 neighbors use the same terrain.
   - Bits set to `-1` on top = tile appears only when there's empty space above.

**Note:** Terrain Set IDs and Terrain IDs are independent and start from `0`, not `1`.

### Assigning Properties to Multiple Tiles at Once

**Method 1: Multiple Tile Selection**
- In Select mode, Shift+click or drag-select multiple tiles.
- Edit properties in the middle column. Only changed properties apply to all selected tiles.

**Method 2: Tile Property Painting**
- Configure a property value in the middle column.
- Click or drag across tiles in the right column to "paint" the property.
- Especially useful for collision shapes.

### Creating Alternative Tiles

Alternative tiles are variants of a base tile with different configuration (flipped, rotated, recolored):

1. Right-click a base tile > **Create an Alternative Tile**.
2. The alternative appears to the right of the base tile in the editor.
3. Select it to edit properties.

**Alternative tile properties:**
- **Alternative ID:** Unique numeric ID. Changing it breaks existing TileMaps.
- **Rendering > Flip H / Flip V:** Horizontal/vertical flip.
- **Rendering > Transpose:** Rotates 90 degrees counter-clockwise then flips vertically.
  - 90 degrees CW = Flip H + Transpose
  - 180 degrees CW = Flip H + Flip V
  - 270 degrees CW = Flip V + Transpose
- **Rendering > Texture Origin:** Visual offset relative to the base tile.
- **Rendering > Modulate:** Color multiplier.
- **Rendering > Material:** Per-tile material (custom blend modes or shaders).
- **Z Index:** Sorting order (higher = in front).
- **Y Sort Origin:** Vertical offset for Y-sorting (only effective if Y Sort Enabled is true on the TileMapLayer).

**Since Godot 4.2:** You can rotate/flip any tile during placement in the TileMap editor toolbar without creating alternative tiles.

**Gotcha:** Alternative tiles do NOT inherit properties from their base tile. You must re-set all properties manually.

---

## 4. Using TileMaps

### Concepts

TileMaps let you "paint" tiles from a TileSet onto a grid to build levels. The `TileMapLayer` node is used for this.

### Specifying the TileSet

Assign a TileSet resource to the TileMapLayer's **TileSet** property in the inspector. For reuse across levels, save the TileSet as an external `.tres` resource file via the dropdown > **Save**.

### Multiple TileMapLayers

Use multiple TileMapLayer nodes to separate foreground, background, and decorative elements. You can place one tile per layer at a given location, allowing overlapping tiles across layers.

**Per-layer properties:**

**General:**
- **Enabled:** Visibility toggle
- **TileSet:** The tileset resource

**Rendering:**
- **Y Sort Origin:** Vertical offset for Y-sorting per tile (requires Y Sort Enabled on CanvasItem)
- **X Draw Order Reversed:** Reverses X-axis draw order (requires Y Sort Enabled)
- **Rendering Quadrant Size:** Tiles are grouped into quadrants for batch rendering. Controls the group size. Does not apply to Y-sorted layers.

**Physics:**
- **Collision Enabled:** Toggle collision
- **Use Kinematic Bodies:** When true, collision shapes are instanced as kinematic bodies
- **Collision Visibility Mode:** Show/hide collision debug shapes

**Navigation:**
- **Navigation Enabled:** Toggle navigation regions
- **Navigation Visible:** Show/hide navigation mesh debug

**Tip:** TileMap built-in navigation has practical limitations. For better pathfinding, bake the TileMap to an optimized navigation mesh using `NavigationRegion2D` or `NavigationServer2D`, then disable the TileMap NavigationLayer.

**Warning:** 2D navigation meshes cannot be stacked/layered. Attempting to stack them on the same navigation map causes merge errors and broken pathfinding.

**Note:** Layers can be reordered via drag-and-drop in the Scene tab. Renaming and reordering is safe, but *removing* a layer deletes all tiles on it.

### TileMap Editor

1. Select a TileMapLayer node.
2. Open the **TileMap** panel at the bottom of the editor.
3. Select tiles from the palette panel (click or hold-drag for multi-select).
4. Shift+click to append to selection.
5. Multi-tile selections place entire patterns per click.
6. Alternative tiles appear to the right of base tiles.
7. Scene tiles appear in their own "Scenes" collection tab.

**Tip:** Pan with middle/right mouse, zoom with mouse wheel.
**Tip:** Grayed-out layers indicate they aren't currently being edited. Toggle via the "Highlight Selected TileMap Layer" icon.

### Painting Modes and Tools

**Selection Mode:**
- Click or drag-rectangle to select placed tiles (empty space excluded)
- Shift to append, Ctrl to remove from selection
- Del to delete selected tiles
- Ctrl+C / Ctrl+V for copy/paste
- Toggle temporarily from Paint mode with Ctrl

**Paint Mode (default):**
- Left-click to place, right-click to erase
- Shift+drag = line drawing
- Ctrl+Shift+drag = rectangle drawing
- Ctrl+click = picker (grab existing tile)

**Line Mode:**
- Draws 1-tile-thick lines in any orientation
- Right-click erases in a line
- Multi-tile selections repeat along the line
- Toggle temporarily with Shift while in Paint/Eraser mode

**Rectangle Mode:**
- Draws axis-aligned rectangles
- Right-click erases in a rectangle
- Multi-tile selections repeat within the rectangle
- Toggle temporarily with Ctrl+Shift while in Paint/Eraser mode

**Bucket Fill:**
- **Contiguous** (default): Only fills matching adjacent tiles (horizontal/vertical, not diagonal)
- **Non-contiguous**: Replaces ALL matching tiles in the entire TileMap
- Right-click replaces matches with empty
- Multi-tile selections repeat within the filled area

**Picker:** Ctrl+click to grab an existing tile and switch to it for painting. Works for multi-tile rectangles too.

**Eraser:** Combines with any painting mode. Left-click replaces tiles with empty. Toggle temporarily by right-clicking in any mode.

### Scattering (Random Painting)

- Enable **Randomization** to randomly choose from selected tiles when painting.
- Works with Paint, Line, Rectangle, and Bucket Fill.
- **Scattering** value (0-1): probability that no tile is placed (for sparse detail like grass).
- Eraser mode ignores randomization/scattering — always removes all tiles in selection.

### Patterns

Save and reuse premade tile arrangements:
1. In Select mode, select tiles and press Ctrl+C.
2. Switch to the **Patterns** tab.
3. Click empty space, press Ctrl+V.
4. To use: click a pattern, switch to any paint mode, left-click in the 2D editor.
5. Patterns repeat with Line, Rectangle, and Bucket Fill tools.

**Note:** Patterns are stored in the TileSet resource (not the TileMap), so they can be reused across different TileMapLayer nodes using the same TileSet.

### Terrain Painting

Three terrain painting modes:
- **Connect:** Tiles connect to surrounding tiles on the same TileMapLayer automatically.
- **Path:** Tiles connect only to tiles painted in the same stroke (until mouse release). Allows adjacent but unconnected elements (e.g., parallel roads).
- **Tile-specific overrides:** Select specific tiles to resolve conflicts.

Any tile with at least one peering bit set to the terrain ID appears in the selection list.

### Handling Missing Tiles

If tiles are removed from the TileSet but referenced in a TileMap:
- Placeholders appear in the editor (not visible at runtime).
- Tile data persists on disk — safe to close/reopen scenes.
- Re-adding a tile with the matching ID restores the appearance.
- Placeholders may not show until you select the TileMapLayer and open the TileMap editor.

---

## 5. 2D Parallax

### Concepts

Parallax simulates depth by moving textures at different speeds relative to the camera. Godot provides the `Parallax2D` node for this.

**Recommended over** the older `ParallaxLayer` and `ParallaxBackground` nodes.

### Node Types

- **Parallax2D** — the primary node; children (Sprite2D, etc.) scroll at a rate defined by `ScrollScale`
- **Camera2D** — drives the parallax effect through camera movement

### Getting Started

1. Place each parallax layer's visuals as children of their own `Parallax2D` node.
2. Ensure textures' top-left corners align with the `(0, 0)` origin. This is critical for the infinite repeat effect.

### Scroll Scale

`ScrollScale` is a `Vector2` multiplier controlling layer scroll speed:
- `1.0` = scrolls at camera speed (no parallax)
- `< 1.0` = appears farther away (moves slower)
- `> 1.0` = appears closer (moves faster)
- `0.0` = completely stationary

**Example values for a 5-layer scene:**
| Layer | ScrollScale |
|---|---|
| Forest | (0.7, 1) |
| Hills | (0.5, 1) |
| Lower Clouds | (0.3, 1) |
| Higher Clouds | (0.2, 1) |
| Sky | (0.1, 1) |

### Infinite Repeat

`RepeatSize` tells the node to snap its position forward/back when the camera scrolls by the set amount. This creates the illusion of infinite scrolling.

**How it works:** A single repeat of all child canvas items is added, offset by `RepeatSize`. As the camera scrolls between the original and the repeat, the node invisibly snaps back.

### Common Problems

**Problem 1: Texture Too Small**

If the texture is smaller than the viewport, infinite repeat breaks. Solutions:
1. **Make viewport smaller:** In Project Settings > Display > Window, set Viewport Width/Height to match your background.
2. **Scale the Parallax2D node:** Set the `Scale` property. All children scale with it.
3. **Scale child nodes:** Scale `Sprite2D` nodes individually. Note: `RepeatSize` and `RegionRect` do NOT account for scaling — adjust manually.
4. **Repeat the texture:** On the Sprite2D:
   - Set `TextureRepeat` to `CanvasItem.TEXTURE_REPEAT_ENABLED`
   - Enable `RegionEnabled`
   - Set `RegionRect` to a multiple of the texture size large enough to cover the viewport

**Problem 2: Poor Positioning (Centered at Origin)**

The infinite repeat canvas starts at `(0, 0)` and extends down-right to the `RepeatSize` value. If textures are centered on `(0, 0)`, only part falls within the repeat canvas, causing partial repeating.

**Fix:** Position textures so they fit within the repeat canvas starting at `(0, 0)`. One loop of the image should be the same size or larger than the viewport.

**Gotcha:** Increasing `RepeatTimes` technically works but is a brute-force fix, not the correct solution.

### Scroll Offset

`ScrollOffset` offsets where the infinite repeat canvas starts. For example, with a 288x208 image, setting `ScrollOffset` to `(-144, 0)` or `(144, 0)` starts halfway across the image.

### Repeat Times

Handles zoom-out scenarios. When `Camera2D.Zoom` is set below `(1, 1)`, the viewport shows more area than the texture covers.

**Fix:** Set `RepeatTimes` to `3` (one extra repeat behind and in front). For vertical parallax, specify a Y value in `RepeatSize` and `RepeatTimes` adds repeats above/below automatically.

**Tip:** Stretch sprites (sky higher, grass lower) to support both normal zoom and zoomed-out views.

### Split Screen

For split-screen with shared parallax:
1. Clone parallax nodes into each `SubViewport`.
2. Leave all parallax nodes at default `VisibilityLayer` = 1.
3. Set SubViewport 1's `CanvasCullMask` to layers 1 and 2.
4. Set SubViewport 2's `CanvasCullMask` to layers 1 and 3.
5. Give parallax nodes in SubViewport 1 a common parent with `VisibilityLayer` = 2.
6. Give parallax nodes in SubViewport 2 a common parent with `VisibilityLayer` = 3.

**How it works:** A canvas item with a `VisibilityLayer` that doesn't match the SubViewport's `CanvasCullMask` hides all its children, even if children do match.

### Previewing in Editor

For editor preview, you can use a `CanvasLayer` with `FollowViewportEnabled` and manual scaling. Alternatively, use [KoBeWi's "Parallax2D Preview" addon](https://github.com/KoBeWi/Godot-Parallax2D-Preview).

---

## 6. 2D Lights and Shadows

### Concepts

By default, 2D scenes are unshaded (no lights/shadows). Godot provides real-time 2D lighting to enhance depth. The system involves multiple cooperating nodes.

### Node Types

| Node | Purpose |
|---|---|
| **CanvasModulate** | Darkens unlit areas; sets the "ambient" base color |
| **PointLight2D** | Omnidirectional or spot light (torches, fire, projectiles) |
| **DirectionalLight2D** | Parallel light rays (sun, moon) |
| **LightOccluder2D** | Defines shadow-casting geometry |
| **Sprite2D / TileMapLayer** | Receive lighting effects |

**Gotcha:** The background color does NOT receive lighting. You must add a Sprite2D (or similar) as a visual background to receive light. Use the Sprite2D's **Region** properties for repeating backgrounds, and set **Texture > Repeat** to **Enabled**.

### PointLight2D Properties

- **Texture:** The light texture. Size determines light size. May have alpha channel (useful with Mix blend mode).
- **Offset:** Offsets the light texture without affecting shadow positions.
- **Texture Scale:** Multiplier for light size. Larger lights cost more performance (more pixels affected).
- **Height:** Virtual height for normal mapping. Default is very close to surfaces, making normal mapping barely visible. Increase this value when using normal maps.

**Tip:** If you lack a pre-made light texture, assign a **New GradientTexture2D** to the Texture property. Set Fill mode to **Radial**, adjust the gradient from opaque white to transparent white, and center the starting location.

### DirectionalLight2D Properties

- **Height:** Virtual height for normal mapping (`0.0` = parallel, `1.0` = perpendicular). Increase when using normal maps. Does NOT affect shadow appearance.
- **Max Distance:** Maximum distance from camera center before shadows are culled (pixels). Decrease to improve performance. Camera2D zoom is NOT accounted for — shadows fade sooner at higher zoom.

**Gotcha:** Directional shadows always appear infinitely long regardless of Height. This is a limitation of Godot's 2D shadow method. For finite directional shadows, disable shadows on the DirectionalLight2D and use a custom shader reading from the 2D signed distance field (auto-generated from LightOccluder2D nodes).

### Common Light Properties (Light2D base class)

- **Enabled:** Toggle visibility (unlike hiding the node, children remain visible)
- **Editor Only:** Light only visible in editor, auto-disabled at runtime
- **Color:** Light color
- **Energy:** Intensity multiplier (higher = brighter)
- **Blend Mode:**
  - **Add** (default) — standard additive lighting
  - **Subtract** — negative light (not physically accurate, for special effects)
  - **Mix** — linear interpolation with pixels under the light texture
- **Range > Z Min / Z Max:** Lowest/highest Z index affected
- **Range > Layer Min / Layer Max:** Lowest/highest visual layer affected
- **Range > Item Cull Mask:** Controls which nodes receive light (based on nodes' Occluder Light Mask)

### Setting Up Shadows

1. Enable **Shadow > Enabled** on a PointLight2D or DirectionalLight2D.
2. No shadows appear yet — you need occluders.
3. Add `LightOccluder2D` nodes with polygon resources.

**LightOccluder2D properties:**
- **SDF Collision:** Contributes to a real-time signed distance field for custom shaders. Enabled by default (no visual or performance impact if not using SDF shaders).
- **Occluder Light Mask:** Works with the light's **Shadow > Item Cull Mask** to control which objects cast shadows for which lights.

**Creating occluders automatically:**
1. Select a Sprite2D node.
2. Click **Sprite2D** menu at top of 2D editor > **Create LightOccluder2D Sibling**.
3. Adjust **Grow (pixels)** and **Shrink (pixels)** in the dialog until the outline matches.
4. Click OK.

**Creating occluders manually:**
1. Add a `LightOccluder2D` node, select it.
2. Click the "+" button at the top of the 2D editor. Confirm creating a polygon resource.
3. Click to create points, right-click to remove. Click on existing line segments and drag to add intermediate points.

### Shadow Properties

- **Color:** Color of shaded areas (default fully black). Alpha controls tint intensity.
- **Filter:**
  - **None** (fastest) — blocky, suits pixel art
  - **PCF5** — soft shadows
  - **PCF13** — softer, most expensive. Use sparingly.
- **Filter Smooth:** Softening amount for PCF5/PCF13. Higher = softer but may cause banding (especially PCF5).
- **Item Cull Mask:** Controls which LightOccluder2D nodes cast shadows.

### Pixel-Art Lighting and Shadows

2D lighting is computed at Viewport pixel resolution, not at the sprite's texel resolution. **Nearest** texture filtering does NOT make lighting/shadows pixelated.

**Fix:** Use a custom shader to snap light sampling to a pixel grid:

```glsl
shader_type canvas_item;

uniform float pixel_size = 4.0;

void fragment() {
    // Snap lighting and shadows to pixel grid.
    LIGHT_VERTEX.xy = floor(LIGHT_VERTEX.xy / pixel_size) * pixel_size;
    SHADOW_VERTEX = floor(SHADOW_VERTEX / pixel_size) * pixel_size;

    // Normal rendering.
    COLOR = texture(TEXTURE, UV);
}
```

This divides positions by `pixel_size`, rounds down with `floor()`, then multiplies back to screen space, forcing discrete grid sampling.

### Normal and Specular Maps

Normal maps vary lighting intensity per-pixel based on surface direction. Specular maps control per-pixel reflectivity.

**Setup:**
1. On a Sprite2D, create a **New CanvasTexture** for the Texture property.
2. Expand the CanvasTexture resource and set:
   - **Diffuse > Texture:** Your base sprite texture
   - **Normal Map > Texture:** Your normal map
   - **Specular > Texture:** Your specular map (usually grayscale)
   - **Specular > Color:** Color multiplier for reflections
   - **Specular > Shininess:** Exponent for reflections (low = bright/diffuse, high = localized/wet-looking)
   - **Texture > Filter / Repeat:** Override texture filtering and repeat modes

**Tip:** Generate normal/specular maps from textures using the free tool [Laigter](https://azagaya.itch.io/laigter).

**After enabling normal maps:** Lights may appear weaker. Increase the **Height** and **Energy** properties on your light nodes.

### Additive Sprites as a Faster Alternative

For short-lived dynamic effects (bullets, explosions), `Sprite2D` nodes with additive blending are much faster than actual 2D lights.

**Setup:**
1. Create a Sprite2D, assign a texture.
2. In Inspector > CanvasItem > Material, add **New CanvasItemMaterial**.
3. In the material, set **Blend Mode** to **Add**.

**Advantages:** Much faster rendering (no separate pipeline). Works with AnimatedSprite2D or Sprite2D + AnimationPlayer for animated "lights."

**Disadvantages:**
- Inaccurate blending formula (cannot properly light fully dark areas)
- Cannot cast shadows
- Ignores normal and specular maps

---

## 7. Canvas Layers

### Concepts

**CanvasItem** is the base class for all 2D nodes (Node2D and Control). CanvasItem nodes are children of a Viewport, which displays them. Children inherit parent transforms.

The Viewport's `CanvasTransform` property enables efficient camera/scrolling by transforming the entire canvas (more performant than moving scene trees).

### The Problem

Some visual elements should NOT move with the camera:
- **Parallax backgrounds** — move at different rates
- **UI / HUD** — must stay fixed on screen
- **Transition effects** — need fixed screen positions

### CanvasLayer Node

`CanvasLayer` creates a separate 2D rendering layer for all its children and grandchildren.

**Key properties:**
- **Layer:** Integer controlling render order. Higher values render on top. Default viewport children are at layer `0`.
  - Parallax background example: layer `-1`
  - HUD / UI example: layer `1`
- **Offset:** Shift the layer's position independently
- **Transform:** Apply an independent transformation
- **Follow Viewport Enabled:** When enabled, the layer follows viewport transformations

Each CanvasLayer maintains its own transform independently from the viewport and from other CanvasLayers. This is how HUD elements stay fixed regardless of camera position.

### Alternative Approaches

CanvasLayers are not always necessary for controlling draw order:
- **Scene tree order:** Nodes lower in the tree draw on top
- **CanvasItem.ZIndex:** Property on individual nodes to control Z ordering

Use CanvasLayers when you need elements to be truly independent of the camera transform, not just for Z ordering.

---

## 8. 2D Particle Systems

### Concepts

Particle systems simulate complex physical effects: sparks, fire, smoke, magic, mist, explosions, etc.

**Core principle:** Particles are emitted at fixed intervals with fixed lifetimes. Each particle shares the same base behavior. Randomness per parameter creates organic variation.

**Formula:** `initial_value = param_value + param_value * randomness`

### Node Types

| Node | Description |
|---|---|
| **GPUParticles2D** | GPU-accelerated particles. More advanced, better for large counts. **Recommended.** |
| **CPUParticles2D** | CPU-driven. Near-feature parity but lower performance at scale. Better on low-end or GPU-bottlenecked systems. |
| **ParticleProcessMaterial** | Resource configuring GPUParticles2D behavior |
| **CanvasItemMaterial** | Required for flipbook animation on particles |

**Note:** No new features planned for CPUParticles2D (only parity PRs accepted). Use GPUParticles2D unless you have a specific reason not to.

**Converting between types:**
- Select the node > 2D workspace > toolbar > **CPUParticles2D > Convert to GPUParticles2D** (or reverse)
- Converting GPU to CPU may lose GPU-only features

### Basic Setup

1. Add a `GPUParticles2D` node. It starts as a white dot with a warning icon (missing material).
2. In Inspector, go to **Process Material** and create a **New ParticleProcessMaterial**.
3. White points should now emit downward.
4. Assign a **Texture** for the particle appearance.

### Texture and Flipbooks

A particle texture can be a single image or a **flipbook** (sprite sheet for particles). Flipbooks reproduce complex effects like smoke, fire, explosions, or introduce random texture variation.

**Flipbook setup:**
1. Assign your flipbook texture to the GPUParticles2D's **Texture** property.
2. In the node's **Material** section (CanvasItem > Material), create a **New CanvasItemMaterial**.
3. In the CanvasItemMaterial, enable **Particle Animation** and set **H Frames** / **V Frames** to match the flipbook's columns/rows.
4. The Animation section in ParticleProcessMaterial is now effective.

**Tip:** If the flipbook has a black background instead of transparent, set the CanvasItemMaterial's **Blend Mode** to **Add** instead of **Mix**. Alternatively, edit the texture to have a transparent background (GIMP: Color > Color to Alpha).

### Time Parameters

**Lifetime:** Seconds each particle lives. When one dies, a new one replaces it.
- `0.5` = fast cycling
- `4.0` = long-lived particles

**One Shot:** Emit all particles once, then stop forever.

**Preprocess:** Simulates N seconds before first draw. Fixes the "empty system on scene load" problem (e.g., torches, mist that should already be active).

**Speed Scale:** Global speed multiplier (default `1`). Lower = slower, higher = faster.

**Explosiveness:** Controls emission timing (0-1):
- `0` = emit at regular intervals (default)
- `1` = emit all particles simultaneously (burst)
- Values in between for partial bursts

**Fixed FPS:** Lock particle rendering to a specific frame rate (e.g., `2` = 2 FPS). Does not slow the system itself, just the visual update rate.

**Gotcha:** Godot 4.3+ does not yet support physics interpolation for 2D particles. Workaround: disable physics interpolation on the particle node via **Node > Physics Interpolation > Mode** in the inspector.

**Fract Delta:** When `true`, uses fractional delta calculation for smoother particle display. Improves accuracy for systems with high randomness or fast-moving particles. Prevents particles from appearing to jump within a single frame. Has a performance tradeoff with high particle counts.

### Drawing Parameters

**Visibility Rect:** Controls when particles are rendered. If this rectangle is outside the viewport, particles are culled.
- `W`/`H` = width/height
- `X`/`Y` = position of upper-left corner relative to the emitter

**Auto-generate:** Select the GPUParticles2D > toolbar > **Particles > Generate Visibility Rect**. Adjust **Generation Time (sec)** (max 25s). If particles need more time, temporarily increase the node's **Preprocess** value.

**Local Coords:**
- **Off (default):** Particles emit in global space. Moving the node does NOT move existing particles.
- **On:** Particles emit in local space. Moving the node moves all particles with it.

**Draw Order:**
- **Index** (default): Drawn in emission order
- **Lifetime**: Drawn in order of remaining lifetime

### ParticleProcessMaterial Settings

The full material configuration (gravity, velocity, acceleration, orbit, scale curves, color gradients, etc.) is documented separately in the ParticleProcessMaterial reference page. Key properties include:

- Direction, Spread, Flatness
- Initial Velocity (min/max)
- Angular Velocity
- Orbit Velocity
- Linear/Radial Acceleration
- Damping
- Angle (rotation)
- Scale (with curve support)
- Color (with gradient ramp support)
- Gravity
- Emission shape (point, sphere, box, ring, custom)

---

## Quick Reference: Node Hierarchy Patterns

### Basic 2D Character
```
CharacterBody2D
  ├── Sprite2D (or AnimatedSprite2D)
  ├── CollisionShape2D
  └── AnimationPlayer (optional)
```

### TileMap Scene
```
Node2D
  ├── TileMapLayer (background, layer -1)
  ├── TileMapLayer (terrain, layer 0)
  ├── TileMapLayer (foreground, layer 1)
  └── CharacterBody2D
```

### Parallax Scene
```
Node2D
  ├── Parallax2D (sky, ScrollScale 0.1)
  │   └── Sprite2D
  ├── Parallax2D (clouds, ScrollScale 0.3)
  │   └── Sprite2D
  ├── Parallax2D (hills, ScrollScale 0.5)
  │   └── Sprite2D
  ├── TileMapLayer (main level)
  ├── CharacterBody2D
  └── Camera2D
```

### 2D Lighting Scene
```
Node2D
  ├── CanvasModulate (ambient darkness)
  ├── Sprite2D (background, receives light)
  ├── Sprite2D (foreground objects)
  │   └── LightOccluder2D (shadow caster)
  ├── PointLight2D (torch)
  ├── DirectionalLight2D (sun)
  └── CharacterBody2D
```

### HUD with Canvas Layer
```
Node2D
  ├── Camera2D
  ├── [game content]
  └── CanvasLayer (layer 1)
      └── Control (UI root)
          ├── Label (score)
          └── TextureRect (health bar)
```

### Particle Effect
```
GPUParticles2D
  └── (ParticleProcessMaterial assigned via Process Material)
  └── (CanvasItemMaterial for flipbook, assigned via Material)
```
