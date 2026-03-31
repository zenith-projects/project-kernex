# 3D Advanced, Export Platforms & Asset Pipeline Reference

> Extracted from Godot 4.6 official documentation. All code examples in C#.

---

## Table of Contents

1. [Standard Material 3D and ORM Material 3D](#standard-material-3d-and-orm-material-3d)
2. [3D Lights and Shadows](#3d-lights-and-shadows)
3. [Using MultiMeshInstance3D](#using-multimeshinstance3d)
4. [Mesh Level of Detail (LOD)](#mesh-level-of-detail-lod)
5. [Occlusion Culling](#occlusion-culling)
6. [Prototyping Levels with CSG](#prototyping-levels-with-csg)
7. [Using GridMaps](#using-gridmaps)
8. [Creating a 3D Particle System](#creating-a-3d-particle-system)
9. [Volumetric Fog and Fog Volumes](#volumetric-fog-and-fog-volumes)
10. [Exporting for Windows](#exporting-for-windows)
11. [Exporting for Linux](#exporting-for-linux)
12. [Exporting for Android](#exporting-for-android)
13. [Exporting for the Web](#exporting-for-the-web)
14. [Exporting for Dedicated Servers](#exporting-for-dedicated-servers)
15. [Available 3D Formats](#available-3d-formats)
16. [Importing Images](#importing-images)
17. [Importing Audio Samples](#importing-audio-samples)
18. [Optimization Using Servers](#optimization-using-servers)

---

## Standard Material 3D and ORM Material 3D

### Introduction

`StandardMaterial3D` and `ORMMaterial3D` (Occlusion, Roughness, Metallic) are default 3D materials that aim to provide most of the features artists look for in a material, without the need for writing shader code. However, they can be converted to shader code if additional functionality is needed.

There are 4 ways to add these materials to an object:

1. **Material property of the mesh** -- every time that mesh is used it will have that material.
2. **Material property of the node** (e.g. MeshInstance3D) -- overrides the mesh's material, applies only to that node.
3. **Material Override property of the node** -- overrides both the node and mesh material properties.
4. **Material Overlay** -- renders a material over the current one (e.g. transparent shield effect).

### BaseMaterial 3D Settings

`StandardMaterial3D` settings are all under the `BaseMaterial3D` category. ORM materials are almost exactly the same but use a single ORM texture instead of separate settings for occlusion, roughness, and metallic. The red channel stores ambient occlusion, green stores roughness, and blue stores metallic. Programs such as Substance Painter and Armor Paint export in this format (use the Unreal Engine export preset).

### Transparency

By default, materials are opaque. To see through a material, it must be made transparent. Godot offers several transparency modes:

- **Disabled**: Material is opaque. Fastest to render, all rendering features supported.
- **Alpha**: Semi-transparent areas drawn with blending. Slow to render. Allows partial transparency (translucency). Cannot cast shadows, not visible in screen-space reflections. Good fit for particle effects and VFX.
- **Alpha Scissor**: Semi-transparent areas below `Alpha Scissor Threshold` are not drawn; above it they are drawn as opaque. Faster than Alpha, no transparency sorting issues. "All or nothing" transparency. Can cast shadows. Ideal for foliage and fences.
- **Alpha Hash**: Semi-transparent areas drawn using dithering. "All or nothing" transparency with limited precision depending on viewport resolution. Can cast shadows. Suited for realistic-looking hair.
- **Depth Pre-Pass**: Renders fully opaque pixels via the opaque pipeline first, then renders the rest with alpha blending. Mostly correct transparency sorting. Can cast shadows.

> **Note:** Godot automatically forces alpha blending if any of these conditions is met: setting transparency to Alpha, setting a blend mode other than Mix, enabling Refraction, Proximity Fade, or Distance Fade.

> **Warning:** Alpha-blended transparency limitations:
> - Significantly slower to render, especially if overlapping
> - May exhibit sorting issues when transparent surfaces overlap
> - Don't cast shadows (can receive them)
> - Don't appear in reflections (other than reflection probes)
> - Screen-space reflections and sharp SDFGI reflections don't appear on alpha-blended materials

### Alpha Antialiasing

Only visible when transparency mode is Alpha Scissor or Alpha Hash. Three modes:

- **Disabled**: No alpha antialiasing.
- **Alpha Edge Blend**: Smooth transition between opaque and transparent areas (alpha to coverage).
- **Alpha Edge Clip**: Sharp but antialiased transition (alpha to coverage + alpha to one).

The `Alpha Antialiasing Edge` property controls the threshold below which pixels are made transparent. Must always be strictly below the alpha scissor threshold. Default of 0.3 is sensible with alpha scissor threshold of 0.5.

> **Important:** For best results, MSAA 3D should be set to at least 2x in Project Settings. Without MSAA, a fixed dithering pattern is applied instead.

### Blend Mode

- **Mix**: Default, alpha controls visibility.
- **Add**: Final color added to screen color. Good for flares/fire effects.
- **Subtract**: Final color subtracted from screen.
- **Multiply**: Final color multiplied with screen.
- **Premultiplied Alpha**: Behaves like Add when alpha is 0.0, like Mix when alpha is 1.0.

Any mode other than Mix forces the object through the transparent pipeline.

### Cull Mode

- **Back** (default): Back of object culled when not visible.
- **Front**: Front of object culled.
- **Disabled**: Double-sided (no culling).

> **Note:** Blender defaults to backface culling disabled. Enable Backface Culling in Blender's Materials tab then re-export to glTF for better performance.

### Depth Draw Mode

- **Opaque Only** (default): Depth drawn only for opaque objects.
- **Always**: Depth drawn for opaque and transparent.
- **Never**: No depth draw.
- **Depth Pre-Pass**: For transparent objects, opaque pass first then transparency.

### No Depth Test

Disabling causes objects to appear over (or under) everything else. Works well with the Render Priority property.

### Depth Test

When set to **Inverted**, the object only appears when occluded. No effect if No Depth Test is enabled.

### Shading

#### Shading Mode

- **Per-Pixel**: Default, calculates lighting per pixel.
- **Per-Vertex**: Calculates lighting per vertex, interpolates. Better performance on low-end/mobile devices.
- **Unshaded**: No lighting calculation. Albedo color output directly. Fastest, useful for VFX, particles.

#### Diffuse Mode

- **Burley** (default): Disney Principled PBS diffuse algorithm.
- **Lambert**: Not affected by roughness.
- **Lambert Wrap**: Extends Lambert to cover more than 90 degrees when roughness increases. Great for hair, cheap subsurface scattering. Energy conserving.
- **Toon**: Hard cut for lighting, smoothing affected by roughness.

#### Specular Mode

- **SchlickGGX**: Most common PBR specular blob.
- **Toon**: Toon blob, changes size depending on roughness.
- **Disabled**: No specular blob.

#### Additional Shading Options

- **Disable Ambient Light**: Object receives no ambient lighting.
- **Disable Fog**: Object unaffected by depth-based or volumetric fog.
- **Disable Specular Occlusion**: Object reflections not reduced by occlusion.

### Vertex Color

- **Use as Albedo**: Vertex color used as albedo color.
- **Is sRGB**: Toggle on if 3D modeling software exports vertex colors as sRGB (most do).

### Albedo

Base color for the material. When Unshaded, it is the only visible color. Albedo color and texture are multiplied together. Alpha channel in albedo is used for transparency (must enable transparency or alpha scissoring).

### Metallic

Defines how reflective the material is. More reflective = less diffuse/ambient light and more reflected light. Energy-conserving model. The **Specular** parameter is a general reflectivity amount (leave at 0.5). Minimum internal reflectivity is 0.04 (impossible to make completely unreflective).

### Roughness

Value of 0 = perfect mirror. Value of 1 = completely blurred reflection. Most materials achieved with the right combination of Metallic and Roughness.

### Emission

How much light is emitted by the material. Added to the final image, not affected by other lighting. Does not affect surrounding geometry unless VoxelGI or SDFGI are used.

### Normal Map

Represents finer shape detail without modifying geometry. Only red and green channels used for better compression.

> **Note:** Godot requires X+, Y+, Z+ coordinates (OpenGL style). DirectX-style normal maps need Y axis flipped.

### Bent Normal Map

Only available in Forward+ and Mobile renderers. Describes the average direction of ambient lighting. Improves how material reacts to lighting:
- Indirect diffuse lighting matches global illumination more closely.
- Specular occlusion calculated using bent normals and ambient occlusion.

Requirements: cosine distribution of rays for baking, tangent space, OpenGL-style coordinates.

### Rim

Emulates micro-fur light scattering. Takes actual light into account (no light = no rim). Size depends on roughness. **Tint** 0 = light color, 1 = albedo color. Intermediate values work best.

### Clearcoat

Secondary pass of transparent coat. Common in car paint and toys. Smaller specular blob on top of existing material.

### Anisotropy

Changes specular blob shape, aligns to tangent space. Common for hair, brushed aluminum. Works well with flowmaps.

### Ambient Occlusion

Baked AO map affects how much ambient light reaches each surface (not direct light by default). Well-baked AO beats SSAO quality. Recommended to bake whenever possible.

### Height

Ray-marched search to emulate displacement of cavities. Creates illusion of depth, does not add real geometry. Should be used together with normal mapping. For physics collision height maps, use `HeightMapShape3D`.

### Subsurface Scattering

Only available in Forward+ renderer. Emulates light penetrating surface, scattering, and exiting. Useful for skin, marble, colored liquids.

### Back Lighting

Controls light transfer from lit side to dark side. Works well for thin objects: plant leaves, grass, human ears.

### Refraction

Distorts transparency similar to real-life refraction. Requires transparent albedo. Takes roughness into account (higher roughness = blurrier). Screen-space effect that forces transparency.

Limitations:
- Transparency sorting issues may occur
- Cannot refract onto itself or other transparent materials
- Off-screen objects cannot appear in refraction
- Opaque materials in front show "refracted" edges

### Detail

Secondary albedo and normal maps blended via detail textures.

- **Mask**: Black/white image controlling blend location. White = detail, Black = regular.
- **Blend Mode**: Mix, Add, Sub, Mul.
- **Albedo**: Detail albedo texture (white if empty).
- **Normal**: Detail normal texture (flat normal if empty).

### UV1 and UV2

Two UV channels per material. Secondary UV useful for AO or emission (baked light). UVs can be scaled and offset.

#### Triplanar Mapping

Textures sampled in X, Y, Z and blended by normal. Can be performed in world space or object space. **World Triplanar** makes textures continuous across objects sharing the same material.

### Sampling

- **Filter**: Texture filtering method.
- **Repeat**: Whether and how textures repeat.

### Shadows

- **Disable Receive Shadows**: Object receives no shadows.
- **Shadow to Opacity**: Lighting modifies alpha so shadowed areas are opaque, non-shadowed are transparent. Useful for AR overlays.

### Billboard

- **Disabled**: Billboard mode off.
- **Enabled**: Object's -Z axis always faces camera's viewing plane.
- **Y-Billboard**: Object's X axis always aligned with camera's viewing plane.
- **Particle Billboard**: Best for particle systems; allows flipbook animation.
- **Billboard Keep Scale**: Enables scaling in billboard mode.

### Grow

Grows vertices in normal direction. Commonly used for cheap outlines: add second material pass, make it black/unshaded, reverse culling (Cull Front), add grow.

> **Note:** Mesh must have connected faces with shared vertices ("smooth shading"). Disconnected faces ("flat shading") will show gaps. Since Godot 4.5, stencil buffer-based outlines are available as an alternative.

### Transform

- **Fixed Size**: Object renders at same size regardless of distance.
- **Use Point Size**: Resize point-geometry points (in pixels).
- **Use Particle Trails**: Enables GPUParticles3D trail shader requirements. Forward+ and Mobile only.
- **Use Z Clip Scale**: Scales toward camera to avoid wall clipping. For player arms, tools. Keep close to 1.0.
- **Use FOV Override**: Overrides Camera3D field of view angle.

### Proximity and Distance Fade

**Proximity Fade**: Soft blending based on proximity to other objects (soft particles, water-to-shore).
**Distance Fade**: Objects fade based on camera distance (light shafts, indicators).

Modes:
- **Pixel Alpha**: Actual transparency changes with distance. Forces transparent pipeline.
- **Pixel Dither**: Fraction of pixels rendered to approximate transparency.
- **Object Dither**: Same dithering but uniform across entire object surface.

> **Note:** To hide a character when too close to camera, prefer Object Dither (fastest) or Pixel Dither over Pixel Alpha.

### Stencil

Since Godot 4.5. Uses the stencil buffer for outlines and X-ray effects.

- **Outline**: Preconfigured stencil material for outlines in Next Pass.
- **X-Ray**: Preconfigured stencil material for X-ray in Next Pass.
- **Custom**: Advanced stencil effects.

Materials writing to stencil buffer are always drawn in the transparent pass.

### Material Settings

#### Render Priority

Controls rendering order. Opaque/transparent queue sorted first, then `render_priority` (higher drawn later). Transparent objects also sorted by depth. Depth testing overrules priority.

#### Next Pass

Setting `next_pass` causes an object to be rendered again with that next material. Depth tests equal between both passes unless grow or vertex transformations are used. Multiple transparent passes should use `render_priority` for correct ordering.

---

## 3D Lights and Shadows

### Introduction

Light sources in Godot:
- Material emission color (doesn't affect nearby objects unless baked or screen-space indirect lighting is enabled)
- Light nodes: `DirectionalLight3D`, `OmniLight3D`, `SpotLight3D`
- Ambient light in Environment or Reflection probes
- Global illumination: `LightmapGI`, `VoxelGI`, or SDFGI

### Light Node Common Properties

- **Color**: Base color for emitted light.
- **Energy**: Energy multiplier. Useful for saturating lights or HDR lighting.
- **Indirect Energy**: Secondary multiplier for indirect light (bounces). Works with LightmapGI, VoxelGI, SDFGI.
- **Volumetric Fog Energy**: Secondary multiplier for volumetric fog. Only effective when volumetric fog is enabled.
- **Negative**: Light becomes subtractive. Useful for compensating dark corners.
- **Specular**: Intensity of specular blob. At zero, becomes a pure diffuse light.
- **Bake Mode**: Sets bake mode for the light.
- **Cull Mask**: Objects in selected layers affected by light. Disabled objects still cast shadows -- adjust Cast Shadow on GeometryInstance3D to prevent this.

### Light Number Limits

**Forward+ renderer:**
- Default limit of 512 clustered elements (omni lights, spot lights, decals, reflection probes) in camera view.
- Adjustable via Project Settings > Rendering > Limits > Cluster Builder > Max Clustered Elements.

**Mobile renderer:**
- 8 OmniLights + 8 SpotLights per mesh resource.
- 256 OmniLights + 256 SpotLights in camera view.
- Cannot be changed.

**Compatibility renderer:**
- 8 OmniLights + 8 SpotLights per mesh resource.
- Adjustable in Rendering > Limits > OpenGL (Max Renderable Elements and Max Lights per Object), at cost of performance and longer shader compilation.

**All renderers:** Up to 8 DirectionalLights visible at once. Each additional DirectionalLight with shadows reduces effective shadow resolution (shared atlas).

> **Tip:** Enable Distance Fade on lights to reduce popping and improve performance. Splitting meshes into smaller portions also helps. Consider baked lightmaps with bake mode set to Static for excess lights.

### Shadow Mapping

Generic shadow parameters:
- **Enabled**: Enable shadow mapping.
- **Opacity**: Shadow opacity factor (default fully opaque, can be made translucent).
- **Bias**: Too low = self-shadowing. Too high = shadows separate from casters.
- **Normal Bias**: Too low = self-shadowing. Too high = misaligned shadows. Prefer increasing Normal Bias before Bias.
- **Transmittance Bias**: For materials with transmittance enabled.
- **Reverse Cull Face**: Some scenes work better with inverted face-culling for shadow mapping.
- **Blur**: Multiplies shadow blur radius. Works with traditional shadow mapping and PCSS. Higher values = softer, more temporally stable shadows but more noticeable grainy pattern.
- **Caster Mask**: Only objects in these layers cast shadows (does not affect which objects receive shadows).

#### Tweaking Shadow Bias

- **Shadow acne**: Shadows "smeared" onto objects when Bias or Normal Bias too low.
- **Peter-panning**: Shadows disconnected from objects when Bias or Normal Bias too high.
- Prefer increasing Normal Bias over Bias (less peter-panning, though shadows may appear thinner).
- Increasing shadow map resolution also fixes bias issues (at performance cost).

> **Note:** No "one size fits all" settings. May need different bias values per light.

> **Note (Compatibility renderer):** Enabling shadows may change light appearance due to multi-pass rendering in sRGB space instead of linear space. Adjust energy to compensate.

### Directional Light

Most common and cheapest light to compute. Models infinite parallel light rays covering the whole scene. Node position does not affect lighting, only direction matters.

**Angular Distance**: Light's angular size in degrees. Values above 0.0 produce softer shadows at greater distances (contact-hardening/PCSS). Sun's angular distance from Earth is approximately 0.5 degrees.

#### Directional Shadow Mapping

Uses Parallel Split Shadow Maps (PSSM), splitting the view frustum into 2 or 4 areas. Default is PSSM with 4 splits. Objects appearing in all 4 splits are rendered 5 times (once per split + once for final scene).

**Split parameters:**
- Each split distance controlled relative to camera far (or shadow Max Distance if > 0.0).
- Default values generally work well; tweaking first split common for close-object detail.
- Always set **Max Distance** according to scene needs. Lower = better shadows and performance.
- **Fade Start**: Controls shadow fade-out aggressiveness. Set to 1.0 if Max Distance fully covers scene.
- **Blend Splits**: Smoother transitions between splits at cost of detail and performance.
- **Pancake Size**: Fix missing shadows with large unsubdivided meshes.

### Omni Light

Point source emitting light spherically in all directions up to a given radius.

- **Range**: Distance the light reaches.
- **Attenuation**: How light fades with distance.
- **Size**: Larger values = slower fade-out, blurrier shadows at distance. Simulates area lights (PCSS).

#### Omni Shadow Mapping

Two algorithms:
- **Dual Paraboloid**: Renders quickly, can cause deformations. Good for irregular/subdivided objects.
- **Cube** (default): More correct, slower.

Shadows are cached in a shadow atlas.

**Projectors**: Omni lights can use projector textures (360-degree panorama mapping). Light appears darker with projector assigned; increase Energy to compensate.

> **Tip:** Convert cubemap projectors to panorama images using web-based conversion tools.

### Spot Light

Emits light only into a cone. Useful for flashlights, car lights, reflectors.

- **Angle**: Aperture angle of the cone.
- **Angle Attenuation**: Softens cone borders.
- Shares Range, Attenuation, Size with OmniLight3D.

#### Spot Shadow Mapping

Significantly faster than omni (only one shadow texture). Supports projector textures (standard 2D mapping, similar to decals).

> **Note:** Spot lights with angles wider than 89 degrees will have no shadows. Use omni light for wider angles.

### Shadow Atlas

Omni and spot lights are assigned to slots of a shadow atlas (configured in Project Settings > Rendering > Lights And Shadows > Positional Shadow).

The atlas is divided into 4 quadrants, each with configurable subdivision. Default allocation allows up to 88 lights with shadows (4 + 4 + 16 + 64).

Each frame:
1. Check if light is on a slot of right size; re-render and move if not.
2. Check if any object affecting the shadow map changed; re-render if so.
3. Otherwise, leave shadow untouched.

If all slots are full, some lights won't render shadows even if enabled.

### Balancing Performance and Quality

#### Shadow Map Size

Default 4096. Decrease to 2048 for low-end GPUs. If positional shadows become too blurry, adjust shadow atlas quadrants to contain fewer shadows.

#### Shadow Filter Mode

- **Soft Very Low**: Auto-decreases blur to hide artifacts from low sample count.
- **Soft Low** (default): Good balance for scenes with detailed textures.
- **Soft Medium/High/Ultra**: Higher quality, higher cost. Auto-increases blur.

Enable TAA, FSR2, or FXAA to hide dithering pattern at lower quality levels.

#### 16-bit vs 32-bit

Default 16-bit for performance. 32-bit reduces artifacts in large scenes but significant performance cost.

#### Light/Shadow Distance Fade

Properties on OmniLight3D and SpotLight3D:
- **Enabled**: Enable distance-based fade (form of LOD).
- **Begin**: Distance where light begins to fade.
- **Shadow**: Distance where shadow begins to fade. Can be closer than light fade.
- **Length**: Distance over which the fade occurs.

#### PCSS Recommendations

- Only use a handful of lights with PCSS at a time.
- Provide a setting for users to disable PCSS (set `light_angular_distance` to 0.0 for directional, `light_size` to 0.0 for positional).

#### Projector Filter Mode

Project Settings > Rendering > Textures > Light Projectors > Filter:
- **Nearest/Linear**: No mipmaps, faster, grainy at distance.
- **Nearest/Linear Mipmaps**: Smoother at distance, blurry at oblique angles.
- **Nearest/Linear Mipmaps Anisotropic**: Highest quality, most expensive.

Use Nearest variants for pixel art projects.

---

## Using MultiMeshInstance3D

### Introduction

In a normal scenario, you use a `MeshInstance3D` node to display a 3D mesh. When you need multiple instances of the same mesh, `MultiMeshInstance3D` creates multiple copies over a surface of a specific mesh (e.g. populating a landscape with trees of random scales and orientations).

### Setting Up the Nodes

Basic setup requires three nodes:
1. **MultiMeshInstance3D** node
2. **MeshInstance3D** as target surface (e.g. landscape)
3. **MeshInstance3D** as source mesh (e.g. tree)

Scene tree structure:
```
Node3D (root)
  +-- MultiMeshInstance3D
  +-- MeshInstance3D (target/landscape)
  +-- MeshInstance3D (source/tree)
```

Select the MultiMeshInstance3D node, click the **MultiMesh** button in the toolbar (next to View), then select **Populate surface**.

### MultiMesh Settings

- **Target Surface**: The mesh used as the target surface for placing copies.
- **Source Mesh**: The mesh to be duplicated on the target surface.
- **Mesh Up Axis**: The axis used as the up axis of the source mesh.
- **Random Rotation**: Randomizes rotation around the up axis.
- **Random Tilt**: Randomizes overall rotation of the source mesh.
- **Random Scale**: Randomizes the scale.
- **Scale**: Base scale of source mesh placed over target.
- **Amount**: Number of mesh instances placed over the target.

Select the target surface (landscape), set source mesh (tree), adjust parameters, press **Populate**. Delete the source mesh instance if satisfied. Repeat with different parameters to change results.

---

## Mesh Level of Detail (LOD)

### Introduction

LOD is one of the most important ways to optimize 3D rendering performance. Historically required manually authoring lower-detail meshes. Godot provides automatic mesh decimation on import using the **meshoptimizer** library.

Mesh LOD works with: `MeshInstance3D`, `MultiMeshInstance3D`, `GPUParticles3D`, `CPUParticles3D`.

### Generating Mesh LOD

By default, LOD generation happens automatically for imported 3D scenes (glTF, .blend, Collada, FBX). Does NOT auto-generate for OBJ files -- change Import As to Scene, then Reimport (requires editor restart).

> **Note:** LOD generation is not perfect. May introduce rendering issues in skinned meshes. Can be disabled per-mesh in Import dock for broken meshes and faster importing.

### Comparing Mesh LOD Visuals and Performance

Use **Disable Mesh LOD** in the 3D viewport's top-left menu (Perspective/Orthogonal) for comparison. Enable **View Frame Time** for FPS and **View Information** for primitive counts.

### Configuring Mesh LOD Performance and Quality

Project setting: `Rendering > Mesh LOD > LOD Change > Threshold Pixels`

To set at runtime:

```csharp
GetTree().Root.MeshLodThreshold = 4.0f;
```

Each viewport has its own `MeshLodThreshold` property. Default threshold of 1 pixel is perceptually lossless. Higher values = more aggressive LOD (better performance, lower quality).

Per-object adjustment: **LOD Bias** on any `GeometryInstance3D`:
- Above 1.0: Later transitions (higher quality, lower performance).
- Below 1.0: Earlier transitions (lower quality, higher performance).

`ReflectionProbe` nodes have their own **Mesh LOD Threshold** property.

> **Note:** LOD selection uses a screen-space metric. Automatically accounts for camera FOV and viewport resolution.

### Using Mesh LOD with MultiMesh and Particles

The point of the node's AABB closest to the camera is used for LOD selection. All instances in a MultiMesh/particle system drawn at the same LOD level.

For GPUParticles3D: ensure visibility AABB is configured (GPUParticles3D > Generate AABB).

For MultiMesh: instances far apart should be in separate MultiMeshInstance3D nodes (also improves frustum/occlusion culling).

---

## Occlusion Culling

### Why Use Occlusion Culling

Occlusion culling is hidden geometry removal. Most effective in indoor scenes with many smaller rooms. Combine with Mesh LOD and Visibility Ranges (HLOD) for best results.

> **Note:** Forward+ renderer already performs a depth prepass, reducing overdraw for opaque pixels. Greatest benefit on Mobile renderer (no depth prepass). Even with depth prepass, complex 3D scenes still benefit from occlusion culling.

### How Occlusion Culling Works in Godot

Occluder geometry is rasterized to a low-resolution buffer on the CPU using the **Embree** software raytracing library. The engine tests occludee AABBs against occluder shapes. The AABB must be **fully occluded** to be culled. Smaller objects are more likely to be effectively culled. Larger occluders (walls) are much more effective than smaller ones (decoration props).

### Setting Up Occlusion Culling

1. Enable `Rendering > Occlusion Culling > Use Occlusion Culling` in Project Settings (enable Advanced toggle).
2. Create occluders using one of two methods:

#### Automatically Baking Occluders (Recommended)

> **Note:** Only `MeshInstance3D` nodes are taken into account. MultiMeshInstance3D, GPUParticles3D, CPUParticles3D, and CSG nodes are NOT included (since Godot 4.4, CSG can be converted to MeshInstance3D before baking). Any `GeometryInstance3D` can be an occludee.

1. Add an `OccluderInstance3D` node to the scene.
2. Select it and click **Bake Occluders** at top of 3D viewport.
3. Purple wireframe lines show the occluder geometry.
4. To exclude dynamic objects: set **Bake > Cull Mask** on OccluderInstance3D, put dynamic objects on a different visual layer.

#### Manually Placing Occluders

Add `OccluderInstance3D` and choose an occluder type:
- `QuadOccluder3D` (single plane)
- `BoxOccluder3D` (cuboid)
- `SphereOccluder3D` (sphere)
- `PolygonOccluder3D` (2D polygon with arbitrary points)
- `ArrayOccluder3D` (for procedural generation from script)

### Previewing Occlusion Culling

In 3D viewport: Perspective > Display Advanced... > **Occlusion Culling Buffer**. Also enable **View Information** and **View Frame Time** for draw calls, primitives, and FPS.

Toggle at runtime:

```csharp
GetTree().Root.UseOcclusionCulling = true;
```

### Performance Considerations

- **Design levels for occlusion**: Add opaque walls to break line of sight. Use pyramid-like terrain elevation for open scenes.
- **Avoid moving OccluderInstance3D during gameplay** (requires BVH rebuild). Toggling visibility is less expensive. For sliding doors: keep OccluderInstance3D static, hide it when door opens, show when door closes.
- **Use simplest possible occluder shapes**: Increase Bake > Simplification if CPU is overloaded. Adjust `Rendering > Occlusion Culling > BVH Build Quality` and `Occlusion Rays Per Thread` if needed.

### Troubleshooting

**Occludee isn't being culled when it should be:**
- Check Bake > Cull Mask on OccluderInstance3D
- Only opaque materials are included in bake (transparent materials excluded even if texture is opaque)
- MultiMesh, GPUParticles3D, CPUParticles3D, CSG not included (add manual occluders)
- Ensure Extra Cull Margin is 0.0 on occludee and Ignore Occlusion Culling is disabled
- Check AABB size (must be fully occluded)

**Occludee is being culled when it shouldn't be:**
- Objects may have been moved after baking -- re-bake
- Dynamic objects may have been included in bake -- adjust cull mask
- Overly aggressive simplification -- decrease Bake > Simplification
- As last resort: enable Ignore Occlusion Culling on the occludee

---

## Prototyping Levels with CSG

### Introduction

CSG (Constructive Solid Geometry) combines basic shapes to create more complex shapes. Mainly intended for prototyping. No built-in UV mapping or 3D polygon editing (though CSGPolygon3D supports extruded 2D polygons).

> **Note:** For full level design tools, consider [FuncGodot](https://github.com/func-godot/func_godot_plugin) or [Cyclops Level Builder](https://github.com/blackears/cyclern).

### CSG Nodes

- `CSGBox3D`
- `CSGCylinder3D` (also supports cone)
- `CSGSphere3D`
- `CSGTorus3D`
- `CSGPolygon3D`
- `CSGMesh3D`
- `CSGCombiner3D`

### Boolean Operations

- **Union**: Geometry merged, intersecting geometry removed.
- **Intersection**: Only intersecting geometry remains.
- **Subtraction**: Second shape subtracted from first, leaving a dent.

### CSGPolygon3D

Extrudes a 2D polygon in three modes:
- **Depth**: Extruded back a given amount.
- **Spin**: Extruded while spinning around origin.
- **Path**: Extruded along a Path3D node (lofting).

### Custom Meshes (CSGMesh3D)

Mesh requirements:
- Must be closed (manifold)
- Each edge connects to exactly two faces
- Has volume

Avoid: negative volume, self-intersection, interior faces.

To make non-manifold meshes manifold in Blender: install 3D Print Toolbox addon, select mesh, go to 3D Print tab > Clean Up > Make Manifold.

### CSGCombiner3D

Empty shape for organization. Only combines children nodes.

### Processing Order

Each CSG node processes its children (union, intersection, subtraction) in tree order, applying them one after another.

> **Note:** Keep CSG geometry relatively simple. Create separate CSG trees for unrelated objects. Only use boolean operations where actually needed.

### Prototyping a Level

Working in **Orthogonal projection** gives better view when combining CSG shapes. Example: room with desk, bed, lamp, bookshelf using nested CSGCombiner3D nodes and CSG primitives.

Key concept: CSG nodes inside a CSGCombiner3D only process operations within that combiner, allowing organizational separation.

### Using Prototype Textures

StandardMaterial3D supports **triplanar mapping** for CSG nodes (no UV map editing). Apply to CSGCombiner3D as Material Override (affects all children) or individually.

Steps: New StandardMaterial3D > Albedo > load Texture > UV1 > enable Triplanar. Adjust Scale and Offset for tiling.

> **Tip:** Copy/paste StandardMaterial3D between CSG nodes via the dropdown arrow next to the material property.

### Converting to MeshInstance3D

Since Godot 4.4. Select CSG node > CSG > **Bake Mesh Instance**. Benefits:
- Bake lightmaps (UV2 generation)
- Bake occlusion culling
- Faster loading (no CSG rebuild)
- Better transform update performance

Also available: CSG > **Bake Collision Shape** (creates CollisionShape3D, must be child of StaticBody3D or AnimatableBody3D).

> **Tip:** Keep the original CSG node in the scene tree for future geometry changes.

### Exporting as glTF

Scene > Export As... > glTF 2.0 Scene. Useful for exporting to 3D modeling software.

---

## Using GridMaps

### Introduction

GridMaps are a 3D level design tool similar to TileMap in 2D. Use a predefined collection of 3D meshes (`MeshLibrary`) placed on a grid. Supports collisions and navigation meshes.

### Creating a MeshLibrary

Scene structure: `Node3D` root with `MeshInstance3D` children. Each child becomes a MeshLibrary item.

#### Collisions

Manually assign `StaticBody3D` and `CollisionShape3D` to each mesh, or use Mesh menu to auto-create collision bodies. Use "Convex" for simple meshes, "Create Trimesh Static Body" for complex shapes.

#### Materials

Only materials from within the meshes are used. Materials set on the node are ignored.

#### NavigationMeshes

Place `NavigationRegion3D` as child of the main `MeshInstance3D`. Add valid NavigationMesh resource and source geometry nodes below, then bake.

> **Note:** Small grid cells may need reduced agent radius and region minimum size.
> **Warning:** Baked cell size of NavigationMesh must match NavigationServer map cell size.

#### Lightmaps

Lightmap UV2 data is reused if present, otherwise auto-generated with texel size of 0.1 units. To use different texel size: set global illumination mode to Static Lightmaps in Import dock before converting to MeshLibrary.

### MeshLibrary Format

Each child of the root should:
- Be a `MeshInstance3D` (visual mesh exported)
- Have materials in the mesh's material slot (not MeshInstance3D's)
- Have up to one `StaticBody3D` child with `CollisionShape3D` children
- Have up to one `NavigationRegion3D` child

Only this specific format is recognized.

### Exporting the MeshLibrary

Scene > Export As... > MeshLibrary... > save as resource.

### Using GridMap

1. Create new scene with `GridMap` node.
2. Drag MeshLibrary resource to **Mesh Library** property.
3. Set **Cells > Size** to match mesh sizes. Uncheck **Center Y** as needed.
4. Configure collision layer/mask/priority and Navigation > Bake Navigation.

### GridMap Panel Tools

- **Transform**: Gizmo for position/rotation.
- **Selection**: Click-drag to select grid areas.
- **Erase**: Click to delete meshes.
- **Paint**: Click to place selected mesh.
- **Pick**: Click existing mesh to select it in panel.
- **Fill**: Fill selected area with chosen mesh.
- **Move**: Move selected meshes.
- **Duplicate**: Copy selected meshes.
- **Delete**: Delete entire selected area.
- **Cursor Rotate X/Y/Z**: Rotate mesh being painted on respective axes.
- **Change Grid Floor**: Adjust working floor level.
- **Filter Meshes**: Search for specific mesh.
- **Settings**: Adjust Pick Distance (max placement distance from camera).

---

## Creating a 3D Particle System

### Required Properties

To start: add `GPUParticles3D` node. Two required parameters: **Process Material** and at least one **Draw Pass**.

### The Process Material

In Inspector: Process Material > New `ParticleProcessMaterial`. This is a special material used to update particle data and behavior on the GPU (massive performance boost over CPU).

### Draw Passes

At least one draw pass required. In Inspector: Draw Passes > Pass 1 > New `QuadMesh`. Set mesh Size to 0.1 for both x and y (easier to distinguish individual particles).

- Up to **4 draw passes** per particle system.
- Each pass can render a different mesh with its own material.
- All passes use data computed by the process material (compute once, feed to multiple render passes).

### Particle Conversion

**GPU to CPU particles**: Viewport menu entry. Not all GPU features available for CPU particles.

Features lost during conversion:
- Multiple draw passes
- Turbulence
- Sub-emitters
- Trails
- Attractors
- Collision

Properties lost:
- Amount Ratio
- Interp to End
- Damping as Friction
- Emission Shape Offset/Scale
- Inherit Velocity Ratio
- Velocity Pivot
- Directional Velocity
- Radial Velocity
- Velocity Limit
- Scale Over Velocity

**CPU to GPU particles**: Also available from viewport menu.

Converting GPU to CPU may be necessary for older devices that don't support modern graphics APIs.

---

## Volumetric Fog and Fog Volumes

> **Note:** Volumetric fog is only supported in the Forward+ renderer.

Volumetric fog can interact with lighting, unlike traditional (non-volumetric) fog. Both can be used simultaneously.

### Volumetric Fog Properties

Enable in WorldEnvironment node's Environment resource:

- **Density**: Base exponential density. Set to lowest desired global density. FogVolumes add/subtract from this. 0.0 disables global volumetric fog while allowing FogVolumes.
- **Albedo**: Color when interacting with lights. Mist/fog = white, smoke = darker.
- **Emission**: Emitted light. Does not cast light on surfaces. Useful for ambient color to soften shadows (single-scattering only).
- **Emission Energy**: Brightness of emitted light.
- **GI Inject**: Scales Global Illumination strength in fog albedo. Small performance cost when > 0.0.
- **Anisotropy**: Direction of scattered light. Near 1.0 = forward scattering. Near 0.0 = equal scattering. Near -1.0 = backward scattering.
- **Length**: Distance over which fog is computed. Lower = more detail, higher = greater range.
- **Detail Spread**: Distribution of froxel size. Higher = more detail closer to camera.
- **Ambient Inject**: Strength of ambient light in fog. Small performance cost when > 0.0.
- **Sky Affect**: How much volumetric fog drawn onto background sky. 0.0 = no effect on sky.

**Temporal Reprojection:**
- **Enabled**: Blends current frame with last frame for smoother fog. Causes "ghosting" with fast-moving FogVolumes/lights. Set Volumetric Fog Energy to 0.0 on short-lived dynamic lighting to avoid ghosting.
- **Amount**: Higher = smoother but more ghosting. Lower = less ghosting but visible jitter.

> **Note:** Volumetric fog has finite range. For large worlds, enable both non-volumetric and volumetric fog and adjust density accordingly.

### Light Interaction

All light types interact with volumetric fog. Adjust per-light with **Volumetric Fog Energy**. Shadows on lights also visible on volumetric fog.

To disable fog-light interaction: set fog Albedo to pure black, or per-light set Volumetric Fog Energy to 0 (also improves performance).

### Using as Volumetric Lighting

Set fog density to minimum (0.0001), increase Volumetric Fog Energy on lights to 200.0-5000.0 to compensate. Not physically accurate but creates volumetric lighting effect.

### Performance Settings

- **Rendering > Environment > Volumetric Fog > Volume Size**: Froxel buffer size. Larger = more detail, lower performance.
- **Volume Depth**: Number of depth slices. Lower = more efficient but possible artifacts during camera movement.
- **Use Filter**: Blurs fog substantially. Reduces fine details, smooths edges/aliasing.

> **Note:** Volumetric fog can cause banding at higher densities. See Color Banding docs for mitigation.

### Fog Volumes (Local Volumetric Fog)

For constrained fog areas or excluding areas from global fog:

1. Ensure Volumetric Fog enabled in Environment (set Density to 0.0 if global fog undesired).
2. Create `FogVolume` node.
3. Assign new `FogMaterial` to Material property.
4. Set Density positive to increase density, negative to subtract from global fog.
5. Configure extents and shape.

> **Note:** Thin fog volumes may flicker. Increase Volume Depth, decrease Length, or make volume thicker with lower density.

#### FogVolume Properties

- **Extents**: Size when Shape is Ellipsoid, Cone, Cylinder, or Box. Non-uniform scaling of cone/cylinder not supported via Extents (scale the FogVolume node instead).
- **Shape**: Ellipsoid, Cone, Cylinder, Box, or World (global).
- **Material**: FogMaterial or custom ShaderMaterial (Fog shaders).

#### FogMaterial Properties

- **Density**: Denser = more opaque. Negative values subtract fog.
- **Albedo**: Single-scattering color, additively blended with other FogVolumes and global fog.
- **Emission**: Color of emitted light. Won't cast light/shadows, useful for independent color modulation.
- **Height Falloff**: How density decreases with height. High = sharp transition, low = smooth. 0.0 = uniform density.
- **Edge Fade**: Hardness of edges. Higher = softer, lower = harder.
- **Density Texture**: 3D texture for varying density within volume. For animated effects, use custom fog shader.

### 3D Noise Density Textures

Since Godot 4.1: `NoiseTexture3D` resource for procedural 3D noise.

Steps: Density Texture > New NoiseTexture3D > click to edit > Noise > New FastNoiseLite. Set width/height/depth matching volume dimensions.

Use low texture sizes (64x64x64 or lower) for performance. Higher detail requires increasing Volume Size project setting.

> **Note:** NoiseTexture3D's Color Ramp only affects the red channel for density textures. Won't tint the fog -- use a custom shader for that.

### Faking Volumetric Fog Using Quads

Alternative to volumetric fog using QuadMeshes:

Advantages:
- Works with any rendering method (Mobile, Compatibility)
- No temporal reprojection needed (suited for fast-moving effects like lasers)
- Generally lower performance cost

Disadvantages:
- Less realistic falloff (especially if camera enters fog)
- Transparency sorting issues possible
- Performance not necessarily better with many sprites near camera

Setup:
1. MeshInstance3D with QuadMesh, set size as desired.
2. New StandardMaterial3D in mesh Material:
   - Shading > Shading Mode: Unshaded
   - Billboard > Mode: Enabled
   - Enable Proximity Fade
   - Distance Fade: Pixel Alpha
3. Set Albedo Texture to a radial gradient.
4. Change texture compression to Lossless in Import dock.
5. Adjust Albedo Color (density via alpha), Proximity Fade Distance, Distance Fade Max Distance.

---

## Exporting for Windows

### Overview

The export system creates a `data.pck` file bundled with an optimized binary (smaller, faster, no editor/debugger).

### Architecture

- **x86_64** (default): Most common. All modern Intel and AMD processors.
- **x86_32**: 32-bit executable. NOT recommended unless targeting old 32-bit Windows (no longer supported by Microsoft).
- **arm64**: Windows on ARM (e.g. Snapdragon X Elite). Native execution without Prism emulator. Does NOT run on x86_64 processors. Recommended to provide ARM version on platforms supporting multiple executables.

### Changing the Executable Icon

Godot auto-converts project icon to ICO. For manual ICO control, see Manually changing application icon for Windows page.

### Code Signing

Requires Windows SDK (on Windows) or osslsigncode (other OS) and a package signing certificate.

> **Warning:** Exporting with embedded PCK files breaks code signing. On Windows, embedded PCK also causes false positives in antivirus programs. Avoid unless distributing via Steam.

#### Setup

1. Editor Settings > Export > Windows > Sign Tool: select SignTool.exe (Windows) or osslsigncode (other).
2. Windows export preset > Options > Code Signing: set Enabled to true, set Identity to signing certificate.

### Environment Variables

| Export Option | Environment Variable |
|---|---|
| Encryption / Encryption Key | `GODOT_SCRIPT_ENCRYPTION_KEY` |
| Options / Codesign / Identity Type | `GODOT_WINDOWS_CODESIGN_IDENTITY_TYPE` |
| Options / Codesign / Identity | `GODOT_WINDOWS_CODESIGN_IDENTITY` |
| Options / Codesign / Password | `GODOT_WINDOWS_CODESIGN_PASSWORD` |

Full export options: `EditorExportPlatformWindows` class reference.

---

## Exporting for Linux

### Overview

Same approach as Windows: `data.pck` bundled with optimized binary.

### Architecture

- **x86_64** (default): Most common.
- **x86_32**: NOT recommended unless targeting old 32-bit distributions. Some distros (e.g. Fedora) are considering removing 32-bit libraries.
- **arm64**: 64-bit ARM (Raspberry Pi 3+). Recommended to provide if targeting ARM hardware.
- **arm32**: Older 32-bit ARM (Raspberry Pi 1-2). Not recommended unless specifically needed.
- **rv64**: RISC-V processors. Niche. No official export templates; must compile your own.
- **ppc64**: 64-bit PowerPC. Niche. No official export templates.
- **loongarch64**: 64-bit LoongArch. Niche. No official export templates.

### Environment Variables

| Export Option | Environment Variable |
|---|---|
| Encryption / Encryption Key | `GODOT_SCRIPT_ENCRYPTION_KEY` |

Full export options: `EditorExportPlatformLinuxBSD` class reference.

---

## Exporting for Android

> **Attention:** C# projects can be exported to Android as of Godot 4.2, but support is experimental with some limitations.

### Install OpenJDK 17

Download and install OpenJDK 17. Higher versions also supported, but JDK 17 recommended for optimal compatibility.

### Download the Android SDK

Install via Android Studio Iguana (2023.2.1) or later, or via `sdkmanager` CLI.

Required packages:
- Android SDK Platform-Tools version 35.0.0+
- Android SDK Build-Tools version 35.0.1
- Android SDK Platform 35
- Android SDK Command-line Tools (latest)
- CMake version 3.10.2.4988404
- NDK version r28b (28.1.13356709)

CLI installation:
```
sdkmanager --sdk_root=<android_sdk_path> "platform-tools" "build-tools;35.0.1" "platforms;android-35" "cmdline-tools;latest" "cmake;3.10.2.4988404" "ndk;28.1.13356709"
```

> **Note (Linux):** Do not use Android SDK from distribution repositories (often outdated).

### Setting Up in Godot

Editor Settings (Editor tab / Godot tab on macOS):
- **Java SDK Path**: Location where OpenJDK 17 was installed.
- **Android SDK Path**: Location where Android SDK was installed (should contain `platform-tools/adb`).

> **Note:** "Could not install to device" error = application with same package name but different signing key already installed. Remove existing app first.

### Launcher Icons

Three types:
- **Main Icon**: Classic icon, used up to Android 8 (exclusive). At least 192x192 px.
- **Adaptive Icons**: Since Android 8. Separate background and foreground icons. At least 432x432 px. Critical elements must be in safe zone (centered circle, 66dp / 264px diameter on xxxhdpi).
- **Themed Icons** (optional): Since Android 13. Monochrome icon. At least 432x432 px.

Fallback chains:
- Main: Provided main > Project icon > Default Godot icon
- Adaptive Foreground: Provided foreground > Main > Project icon > Default Godot foreground
- Adaptive Background: Provided background > Default Godot background

### Exporting for Google Play Store

All new apps must be AAB (Android App Bundle) since August 2021.

Generate keystore:
```
keytool -v -genkey -keystore mygame.keystore -alias mygame -keyalg RSA -validity 10000
```

Use only upper/lowercase letters and numbers for passwords. Special characters may cause errors.

Export preset settings:
- **Release**: Path to keystore file.
- **Release User**: Key alias.
- **Release Password**: Key password (must match keystore password).
- Uncheck **Export With Debug**.

### Optimizing File Size

For APKs: uncheck either Armeabi-v7a or Arm64-v8a in export preset to create single-architecture APK. ARMv7 runs on ARMv8 (not vice versa). Not needed for AABs (Google auto-splits).

### Environment Variables

| Export Option | Environment Variable |
|---|---|
| Encryption / Encryption Key | `GODOT_SCRIPT_ENCRYPTION_KEY` |
| Options / Keystore / Debug | `GODOT_ANDROID_KEYSTORE_DEBUG_PATH` |
| Options / Keystore / Debug User | `GODOT_ANDROID_KEYSTORE_DEBUG_USER` |
| Options / Keystore / Debug Password | `GODOT_ANDROID_KEYSTORE_DEBUG_PASSWORD` |
| Options / Keystore / Release | `GODOT_ANDROID_KEYSTORE_RELEASE_PATH` |
| Options / Keystore / Release User | `GODOT_ANDROID_KEYSTORE_RELEASE_USER` |
| Options / Keystore / Release Password | `GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD` |

Full export options: `EditorExportPlatformAndroid` class reference.

---

## Exporting for the Web

### Overview

HTML5 export publishes games to the browser. Requires WebAssembly and WebGL 2.0 support.

> **Attention:** C# projects using Godot 4 currently cannot be exported to web. Use Godot 3 for C# on web platforms.

> **Tip:** Use browser developer console (F12 or Ctrl+Shift+I / Cmd+Option+I) for debug information.

### Thread Support and SharedArrayBuffer

Since Godot 4.3, single-threaded export is the default and preferred method. Avoids cross-origin isolation requirements. More compatible with itch.io, Poki, CrazyGames. Works well on macOS/iOS.

Multi-threaded export requires specific server-side headers and complete cross-origin isolation (no ads, no third-party integrations).

### Export File Name

Recommend exporting as `index.html` (default file loaded by web servers).

> **Attention:** Godot 4 Web export expects files to keep their original names. Renaming exported files can cause issues.

### WebGL Version

Godot 4 targets WebGL 2.0 only (Compatibility renderer). Forward+/Mobile not supported on web. No WebGPU support currently. Safari has several WebGL 2.0 issues -- recommend Chromium-based or Firefox.

### Mobile Considerations

Web export runs on mobile with caveats. Native Android/iOS exports always perform better. Use Feature tags for low-end settings on web exports. Compile optimized export template with unused features disabled for smaller WebAssembly payload.

### Audio Playback

Since Godot 4.3, uses Web Audio API (Sample playback mode) by default for low latency even without threads.

Limitations of Sample mode:
- AudioEffects not supported
- Reverberation and doppler not supported
- Procedural audio not supported
- Positional audio may not always work correctly

Change to Godot's own audio system via `Audio > General > Default Playback Type.web` or per-node `Playback Type` set to Stream. Increases latency but enables full audio features.

### Export Options

- **Extension Support**: Enable for GDExtension (requires extensions compiled for web; needs cross-origin isolation headers).
- **VRAM Texture Compression**: Enable for targeted platforms (both Desktop and Mobile for maximum compatibility at cost of larger export).
- **Custom HTML Shell**: Override default HTML page.
- **Head Include**: Appended to `<head>` element (load webfonts, third-party JS, CSS).
- **Canvas Resize Policy**: Default matches browser window. Set to None for fixed size with custom JS control. Set to Project for native-like behavior.

> **Important:** Each project must generate its own HTML file. Placeholders are replaced on export. Direct HTML modifications are lost on re-export. Use Custom HTML shell for customization.

### Thread and Extension Support

Thread Support enables multithreading and low-latency Stream audio. Requires cross-origin isolation headers.

Extensions Support enables GDExtension loading. Also requires cross-origin isolation headers.

### Progressive Web App (PWA)

When enabled:
- Configures icons, display mode, screen orientation.
- Allows offline loading via service worker.
- Ensures cross-origin isolation headers always present (even without server configuration).
- Offline Page: HTML page displayed when offline data evicted from cache.

### Limitations

- **Secure Context**: Many features only available over HTTPS (localhost usually exempt).
- **Cookies**: Users must allow cookies (IndexedDB) for `user://` persistence. Check with `OS.IsUserFsPersistent()`.
- **Background Processing**: Project paused when tab not active. Does not apply to unfocused windows.
- **Full Screen / Mouse Capture**: Must occur as response to JavaScript input event (within `_Input` or `_UnhandledInput`).
- **Audio**: Some browsers restrict autoplay. Request user interaction (click/tap/keypress) first.
- **Networking**: Only HTTP client, HTTP requests, WebSocket (client), and WebRTC supported. No low-level networking. HTTP class restrictions: no StreamPeer, no threaded/blocking mode, no chunked responses, subject to same-origin policy.
- **Clipboard**: Requires Clipboard API and secure context. Asynchronous nature may be unreliable from scripts.
- **Gamepads**: Not detected until button pressed. May have wrong mapping. Requires secure context.

### Serving the Files

Generated files:
- `.html`: Main page. Can be renamed to `index.html`. Can be used in `<iframe>`.
- `.wasm`: WebAssembly module (serve as `application/wasm`).
- `.pck`: Godot main pack (serve as `application/octet-stream`).
- `.js`: Start-up code.
- `.png`: Boot splash image.

**Cross-origin isolation headers** (required only when using threads):
```
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
```

If unable to set headers, enable PWA for service worker-based workaround. Secure context still required.

Server-side compression recommended (especially `.pck` and `.wasm`). Brotli precompression for further savings.

Hosts with on-the-fly compression: GitHub Pages (gzip).
Hosts without: itch.io, GitLab Pages (supports manual gzip precompression).

### Troubleshooting

**Running locally shows another project**: Service worker caching issue. Unregister current service worker in DevTools > Application tab > Update on reload or Unregister.

### Environment Variables

| Export Option | Environment Variable |
|---|---|
| Encryption / Encryption Key | `GODOT_SCRIPT_ENCRYPTION_KEY` |

Full export options: `EditorExportPlatformWeb` class reference.

---

## Exporting for Dedicated Servers

### Overview

For machines without GPU or display server, run Godot with `--headless` command line argument or use a dedicated server export.

### Editor vs Export Template

- **Export template**: Use for running dedicated servers. Smaller, more optimized, no editor functionality.
- **Editor binary**: Contains editor functionality, for exporting projects. Can run servers but not recommended.

### Export Approaches

1. Create separate export preset for server platform, export as usual.
2. Export PCK file only, place in same folder as export template binary, rename binary to match PCK name (minus extension), run binary.

### Exporting with Resource Stripping

Default export includes all resources (textures, etc.) making PCK as large as client. Godot offers dedicated server export mode.

Setup: Create dedicated export preset > Resources tab > change to **Export as dedicated server** mode. This auto-adds the `dedicated_server` feature tag and forces `--headless`.

#### Resource Options

- **Strip Visuals**: Export with visual files replaced by placeholder classes (stores image size only). **Recommended default.**
- **Keep**: Export resource as usual. Use when server needs image data (e.g. collision from image pixels).
- **Remove**: File not included in PCK. Ensure server doesn't reference client-only resources.

> **Warning:** Be careful with Remove -- scenes referencing removed files won't load. Remove references in scene files and load via `ResourceLoader.Load()` in scripts for fine control.

> **Tip:** Check exported PCK structure using Export PCK/ZIP... with .zip extension.

### Starting the Dedicated Server

Detect dedicated server mode:

```csharp
// Note: Feature tags are case-sensitive.
if (OS.HasFeature("dedicated_server"))
{
    // Run your server startup code here...
}
```

Detect headless mode:

```csharp
if (DisplayServer.GetName() == "headless")
{
    // Run your server startup code here...
    // Using this check, you can start a dedicated server by running
    // a Godot binary (editor or export template) with the --headless
    // command-line argument.
}
```

Custom command-line argument:

```csharp
using System.Linq;

if (OS.GetCmdlineUserArgs().Contains("--server"))
{
    // Run your server startup code here...
    // Using this check, you can start a dedicated server by running
    // a Godot binary (editor or export template) with the --server
    // command-line argument.
}
```

### Next Steps

- On Linux, create a **systemd service** for auto-restart after crash/reboot and log rotation. Enable `application/run/flush_stdout_on_print` project setting for journald logging.
- Consider wrapping in a **Docker container** for automatic scaling setups.

---

## Available 3D Formats

### Supported Formats

Godot works with scenes. The entire scene from 3D modeling software transfers as close as possible.

| Format | Extension | Notes |
|---|---|---|
| **glTF 2.0** (recommended) | `.gltf` (text), `.glb` (binary) | Full support |
| **.blend** (Blender) | `.blend` | Calls Blender to export to glTF transparently. Requires Blender installed. |
| **DAE** (COLLADA) | `.dae` | Older format, supported |
| **OBJ** (Wavefront) | `.obj` + `.mtl` | Limited: no pivots, skeletons, animations, UV2, PBR materials |
| **FBX** | `.fbx` | Supported via ufbx library (default since 4.3). FBX2glTF also available. |

### Exporting glTF 2.0 from Blender (Recommended)

Three export options:
1. **glTF binary (.glb)**: Smaller, includes mesh and textures.
2. **glTF text with separate data (.gltf + .bin + textures)**: Good for version control (text-based diffs) and separate texture files.

Import process: glTF data loaded into in-memory `GLTFState` > generates Godot scene.

> **Warning:** If model contains blend shapes ("shape keys"/"morph targets"), set Blender's Data > Armature > Export Deformation Bones Only to Enabled. Exporting non-deforming bones leads to incorrect shading.

> **Note:** Blender versions older than 3.2 don't export emissive textures with glTF.

> **Note:** Blender defaults to backface culling disabled. Materials export with Cull Mode Disabled. Enable Backface Culling in Blender's Materials tab for better performance.

### Importing .blend Files Directly

Requires Blender 3.0+ (recommend 3.5+ for best results). Use official Blender from blender.org.

Configure Blender path: Editor Settings > Filesystem > Import > Blender > Blender Path.

To disable .blend import: Filesystem > Import > Blender > Enabled in advanced Project Settings.

> **Note:** All team members need Blender installed. Not available on Android/web editors.

### Exporting DAE from Blender

Built-in COLLADA support works for simple scenes. For complex scenes/animations, use glTF instead.

### Importing OBJ Files

Two import modes:
1. Load directly in MeshInstance3D (default).
2. Change to "OBJ as Scene" in Import dock (allows same options as glTF/Collada, including UV2 unwrapping for lightmaps).

> **Note:** Blender 3.4+ exports RGB vertex colors in OBJ. Godot imports them but requires Vertex Color > Use As Albedo enabled. Brightness clamped to 1.0.

### Importing FBX Files

Default since Godot 4.3: **ufbx** import method. Files added in previous versions (4.2) continue using FBX2glTF unless manually changed.

To disable FBX import: Filesystem > Import > FBX > Enabled in advanced Project Settings.

For FBX2glTF workflow (generally not recommended): download executable, set path in Editor Settings > Filesystem > Import > FBX > FBX2glTFPath.

---

## Importing Images

### Supported Image Formats

Godot supports multiple image formats for textures and graphics assets.

### Import Options

#### Changing Import Type

Modify how Godot processes imported images by adjusting the import type (2D texture, 3D texture, other specialized formats).

#### Detect 3D

Automatically identifies whether an image should use 3D-specific settings.

### Compress Options

- **Mode**: Compression algorithm. Balances file size vs quality and runtime performance.
- **High Quality**: Better visual results, larger file size, longer processing.
- **HDR Compression**: Optimized for high dynamic range imagery.
- **Normal Map**: Adjusts compression to preserve directional information.
- **Channel Pack**: Combines multiple grayscale textures into one image using different color channels.

### Mipmaps

- **Generate**: Creates progressively smaller versions. Enhances performance when viewing textures from distance.
- **Limit**: Restricts maximum mipmap levels to control memory consumption.

### Roughness

- **Mode**: Configure roughness data processing.
- **Src Normal**: Source normal map for deriving roughness information.

### Process Options

- **Fix Alpha Border**: Prevents artifacts at transparent edges.
- **Premult Alpha**: Combines transparency with color values for optimized blending.
- **Normal Map Invert Y**: Inverts Y-channel for normal maps using different coordinate conventions.
- **HDR as sRGB**: Treat HDR data as sRGB color space.
- **HDR Clamp Exposure**: Limit maximum exposure values.
- **Size Limit**: Auto-downscale textures exceeding specified dimensions.

### Detect 3D > Compress To

Target compression format when 3D detection is active.

### SVG Options

- **Scale**: Controls scaling factor when importing SVG. Affects resolution of rasterized output.

### Editor Options

- **Scale With Editor Scale**: Preview textures adjust to interface scaling.
- **Convert Colors With Editor Theme**: Auto-recolor editor-displayed textures to match theme.

### Importing SVG with Text

SVG text requires proper Scale setting to maintain readability (rasterized to bitmap).

### Best Practices

#### High-Resolution 2D Textures Without Artifacts

Use mipmapping and appropriate filtering modes. Consider multiple texture resolutions selected dynamically per platform.

#### Appropriate Texture Sizes in 3D

Balance resolution against platform memory constraints. Excessive resolution wastes memory without proportional improvements, especially for surfaces viewed from distance.

---

## Importing Audio Samples

### Supported Formats

- **WAV** (.wav): Uncompressed or lossless audio.
- **Ogg Vorbis** (.ogg): Compressed audio.
- **MP3** (.mp3): Compressed audio.

### Import Process

Place audio files in project directory. Godot auto-detects and imports. Adjust settings in FileSystem dock > Import dock > Reimport.

### WAV Import Options

- **Force > 8 Bit**: Converts to 8-bit depth. Reduces size, decreases quality. Use for retro style.
- **Force > Mono**: Converts stereo to mono. Halves file size. Suitable for non-directional sound effects.
- **Force > Max Rate**: Limits sample rate (8 kHz, 11.025 kHz, 22.05 kHz, 44.1 kHz).
- **Edit > Trim**: Removes silent portions from beginning and end.
- **Edit > Normalize**: Adjusts levels to use full dynamic range without clipping.
- **Edit > Loop Mode**:
  - Disabled: No looping
  - Forward: Start to end loop
  - Ping-Pong: Forward then backward
  - Backward: Reverse playback
- **Compress > Mode**:
  - Disabled: Original quality
  - Lossy: Reduced size with minor quality loss
  - VORBIS: Ogg Vorbis compression

### Ogg Vorbis and MP3 Import Options

- **Loop**: Enable/disable audio looping.
- **Loop Offset**: Sample position where loop restarts.
- **BPM**: Beats per minute for musical synchronization.
- **Beat Count**: Number of beats in segment.
- **Bar Beats**: Beats per musical measure/bar.

### Best Practices

#### Appropriate Quality Settings

Console/desktop tolerate higher quality; mobile benefits from aggressive optimization. Music typically requires higher bitrates than sound effects.

#### Use Real-Time Audio Effects to Reduce File Size

Apply reverb, echo, pitch shifting during playback instead of encoding into source files. Reduces storage, preserves quality, maintains flexibility. Use Godot's audio processing pipeline.

---

## Optimization Using Servers

### Overview

The scene system adds ease of use but comes with tradeoffs:
- Extra layer of complexity
- Lower performance than direct APIs
- Cannot use multiple threads to control them
- More memory needed

For tens of thousands of instances processed every frame, low-level server APIs may be needed.

### Servers

The scene system is optional and can be completely bypassed. Core servers:

- **RenderingServer**: Handles everything related to graphics.
- **PhysicsServer3D**: Handles everything related to 3D physics.
- **PhysicsServer2D**: Handles everything related to 2D physics.
- **AudioServer**: Handles everything related to audio.

### RIDs (Resource IDs)

Opaque handles to server implementations. Allocated and freed manually. Almost every server function requires RIDs.

Most nodes and resources contain RIDs internally. Anything inheriting `Resource` can be directly cast to an RID.

> **Warning:** Resources are reference-counted, but references to a resource's RID are NOT counted. Keep a reference to the resource outside the server, otherwise both resource and RID will be erased.

Key RID access methods:
- `CanvasItem.GetCanvasItem()`: Canvas item RID.
- `CanvasLayer.GetCanvas()`: Canvas RID.
- `Viewport.GetViewportRid()`: Viewport RID.
- `World2D`: Contains functions for RenderingServer Canvas and PhysicsServer2D Space.
- `World3D`: Contains functions for RenderingServer Scenario and PhysicsServer3D Space.
- `VisualInstance3D.GetInstance()` and `VisualInstance3D.GetBase()`: Instance and base RIDs.

> **Important:** Do not control RIDs from objects that already have a node associated. Use server functions for creating/controlling new ones.

### Creating a Sprite (2D Example)

```csharp
public partial class MyNode2D : Node2D
{
    // RenderingServer expects references to be kept around.
    private Texture2D _texture;

    public override void _Ready()
    {
        // Create a canvas item, child of this node.
        Rid ciRid = RenderingServer.CanvasItemCreate();
        // Make this node the parent.
        RenderingServer.CanvasItemSetParent(ciRid, GetCanvasItem());
        // Draw a texture on it.
        // Remember to keep this reference.
        _texture = ResourceLoader.Load<Texture2D>("res://my_texture.png");
        // Add it, centered.
        RenderingServer.CanvasItemAddTextureRect(ciRid,
            new Rect2(-_texture.GetSize() / 2, _texture.GetSize()),
            _texture.GetRid());
        // Add the item, rotated 45 degrees and translated.
        Transform2D xform = Transform2D.Identity
            .Rotated(Mathf.DegToRad(45))
            .Translated(new Vector2(20, 30));
        RenderingServer.CanvasItemSetTransform(ciRid, xform);
        // Reset physics interpolation for this item.
        RenderingServer.CanvasItemResetPhysicsInterpolation(ciRid);
    }
}
```

> **Note:** When creating canvas items using RenderingServer, reset physics interpolation on the first frame with `RenderingServer.CanvasItemResetPhysicsInterpolation()` to ensure proper synchronization and prevent teleporting on scene load.

Canvas Item API: Primitives can be added but not modified once added. Must clear and re-add:

```csharp
RenderingServer.CanvasItemClear(ciRid);
```

Transform can be set as many times as desired without clearing.

### Instantiating a Mesh into 3D Space

```csharp
public partial class MyNode3D : Node3D
{
    // RenderingServer expects references to be kept around.
    private Mesh _mesh;

    public override void _Ready()
    {
        // Create a visual instance (for 3D).
        Rid instance = RenderingServer.InstanceCreate();
        // Set the scenario from the world.
        Rid scenario = GetWorld3D().Scenario;
        RenderingServer.InstanceSetScenario(instance, scenario);
        // Add a mesh to it. Remember to keep this reference.
        _mesh = ResourceLoader.Load<Mesh>("res://my_mesh.obj");
        RenderingServer.InstanceSetBase(instance, _mesh.GetRid());
        // Move the mesh around.
        Transform3D xform = new Transform3D(Basis.Identity, new Vector3(2, 3, 0));
        RenderingServer.InstanceSetTransform(instance, xform);
    }
}
```

### Creating a 2D RigidBody and Moving a Sprite

```csharp
public partial class MyNode2D : Node2D
{
    private Rid _canvasItem;

    private void BodyMoved(PhysicsDirectBodyState2D state, int index)
    {
        RenderingServer.CanvasItemSetTransform(_canvasItem, state.Transform);
    }

    public override void _Ready()
    {
        // Create the body.
        var body = PhysicsServer2D.BodyCreate();
        PhysicsServer2D.BodySetMode(body, PhysicsServer2D.BodyMode.Rigid);
        // Add a shape.
        var shape = PhysicsServer2D.RectangleShapeCreate();
        // Set rectangle extents.
        PhysicsServer2D.ShapeSetData(shape, new Vector2(10, 10));
        // Make sure to keep the shape reference!
        PhysicsServer2D.BodyAddShape(body, shape);
        // Set space, so it collides in the same space as current scene.
        PhysicsServer2D.BodySetSpace(body, GetWorld2D().Space);
        // Move initial position.
        PhysicsServer2D.BodySetState(body,
            PhysicsServer2D.BodyState.Transform,
            new Transform2D(0, new Vector2(10, 20)));
        // Add the transform callback, when body moves.
        // The last parameter is optional, can be used as index
        // if you have many bodies and a single callback.
        PhysicsServer2D.BodySetForceIntegrationCallback(body,
            new Callable(this, MethodName.BodyMoved), 0);
        // Also create a sprite using RenderingServer here.
        // See the section above on creating a sprite.
    }
}
```

The 3D version is very similar, using `RigidBody3D` and `PhysicsServer3D` respectively.

### Getting Data from the Servers

> **Warning:** Never request information from RenderingServer, PhysicsServer2D, or PhysicsServer3D by calling functions unless you know what you're doing. These servers often run asynchronously for performance. Calling any function that returns a value will stall them and force them to process all pending work. This will severely decrease performance if called every frame.

Most server APIs are designed so it's not possible to request information back, until it's actual data that can be saved.

---

*Source: Godot Engine 4.6 Documentation. License: CC BY 3.0. Copyright 2014-present Juan Linietsky, Ariel Manzur and the Godot community.*
