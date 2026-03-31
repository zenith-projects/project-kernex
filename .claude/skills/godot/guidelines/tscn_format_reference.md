# TSCN & TRES File Format Reference

Complete reference for Godot 4.6's text-based scene and resource file formats. Sourced from official documentation.

---

## TSCN File Structure

A `.tscn` (Text Scene) file is a human-readable, text-based representation of a Godot scene. It has six sections that MUST appear in this order:

```
1. [gd_scene]       — File header
2. [ext_resource]    — External file references (0 or more)
3. [sub_resource]    — Inline resources (0 or more)
4. [node]            — Scene tree nodes (1 or more)
5. [connection]      — Signal connections (0 or more)
6. [editable]        — Editable instance paths (0 or more)
```

---

## 1. File Header

```ini
[gd_scene format=3 uid="uid://cecaux1sm7mo0"]
```

| Parameter | Description |
|-----------|-------------|
| `format` | Always `3` for Godot 4.x (was `2` for Godot 3.x) |
| `uid` | Unique identifier string. Godot generates this. Used to track files across renames/moves. |
| `load_steps` | **DEPRECATED since Godot 4.6.** Scenes saved before 4.6 may have it; ignore if present. Do NOT include when writing new scenes. |

---

## 2. External Resources

External resources reference files outside this scene file.

```ini
[ext_resource type="Script" uid="uid://c4cp0al3ljsjv" path="res://scripts/Player.cs" id="1_abc"]
[ext_resource type="Texture2D" uid="uid://c082xolc76p8n" path="res://sprites/player.png" id="2_def"]
[ext_resource type="PackedScene" uid="uid://xyz123" path="res://scenes/HealthComponent.tscn" id="3_ghi"]
```

| Parameter | Description |
|-----------|-------------|
| `type` | Resource class: `Script`, `Texture2D`, `PackedScene`, `AudioStream`, `Material`, etc. |
| `uid` | Unique file identifier (Godot 4+). Optional but recommended. |
| `path` | File path relative to project root (`res://`) |
| `id` | Reference ID used within THIS file. Format: `"number_randomchars"` |

**Common resource types:**
- `Script` — .cs or .gd files
- `Texture2D` — .png, .jpg, .svg images
- `PackedScene` — .tscn scene files (for instancing)
- `AudioStream` — .wav, .ogg, .mp3 audio
- `Material` / `ShaderMaterial` — material resources
- `Font` — .ttf, .otf font files
- `SpriteFrames` — animation frames resource
- `Theme` — .tres theme files

**Referencing in nodes:** Use `ExtResource("id")`
```ini
[node name="Player" type="CharacterBody2D"]
script = ExtResource("1_abc")

[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("2_def")
```

---

## 3. Sub-Resources (Internal Resources)

Sub-resources are resources defined inline within the scene file.

```ini
[sub_resource type="CircleShape2D" id="CircleShape2D_abc"]
radius = 16.0

[sub_resource type="RectangleShape2D" id="RectangleShape2D_def"]
size = Vector2(32, 64)

[sub_resource type="StyleBoxFlat" id="StyleBoxFlat_ghi"]
bg_color = Color(0.2, 0.2, 0.2, 1)
corner_radius_top_left = 5
corner_radius_top_right = 5
corner_radius_bottom_left = 5
corner_radius_bottom_right = 5
```

| Parameter | Description |
|-----------|-------------|
| `type` | Resource class: `CircleShape2D`, `RectangleShape2D`, `StandardMaterial3D`, etc. |
| `id` | Unique ID within this file. Format: `"TypeName_randomchars"` |

Properties are listed on subsequent lines with `key = value` format.

**Referencing in nodes:** Use `SubResource("id")`
```ini
[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("CircleShape2D_abc")
```

**Common sub-resource types:**

Shapes:
- `CircleShape2D` — `radius = 16.0`
- `RectangleShape2D` — `size = Vector2(32, 64)`
- `CapsuleShape2D` — `radius = 16.0`, `height = 48.0`
- `WorldBoundaryShape2D` — infinite line
- `SphereShape3D` — `radius = 1.0`
- `BoxShape3D` — `size = Vector3(1, 1, 1)`
- `CapsuleShape3D` — `radius = 0.5`, `height = 2.0`

Materials:
- `StandardMaterial3D` — `albedo_color = Color(1, 0, 0, 1)`
- `ShaderMaterial` — `shader = ExtResource("shader_id")`

Meshes:
- `SphereMesh` — `radius = 1.0`, `height = 2.0`
- `BoxMesh` — `size = Vector3(1, 1, 1)`
- `PlaneMesh` — `size = Vector2(10, 10)`

Animations:
- `Animation` — keyframe data
- `AnimationLibrary` — collection of animations

---

## 4. Node Definitions

