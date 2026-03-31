# Example 03: Manual TSCN File Editing

## User Input
> "I need to add a torch with a light and particle effect to my dungeon scene. Can you edit the TSCN directly?"

## Expected Output

### Step 1: Analyze the existing TSCN

First, read the scene file to understand the current structure:

```ini
[gd_scene load_steps=3 format=3 uid="uid://dungeon123"]

[ext_resource type="PackedScene" path="res://scenes/characters/Player.tscn" id="1_player"]

[sub_resource type="BoxShape3D" id="BoxShape3D_floor"]
size = Vector3(20, 0.5, 20)

[node name="Dungeon" type="Node3D"]

[node name="Floor" type="StaticBody3D" parent="."]
[node name="CollisionShape3D" type="CollisionShape3D" parent="Floor"]
shape = SubResource("BoxShape3D_floor")
[node name="MeshInstance3D" type="MeshInstance3D" parent="Floor"]

[node name="Player" parent="." instance=ExtResource("1_player")]
position = Vector3(0, 1, 0)
```

### Step 2: Plan the additions

Torch needs:
- MeshInstance3D (torch model or simple cylinder)
- OmniLight3D (flickering light)
- GPUParticles3D (fire particles)
- Sub-resources for the mesh and particle material

### Step 3: Edit the TSCN

**Updated load_steps:** Was 3 (1 ext_resource + 1 sub_resource + 1). Adding 4 sub-resources → 3 + 4 = 7.

```ini
[gd_scene load_steps=7 format=3 uid="uid://dungeon123"]

[ext_resource type="PackedScene" path="res://scenes/characters/Player.tscn" id="1_player"]

[sub_resource type="BoxShape3D" id="BoxShape3D_floor"]
size = Vector3(20, 0.5, 20)

[sub_resource type="CylinderMesh" id="CylinderMesh_torch"]
top_radius = 0.05
bottom_radius = 0.08
height = 0.8

[sub_resource type="StandardMaterial3D" id="Material_torch"]
albedo_color = Color(0.4, 0.25, 0.1, 1)

[sub_resource type="ParticleProcessMaterial" id="ParticleMat_fire"]
direction = Vector3(0, 1, 0)
spread = 15.0
gravity = Vector3(0, 2, 0)
initial_velocity_min = 0.5
initial_velocity_max = 1.5
scale_min = 0.1
scale_max = 0.3
color = Color(1, 0.5, 0, 1)

[node name="Dungeon" type="Node3D"]

[node name="Floor" type="StaticBody3D" parent="."]
[node name="CollisionShape3D" type="CollisionShape3D" parent="Floor"]
shape = SubResource("BoxShape3D_floor")
[node name="MeshInstance3D" type="MeshInstance3D" parent="Floor"]

[node name="Player" parent="." instance=ExtResource("1_player")]
position = Vector3(0, 1, 0)

[node name="Torches" type="Node3D" parent="."]

[node name="Torch1" type="Node3D" parent="Torches"]
position = Vector3(5, 1.5, 3)

[node name="TorchMesh" type="MeshInstance3D" parent="Torches/Torch1"]
mesh = SubResource("CylinderMesh_torch")
surface_material_override/0 = SubResource("Material_torch")

[node name="FireLight" type="OmniLight3D" parent="Torches/Torch1"]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.5, 0)
light_color = Color(1, 0.7, 0.3, 1)
light_energy = 2.0
omni_range = 8.0
omni_attenuation = 1.5

[node name="FireParticles" type="GPUParticles3D" parent="Torches/Torch1"]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.4, 0)
amount = 30
lifetime = 0.8
process_material = SubResource("ParticleMat_fire")

[node name="Torch2" type="Node3D" parent="Torches"]
position = Vector3(-5, 1.5, 3)

[node name="TorchMesh" type="MeshInstance3D" parent="Torches/Torch2"]
mesh = SubResource("CylinderMesh_torch")
surface_material_override/0 = SubResource("Material_torch")

[node name="FireLight" type="OmniLight3D" parent="Torches/Torch2"]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.5, 0)
light_color = Color(1, 0.7, 0.3, 1)
light_energy = 2.0
omni_range = 8.0
omni_attenuation = 1.5

[node name="FireParticles" type="GPUParticles3D" parent="Torches/Torch2"]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.4, 0)
amount = 30
lifetime = 0.8
process_material = SubResource("ParticleMat_fire")
```

### Better Approach: Make Torch a Reusable Scene

