# Shaders — Advanced Reference

> Extracted from Godot 4.6 official documentation. All GDScript converted to C#. Shader code (GLSL) remains as-is.

---

## Table of Contents

1. [Spatial Shaders](#1-spatial-shaders)
2. [CanvasItem Shaders](#2-canvasitem-shaders)
3. [Particle Shaders](#3-particle-shaders)
4. [Shader Preprocessor](#4-shader-preprocessor)
5. [Your Second 3D Shader](#5-your-second-3d-shader)
6. [Visual Shaders](#6-visual-shaders)
7. [Compute Shaders](#7-compute-shaders)
8. [Advanced Post-Processing](#8-advanced-post-processing)
9. [Using a SubViewport as a Texture](#9-using-a-subviewport-as-a-texture)
10. [Converting GLSL to Godot Shaders](#10-converting-glsl-to-godot-shaders)

---

## 1. Spatial Shaders

Spatial shaders are used for shading 3D objects. They are the most complex type of shader Godot offers. Spatial shaders are highly configurable with different render modes and different rendering options (e.g. Subsurface Scattering, Transmission, Ambient Occlusion, Rim lighting, etc.). Users can optionally write vertex, fragment, and light processor functions to affect how objects are drawn.

### 1.1 Render Modes

For visual examples of these render modes, see `StandardMaterial3D` and `ORMMaterial3D`.

| Render Mode | Description |
|---|---|
| `blend_mix` | Mix blend mode (alpha is transparency), default. |
| `blend_add` | Additive blend mode. |
| `blend_sub` | Subtractive blend mode. |
| `blend_mul` | Multiplicative blend mode. |
| `blend_premul_alpha` | Premultiplied alpha blend mode (fully transparent = add, fully opaque = mix). |
| `depth_draw_opaque` | Only draw depth for opaque geometry (not transparent). |
| `depth_draw_always` | Always draw depth (opaque and transparent). |
| `depth_draw_never` | Never draw depth. |
| `depth_prepass_alpha` | Do opaque depth pre-pass for transparent geometry. |
| `depth_test_disabled` | Disable depth testing. |
| `depth_test_default` | Depth test will discard the pixel if it is behind other pixels. In Forward+ only, the pixel is also discarded if it's at the exact same depth as another pixel. |
| `depth_test_inverted` | Depth test will discard the pixel if it is in front of other pixels. Useful for stencil effects. |
| `sss_mode_skin` | Subsurface Scattering mode for skin (optimizes visuals for human skin, e.g. boosted red channel). |
| `cull_back` | Cull back-faces (default). |
| `cull_front` | Cull front-faces. |
| `cull_disabled` | Culling disabled (double sided). |
| `unshaded` | Result is just albedo. No lighting/shading happens in material, making it faster to render. |
| `wireframe` | Geometry draws using lines (useful for troubleshooting). |
| `debug_shadow_splits` | Directional shadows are drawn using different colors for each split (useful for troubleshooting). |
| `diffuse_burley` | Burley (Disney PBS) for diffuse (default). |
| `diffuse_lambert` | Lambert shading for diffuse. |
| `diffuse_lambert_wrap` | Lambert-wrap shading (roughness-dependent) for diffuse. |
| `diffuse_toon` | Toon shading for diffuse. |
| `specular_schlick_ggx` | Schlick-GGX for direct light specular lobes (default). |
| `specular_toon` | Toon for direct light specular lobes. |
| `specular_disabled` | Disable direct light specular lobes. Does not affect reflected light (use `SPECULAR = 0.0` instead). |
| `skip_vertex_transform` | `VERTEX`, `NORMAL`, `TANGENT`, and `BITANGENT` need to be transformed manually in the `vertex()` function. |
| `world_vertex_coords` | `VERTEX`, `NORMAL`, `TANGENT`, and `BITANGENT` are modified in world space instead of model space. |
| `ensure_correct_normals` | Use when non-uniform scale is applied to mesh (note: currently unimplemented). |
| `shadows_disabled` | Disable computing shadows in shader. The shader will not receive shadows, but can still cast them. |
| `ambient_light_disabled` | Disable contribution from ambient light and radiance map. |
| `shadow_to_opacity` | Lighting modifies the alpha so shadowed areas are opaque and non-shadowed areas are transparent. Useful for overlaying shadows onto a camera feed in AR. |
| `vertex_lighting` | Use vertex-based lighting instead of per-pixel lighting. |
| `particle_trails` | Enables the trails when used on particle geometry. |
| `alpha_to_coverage` | Alpha antialiasing mode. |
| `alpha_to_coverage_and_one` | Alpha antialiasing mode. |
| `fog_disabled` | Disable receiving depth-based or volumetric fog. Useful for `blend_add` materials like particles. |

### 1.2 Stencil Modes

> **Note:** Stencil support is experimental. We will try to not break compatibility as much as possible, but if significant flaws are found in the API, it may change in the next minor version.

Stencil operations are a set of operations that allow writing to an efficient buffer in a hardware-accelerated manner. This is generally used to mask in or out parts of the scene.

Common uses:
- **Outlines:** Mask out the inner mesh that is being outlined to avoid inner outlines.
- **X-Ray:** Display a mesh behind other objects.
- **Portals:** Draw geometry that is normally "impossible" (non-Euclidean) by masking objects.

> **Note:** You can only read from the stencil buffer in the transparent pass. Any attempt to read in the opaque pass will fail, as it's currently not supported behavior. Note that for compositor effects, the main renderer's stencil buffer cannot be copied to a custom texture.

| Stencil Mode | Description |
|---|---|
| `read` | Read from the stencil buffer. |
| `write` | Write reference value to the stencil buffer. |
| `write_if_depth_fail` | Write reference value to the stencil buffer if the depth test fails. |
| `compare_always` | Always pass stencil test. |
| `compare_equal` | Pass stencil test if the reference value is equal to the stencil buffer value. |
| `compare_not_equal` | Pass stencil test if the reference value is not equal to the stencil buffer value. |
| `compare_less` | Pass stencil test if the reference value is less than the stencil buffer value. |
| `compare_less_or_equal` | Pass stencil test if the reference value is less than or equal to the stencil buffer value. |
| `compare_greater` | Pass stencil test if the reference value is greater than the stencil buffer value. |
| `compare_greater_or_equal` | Pass stencil test if the reference value is greater than or equal to the stencil buffer value. |

### 1.3 Built-ins

Values marked as `in` are read-only. Values marked as `out` can optionally be written to and will not necessarily contain sensible values. Values marked as `inout` provide a sensible default value, and can optionally be written to. Samplers cannot be written to so they are not marked.

Not all built-ins are available in all processing functions. To access a vertex built-in from the `fragment()` function, you can use a varying. The same applies for accessing fragment built-ins from the `light()` function.

### 1.4 Global Built-ins

Global built-ins are available everywhere, including custom functions.

| Built-in | Description |
|---|---|
| `in float TIME` | Global time since the engine has started, in seconds. It repeats after every 3,600 seconds (which can be changed with the rollover setting). It's affected by `time_scale` but not by pausing. If you need a `TIME` variable that is not affected by time scale, add your own global shader uniform and update it each frame. |
| `in float PI` | A PI constant (3.141592). The ratio of a circle's circumference to its diameter and the number of radians in a half turn. |
| `in float TAU` | A TAU constant (6.283185). Equivalent to `PI * 2` and the number of radians in a full turn. |
| `in float E` | An E constant (2.718281). Euler's number, the base of the natural logarithm. |
| `in bool OUTPUT_IS_SRGB` | `true` when output is in sRGB color space (this is `true` in the Compatibility renderer, `false` in Forward+ and Mobile). |
| `in float CLIP_SPACE_FAR` | Clip space far z value. In the Forward+ or Mobile renderers, it's `0.0`. In the Compatibility renderer, it's `-1.0`. |

### 1.5 Vertex Built-ins

Vertex data (`VERTEX`, `NORMAL`, `TANGENT`, and `BITANGENT`) are presented in model space (also called local space). If not written to, these values will not be modified and be passed through as they came, then transformed into view space to be used in `fragment()`.

They can optionally be presented in world space by using the `world_vertex_coords` render mode.

Users can disable the built-in modelview transform (projection will still happen later) and do it manually with the following code:

```glsl
shader_type spatial;
render_mode skip_vertex_transform;

void vertex() {
    VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    BINORMAL = normalize((MODELVIEW_MATRIX * vec4(BINORMAL, 0.0)).xyz);
    TANGENT = normalize((MODELVIEW_MATRIX * vec4(TANGENT, 0.0)).xyz);
}
```

Other built-ins, such as `UV`, `UV2`, and `COLOR`, are also passed through to the `fragment()` function if not modified.

Users can override the modelview and projection transforms using the `POSITION` built-in. If `POSITION` is written to anywhere in the shader, it will always be used, so the user becomes responsible for ensuring that it always has an acceptable value. When `POSITION` is used, the value from `VERTEX` is ignored and projection does not happen. However, the value passed to the fragment shader still comes from `VERTEX`.

For instancing, the `INSTANCE_CUSTOM` variable contains the instance custom data. When using particles, this information is usually:
- **x:** Rotation angle in radians.
- **y:** Phase during lifetime (0.0 to 1.0).
- **z:** Animation frame.

| Built-in | Description |
|---|---|
| `in vec2 VIEWPORT_SIZE` | Size of viewport (in pixels). |
| `in mat4 VIEW_MATRIX` | World space to view space transform. |
| `in mat4 INV_VIEW_MATRIX` | View space to world space transform. |
| `in mat4 MAIN_CAM_INV_VIEW_MATRIX` | View space to world space transform of the camera used to draw the current viewport. |
| `in mat4 INV_PROJECTION_MATRIX` | Clip space to view space transform. |
| `in vec3 NODE_POSITION_WORLD` | Node position, in world space. |
| `in vec3 NODE_POSITION_VIEW` | Node position, in view space. |
| `in vec3 CAMERA_POSITION_WORLD` | Camera position, in world space. Represents the midpoint of the two eyes when in multiview/stereo rendering. |
| `in vec3 CAMERA_DIRECTION_WORLD` | Camera direction, in world space. |
| `in uint CAMERA_VISIBLE_LAYERS` | Cull layers of the camera rendering the current pass. |
| `in int INSTANCE_ID` | Instance ID for instancing. |
| `in vec4 INSTANCE_CUSTOM` | Instance custom data (for particles, mostly). |
| `in int VIEW_INDEX` | The view that we are rendering. `VIEW_MONO_LEFT` (0) for Mono (not multiview) or left eye, `VIEW_RIGHT` (1) for right eye. |
| `in int VIEW_MONO_LEFT` | Constant for Mono or left eye, always 0. |
| `in int VIEW_RIGHT` | Constant for right eye, always 1. |
| `in vec3 EYE_OFFSET` | Position offset for the eye being rendered, in view space. Only applicable for multiview rendering. |
| `inout vec3 VERTEX` | Position of the vertex, in model space. In world space if `world_vertex_coords` is used. |
| `in int VERTEX_ID` | The index of the current vertex in the vertex buffer. |
| `inout vec3 NORMAL` | Normal in model space. In world space if `world_vertex_coords` is used. |
| `inout vec3 TANGENT` | Tangent in model space. In world space if `world_vertex_coords` is used. |
| `inout vec3 BINORMAL` | Binormal in model space. In world space if `world_vertex_coords` is used. |
| `out vec4 POSITION` | If written to, overrides final vertex position in clip space. |
| `inout vec2 UV` | UV main channel. |
| `inout vec2 UV2` | UV secondary channel. |
| `inout vec4 COLOR` | Color from vertices. Limited to values between 0.0 and 1.0 for each channel and 8 bits per channel precision (256 possible levels). Alpha channel is supported. Values outside the allowed range are clamped, and values may be rounded due to precision limitations. Use `CUSTOM0`-`CUSTOM3` to pass data with more precision if needed. |
| `out float ROUGHNESS` | Roughness for vertex lighting. |
| `inout float POINT_SIZE` | Point size for point rendering. |
| `inout mat4 MODELVIEW_MATRIX` | Model/local space to view space transform (use if possible). |
| `inout mat3 MODELVIEW_NORMAL_MATRIX` | Normal transform for modelview. |
| `in mat4 MODEL_MATRIX` | Model/local space to world space transform. |
| `in mat3 MODEL_NORMAL_MATRIX` | Normal transform for model. |
| `inout mat4 PROJECTION_MATRIX` | View space to clip space transform. |
| `in uvec4 BONE_INDICES` | Bone indices for skeletal animation. |
| `in vec4 BONE_WEIGHTS` | Bone weights for skeletal animation. |
| `in vec4 CUSTOM0` | Custom value from vertex primitive. When using extra UVs, `xy` is UV3 and `zw` is UV4. |
| `in vec4 CUSTOM1` | Custom value from vertex primitive. When using extra UVs, `xy` is UV5 and `zw` is UV6. |
| `in vec4 CUSTOM2` | Custom value from vertex primitive. When using extra UVs, `xy` is UV7 and `zw` is UV8. |
| `in vec4 CUSTOM3` | Custom value from vertex primitive. |
| `out float Z_CLIP_SCALE` | If written to, scales the vertex towards the camera to avoid clipping into things like walls. Lighting and shadows will continue to work correctly when this is written to, but screen-space effects like SSAO and SSR may break with lower scales. Try to keep this value as close to 1.0 as possible. |

> **Note:** `MODELVIEW_MATRIX` combines both the `MODEL_MATRIX` and `VIEW_MATRIX` and is better suited when floating point issues may arise. For example, if the object is very far away from the world origin, you may run into floating point issues when using the separated `MODEL_MATRIX` and `VIEW_MATRIX`.

> **Note:** `INV_VIEW_MATRIX` is the matrix used for rendering the object in that pass, unlike `MAIN_CAM_INV_VIEW_MATRIX`, which is the matrix of the camera in the scene. In the shadow pass, `INV_VIEW_MATRIX`'s view is based on the camera that is located at the position of the light.

### 1.6 Fragment Built-ins

The default use of a Godot fragment processor function is to set up the material properties of your object and to let the built-in renderer handle the final shading. However, you are not required to use all these properties, and if you don't write to them, Godot will optimize away the corresponding functionality.

| Built-in | Description |
|---|---|
| `in vec2 VIEWPORT_SIZE` | Size of viewport (in pixels). |
| `in vec4 FRAGCOORD` | Coordinate of pixel center in screen space. `xy` specifies position in window. Origin is upper left. `z` specifies fragment depth. It is also used as the output value for the fragment depth unless `DEPTH` is written to. |
| `in bool FRONT_FACING` | `true` if current face is front facing, `false` otherwise. |
| `in vec3 VIEW` | Normalized vector from fragment position to camera (in view space). This is the same for both perspective and orthogonal cameras. |
| `in vec2 UV` | UV that comes from the `vertex()` function. |
| `in vec2 UV2` | UV2 that comes from the `vertex()` function. |
| `in vec4 COLOR` | COLOR that comes from the `vertex()` function. |
| `in vec2 POINT_COORD` | Point coordinate for drawing points with `POINT_SIZE`. |
| `in mat4 MODEL_MATRIX` | Model/local space to world space transform. |
| `in mat3 MODEL_NORMAL_MATRIX` | Model/local space to world space transform for normals. This is the same as `MODEL_MATRIX` by default unless the object is scaled non-uniformly, in which case this is set to `transpose(inverse(mat3(MODEL_MATRIX)))`. |
| `in mat4 VIEW_MATRIX` | World space to view space transform. |
| `in mat4 INV_VIEW_MATRIX` | View space to world space transform. |
| `in mat4 PROJECTION_MATRIX` | View space to clip space transform. |
| `in mat4 INV_PROJECTION_MATRIX` | Clip space to view space transform. |
| `in vec3 NODE_POSITION_WORLD` | Node position, in world space. |
| `in vec3 NODE_POSITION_VIEW` | Node position, in view space. |
| `in vec3 CAMERA_POSITION_WORLD` | Camera position, in world space. Represents the midpoint of the two eyes when in multiview/stereo rendering. |
| `in vec3 CAMERA_DIRECTION_WORLD` | Camera direction, in world space. |
| `in uint CAMERA_VISIBLE_LAYERS` | Cull layers of the camera rendering the current pass. |
| `in vec3 VERTEX` | Position of the fragment (pixel), in view space. It is the `VERTEX` value from `vertex()` interpolated between the face's vertices and transformed into view space. If `skip_vertex_transform` is enabled, it may not be in view space. |
| `inout vec3 LIGHT_VERTEX` | A writable version of `VERTEX` that can be used to alter light and shadows. Writing to this will not change the position of the fragment. |
| `in int VIEW_INDEX` | The view that we are rendering. Used to distinguish between views in multiview/stereo rendering. `VIEW_MONO_LEFT` (0) for Mono (not multiview) or left eye, `VIEW_RIGHT` (1) for right eye. |
| `in int VIEW_MONO_LEFT` | Constant for Mono or left eye, always 0. |
| `in int VIEW_RIGHT` | Constant for right eye, always 1. |
| `in vec3 EYE_OFFSET` | Position offset for the eye being rendered, in view space. Only applicable for multiview rendering. |
| `sampler2D SCREEN_TEXTURE` | Removed in Godot 4. Use a `sampler2D` with `hint_screen_texture` instead. |
| `in vec2 SCREEN_UV` | Screen UV coordinate for the current pixel. |
| `sampler2D DEPTH_TEXTURE` | Removed in Godot 4. Use a `sampler2D` with `hint_depth_texture` instead. |
| `out float DEPTH` | Custom depth value (range [0.0, 1.0]). If `DEPTH` is written to in any shader branch, then you are responsible for setting `DEPTH` for all other branches. Otherwise, the graphics API will leave them uninitialized. |
| `inout vec3 NORMAL` | Normal that comes from the `vertex()` function, in view space. If `skip_vertex_transform` is enabled, it may not be in view space. |
| `inout vec3 TANGENT` | Tangent that comes from the `vertex()` function, in view space. If `skip_vertex_transform` is enabled, it may not be in view space. |
| `inout vec3 BINORMAL` | Binormal that comes from the `vertex()` function, in view space. If `skip_vertex_transform` is enabled, it may not be in view space. |
| `out vec3 NORMAL_MAP` | Set normal here if reading normal from a texture instead of `NORMAL`. |
| `out float NORMAL_MAP_DEPTH` | Depth from `NORMAL_MAP`. Defaults to 1.0. |
| `out vec3 ALBEDO` | Albedo (default white). Base color. |
| `out float ALPHA` | Alpha (range [0.0, 1.0]). If read from or written to, the material will go to the transparent pipeline. |
| `out float ALPHA_SCISSOR_THRESHOLD` | If written to, values below a certain amount of alpha are discarded. |
| `out float ALPHA_HASH_SCALE` | Alpha hash scale when using the alpha hash transparency mode. Defaults to 1.0. Higher values result in more visible pixels in the dithering pattern. |
| `out float ALPHA_ANTIALIASING_EDGE` | The threshold below which alpha to coverage antialiasing should be used. Defaults to 0.0. Requires the `alpha_to_coverage` render mode. Should be set to a value lower than `ALPHA_SCISSOR_THRESHOLD` to be effective. |
| `out vec2 ALPHA_TEXTURE_COORDINATE` | The texture coordinate to use for alpha-to-coverage antialiasing. Requires the `alpha_to_coverage` render mode. Typically set to `UV * vec2(albedo_texture_size)` where `albedo_texture_size` is the size of the albedo texture in pixels. |
| `out float PREMUL_ALPHA_FACTOR` | Premultiplied alpha factor. Only effective if `render_mode blend_premul_alpha;` is used. This should be written to when using a shaded material with premultiplied alpha blending for interaction with lighting. This is not required for unshaded materials. |
| `out float METALLIC` | Metallic (range [0.0, 1.0]). |
| `out float SPECULAR` | Specular (not physically accurate to change). Defaults to 0.5. 0.0 disables reflections. |
| `out float ROUGHNESS` | Roughness (range [0.0, 1.0]). |
| `out float RIM` | Rim (range [0.0, 1.0]). If used, Godot calculates rim lighting. Rim size depends on `ROUGHNESS`. |
| `out float RIM_TINT` | Rim Tint, range from 0.0 (white) to 1.0 (albedo). If used, Godot calculates rim lighting. |
| `out float CLEARCOAT` | Small specular blob added on top of the existing one. If used, Godot calculates clearcoat. |
| `out float CLEARCOAT_GLOSS` | Gloss of clearcoat. If used, Godot calculates clearcoat. |
| `out float ANISOTROPY` | For distorting the specular blob according to tangent space. |
| `out vec2 ANISOTROPY_FLOW` | Distortion direction, use with flowmaps. |
| `out float SSS_STRENGTH` | Strength of subsurface scattering. If used, subsurface scattering will be applied to the object. |
| `out vec4 SSS_TRANSMITTANCE_COLOR` | Color of subsurface scattering transmittance. If used, subsurface scattering transmittance will be applied to the object. |
| `out float SSS_TRANSMITTANCE_DEPTH` | Depth of subsurface scattering transmittance. Higher values allow the effect to reach deeper into the object. |
| `out float SSS_TRANSMITTANCE_BOOST` | Boosts the subsurface scattering transmittance if set above 0.0. This makes the effect show up even on directly lit surfaces. |
| `inout vec3 BACKLIGHT` | Color of backlighting (works like direct light, but it's received even if the normal is slightly facing away from the light). If used, backlighting will be applied to the object. Can be used as a cheaper approximation of subsurface scattering. |
| `out float AO` | Strength of ambient occlusion. For use with pre-baked AO. |
| `out float AO_LIGHT_AFFECT` | How much ambient occlusion affects direct light (range [0.0, 1.0], default 0.0). |
| `out vec3 EMISSION` | Emission color (can go over (1.0, 1.0, 1.0) for HDR). |
| `out vec4 FOG` | If written to, blends final pixel color with `FOG.rgb` based on `FOG.a`. |
| `out vec4 RADIANCE` | If written to, blends environment map radiance with `RADIANCE.rgb` based on `RADIANCE.a`. |
| `out vec4 IRRADIANCE` | If written to, blends environment map irradiance with `IRRADIANCE.rgb` based on `IRRADIANCE.a`. |

> **Note:** Shaders going through the transparent pipeline when `ALPHA` is written to may exhibit transparency sorting issues. Read the transparency sorting section in the 3D rendering limitations page for more information and ways to avoid issues.

### 1.7 Light Built-ins

Writing light processor functions is completely optional. You can skip the `light()` function by using the `unshaded` render mode. If no light function is written, Godot will use the material properties written to in the `fragment()` function to calculate the lighting for you (subject to the render mode).

The `light()` function is called for every light in every pixel. It is called within a loop for each light type.

Below is an example of a custom `light()` function using a Lambertian lighting model:

```glsl
void light() {
    DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR / PI;
}
```

If you want the lights to add together, add the light contribution to `DIFFUSE_LIGHT` using `+=`, rather than overwriting it.

> **Warning:** The `light()` function will not be run if the `vertex_lighting` render mode is enabled, or if **Rendering > Quality > Shading > Force Vertex Shading** is enabled in the Project Settings. (It's enabled by default on mobile platforms.)

| Built-in | Description |
|---|---|
| `in vec2 VIEWPORT_SIZE` | Size of viewport (in pixels). |
| `in vec4 FRAGCOORD` | Coordinate of pixel center in screen space. `xy` specifies position in window, `z` specifies fragment depth if `DEPTH` is not used. Origin is lower-left. |
| `in mat4 MODEL_MATRIX` | Model/local space to world space transform. |
| `in mat4 INV_VIEW_MATRIX` | View space to world space transform. |
| `in mat4 VIEW_MATRIX` | World space to view space transform. |
| `in mat4 PROJECTION_MATRIX` | View space to clip space transform. |
| `in mat4 INV_PROJECTION_MATRIX` | Clip space to view space transform. |
| `in vec3 NORMAL` | Normal vector, in view space. |
| `in vec2 SCREEN_UV` | Screen UV coordinate for the current pixel. |
| `in vec2 UV` | UV that comes from the `vertex()` function. |
| `in vec2 UV2` | UV2 that comes from the `vertex()` function. |
| `in vec3 VIEW` | View vector, in view space. |
| `in vec3 LIGHT` | Light vector, in view space. |
| `in vec3 LIGHT_COLOR` | Light color multiplied by light energy multiplied by PI. The PI multiplication is present because physically-based lighting models include a division by PI. |
| `in float SPECULAR_AMOUNT` | For `OmniLight3D` and `SpotLight3D`, 2.0 multiplied by `light_specular`. For `DirectionalLight3D`, 1.0. |
| `in bool LIGHT_IS_DIRECTIONAL` | `true` if this pass is a `DirectionalLight3D`. |
| `in float ATTENUATION` | Attenuation based on distance or shadow. |
| `in vec3 ALBEDO` | Base albedo. |
| `in vec3 BACKLIGHT` | Backlight color. |
| `in float METALLIC` | Metallic. |
| `in float ROUGHNESS` | Roughness. |
| `out vec3 DIFFUSE_LIGHT` | Diffuse light result. |
| `out vec3 SPECULAR_LIGHT` | Specular light result. |
| `out float ALPHA` | Alpha (range [0.0, 1.0]). If written to, the material will go to the transparent pipeline. |

> **Note:** Shaders going through the transparent pipeline when `ALPHA` is written to may exhibit transparency sorting issues. Transparent materials also cannot cast shadows or appear in `hint_screen_texture` and `hint_depth_texture` uniforms. This in turn prevents those materials from appearing in screen-space reflections or refraction. SDFGI sharp reflections are not visible on transparent materials (only rough reflections are visible on transparent materials).

---

## 2. CanvasItem Shaders

CanvasItem shaders are used to draw all 2D elements in Godot. These include all nodes that inherit from CanvasItems, and all GUI elements.

CanvasItem shaders contain fewer built-in variables and functionality than Spatial shaders, but they maintain the same basic structure with vertex, fragment, and light processor functions.

### 2.1 Render Modes

| Render Mode | Description |
|---|---|
| `blend_mix` | Mix blend mode (alpha is transparency), default. |
| `blend_add` | Additive blend mode. |
| `blend_sub` | Subtractive blend mode. |
| `blend_mul` | Multiplicative blend mode. |
| `blend_premul_alpha` | Pre-multiplied alpha blend mode. |
| `blend_disabled` | Disable blending, values (including alpha) are written as-is. |
| `unshaded` | Result is just albedo. No lighting/shading happens in material. |
| `light_only` | Only draw in the light pass. |
| `skip_vertex_transform` | `VERTEX` needs to be transformed manually in the `vertex()` function. |
| `world_vertex_coords` | `VERTEX` is modified in world coordinates instead of local. |

### 2.2 Global Built-ins

| Built-in | Description |
|---|---|
| `in float TIME` | Global time since the engine has started, in seconds. It repeats after every 3,600 seconds (which can be changed with the rollover setting). It's affected by `time_scale` but not by pausing. If you need a `TIME` variable that is not affected by time scale, add your own global shader uniform and update it each frame. |
| `in float PI` | A PI constant (3.141592). |
| `in float TAU` | A TAU constant (6.283185). Equivalent to `PI * 2`. |
| `in float E` | An E constant (2.718281). Euler's number, the base of the natural logarithm. |

### 2.3 Vertex Built-ins

Vertex data (`VERTEX`) is presented in local space (pixel coordinates, relative to the Node2D's origin). If not written to, these values will not be modified and be passed through as they came.

The user can disable the built-in model to world transform (world to screen and projection will still happen later) and do it manually:

```glsl
shader_type canvas_item;
render_mode skip_vertex_transform;

void vertex() {
    VERTEX = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}
```

For instancing, the `INSTANCE_CUSTOM` variable contains the instance custom data. When using particles, this information is usually:
- **x:** Rotation angle in radians.
- **y:** Phase during lifetime (0.0 to 1.0).
- **z:** Animation frame.

| Built-in | Description |
|---|---|
| `in mat4 MODEL_MATRIX` | Local space to world space transform. World space is the coordinates you normally use in the editor. |
| `in mat4 CANVAS_MATRIX` | World space to canvas space transform. In canvas space the origin is the upper-left corner of the screen and coordinates range from (0.0, 0.0) to viewport size. |
| `in mat4 SCREEN_MATRIX` | Canvas space to clip space transform. In clip space coordinates range from (-1.0, -1.0) to (1.0, 1.0). |
| `in int INSTANCE_ID` | Instance ID for instancing. |
| `in vec4 INSTANCE_CUSTOM` | Instance custom data. |
| `in bool AT_LIGHT_PASS` | Always `false`. |
| `in vec2 TEXTURE_PIXEL_SIZE` | Normalized pixel size of the default 2D texture. For a Sprite2D with a texture of size 64x32px, `TEXTURE_PIXEL_SIZE = vec2(1.0/64.0, 1.0/32.0)`. |
| `inout vec2 VERTEX` | Vertex position, in local space. |
| `in int VERTEX_ID` | The index of the current vertex in the vertex buffer. |
| `inout vec2 UV` | Normalized texture coordinates. Range from 0.0 to 1.0. |
| `inout vec4 COLOR` | Color from vertex primitive multiplied by the CanvasItem's `modulate` multiplied by CanvasItem's `self_modulate`. |
| `inout float POINT_SIZE` | Point size for point drawing. |
| `in vec4 CUSTOM0` | Custom value from vertex primitive. |
| `in vec4 CUSTOM1` | Custom value from vertex primitive. |

### 2.4 Fragment Built-ins

#### COLOR and TEXTURE

The built-in variable `COLOR` is used for a few things:
- In the `vertex()` function, `COLOR` contains the color from the vertex primitive multiplied by the CanvasItem's `modulate` multiplied by the CanvasItem's `self_modulate`.
- In the `fragment()` function, the input value `COLOR` is that same value multiplied by the color from the default `TEXTURE` (if present).
- In the `fragment()` function, `COLOR` is also the final output.

Certain nodes (for example, Sprite2D) display a texture by default. When using a custom `fragment()` function, you have a few options on how to sample this texture.

To read only the contents of the default texture, ignoring the vertex `COLOR`:

```glsl
void fragment() {
    COLOR = texture(TEXTURE, UV);
}
```

To read the contents of the default texture multiplied by vertex `COLOR`:

```glsl
void fragment() {
    // Equivalent to an empty fragment() function, since COLOR is also the output variable.
    COLOR = COLOR;
}
```

To read only the vertex `COLOR` in `fragment()`, ignoring the main texture, you must pass `COLOR` as a varying, then read it in `fragment()`:

```glsl
varying vec4 vertex_color;
void vertex() {
    vertex_color = COLOR;
}
void fragment() {
    COLOR = vertex_color;
}
```

#### NORMAL

Similarly, if a normal map is used in the CanvasTexture, Godot uses it by default and assigns its value to the built-in `NORMAL` variable. If you are using a normal map meant for use in 3D, it will appear inverted. In order to use it in your shader, you must assign it to the `NORMAL_MAP` property. Godot will handle converting it for use in 2D and overwriting `NORMAL`.

```glsl
NORMAL_MAP = texture(NORMAL_TEXTURE, UV).rgb;
```

| Built-in | Description |
|---|---|
| `in vec4 FRAGCOORD` | Coordinate of pixel center. In screen space. `xy` specifies position in viewport. Upper-left of the viewport is the origin, (0.0, 0.0). |
| `in vec2 SCREEN_PIXEL_SIZE` | Size of individual pixels. Equal to the inverse of resolution. |
| `in vec4 REGION_RECT` | Visible area of the sprite region in format (x, y, width, height). Varies according to Sprite2D's `region_enabled` property. |
| `in vec2 POINT_COORD` | Coordinate for drawing points. |
| `sampler2D TEXTURE` | Default 2D texture. |
| `in vec2 TEXTURE_PIXEL_SIZE` | Normalized pixel size of the default 2D texture. |
| `in bool AT_LIGHT_PASS` | Always `false`. |
| `sampler2D SPECULAR_SHININESS_TEXTURE` | Specular shininess texture of this object. |
| `in vec4 SPECULAR_SHININESS` | Specular shininess color, as sampled from the texture. |
| `in vec2 UV` | UV from the `vertex()` function. For a Sprite2D with region enabled, this will sample the entire texture. Use `REGION_RECT` instead to sample only the region defined in the Sprite2D's properties. |
| `in vec2 SCREEN_UV` | Screen UV coordinate for the current pixel. |
| `sampler2D SCREEN_TEXTURE` | Removed in Godot 4. Use a `sampler2D` with `hint_screen_texture` instead. |
| `inout vec3 NORMAL` | Normal read from `NORMAL_TEXTURE`. Writable. |
| `sampler2D NORMAL_TEXTURE` | Default 2D normal texture. |
| `out vec3 NORMAL_MAP` | Configures normal maps meant for 3D for use in 2D. If used, overrides `NORMAL`. |
| `out float NORMAL_MAP_DEPTH` | Normal map depth for scaling. |
| `inout vec2 VERTEX` | Pixel position in screen space. |
| `inout vec2 SHADOW_VERTEX` | Same as `VERTEX` but can be written to alter shadows. |
| `inout vec3 LIGHT_VERTEX` | Same as `VERTEX` but can be written to alter lighting. Z component represents height. |
| `inout vec4 COLOR` | `COLOR` from the `vertex()` function multiplied by the `TEXTURE` color. Also output color value. |

### 2.5 Light Built-ins

Light processor functions work differently in Godot 4.x than they did in Godot 3.x. In Godot 4.x all lighting is done during the regular draw pass. In other words, Godot no longer draws the object again for each light.

Use the `unshaded` render mode if you do not want the `light()` function to run. Use the `light_only` render mode if you only want to see the impact of lighting on an object; this can be useful when you only want the object visible where it is covered by light.

If you define a `light()` function it will replace the built-in light function, even if your light function is empty.

Below is an example of a light shader that takes a CanvasItem's normal map into account:

```glsl
void light() {
    float cNdotL = max(0.0, dot(NORMAL, LIGHT_DIRECTION));
    LIGHT = vec4(LIGHT_COLOR.rgb * COLOR.rgb * LIGHT_ENERGY * cNdotL, LIGHT_COLOR.a);
}
```

| Built-in | Description |
|---|---|
| `in vec4 FRAGCOORD` | Coordinate of pixel center. In screen space. `xy` specifies position in viewport. Upper-left of the viewport is the origin, (0.0, 0.0). |
| `in vec3 NORMAL` | Input normal. |
| `in vec4 COLOR` | Input color. This is the output of the `fragment()` function. |
| `in vec2 UV` | UV from the `vertex()` function, equivalent to the UV in the `fragment()` function. |
| `sampler2D TEXTURE` | Current texture in use for the CanvasItem. |
| `in vec2 TEXTURE_PIXEL_SIZE` | Normalized pixel size of `TEXTURE`. |
| `in vec2 SCREEN_UV` | Screen UV coordinate for the current pixel. |
| `in vec2 POINT_COORD` | UV for Point Sprite. |
| `in vec4 LIGHT_COLOR` | Color of the Light2D. If the light is a PointLight2D, multiplied by the light's texture. |
| `in float LIGHT_ENERGY` | Energy multiplier of the Light2D. |
| `in vec3 LIGHT_POSITION` | Position of the Light2D in screen space. If using a DirectionalLight2D this is always (0.0, 0.0, 0.0). |
| `in vec3 LIGHT_DIRECTION` | Direction of the Light2D in screen space. |
| `in bool LIGHT_IS_DIRECTIONAL` | `true` if this pass is a DirectionalLight2D. |
| `in vec3 LIGHT_VERTEX` | Pixel position, in screen space as modified in the `fragment()` function. |
| `inout vec4 LIGHT` | Output color for this Light2D. |
| `in vec4 SPECULAR_SHININESS` | Specular shininess, as set in the object's texture. |
| `out vec4 SHADOW_MODULATE` | Multiply shadows cast at this point by this color. |

### 2.6 SDF Functions

There are a few additional functions implemented to sample an automatically generated Signed Distance Field texture. These functions are available in the `fragment()` and `light()` functions of CanvasItem shaders. Custom functions may also use them as long as they are called from supported functions.

The signed distance field is generated from LightOccluder2D nodes present in the scene with the **SDF Collision** property enabled (which is the default).

| Function | Description |
|---|---|
| `float texture_sdf(vec2 sdf_pos)` | Performs an SDF texture lookup. |
| `vec2 texture_sdf_normal(vec2 sdf_pos)` | Calculates a normal from the SDF texture. |
| `vec2 sdf_to_screen_uv(vec2 sdf_pos)` | Converts an SDF to screen UV. |
| `vec2 screen_uv_to_sdf(vec2 uv)` | Converts screen UV to an SDF. |

---

## 3. Particle Shaders

Particle shaders are a special type of shader that runs before the object is drawn. They are used for calculating material properties such as color, position, and rotation. They can be drawn with any regular material for CanvasItem or Spatial, depending on whether they are 2D or 3D.

Particle shaders are unique because they are not used to draw the object itself; they are used to calculate particle properties, which are then used by a CanvasItem or Spatial shader. They contain two processor functions: `start()` and `process()`.

Unlike other shader types, particle shaders keep the data that was output the previous frame. Therefore, particle shaders can be used for complex effects that take place over multiple frames.

> **Note:** Particle shaders are only available with GPU-based particle nodes (`GPUParticles2D` and `GPUParticles3D`). CPU-based particle nodes (`CPUParticles2D` and `CPUParticles3D`) are rendered on the GPU (which means they can use custom CanvasItem or Spatial shaders), but their motion is simulated on the CPU.

### 3.1 Render Modes

| Render Mode | Description |
|---|---|
| `keep_data` | Do not clear previous data on restart. |
| `disable_force` | Disable attractor force. |
| `disable_velocity` | Ignore `VELOCITY` value. |
| `collision_use_scale` | Scale the particle's size for collisions. |

### 3.2 Global Built-ins

| Built-in | Description |
|---|---|
| `in float TIME` | Global time since the engine has started, in seconds. It repeats after every 3,600 seconds (configurable). Affected by `time_scale` but not by pausing. |
| `in float PI` | A PI constant (3.141592). |
| `in float TAU` | A TAU constant (6.283185). |
| `in float E` | An E constant (2.718281). |

### 3.3 Start and Process Built-ins

These properties can be accessed from both the `start()` and `process()` functions.

| Built-in | Description |
|---|---|
| `in float LIFETIME` | Particle lifetime. |
| `in float DELTA` | Delta process time. |
| `in uint NUMBER` | Unique number since emission start. |
| `in uint INDEX` | Particle index (from total particles). |
| `in mat4 EMISSION_TRANSFORM` | Emitter transform (used for non-local systems). |
| `in uint RANDOM_SEED` | Random seed used as base for random. |
| `inout bool ACTIVE` | `true` when the particle is active, can be set to `false`. |
| `inout vec4 COLOR` | Particle color, can be written to and accessed in the mesh's vertex function. |
| `inout vec3 VELOCITY` | Particle velocity, can be modified. |
| `inout mat4 TRANSFORM` | Particle transform. |
| `inout vec4 CUSTOM` | Custom particle data. Accessible from the mesh's shader as `INSTANCE_CUSTOM`. |
| `inout float MASS` | Particle mass, intended to be used with attractors. 1.0 by default. |
| `in vec4 USERDATAX` | Vector that enables the integration of supplementary user-defined data into the particle process shader. `USERDATAX` are six built-ins identified by number, X can be numbers between 1 and 6 (e.g., `USERDATA3`). |
| `in uint FLAG_EMIT_POSITION` | A flag for the last argument of `emit_subparticle()` to assign a position to a new particle's transform. |
| `in uint FLAG_EMIT_ROT_SCALE` | A flag for the last argument of `emit_subparticle()` to assign a rotation and scale to a new particle's transform. |
| `in uint FLAG_EMIT_VELOCITY` | A flag for the last argument of `emit_subparticle()` to assign a velocity to a new particle. |
| `in uint FLAG_EMIT_COLOR` | A flag for the last argument of `emit_subparticle()` to assign a color to a new particle. |
| `in uint FLAG_EMIT_CUSTOM` | A flag for the last argument of `emit_subparticle()` to assign a custom data vector to a new particle. |
| `in vec3 EMITTER_VELOCITY` | Velocity of the Particles2D/3D node. |
| `in float INTERPOLATE_TO_END` | Value of the `interp_to_end` property of the Particles node. |
| `in uint AMOUNT_RATIO` | Value of the `amount_ratio` property of the Particles node. |

> **Note:** In order to use the `COLOR` variable in a `StandardMaterial3D`, set `vertex_color_use_as_albedo` to `true`. In a `ShaderMaterial`, access it with the `COLOR` variable.

### 3.4 Start Built-ins

| Built-in | Description |
|---|---|
| `in bool RESTART_POSITION` | `true` if particle is restarted, or emitted without a custom position (i.e. this particle was created by `emit_subparticle()` without the `FLAG_EMIT_POSITION` flag). |
| `in bool RESTART_ROT_SCALE` | `true` if particle is restarted, or emitted without a custom rotation or scale (i.e. this particle was created by `emit_subparticle()` without the `FLAG_EMIT_ROT_SCALE` flag). |
| `in bool RESTART_VELOCITY` | `true` if particle is restarted, or emitted without a custom velocity (i.e. this particle was created by `emit_subparticle()` without the `FLAG_EMIT_VELOCITY` flag). |
| `in bool RESTART_COLOR` | `true` if particle is restarted, or emitted without a custom color (i.e. this particle was created by `emit_subparticle()` without the `FLAG_EMIT_COLOR` flag). |
| `in bool RESTART_CUSTOM` | `true` if particle is restarted, or emitted without a custom property (i.e. this particle was created by `emit_subparticle()` without the `FLAG_EMIT_CUSTOM` flag). |

### 3.5 Process Built-ins

| Built-in | Description |
|---|---|
| `in bool RESTART` | `true` if the current process frame is the first for the particle. |
| `in bool COLLIDED` | `true` when the particle has collided with a particle collider. |
| `in vec3 COLLISION_NORMAL` | A normal of the last collision. If there is no collision detected it is equal to `(0.0, 0.0, 0.0)`. |
| `in float COLLISION_DEPTH` | A length of the normal of the last collision. If there is no collision detected it is equal to `0.0`. |
| `in vec3 ATTRACTOR_FORCE` | A combined force of the attractors at the moment on that particle. |

### 3.6 Process Functions

`emit_subparticle()` is currently the only custom function supported by particle shaders. It allows users to add a new particle with specified parameters from a sub-emitter. The newly created particle will only use the properties that match the `flags` parameter. For example, the following code will emit a particle with a specified position, velocity, and color, but unspecified rotation, scale, and custom value:

```glsl
mat4 custom_transform = mat4(1.0);
custom_transform[3].xyz = vec3(10.5, 0.0, 4.0);
emit_subparticle(custom_transform, vec3(1.0, 0.5, 1.0), vec4(1.0, 0.0, 0.0, 1.0), vec4(1.0), FLAG_EMIT_POSITION | FLAG_EMIT_VELOCITY | FLAG_EMIT_COLOR);
```

| Function | Description |
|---|---|
| `bool emit_subparticle(mat4 xform, vec3 velocity, vec4 color, vec4 custom, uint flags)` | Emits a particle from a sub-emitter. |

---

## 4. Shader Preprocessor

### 4.1 Why Use a Shader Preprocessor?

In programming languages, a *preprocessor* allows changing the code before the compiler reads it. Unlike the compiler, the preprocessor does not care about whether the syntax of the preprocessed code is valid. The preprocessor always performs what the *directives* tell it to do. A directive is a statement starting with a hash symbol (`#`). It is not a *keyword* of the shader language (such as `if` or `for`), but a special kind of token within the language.

To avoid repetition and improve code reuse, you can use a shader preprocessor within text-based shaders. The syntax is similar to what most GLSL shader compilers support (which in turn is similar to the C/C++ preprocessor).

> **Note:** The shader preprocessor is not available in visual shaders. If you need to introduce preprocessor statements to a visual shader, you can convert it to a text-based shader using the **Convert to Shader** option in the VisualShader inspector resource dropdown. This conversion is a one-way operation; text shaders cannot be converted back to visual shaders.

### 4.2 Directives

#### General Syntax

- Preprocessor directives do not use brackets (`{}`), but can use parentheses.
- Preprocessor directives never end with semicolons (with the exception of `#define`, where this is allowed but potentially dangerous).
- Preprocessor directives can span several lines by ending each line with a backslash (`\`). The first line break not featuring a backslash will end the preprocessor statement.

#### `#define`

**Syntax:** `#define <identifier> [replacement_code]`

Defines the identifier after that directive as a macro, and replaces all successive occurrences of it with the replacement code given in the shader. Replacement is performed on a "whole words" basis, which means no replacement is performed if the string is part of another string (without any spaces or operators separating it).

Defines with replacements may also have one or more *arguments*, which can then be passed when referencing the define (similar to a function call).

If the replacement code is not defined, the identifier may only be used with `#ifdef` or `#ifndef` directives.

If the *concatenation* symbol (`##`) is present in the replacement code then it will be removed upon macro insertion, together with any space surrounding it, and join the surrounding words and arguments into a new token.

```glsl
uniform sampler2D material0;

#define SAMPLE(N) vec4 tex##N = texture(material##N, UV)

void fragment() {
    SAMPLE(0);
    ALBEDO = tex0.rgb;
}
```

Compared to constants (`const CONSTANT = value;`), `#define` can be used anywhere within the shader (including in uniform hints). `#define` can also be used to insert arbitrary shader code at any location, while constants cannot do that.

```glsl
shader_type spatial;

// Notice the lack of semicolon at the end of the line, as the replacement text
// shouldn't insert a semicolon on its own.
// If the directive ends with a semicolon, the semicolon is inserted in every usage
// of the directive, even when this causes a syntax error.
#define USE_MY_COLOR
#define MY_COLOR vec3(1, 0, 0)

// Replacement with arguments.
// All arguments are required (no default values can be provided).
#define BRIGHTEN_COLOR(r, g, b) vec3(r + 0.5, g + 0.5, b + 0.5)

// Multiline replacement using backslashes for continuation:
#define SAMPLE(param1, param2, param3, param4) long_function_call( \
        param1, \
        param2, \
        param3, \
        param4 \
)

void fragment() {
#ifdef USE_MY_COLOR
    ALBEDO = MY_COLOR;
#endif
}
```

Defining a `#define` for an identifier that is already defined results in an error. To prevent this, use `#undef <identifier>`.

#### `#undef`

**Syntax:** `#undef identifier`

The `#undef` directive may be used to cancel a previously defined `#define` directive:

```glsl
#define MY_COLOR vec3(1, 0, 0)

vec3 get_red_color() {
    return MY_COLOR;
}

#undef MY_COLOR
#define MY_COLOR vec3(0, 1, 0)

vec3 get_green_color() {
    return MY_COLOR;
}

// Like in most preprocessors, undefining a define that was not previously defined is allowed
// (and won't print any warning or error).
#undef THIS_DOES_NOT_EXIST
```

Without `#undef` in the above example, there would be a macro redefinition error.

#### `#if`

**Syntax:** `#if <condition>`

The `#if` directive checks whether the `condition` passed. If it evaluates to a non-zero value, the code block is included, otherwise it is skipped.

To evaluate correctly, the condition must be an expression giving a simple floating-point, integer or boolean result. There may be multiple condition blocks connected by `&&` (AND) or `||` (OR) operators. It may be continued by an `#else` block, but **must** be ended with the `#endif` directive.

```glsl
#define VAR 3
#define USE_LIGHT 0 // Evaluates to `false`.
#define USE_COLOR 1 // Evaluates to `true`.

#if VAR == 3 && (USE_LIGHT || USE_COLOR)
// Condition is `true`. Include this portion in the final shader.
#endif
```

Using the `defined()` *preprocessor function*, you can check whether the passed identifier is defined by a `#define` placed above that directive. The `defined()` function's result can be negated by using the `!` (boolean NOT) symbol in front of it.

```glsl
#define USE_LIGHT
#define USE_COLOR

// Correct syntax:
#if defined(USE_LIGHT) || defined(USE_COLOR) || !defined(USE_REFRACTION)
// Condition is `true`. Include this portion in the final shader.
#endif
```

Be careful, as `defined()` must only wrap a single identifier within parentheses, never more:

```glsl
// Incorrect syntax (parentheses are not placed where they should be):
#if defined(USE_LIGHT || USE_COLOR || !USE_REFRACTION)
// This will cause an error or not behave as expected.
#endif
```

> **Tip:** In the shader editor, preprocessor branches that evaluate to `false` (and are therefore excluded from the final compiled shader) will appear grayed out. This does not apply to runtime `if` statements.

**`#if` preprocessor versus `if` statement: Performance caveats**

The shading language supports runtime `if` statements:

```glsl
uniform bool USE_LIGHT = true;

if (USE_LIGHT) {
    // This part is included in the compiled shader, and always run.
} else {
    // This part is included in the compiled shader, but never run.
}
```

If the uniform is never changed, this behaves identical to the following usage of the `#if` preprocessor statement:

```glsl
#define USE_LIGHT

#if defined(USE_LIGHT)
// This part is included in the compiled shader, and always run.
#else
// This part is *not* included in the compiled shader (and therefore never run).
#endif
```

However, the `#if` variant can be faster in certain scenarios. This is because all runtime branches in a shader are still compiled and variables within those branches may still take up register space, even if they are never run in practice.

Modern GPUs are quite effective at performing "static" branching. "Static" branching refers to `if` statements where *all* pixels/vertices evaluate to the same result in a given shader invocation. However, high amounts of VGPRs (which can be caused by having too many branches) can still slow down shader execution significantly.

#### `#elif`

The `#elif` directive stands for "else if" and checks the condition passed if the above `#if` evaluated to `false`. `#elif` can only be used within an `#if` block. It is possible to use several `#elif` statements after an `#if` statement.

```glsl
#define VAR 2

#if VAR == 0
// Not included.
#elif VAR == 1
// Not included.
#elif VAR == 2
// Condition is `true`. Include this portion in the final shader.
#else
// Not included.
#endif
```

Like with `#if`, the `defined()` preprocessor function can be used:

```glsl
#define SHADOW_QUALITY_MEDIUM

#if defined(SHADOW_QUALITY_HIGH)
// High shadow quality.
#elif defined(SHADOW_QUALITY_MEDIUM)
// Medium shadow quality.
#else
// Low shadow quality.
#endif
```

#### `#ifdef`

**Syntax:** `#ifdef <identifier>`

This is a shorthand for `#if defined(...)`. Checks whether the passed identifier is defined by `#define` placed above that directive.

```glsl
#define USE_LIGHT

#ifdef USE_LIGHT
// USE_LIGHT is defined. Include this portion in the final shader.
#endif
```

The processor does *not* support `#elifdef` as a shortcut for `#elif defined(...)`. Instead, use the following series of `#ifdef` and `#else` when you need more than two branches:

```glsl
#define SHADOW_QUALITY_MEDIUM

#ifdef SHADOW_QUALITY_HIGH
// High shadow quality.
#else
#ifdef SHADOW_QUALITY_MEDIUM
// Medium shadow quality.
#else
// Low shadow quality.
#endif // This ends SHADOW_QUALITY_MEDIUM's branch.
#endif // This ends SHADOW_QUALITY_HIGH's branch.
```

#### `#ifndef`

**Syntax:** `#ifndef <identifier>`

This is a shorthand for `#if !defined(...)`. Similar to `#ifdef`, but checks whether the passed identifier is **not** defined by `#define` before that directive. This is the exact opposite of `#ifdef`.

```glsl
#define USE_LIGHT

#ifndef USE_LIGHT
// Evaluates to `false`. This portion won't be included in the final shader.
#endif

#ifndef USE_COLOR
// Evaluates to `true`. This portion will be included in the final shader.
#endif
```

#### `#else`

**Syntax:** `#else`

Defines the optional block which is included when the previously defined `#if`, `#elif`, `#ifdef` or `#ifndef` directive evaluates to `false`.

```glsl
shader_type spatial;

#define MY_COLOR vec3(1.0, 0, 0)

void fragment() {
#ifdef MY_COLOR
    ALBEDO = MY_COLOR;
#else
    ALBEDO = vec3(0, 0, 1.0);
#endif
}
```

#### `#endif`

**Syntax:** `#endif`

Used as terminator for the `#if`, `#ifdef`, `#ifndef` or subsequent `#else` directives.

#### `#error`

**Syntax:** `#error <message>`

The `#error` directive forces the preprocessor to emit an error with optional message. For example, it's useful when used within `#if` block to provide a strict limitation of the defined value.

```glsl
#define MAX_LOD 3
#define LOD 4

#if LOD > MAX_LOD
#error LOD exceeds MAX_LOD
#endif
```

#### `#include`

**Syntax:** `#include "path"`

The `#include` directive includes the *entire* content of a shader include file in a shader. `"path"` can be an absolute `res://` path or relative to the current shader file. Relative paths are only allowed in shaders that are saved to `.gdshader` or `.gdshaderinc` files, while absolute paths can be used in shaders that are built into a scene/resource file.

You can create new shader includes by using the **File > Create Shader Include** menu option of the shader editor, or by creating a new `ShaderInclude` resource in the FileSystem dock.

Shader includes can be included from within any shader, or other shader include, at any point in the file.

When including shader includes in the global scope of a shader, it is recommended to do this after the initial `shader_type` statement.

You can also include shader includes from within the body of a function. Please note that the shader editor is likely going to report errors for your shader include's code, as it may not be valid outside of the context that it was written for. You can either choose to ignore these errors (the shader will still compile fine), or you can wrap the include in an `#ifdef` block that checks for a define from your shader.

`#include` is useful for creating libraries of helper functions (or macros) and reducing code duplication. When using `#include`, be careful about naming collisions, as redefining functions or macros is not allowed.

`#include` is subject to several restrictions:
- Only shader include resources (ending with `.gdshaderinc`) can be included. `.gdshader` files cannot be included by another shader, but a `.gdshaderinc` file can include other `.gdshaderinc` files.
- Cyclic dependencies are not allowed and will result in an error.
- To avoid infinite recursion, include depth is limited to 25 steps.

Example shader include file:

```glsl
// fancy_color.gdshaderinc

// While technically allowed, there is usually no `shader_type` declaration in include files.

vec3 get_fancy_color() {
    return vec3(0.3, 0.6, 0.9);
}
```

Example base shader (using the include file we created above):

```glsl
// material.gdshader

shader_type spatial;

#include "res://fancy_color.gdshaderinc"

void fragment() {
    // No error, as we've included a definition for `get_fancy_color()` via the shader include.
    COLOR = get_fancy_color();
}
```

#### `#pragma`

**Syntax:** `#pragma value`

The `#pragma` directive provides additional information to the preprocessor or compiler.

Currently, it may have only one value: `disable_preprocessor`. If you don't need the preprocessor, use that directive to speed up shader compilation by excluding the preprocessor step.

```glsl
#pragma disable_preprocessor

#if USE_LIGHT
// This causes a shader compilation error, as the `#if USE_LIGHT` and `#endif`
// are included as-is in the final shader code.
#endif
```

### 4.3 Built-in Defines

#### Current Renderer

Since Godot 4.4, you can check which renderer is currently used with the built-in defines `CURRENT_RENDERER`, `RENDERER_COMPATIBILITY`, `RENDERER_MOBILE`, and `RENDERER_FORWARD_PLUS`:

- `CURRENT_RENDERER` is set to either 0, 1, or 2 depending on the current renderer.
- `RENDERER_COMPATIBILITY` is always 0.
- `RENDERER_MOBILE` is always 1.
- `RENDERER_FORWARD_PLUS` is always 2.

As an example, this shader sets `ALBEDO` to a different color in each renderer:

```glsl
shader_type spatial;

void fragment() {
#if CURRENT_RENDERER == RENDERER_COMPATIBILITY
    ALBEDO = vec3(0.0, 0.0, 1.0);
#elif CURRENT_RENDERER == RENDERER_MOBILE
    ALBEDO = vec3(1.0, 0.0, 0.0);
#else // CURRENT_RENDERER == RENDERER_FORWARD_PLUS
    ALBEDO = vec3(0.0, 1.0, 0.0);
#endif
}
```

---

## 5. Your Second 3D Shader

From a high-level, what Godot does is give the user a bunch of parameters that can be optionally set (`AO`, `SSS_Strength`, `RIM`, etc.). These parameters correspond to different complex effects (Ambient Occlusion, SubSurface Scattering, Rim Lighting, etc.). When not written to, the code is thrown out before it is compiled and so the shader does not incur the cost of the extra feature. This makes it easy for users to have complex PBR-correct shading, without writing complex shaders. Of course, Godot also allows you to ignore all these parameters and write a fully customized shader.

A difference between the vertex function and a fragment function is that the vertex function runs per vertex and sets properties such as `VERTEX` (position) and `NORMAL`, while the fragment shader runs per pixel and, most importantly, sets the `ALBEDO` color of the MeshInstance3D.

### 5.1 Your First Spatial Fragment Function

The standard use of the fragment function in Godot is to set up different material properties and let Godot handle the rest. In order to provide even more flexibility, Godot also provides render modes. Render modes are set at the top of the shader, directly below `shader_type`, and they specify what sort of functionality you want the built-in aspects of the shader to have.

For example, if you do not want to have lights affect an object, set the render mode to `unshaded`:

```glsl
render_mode unshaded;
```

You can also stack multiple render modes together. For example, if you want to use toon shading instead of more-realistic PBR shading, set the diffuse mode and specular mode to toon:

```glsl
render_mode diffuse_toon, specular_toon;
```

In this part of the tutorial, we will walk through how to take the bumpy terrain from the previous part and turn it into an ocean.

First let's set the color of the water. We do that by setting `ALBEDO`. `ALBEDO` is a `vec3` that contains the color of the object.

```glsl
void fragment() {
    ALBEDO = vec3(0.1, 0.3, 0.5);
}
```

The PBR model that Godot uses relies on two main parameters: `METALLIC` and `ROUGHNESS`.

- `ROUGHNESS` specifies how smooth/rough the surface of a material is. A low `ROUGHNESS` will make a material appear like a shiny plastic, while a high roughness makes the material appear more solid in color.
- `METALLIC` specifies how much like a metal the object is. It is better set close to 0 or 1. A high `METALLIC` almost ignores `ALBEDO` altogether, and looks like a mirror of the sky. While a low `METALLIC` has a more equal representation of sky color and `ALBEDO` color.

> **Note:** `METALLIC` should be close to 0 or 1 for proper PBR shading. Only set it between them for blending between materials.

Water is not a metal, so we will set its `METALLIC` property to `0.0`. Water is also highly reflective, so we will set its `ROUGHNESS` property to be quite low as well.

```glsl
void fragment() {
    METALLIC = 0.0;
    ROUGHNESS = 0.01;
    ALBEDO = vec3(0.1, 0.3, 0.5);
}
```

In order to increase the specular reflections, we will change the render mode for specular to toon because the toon render mode has larger specular highlights:

```glsl
render_mode specular_toon;
```

We will add rim lighting. Rim lighting increases the effect of light at glancing angles. Usually it is used to emulate the way light passes through fabric on the edges of an object, but we will use it here to help achieve a nice watery effect.

```glsl
void fragment() {
    RIM = 0.2;
    METALLIC = 0.0;
    ROUGHNESS = 0.01;
    ALBEDO = vec3(0.1, 0.3, 0.5);
}
```

In order to add fresnel reflectance, we will compute a fresnel term in our fragment shader. The `NORMAL` vector points away from the mesh's surface, while the `VIEW` vector is the direction between your eye and that point on the surface. The dot product between them is a handy way to tell when you are looking at the surface head-on or at a glancing angle.

```glsl
float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
```

And mix it into both `ROUGHNESS` and `ALBEDO`:

```glsl
void fragment() {
    float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
    RIM = 0.2;
    METALLIC = 0.0;
    ROUGHNESS = 0.01 * (1.0 - fresnel);
    ALBEDO = vec3(0.1, 0.3, 0.5) + (0.1 * fresnel);
}
```

### 5.2 Animating with TIME

Going back to the vertex function, we can animate the waves using the built-in variable `TIME`.

```glsl
float height(vec2 position, float time) {
    return texture(noise, position / 10.0).x;
}

void vertex() {
    vec2 pos = VERTEX.xz;
    float k = height(pos, TIME);
    VERTEX.y = k;
}
```

Instead of using a normalmap to calculate normals, we compute them manually in the `vertex()` function:

```glsl
NORMAL = normalize(vec3(k - height(pos + vec2(0.1, 0.0), TIME), 0.1, k - height(pos + vec2(0.0, 0.1), TIME)));
```

Now, we make the `height()` function a little more complicated by offsetting `position` by the cosine of `TIME`:

```glsl
float height(vec2 position, float time) {
    vec2 offset = 0.01 * cos(position + time);
    return texture(noise, (position / 10.0) - offset).x;
}
```

### 5.3 Advanced Effects: Waves

The `wave()` function:

```glsl
float wave(vec2 position){
    position += texture(noise, position / 10.0).x * 2.0 - 1.0;
    vec2 wv = 1.0 - abs(sin(position));
    return pow(1.0 - pow(wv.x * wv.y, 0.65), 4.0);
}
```

Line-by-line explanation:
1. `position += texture(noise, position / 10.0).x * 2.0 - 1.0;` -- Offset the position by the noise texture. This will make the waves curve.
2. `vec2 wv = 1.0 - abs(sin(position));` -- Define a wave-like function using `sin()` and `position`. `abs()` gives a sharp ridge and constrains to the 0-1 range. Subtracting from `1.0` puts the peak on top.
3. `return pow(1.0 - pow(wv.x * wv.y, 0.65), 4.0);` -- Multiply the x-directional wave by the y-directional wave and raise it to a power to sharpen the peaks.

Layering multiple waves on top of each other at varying frequencies and amplitudes:

```glsl
float height(vec2 position, float time) {
    float d = wave((position + time) * 0.4) * 0.3;
    d += wave((position - time) * 0.3) * 0.3;
    d += wave((position + time) * 0.5) * 0.2;
    d += wave((position - time) * 0.6) * 0.2;
    return d;
}
```

Note that we add `time` to two and subtract it from the other two. This makes the waves move in different directions creating a complex effect. Also note that the amplitudes all add up to 1.0, keeping the wave in the 0-1 range.

---

## 6. Visual Shaders

VisualShaders are the visual alternative for creating shaders. As shaders are inherently linked to visuals, the graph-based approach with previews of textures, materials, etc. offers a lot of additional convenience compared to purely script-based shaders. On the other hand, VisualShaders do not expose all features of the shader script and using both in parallel might be necessary for specific effects.

### 6.1 Creating a VisualShader

VisualShaders can be created in any `ShaderMaterial`. To begin using VisualShaders, create a new `ShaderMaterial` in an object of your choice.

Then assign a Shader resource to the `Shader` property. Click on the new Shader resource and the **Create Shader** dialog will open automatically. Change the **Type** option to **VisualShader** in the dropdown, then give it a name.

Click on the visual shader you just created to open the Shader Editor. The layout of the Shader Editor comprises four parts: a file list on the left, the upper toolbar, the graph itself, and a material preview on the right that can be toggled off.

Toolbar controls (left to right):
- The arrow toggles the files panel's visibility.
- The **File** button opens a dropdown menu for saving, loading, and creating files.
- The **Add Node** button displays a popup menu to let you add nodes to the shader graph.
- The drop-down menu selects the shader type: Vertex, Fragment and Light. It defines what built-in nodes will be available.
- Buttons and number input control zooming level, grid snapping and grid line distance.
- The toggle controls graph minimap visibility.
- The **automatically arrange selected nodes** button organizes selected nodes.
- The **Manage Varyings** button opens a dropdown that lets you add or remove a varying.
- The **show generated code** button shows shader code corresponding to your graph.
- The toggle turns the material preview on or off.
- The **Online Docs** button opens documentation in your web browser.
- The last button allows you to put the shader editor in its own window.

> **Note:** Although VisualShaders do not require coding, they share the same logic with script shaders. The visual shader graph is converted to a script shader behind the scene, and you can see this code by pressing the last button in the toolbar.

### 6.2 Using the Visual Shader Editor

By default, every new VisualShader will have an output node. Every node connection ends at one of the output node's sockets. To add a new node, click on the **Add Node** button on the upper left corner or right click on any empty location in the graph.

When connecting any `scalar` output to a `vector` input, all components of the vector will take the value of the scalar.

When connecting any `vector` output to a `scalar` input, the value of the scalar will be the average of the vector's components.

### 6.3 Visual Shader Node Interface

Visual shader nodes have input and output ports. Input ports are on the left side, output ports on the right side.

Port types:

| Type | Color | Description |
|---|---|---|
| Scalar | Gray | Scalar is a single value. |
| Vector | Purple | Vector is a set of values. |
| Boolean | Green | On or off, true or false. |
| Transform | Pink | A matrix, usually used to transform vertices. |
| Sampler | Orange | A texture sampler. It can be used to sample textures. |

### 6.4 Notable Visual Shader Nodes

#### Expression Node

The `Expression` node allows you to write Godot Shading Language (GLSL-like) expressions inside your visual shaders. The node has buttons to add any amount of required input and output ports and can be resized. You can also set up the name and type of each port. The expression you have entered will apply immediately to the material. Any parsing or compilation errors will be printed to the Output tab. The outputs are initialized to their zero value by default. The node is located under the **Special** tab and can be used in all shader modes.

The possibilities of this node are almost limitless -- you can write complex procedures, and use all the power of text-based shaders, such as loops, the `discard` keyword, extended types, etc.

#### Reroute Node

The `Reroute` node is used purely for organizational purposes. In a complicated shader with many nodes you may find that the paths between nodes can make things hard to read. Reroute allows you to adjust the path between nodes. You can even have multiple reroute nodes for a single path, which can be used to make right angles.

#### Fresnel Node

The `Fresnel` node is designed to accept normal and view vectors and produces a scalar which is the saturated dot product between them. Additionally, you can set up the inversion and the power of equation. The Fresnel node is great for adding a rim-like lighting effect to objects.

#### Boolean Node

The `Boolean` node can be converted to `Scalar` or `Vector` to represent `0` or `1` and `(0, 0, 0)` or `(1, 1, 1)` respectively. This property can be used to enable or disable some effect parts with one click.

#### If Node

The `If` node allows you to set up a vector which will be returned as the result of the comparison between `a` and `b`. There are three vectors which can be returned: `a == b` (in that case the tolerance parameter is provided as a comparison threshold -- by default it is equal to `0.00001`), `a > b` and `a < b`.

#### Switch Node

The `Switch` node returns a vector if the boolean condition is `true` or `false`. If you want to convert a vector to a true boolean, all components of the vector should be non-zero.

#### Mesh Emitter

The `Mesh Emitter` node is used for emitting particles from mesh vertices. This is only available for shaders that are in `Particles` mode.

Keep in mind that not all 3D objects are mesh files. A glTF file cannot be dragged and dropped into the graph. However, you can create an inherited scene from it, save the mesh in that scene as its own file, and use that. You can also drag and drop `.obj` files into the graph editor to add the node for that specific mesh.

---

## 7. Compute Shaders

A compute shader is a special type of shader program that is orientated towards general purpose programming. In other words, they are more flexible than vertex shaders and fragment shaders as they don't have a fixed purpose (i.e. transforming vertices or writing colors to an image). Unlike fragment shaders and vertex shaders, compute shaders have very little going on behind the scenes. The code you write is what the GPU runs and very little else. This can make them a very useful tool to offload heavy calculations to the GPU.

> **Note:** Compute shaders can only be used from RenderingDevice-based renderers (the Forward+ or Mobile renderer). Note that compute shader support is generally poor on mobile devices (due to driver bugs), even if they are technically supported.

### 7.1 Writing the Compute Shader

First, create a new file called `compute_example.glsl` in your project folder in an **external** text editor. When you write compute shaders in Godot, you write them in GLSL directly.

```glsl
#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 2, local_size_y = 1, local_size_z = 1) in;

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) restrict buffer MyDataBuffer {
    float data[];
}
my_data_buffer;

// The code we want to execute in each invocation
void main() {
    // gl_GlobalInvocationID.x uniquely identifies this invocation across all work groups
    my_data_buffer.data[gl_GlobalInvocationID.x] *= 2.0;
}
```

This code takes an array of floats, multiplies each element by 2 and stores the results back in the buffer array.

**Line-by-line explanation:**

`#[compute]` and `#version 450` -- These two lines communicate that the following code is a compute shader (Godot-specific hint) and that the code is using GLSL version 450. You should never have to change these two lines.

`layout(local_size_x = 2, local_size_y = 1, local_size_z = 1) in;` -- Communicates the number of invocations to be used in each workgroup. Invocations are instances of the shader that are running within the same workgroup. When we launch a compute shader from the CPU, we tell it how many workgroups to run. Workgroups run in parallel to each other. While running one workgroup, you cannot access information in another workgroup. However, invocations in the same workgroup can have some limited access to other invocations.

Think about workgroups and invocations as a giant nested `for` loop:

```
for (x in workgroup_size_x):
    for (y in workgroup_size_y):
        for (z in workgroup_size_z):
            // Each workgroup runs independently and in parallel.
            for (local_x in invocation_size_x):
                for (local_y in invocation_size_y):
                    for (local_z in invocation_size_z):
                        // Compute shader runs here.
```

The `layout` property with `set` and `binding` positions tells the shader where to look for the buffer -- we will need to match these positions from the CPU side later. The `restrict` keyword tells the shader that this buffer is only going to be accessed from one place in this shader. Always use `restrict` when you can.

`gl_GlobalInvocationID` gives you the global unique ID for the current invocation.

### 7.2 Create a Local RenderingDevice

To interact with and execute a compute shader, we need a script. Create a new script and attach it to any Node in your scene.

```csharp
// Create a local rendering device.
var rd = RenderingServer.CreateLocalRenderingDevice();
```

After that, we can load the newly created shader file and create a precompiled version of it:

```csharp
// Load GLSL shader
var shaderFile = GD.Load<RDShaderFile>("res://compute_example.glsl");
var shaderBytecode = shaderFile.GetSpirV();
var shader = rd.ShaderCreateFromSpirV(shaderBytecode);
```

> **Warning:** Local RenderingDevices cannot be debugged using tools such as RenderDoc.

### 7.3 Provide Input Data

We need to create a buffer to pass values to a compute shader. We are dealing with an array of floats, so we will use a storage buffer. A storage buffer takes an array of bytes and allows the CPU to transfer data to and from the GPU.

```csharp
// Prepare our data. We use floats in the shader, so we need 32 bit.
float[] input = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var inputBytes = new byte[input.Length * sizeof(float)];
Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

// Create a storage buffer that can hold our float values.
// Each float has 4 bytes (32 bit) so 10 x 4 = 40 bytes
var buffer = rd.StorageBufferCreate((uint)inputBytes.Length, inputBytes);
```

With the buffer in place we need to tell the rendering device to use this buffer. To do that we will need to create a uniform and assign it to a uniform set:

```csharp
// Create a uniform to assign the buffer to the rendering device
var uniform = new RDUniform
{
    UniformType = RenderingDevice.UniformType.StorageBuffer,
    Binding = 0 // This needs to match the "binding" in our shader file
};
uniform.AddId(buffer);
var uniformSet = rd.UniformSetCreate(new Godot.Collections.Array<RDUniform> { uniform }, shader, 0);
// The last parameter (the 0) needs to match the "set" in our shader file
```

### 7.4 Defining a Compute Pipeline

The steps we need to do to compute our result are:
1. Create a new pipeline.
2. Begin a list of instructions for our GPU to execute.
3. Bind our compute list to our pipeline.
4. Bind our buffer uniform to our pipeline.
5. Specify how many workgroups to use.
6. End the list of instructions.

```csharp
// Create a compute pipeline
var pipeline = rd.ComputePipelineCreate(shader);
var computeList = rd.ComputeListBegin();
rd.ComputeListBindComputePipeline(computeList, pipeline);
rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
rd.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
rd.ComputeListEnd();
```

Note that we are dispatching the compute shader with 5 work groups in the X axis, and one in the others. Since we have 2 local invocations in the X axis (specified in our shader), 10 compute shader invocations will be launched in total. If you read or write to indices outside of the range of your buffer, you may access memory outside of your shader's control or parts of other variables which may cause issues on some hardware.

### 7.5 Execute a Compute Shader

To execute our compute shader we need to submit the pipeline to the GPU and wait for the execution to finish:

```csharp
// Submit to GPU and wait for sync
rd.Submit();
rd.Sync();
```

Ideally, you would not call `Sync()` to synchronize the RenderingDevice right away as it will cause the CPU to wait for the GPU to finish working. In general, you will want to wait *at least* 2 or 3 frames before synchronizing so that the GPU is able to run in parallel with the CPU.

> **Warning:** Long computations can cause Windows graphics drivers to "crash" due to TDR being triggered by Windows. This is a mechanism that reinitializes the graphics driver after a certain amount of time has passed without any activity from the graphics driver (usually 5 to 10 seconds). Depending on the duration your compute shader takes to execute, you may need to split it into multiple dispatches to reduce the time each dispatch takes and reduce the chances of triggering a TDR. Slower GPUs may be more prone to TDRs when running a given compute shader compared to a faster GPU.

### 7.6 Retrieving Results

```csharp
// Read back the data from the buffer
var outputBytes = rd.BufferGetData(buffer);
var output = new float[input.Length];
Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
GD.Print("Input: ", string.Join(", ", input));
GD.Print("Output: ", string.Join(", ", output));
```

### 7.7 Freeing Memory

The `buffer`, `pipeline`, and `uniformSet` variables we've been using are each an RID. Because `RenderingDevice` is meant to be a lower-level API, RIDs are not freed automatically. This means that once you are done using `buffer` or any other RID, you are responsible for freeing its memory manually using the `RenderingDevice`'s `FreeRid()` method.

> **See Also:** The demo projects repository contains a **Compute Shader Heightmap demo**. This project performs heightmap image generation on the CPU and GPU separately, which lets you compare how a similar algorithm can be implemented in two different ways (with the GPU implementation being faster in most cases).

---

## 8. Advanced Post-Processing

### 8.1 Introduction

This describes an advanced method for post-processing in Godot. In particular, it will explain how to write a post-processing shader that uses the depth buffer.

### 8.2 Full Screen Quad

One way to make custom post-processing effects is by using a viewport. However, there are two main drawbacks:
- The depth buffer cannot be accessed.
- The effect of the post-processing shader is not visible in the editor.

To get around the limitation on using the depth buffer, use a `MeshInstance3D` with a `QuadMesh` primitive. This allows us to use a shader and to access the depth texture of the scene. Next, use a vertex shader to make the quad cover the screen at all times so that the post-processing effect will be applied at all times, including in the editor.

First, create a new `MeshInstance3D` and set its mesh to a `QuadMesh`. This creates a quad centered at position `(0, 0, 0)` with a width and height of `1`. Set the width and height to `2` and enable **Flip Faces**. Right now, the quad occupies a position in world space at the origin. However, we want it to move with the camera so that it always covers the entire screen. To do this, we will bypass the coordinate transforms that translate the vertex positions through the different coordinate spaces and treat the vertices as if they were already in clip space.

The vertex shader expects coordinates to be output in clip space, which are coordinates ranging from `-1` at the left and bottom of the screen to `1` at the top and right of the screen. This is why the `QuadMesh` needs to have height and width of `2`. Godot handles the transform from model to view space to clip space behind the scenes, so we need to nullify the effects of Godot's transformations. We do this by setting the `POSITION` built-in to our desired position. `POSITION` bypasses the built-in transformations and sets the vertex position in clip space directly.

```glsl
shader_type spatial;
// Prevent the quad from being affected by lighting and fog. This also improves performance.
render_mode unshaded, fog_disabled;

void vertex() {
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}
```

> **Note:** In versions of Godot earlier than 4.3, this code recommended using `POSITION = vec4(VERTEX, 1.0);` which implicitly assumed the clip-space near plane was at 0.0. That code is now incorrect and will not work in versions 4.3+ as we use a "reversed-z" depth buffer now where the near plane is at 1.0.

Even with this vertex shader, the quad keeps disappearing. This is due to frustum culling, which is done on the CPU. Frustum culling uses the camera matrix and the AABBs of Meshes to determine if the Mesh will be visible *before* passing it to the GPU. To keep the quad from being culled, there are a few options:
- Add the `QuadMesh` as a child to the camera, so the camera is always pointed at it.
- Set the Geometry property `extra_cull_margin` as large as possible in the `QuadMesh`.

The second option ensures that the quad is visible in the editor, while the first option guarantees that it will still be visible even if the camera moves outside the cull margin. You can also use both options.

### 8.3 Depth Texture

To read from the depth texture, we first need to create a texture uniform set to the depth buffer by using `hint_depth_texture`:

```glsl
uniform sampler2D depth_texture : hint_depth_texture;
```

Once defined, the depth texture can be read with the `texture()` function:

```glsl
float depth = texture(depth_texture, SCREEN_UV).x;
```

> **Note:** Similar to accessing the screen texture, accessing the depth texture is only possible when reading from the current viewport. The depth texture cannot be accessed from another viewport to which you have rendered.

The values returned by `depth_texture` are between `1.0` and `0.0` (corresponding to the near and far plane, respectively, because of using a "reverse-z" depth buffer) and are nonlinear. When displaying depth directly from the `depth_texture`, everything will look almost black unless it is very close due to that nonlinearity. In order to make the depth value align with world or model coordinates, we need to linearize the value.

Firstly, take the screen space coordinates and transform them into normalized device coordinates (NDC). NDC run `-1.0` to `1.0` in `x` and `y` directions and from `0.0` to `1.0` in the `z` direction when using the Vulkan backend. Reconstruct the NDC using `SCREEN_UV` for the `x` and `y` axis, and the depth value for `z`.

```glsl
void fragment() {
    float depth = texture(depth_texture, SCREEN_UV).x;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
}
```

> **Note:** This tutorial assumes the use of the Forward+ or Mobile renderers, which both use Vulkan NDCs with a Z-range of [0.0, 1.0]. In contrast, the Compatibility renderer uses OpenGL NDCs with a Z-range of [-1.0, 1.0]. For the Compatibility renderer, replace the NDC calculation with:
> ```glsl
> vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
> ```
> You can also use the `CURRENT_RENDERER` and `RENDERER_COMPATIBILITY` built-in defines for a shader that will work in all renderers:
> ```glsl
> #if CURRENT_RENDERER == RENDERER_COMPATIBILITY
> vec3 ndc = vec3(SCREEN_UV, depth) * 2.0 - 1.0;
> #else
> vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
> #endif
> ```

Convert NDC to view space by multiplying the NDC by `INV_PROJECTION_MATRIX`. View space gives positions relative to the camera, so the `z` value will give us the distance to the point.

```glsl
void fragment() {
    // ...
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float linear_depth = -view.z;
}
```

Because the camera is facing the negative `z` direction, the position will have a negative `z` value. In order to get a usable depth value, we have to negate `view.z`.

The world position can be constructed from the depth buffer using the following code, using the `INV_VIEW_MATRIX` to transform the position from view space into world space:

```glsl
void fragment() {
    // ...
    vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    vec3 world_position = world.xyz / world.w;
}
```

### 8.4 Example Shader

This shader lets you visualize the linear depth or world space coordinates, depending on which line is commented out:

```glsl
shader_type spatial;
// Prevent the quad from being affected by lighting and fog. This also improves performance.
render_mode unshaded, fog_disabled;

uniform sampler2D depth_texture : hint_depth_texture;

void vertex() {
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
    float depth = texture(depth_texture, SCREEN_UV).x;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    view.xyz /= view.w;
    float linear_depth = -view.z;

    vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    vec3 world_position = world.xyz / world.w;

    // Visualize linear depth
    ALBEDO.rgb = vec3(fract(linear_depth));

    // Visualize world coordinates
    //ALBEDO.rgb = fract(world_position).xyz;
}
```

### 8.5 An Optimization

You can benefit from using a single large triangle rather than using a full screen quad. However, the benefit is quite small and only beneficial when running especially complex fragment shaders.

Set the Mesh in the `MeshInstance3D` to an `ArrayMesh`. An `ArrayMesh` is a tool that allows you to easily construct a Mesh from Arrays for vertices, normals, colors, etc.

Attach a script to the `MeshInstance3D` and use the following code:

```csharp
using Godot;

public partial class FullScreenTriangle : MeshInstance3D
{
    public override void _Ready()
    {
        // Create a single triangle out of vertices:
        var verts = new Vector3[]
        {
            new Vector3(-1.0f, -1.0f, 0.0f),
            new Vector3(3.0f, -1.0f, 0.0f),
            new Vector3(-1.0f, 3.0f, 0.0f)
        };

        // Create an array of arrays.
        // This could contain normals, colors, UVs, etc.
        var meshArray = new Godot.Collections.Array();
        meshArray.Resize((int)Mesh.ArrayType.Max); // Required size for ArrayMesh Array
        meshArray[(int)Mesh.ArrayType.Vertex] = verts; // Position of vertex array

        // Create mesh from meshArray:
        var arrayMesh = Mesh as ArrayMesh;
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshArray);
    }
}
```

> **Note:** The triangle is specified in normalized device coordinates. NDC run from -1.0 to 1.0 in both the x and y directions. This makes the screen 2 units wide and 2 units tall. In order to cover the entire screen with a single triangle, use a triangle that is 4 units wide and 4 units tall, double its height and width.

The one drawback to using an `ArrayMesh` over using a `QuadMesh` is that the `ArrayMesh` is not visible in the editor because the triangle is not constructed until the scene is run. To get around that, construct a single triangle Mesh in a modeling program and use that in the `MeshInstance3D` instead.

---

## 9. Using a SubViewport as a Texture

### 9.1 Introduction

This section introduces using the `SubViewport` as a texture that can be applied to 3D objects. It walks through the process of making a procedural planet.

This assumes you are familiar with how to set up a basic scene including: a `Camera3D`, a light source, a `MeshInstance3D` with a Primitive Mesh, and applying a `StandardMaterial3D` to the mesh.

Topics covered:
- How to use a SubViewport as a render texture
- Mapping a texture to a sphere with equirectangular mapping
- Fragment shader techniques for procedural planets
- Setting a Roughness map from a Viewport Texture

### 9.2 Setting Up the Scene

Create a new scene and add the following nodes:
- Root node
  - `MeshInstance3D` (with `SphereMesh`)
  - `SubViewport`
    - `ColorRect`

### 9.3 Setting Up the SubViewport

Click on the `SubViewport` node and set its size to `(1024, 512)`. The SubViewport can actually be any size so long as the width is double the height. The width needs to be double the height so that the image will accurately map onto the sphere, as we will be using equirectangular projection.

Next disable 3D. We will be using a `ColorRect` to render the surface.

Select the `ColorRect` and in the inspector set the anchors preset to **Full Rect**. This will ensure that the `ColorRect` takes up the entire `SubViewport`.

Next, add a `ShaderMaterial` to the `ColorRect` (ColorRect > CanvasItem > Material > Material > New ShaderMaterial).

Create a new shader and add the following:

```glsl
shader_type canvas_item;

void fragment() {
    COLOR = vec4(UV.x, UV.y, 0.5, 1.0);
}
```

### 9.4 Applying the Texture

Go into the `MeshInstance3D` and add a `StandardMaterial3D`:

MeshInstance3D > GeometryInstance > Geometry > Material Override > New StandardMaterial3D

Go to the **Resource** section and check the **Local to scene** box. Then, go to the **Albedo** section and click beside the **Texture** property to add an Albedo Texture. Choose **New ViewportTexture**.

Click on the ViewportTexture you just created in the inspector, then click **Assign**. Then, from the menu that pops up, select the Viewport that we rendered to earlier.

Notice the ugly seam that forms where the texture wraps around. This is because we are picking a color based on UV coordinates and UV coordinates do not wrap around the texture. This is a classic problem in 2D map projection.

### 9.5 Making the Planet Texture

To get a range of coordinates that wrap around the sphere in a nice way, use functions that repeat on the domain of our texture. `sin` and `cos` are two such functions.

```glsl
COLOR.xyz = vec3(sin(UV.x * 3.14159 * 4.0) * cos(UV.y * 3.14159 * 4.0) * 0.5 + 0.5);
```

The seam has now disappeared, but there is pinching at the poles. This pinching is due to the way Godot maps textures to spheres using equirectangular projection.

For each pixel, we calculate its 3D position on the sphere. From that, we use 3D noise to determine a color value. By calculating the noise in 3D, we solve the problem of the pinching at the poles. The following code converts the UVs into Cartesian coordinates:

```glsl
float theta = UV.y * 3.14159;
float phi = UV.x * 3.14159 * 2.0;
vec3 unit = vec3(0.0, 0.0, 0.0);

unit.x = sin(phi) * sin(theta);
unit.y = cos(theta) * -1.0;
unit.z = cos(phi) * sin(theta);
unit = normalize(unit);
```

3D noise function (from Shadertoy, by Inigo Quilez, published under the MIT licence):

```glsl
vec3 hash(vec3 p) {
    p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
             dot(p, vec3(269.5, 183.3, 246.1)),
             dot(p, vec3(113.5, 271.9, 124.6)));

    return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

float noise(vec3 p) {
    vec3 i = floor(p);
    vec3 f = fract(p);
    vec3 u = f * f * (3.0 - 2.0 * f);

    return mix(mix(mix(dot(hash(i + vec3(0.0, 0.0, 0.0)), f - vec3(0.0, 0.0, 0.0)),
                       dot(hash(i + vec3(1.0, 0.0, 0.0)), f - vec3(1.0, 0.0, 0.0)), u.x),
                   mix(dot(hash(i + vec3(0.0, 1.0, 0.0)), f - vec3(0.0, 1.0, 0.0)),
                       dot(hash(i + vec3(1.0, 1.0, 0.0)), f - vec3(1.0, 1.0, 0.0)), u.x), u.y),
               mix(mix(dot(hash(i + vec3(0.0, 0.0, 1.0)), f - vec3(0.0, 0.0, 1.0)),
                       dot(hash(i + vec3(1.0, 0.0, 1.0)), f - vec3(1.0, 0.0, 1.0)), u.x),
                   mix(dot(hash(i + vec3(0.0, 1.0, 1.0)), f - vec3(0.0, 1.0, 1.0)),
                       dot(hash(i + vec3(1.0, 1.0, 1.0)), f - vec3(1.0, 1.0, 1.0)), u.x), u.y), u.z);
}
```

To use the noise:

```glsl
float n = noise(unit * 5.0);
COLOR.xyz = vec3(n * 0.5 + 0.5);
```

### 9.6 Coloring the Planet

To make a gradient between water and land, use the `mix` function. `mix` takes two values to interpolate between and a third argument to choose how much to interpolate between them. In other APIs, this function is often called `lerp`.

```glsl
COLOR.xyz = mix(vec3(0.05, 0.3, 0.5), vec3(0.9, 0.4, 0.1), n * 0.5 + 0.5);
```

For a relatively clear separation between land and sea, change the last term to `smoothstep(-0.1, 0.0, n)`:

```glsl
COLOR.xyz = mix(vec3(0.05, 0.3, 0.5), vec3(0.9, 0.4, 0.1), smoothstep(-0.1, 0.0, n));
```

`smoothstep` returns `0` if the third argument is below the first and `1` if the third argument is larger than the second and smoothly blends between `0` and `1` if the third number is between the first and the second.

To make the land edges rougher, layer levels of noise over one another at various frequencies:

```glsl
float n = noise(unit * 5.0) * 0.5;
n += noise(unit * 10.0) * 0.25;
n += noise(unit * 20.0) * 0.125;
n += noise(unit * 40.0) * 0.0625;
```

### 9.7 Making an Ocean

The ocean and the land reflect light differently. We want the ocean to shine a little more than the land. We can do this by passing a fourth value into the `alpha` channel of our output `COLOR` and using it as a Roughness map.

```glsl
COLOR.a = 0.3 + 0.7 * smoothstep(-0.1, 0.0, n);
```

This line returns `0.3` for water and `1.0` for land, meaning the land will be quite rough while the water will be quite smooth.

In the material, under the **Metallic** section, make sure `Metallic` is set to `0` and `Specular` is set to `1`. Under the **Roughness** section, set the roughness texture to a ViewportTexture pointing to our planet texture SubViewport. Finally, set the **Texture Channel** to **Alpha**.

By default, when something is rendered with an alpha value, it gets drawn as a transparent object over the background. To correct this, go into the SubViewport and enable the **Transparent Bg** property. Since we are now rendering one transparent object on top of another, we want to enable `blend_premul_alpha`:

```glsl
render_mode blend_premul_alpha;
```

This pre-multiplies the colors by the alpha value and then blends them correctly together.

---

## 10. Converting GLSL to Godot Shaders

This section explains the differences between Godot's shading language and GLSL and gives practical advice on how to migrate shaders from other sources, such as Shadertoy and The Book of Shaders, into Godot shaders.

### 10.1 GLSL

Godot uses a shading language based on GLSL with the addition of a few quality-of-life features. Accordingly, most features available in GLSL are available in Godot's shading language.

#### Shader Programs

In GLSL, each shader uses a separate program. You have one program for the vertex shader and one for the fragment shader. In Godot, you have a single shader that contains a `vertex` and/or a `fragment` function. If you only choose to write one, Godot will supply the other.

Godot allows uniform variables and functions to be shared by defining the fragment and vertex shaders in one file. In GLSL, the vertex and fragment programs cannot share variables except when varyings are used.

#### Vertex Attributes

In GLSL, you can pass in per-vertex information using attributes and have the flexibility to pass in as much or as little as you want. In Godot, you have a set number of input attributes, including `VERTEX` (position), `COLOR`, `UV`, `UV2`, `NORMAL`. Each shader's page in the shader reference section of the documentation comes with a complete list of its vertex attributes.

#### gl_Position

`gl_Position` receives the final position of a vertex specified in the vertex shader. It is specified by the user in clip space. In Godot, `VERTEX` specifies the vertex position in model space at the beginning of the `vertex` function. Godot also handles the final conversion to clip space after the user-defined `vertex` function is run. If you want to skip the conversion from model to view space, you can set the `render_mode` to `skip_vertex_transform`. If you want to skip all transforms, set `render_mode` to `skip_vertex_transform` and set the `PROJECTION_MATRIX` to `mat4(1.0)` in order to nullify the final transform from view space to clip space.

#### Varyings

Varyings are a type of variable that can be passed from the vertex shader to the fragment shader. In modern GLSL (3.0 and up), varyings are defined with the `in` and `out` keywords. A variable going out of the vertex shader is defined with `out` in the vertex shader and `in` inside the fragment shader.

#### Main

In GLSL, each shader program looks like a self-contained C-style program. The main entry point is `main`. If you are copying a vertex shader, rename `main` to `vertex` and if you are copying a fragment shader, rename `main` to `fragment`.

#### Macros

The Godot shader preprocessor supports the following macros:
- `#define` / `#undef`
- `#if`, `#elif`, `#else`, `#endif`, `defined()`, `#ifdef`, `#ifndef`
- `#include` (only `.gdshaderinc` files and with a maximum depth of 25)
- `#pragma disable_preprocessor`, which disables preprocessing for the rest of the file

#### Variables

GLSL has many built-in variables that are hard-coded. These variables are not uniforms, so they are not editable from the main program.

| GLSL Variable | Type | Godot Equivalent | Description |
|---|---|---|---|
| `gl_FragColor` | `out vec4` | `COLOR` | Output color for each pixel. |
| `gl_FragCoord` | `vec4` | `FRAGCOORD` | For full screen quads. For smaller quads, use UV. |
| `gl_Position` | `vec4` | `VERTEX` | Position of Vertex, output from Vertex Shader. |
| `gl_PointSize` | `float` | `POINT_SIZE` | Size of Point primitive. |
| `gl_PointCoord` | `vec2` | `POINT_COORD` | Position on point when drawing Point primitives. |
| `gl_FrontFacing` | `bool` | `FRONT_FACING` | True if front face of primitive. |

#### Coordinates

`gl_FragCoord` in GLSL and `FRAGCOORD` in the Godot shading language use the same coordinate system. If using UV in Godot, the y-coordinate will be flipped upside down.

#### Precision

In GLSL, you can define the precision of a given type (`float` or `int`) at the top of the shader with the `precision` keyword. In Godot, you can set the precision of individual variables as you need by placing precision qualifiers `lowp`, `mediump`, and `highp` before the type when defining the variable.

### 10.2 Shadertoy

Shadertoy is a website that makes it easy to write fragment shaders and create pure magic. Shadertoy does not give the user full control over the shader. It handles all the input and uniforms and only lets the user write the fragment shader.

#### Types

Shadertoy uses the webgl spec, so it runs a slightly different version of GLSL. However, it still has the regular types, including constants and macros.

#### mainImage

The main point of entry to a Shadertoy shader is the `mainImage` function. `mainImage` has two parameters, `fragColor` and `fragCoord`, which correspond to `COLOR` and `FRAGCOORD` in Godot, respectively. These parameters are handled automatically in Godot, so you do not need to include them as parameters yourself. Anything in the `mainImage` function should be copied into the `fragment` function when porting to Godot.

#### Variables

| Shadertoy Variable | Type | Godot Equivalent | Description |
|---|---|---|---|
| `fragColor` | `out vec4` | `COLOR` | Output color for each pixel. |
| `fragCoord` | `vec2` | `FRAGCOORD.xy` | For full screen quads. For smaller quads, use UV. |
| `iResolution` | `vec3` | `1.0 / SCREEN_PIXEL_SIZE` | Can also pass in manually. |
| `iTime` | `float` | `TIME` | Time since shader started. |
| `iTimeDelta` | `float` | Provide with Uniform | Time to render previous frame. |
| `iFrame` | `float` | Provide with Uniform | Frame number. |
| `iChannelTime[4]` | `float` | Provide with Uniform | Time since that particular texture started. |
| `iMouse` | `vec4` | Provide with Uniform | Mouse position in pixel coordinates. |
| `iDate` | `vec4` | Provide with Uniform | Current date, expressed in seconds. |
| `iChannelResolution[4]` | `vec3` | `1.0 / TEXTURE_PIXEL_SIZE` | Resolution of particular texture. |
| `iChanneli` | `Sampler2D` | `TEXTURE` | Godot provides only one built-in; user can make more. |

#### Coordinates

`fragCoord` behaves the same as `gl_FragCoord` in GLSL and `FRAGCOORD` in Godot.

### 10.3 The Book of Shaders

Similar to Shadertoy, The Book of Shaders provides access to a fragment shader in the web browser, with which the user may interact. The user is restricted to writing fragment shader code with a set list of uniforms passed in and with no ability to add additional uniforms.

#### Types

The Book of Shaders uses the webgl spec, so it runs a slightly different version of GLSL. However, it still has the regular types, including constants and macros.

#### Main

The entry point for a Book of Shaders fragment shader is `main`, just like in GLSL. Everything written in a Book of Shaders `main` function should be copied into Godot's `fragment` function.

#### Variables

| Book of Shaders Variable | Type | Godot Equivalent | Description |
|---|---|---|---|
| `gl_FragColor` | `out vec4` | `COLOR` | Output color for each pixel. |
| `gl_FragCoord` | `vec4` | `FRAGCOORD` | For full screen quads. For smaller quads, use UV. |
| `u_resolution` | `vec2` | `1.0 / SCREEN_PIXEL_SIZE` | Can also pass in manually. |
| `u_time` | `float` | `TIME` | Time since shader started. |
| `u_mouse` | `vec2` | Provide with Uniform | Mouse position in pixel coordinates. |

#### Coordinates

The Book of Shaders uses the same coordinate system as GLSL.