Nodes define the scene tree hierarchy.

### Root Node
```ini
[node name="Player" type="CharacterBody2D"]
script = ExtResource("1_abc")
collision_layer = 2
collision_mask = 1
```

The root node has no `parent` attribute.

### Child Nodes
```ini
[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("2_def")
position = Vector2(0, -16)

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("CircleShape2D_abc")
```

`parent="."` means direct child of root.

### Nested Children
```ini
[node name="Hand" type="Node2D" parent="Arm"]
[node name="Weapon" type="Sprite2D" parent="Arm/Hand"]
```

Parent paths are relative to the root node.

### Instanced Scenes
```ini
[node name="Enemy1" parent="Enemies" instance=ExtResource("enemy_scene")]
position = Vector2(100, 200)
speed = 150.0
```

`instance=ExtResource(...)` makes this node an instance of another scene. Properties listed override the instanced scene's defaults.

### Node Attributes

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `name` | string (quoted) | **Required.** Node name | `"Player"` |
| `type` | string (quoted) | Node class (omit for instanced scenes) | `"CharacterBody2D"` |
| `parent` | string (quoted) | Path to parent (omit for root) | `"."`, `"Arm/Hand"` |
| `instance` | ExtResource ref | Instance of packed scene | `ExtResource("id")` |
| `instance_placeholder` | string (quoted) | Lazy-loaded instance placeholder | `"res://heavy_scene.tscn"` |
| `unique_id` | integer (unquoted) | Unique node ID within scene (Godot 4.x) | `1358867382` |
| `owner` | string (quoted) | Node's owner path (relative to root) | `"."` |
| `groups` | array | Group memberships | `["enemies", "damageable"]` |
| `index` | integer (unquoted) | Sibling child order position | `0`, `1`, `2` |

### Property Value Types

```ini
# Basic types
name = "Player"                            # String
speed = 200.0                              # float
max_health = 100                           # int
is_active = true                           # bool

# Vector types
position = Vector2(100, 200)               # Vector2
scale = Vector2(1.5, 1.5)                 # Vector2
position = Vector3(0, 1, 0)               # Vector3

# Color
modulate = Color(1, 0.5, 0, 1)            # Color (RGBA, 0-1)
self_modulate = Color(1, 1, 1, 0.5)       # Semi-transparent

# Transform
transform = Transform2D(1, 0, 0, 1, 100, 200)  # 2D transform
transform = Transform3D(...)                      # 3D transform

# NodePath
target_path = NodePath("../Enemy")          # Path to another node

# Resource references
script = ExtResource("1_abc")              # External resource
shape = SubResource("CircleShape2D_xyz")   # Sub-resource
texture = null                             # Null/empty

# Arrays
collision_layer = 2                        # Bitmask as int
collision_mask = 5                         # Bitmask as int (1 + 4)

# Packed arrays
points = PackedVector2Array(0, 0, 100, 0, 100, 100)
```

---

## 5. Signal Connections

Signal connections are listed at the end of the file.

```ini
[connection signal="body_entered" from="HitboxArea" to="." method="OnBodyEntered"]
[connection signal="timeout" from="Timer" to="." method="OnTimerTimeout"]
[connection signal="pressed" from="UI/StartButton" to="." method="OnStartButtonPressed"]
[connection signal="area_entered" from="DetectionZone" to="." method="OnDetectionZoneAreaEntered" flags=3]
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `signal` | string (quoted) | Signal name (snake_case) |
| `from` | NodePath (quoted) | Path to emitting node (relative to root) |
| `to` | NodePath (quoted) | Path to receiving node (`.` = root) |
| `method` | string (quoted) | Method name (PascalCase for C#) |
| `flags` | integer (unquoted) | Optional. ConnectFlags bitmask |
| `binds` | array | Optional. Extra args bound to callback: `binds=[42, "hello"]` |
| `unbinds` | integer (unquoted) | Optional. Signal args to remove from end |

**Connection flags (bitmask values):**
- `1` = CONNECT_DEFERRED
- `2` = CONNECT_PERSIST
- `4` = CONNECT_ONE_SHOT
- `8` = CONNECT_REFERENCE_COUNTED
- Default when omitted = CONNECT_PERSIST (2)
- Most common in editor = `3` (DEFERRED | PERSIST)

```ini
[connection signal="pressed" from="Button" to="." method="OnPressed" flags=3]
[connection signal="hit" from="." to="." method="OnHit" binds=[42, "hello"]]
[connection signal="area_entered" from="Zone" to="." method="OnZone" unbinds=1]
```

---

## TRES File Format

`.tres` (Text Resource) files store resources in the same text format.

### Header
```ini
[gd_resource type="Resource" script_class="EnemyData" load_steps=2 format=3]
```

For custom resources with C# scripts:
```ini
[gd_resource type="Resource" script_class="EnemyData" load_steps=2 format=3]