Instead of duplicating the torch nodes, create `Torch.tscn` as a reusable component:

**scenes/environment/Torch.tscn:**
```ini
[gd_scene load_steps=5 format=3 uid="uid://torch_comp"]

[ext_resource type="Script" path="res://scripts/environment/Torch.cs" id="1_script"]

[sub_resource type="CylinderMesh" id="CylinderMesh_torch"]
top_radius = 0.05
bottom_radius = 0.08
height = 0.8

[sub_resource type="StandardMaterial3D" id="Material_torch"]
albedo_color = Color(0.4, 0.25, 0.1, 1)

[sub_resource type="ParticleProcessMaterial" id="ParticleMat_fire"]
direction = Vector3(0, 1, 0)
spread = 15.0
gravity = Vector3(0, 2, 0)
initial_velocity_min = 0.5
initial_velocity_max = 1.5
color = Color(1, 0.5, 0, 1)

[node name="Torch" type="Node3D"]
script = ExtResource("1_script")
LightColor = Color(1, 0.7, 0.3, 1)
LightEnergy = 2.0
LightRange = 8.0
FlickerSpeed = 3.0
FlickerIntensity = 0.3

[node name="TorchMesh" type="MeshInstance3D" parent="."]
mesh = SubResource("CylinderMesh_torch")
surface_material_override/0 = SubResource("Material_torch")

[node name="FireLight" type="OmniLight3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.5, 0)
light_color = Color(1, 0.7, 0.3, 1)
light_energy = 2.0
omni_range = 8.0

[node name="FireParticles" type="GPUParticles3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0.4, 0)
amount = 30
lifetime = 0.8
process_material = SubResource("ParticleMat_fire")
```

**scripts/environment/Torch.cs:**
```csharp
using Godot;

public partial class Torch : Node3D
{
    [Export] public Color LightColor { get; set; } = new Color(1, 0.7f, 0.3f);
    [Export] public float LightEnergy { get; set; } = 2.0f;
    [Export] public float LightRange { get; set; } = 8.0f;
    [Export] public float FlickerSpeed { get; set; } = 3.0f;
    [Export] public float FlickerIntensity { get; set; } = 0.3f;

    private OmniLight3D _light;
    private float _baseEnergy;
    private float _time;

    public override void _Ready()
    {
        _light = GetNode<OmniLight3D>("FireLight");
        _light.LightColor = LightColor;
        _light.LightEnergy = LightEnergy;
        _light.OmniRange = LightRange;
        _baseEnergy = LightEnergy;
    }

    public override void _Process(double delta)
    {
        // Organic flicker effect
        _time += (float)delta * FlickerSpeed;
        float noise = Mathf.Sin(_time * 7.3f) * 0.5f + Mathf.Sin(_time * 13.1f) * 0.3f;
        _light.LightEnergy = _baseEnergy + noise * FlickerIntensity;
    }
}
```

**Then in Dungeon.tscn — much cleaner:**
```ini
[gd_scene load_steps=4 format=3 uid="uid://dungeon123"]

[ext_resource type="PackedScene" path="res://scenes/characters/Player.tscn" id="1_player"]
[ext_resource type="PackedScene" path="res://scenes/environment/Torch.tscn" id="2_torch"]

[sub_resource type="BoxShape3D" id="BoxShape3D_floor"]
size = Vector3(20, 0.5, 20)

[node name="Dungeon" type="Node3D"]

[node name="Floor" type="StaticBody3D" parent="."]
[node name="CollisionShape3D" type="CollisionShape3D" parent="Floor"]
shape = SubResource("BoxShape3D_floor")

[node name="Player" parent="." instance=ExtResource("1_player")]
position = Vector3(0, 1, 0)

[node name="Torches" type="Node3D" parent="."]

[node name="Torch1" parent="Torches" instance=ExtResource("2_torch")]
position = Vector3(5, 1.5, 3)

[node name="Torch2" parent="Torches" instance=ExtResource("2_torch")]
position = Vector3(-5, 1.5, 3)
LightEnergy = 1.5
LightRange = 6.0
```

## Key Takeaways
- **Always prefer composition**: Extract reusable sub-scenes (Torch.tscn) instead of duplicating nodes
- **load_steps must match**: Count ext_resource + sub_resource + 1
- **Sub-resources are shared**: Both torches reuse the same mesh and material
- **Instanced scenes override properties**: Torch2 has different LightEnergy and LightRange
- **Flicker via code**: Simple sine-wave noise for organic torch light