[ext_resource type="Script" path="res://scripts/resources/EnemyData.cs" id="1_abc"]

[resource]
script = ExtResource("1_abc")
enemy_name = "Goblin"
max_health = 50
move_speed = 120.0
```

### Theme Resources (.tres)
```ini
[gd_resource type="Theme" load_steps=3 format=3]

[sub_resource type="StyleBoxFlat" id="StyleBoxFlat_normal"]
bg_color = Color(0.2, 0.2, 0.3, 1)
corner_radius_top_left = 8
corner_radius_top_right = 8
corner_radius_bottom_left = 8
corner_radius_bottom_right = 8

[sub_resource type="StyleBoxFlat" id="StyleBoxFlat_hover"]
bg_color = Color(0.3, 0.3, 0.4, 1)
corner_radius_top_left = 8
corner_radius_top_right = 8
corner_radius_bottom_left = 8
corner_radius_bottom_right = 8

[resource]
Button/styles/normal = SubResource("StyleBoxFlat_normal")
Button/styles/hover = SubResource("StyleBoxFlat_hover")
Button/font_colors/font_color = Color(0.9, 0.9, 0.9, 1)
```

---

## 6. Editable Instance Paths

The `[editable]` tag appears at the end of the file (after connections) and marks which instanced scene node paths can have their children edited/overridden in the parent scene.

```ini
[node name="Player" parent="." instance=ExtResource("1")]
position = Vector2(640, 400)

[node name="StateMachine" parent="Player" index="0"]
start_state = NodePath("Idle")

[editable path="Player"]
```

This allows modifying `Player`'s internal child nodes (like `StateMachine`) in the parent scene.

---

## Complete Variant Value Type Reference

Every property value in TSCN/TRES uses the Godot Variant text format:

### Primitives
```ini
null
true / false
42                                    # int
3.14                                  # float
"hello world"                         # String
&"StringName"                         # StringName (note & prefix)
```

### Math Types
```ini
Vector2(1.5, 2.5)
Vector2i(1, 2)
Vector3(1.0, 2.0, 3.0)
Vector3i(1, 2, 3)
Vector4(1.0, 2.0, 3.0, 4.0)
Vector4i(1, 2, 3, 4)
Rect2(0, 0, 100, 100)                # x, y, width, height
Rect2i(0, 0, 100, 100)
Plane(0, 1, 0, 0.5)                  # normal x, y, z, distance
Quaternion(0.191, -0.047, -0.008, 0.980)
AABB(0, 0, 0, 1, 1, 1)               # x, y, z, size_x, size_y, size_z
```

### Transform Types
```ini
Transform2D(1, 0, 0, 1, 0, 0)        # xx, xy, yx, yy, origin_x, origin_y
Basis(1, 0, 0, 0, 1, 0, 0, 0, 1)     # 9 floats, row-major
Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0)  # basis (9) + origin (3)
Projection(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)  # 4x4 matrix
```

### Color and Paths
```ini
Color(1, 0.5, 0, 1)                  # r, g, b, a (0.0 to 1.0)
NodePath("Path/To/Node")
NodePath("Node:property_name")
NodePath("")                          # null path
```

### Resource References
```ini
ExtResource("id_string")
SubResource("id_string")
```

### Collections
```ini
[value1, value2, value3]              # Array
["string1", "string2"]
Array[int]([1, 2, 3])                # Typed array
Dictionary[String, int]({"a": 1})    # Typed dictionary
{"key": value, "key2": value2}       # Dictionary
```

### Packed Arrays
```ini
PackedByteArray(0, 1, 255, 128)
PackedInt32Array(1, 2, 3, 4)
PackedInt64Array(1, 2, 3, 4)
PackedFloat32Array(0.0, 1.0, 1.5)
PackedFloat64Array(0.0, 1.0, 2.0)
PackedStringArray("one", "two")
PackedVector2Array(0, 0, 100, 0)      # pairs of x,y
PackedVector3Array(0, 0, 0, 1, 1, 1)  # triples of x,y,z
PackedVector4Array(0, 0, 0, 1)        # quads of x,y,z,w
PackedColorArray(1, 1, 1, 0.5)        # quads of r,g,b,a
```

---

## Scene Inheritance in TSCN

An inherited scene file references its base scene as an `ext_resource` and uses `instance=` on the root node:

```ini
[gd_scene format=3 uid="uid://derived_uid"]

[ext_resource type="PackedScene" uid="uid://base_uid" path="res://scenes/BaseEnemy.tscn" id="1"]
[ext_resource type="Texture2D" path="res://sprites/goblin.png" id="2_tex"]

[node name="Goblin" instance=ExtResource("1")]
MoveSpeed = 120.0
MaxHealth = 50

[node name="Sprite2D" parent="." index="0"]
texture = ExtResource("2_tex")

[node name="NewAbility" type="Node" parent="."]
script = ExtResource("3_ability")

[editable path="."]
```

- Only overridden properties appear in the derived file
- New nodes can be added
- `[editable path="."]` marks the root as editable
- Changes to the base scene auto-propagate EXCEPT for overridden properties

---

## Complete TSCN Example (Godot 4.6)

A full player character scene with C# script:

```ini
[gd_scene format=3 uid="uid://player123"]

[ext_resource type="Script" uid="uid://abc1" path="res://scripts/Player.cs" id="1_script"]
[ext_resource type="Texture2D" uid="uid://abc2" path="res://assets/sprites/player.png" id="2_texture"]
[ext_resource type="PackedScene" uid="uid://abc3" path="res://scenes/components/HealthComponent.tscn" id="3_health"]

[sub_resource type="CapsuleShape2D" id="CapsuleShape2D_abc"]
radius = 12.0
height = 32.0

[node name="Player" type="CharacterBody2D"]
collision_layer = 2
collision_mask = 1
script = ExtResource("1_script")
Speed = 200.0
JumpForce = -400.0

[node name="Sprite2D" type="Sprite2D" parent="."]
texture = ExtResource("2_texture")
offset = Vector2(0, -16)

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
position = Vector2(0, -16)
shape = SubResource("CapsuleShape2D_abc")

[node name="HealthComponent" parent="." instance=ExtResource("3_health")]
MaxHealth = 100

[node name="Camera2D" type="Camera2D" parent="."]
position_smoothing_enabled = true
position_smoothing_speed = 5.0

[connection signal="Died" from="HealthComponent" to="." method="OnHealthComponentDied"]
[connection signal="HealthChanged" from="HealthComponent" to="." method="OnHealthChanged"]
```

---

## Manually Editing TSCN Files

### Rules for Safe Manual Editing

1. **Never edit while Godot has the scene open** — close the scene in editor first, or use Godot's "reload" feature after editing
2. **Keep load_steps accurate** — Count all ext_resource + sub_resource + 1
3. **Use unique IDs** — No two resources or sub-resources can share an ID
4. **Maintain parent path consistency** — If you rename a node, update all `parent` references to it
5. **Preserve ordering** — ext_resource → sub_resource → nodes → connections
6. **Properties at defaults are stripped** — When Godot re-saves, it removes properties equal to defaults

### Common Manual Edits

**Adding a new node:**
```ini
# 1. If it needs a script, add ext_resource at the top
[ext_resource type="Script" path="res://scripts/NewScript.cs" id="new_id"]

# 2. Update load_steps in header (increment by number of new resources)

# 3. Add the node in the correct position (after its parent)
[node name="NewNode" type="Node2D" parent="."]
script = ExtResource("new_id")
```

**Removing a node:**
1. Remove the `[node]` block
2. Remove any `[connection]` blocks referencing it
3. Remove any `ext_resource` or `sub_resource` only used by it
4. Update `load_steps` accordingly
5. Update any `parent` paths that referenced this node

**Changing node hierarchy:**
```ini
# Move "Weapon" from being child of root to child of "Hand"
# Before:
[node name="Weapon" type="Sprite2D" parent="."]

# After:
[node name="Weapon" type="Sprite2D" parent="Hand"]
```

**Adding a signal connection:**
```ini
# Add at the end of the file, in the [connection] section
[connection signal="area_entered" from="DetectionArea" to="." method="OnDetectionAreaEntered"]
```

---

## ID Generation

Godot generates IDs in this format:
- ext_resource: `"number_randomchars"` (e.g., `"1_abc"`, `"2_def"`)
- sub_resource: `"TypeName_randomchars"` (e.g., `"CircleShape2D_xyz"`, `"Animation_abc"`)

When manually creating IDs, follow the same pattern. The randomchars portion can be any alphanumeric string — just ensure uniqueness within the file.

---

## Bitmask Values for Collision Layers

Collision layers and masks use integer bitmask values:

| Layer | Bitmask Value |
|-------|--------------|
| Layer 1 | 1 |
| Layer 2 | 2 |
| Layer 3 | 4 |
| Layer 4 | 8 |
| Layer 5 | 16 |
| Layer 6 | 32 |
| Layer 7 | 64 |
| Layer 8 | 128 |

To combine layers: add the values.
- Layers 1+3 = 1+4 = `5`
- Layers 1+2+6 = 1+2+32 = `35`
- Layers 1+3+6 = 1+4+32 = `37`

```ini
[node name="Player" type="CharacterBody2D"]
collision_layer = 2      # Player is on layer 2
collision_mask = 37       # Detects layers 1 (world) + 3 (enemies) + 6 (pickups)
```
