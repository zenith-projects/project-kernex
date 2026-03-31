# Example 07: Custom Resource System

## User Input
> "How do I use Custom Resources to define game data like weapons, enemies, and abilities?"

## Expected Output

Custom Resources are Godot's data containers — they separate DATA from BEHAVIOR. Define the data class in C#, create .tres files for each item, and reference them from scenes via [Export].

---

### Weapon Data Resource

```csharp
// scripts/resources/WeaponData.cs
using Godot;

[GlobalClass]
public partial class WeaponData : Resource
{
    [ExportGroup("Identity")]
    [Export] public string WeaponId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "Weapon";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    [ExportGroup("Combat Stats")]
    [Export(PropertyHint.Range, "1,999,1")] public int BaseDamage { get; set; } = 10;
    [Export(PropertyHint.Range, "0.1,5.0,0.1")] public float AttackSpeed { get; set; } = 1.0f;
    [Export(PropertyHint.Range, "0.5,10.0,0.5")] public float Range { get; set; } = 2.0f;
    [Export] public float KnockbackForce { get; set; } = 5f;
    [Export] public DamageType Type { get; set; } = DamageType.Physical;

    [ExportGroup("Visuals")]
    [Export] public PackedScene WeaponModel { get; set; }
    [Export] public PackedScene HitEffect { get; set; }
    [Export] public PackedScene SwingTrail { get; set; }

    [ExportGroup("Audio")]
    [Export] public AudioStream SwingSound { get; set; }
    [Export] public AudioStream HitSound { get; set; }
}

public enum DamageType { Physical, Magic, Fire, Ice, Lightning }
```

### Saved as .tres:

**resources/data/weapons/iron_sword.tres:**
```ini
[gd_resource type="Resource" script_class="WeaponData" load_steps=4 format=3]

[ext_resource type="Script" path="res://scripts/resources/WeaponData.cs" id="1_script"]
[ext_resource type="Texture2D" path="res://assets/sprites/items/iron_sword.png" id="2_icon"]
[ext_resource type="AudioStream" path="res://assets/audio/sfx/sword_swing.wav" id="3_swing"]

[resource]
script = ExtResource("1_script")
WeaponId = "iron_sword"
DisplayName = "Iron Sword"
Description = "A sturdy iron blade, reliable and sharp."
Icon = ExtResource("2_icon")
BaseDamage = 15
AttackSpeed = 1.2
Range = 2.0
KnockbackForce = 6.0
Type = 0
SwingSound = ExtResource("3_swing")
```

---

### Enemy Data Resource

```csharp
// scripts/resources/EnemyData.cs
using Godot;

[GlobalClass]
public partial class EnemyData : Resource
{
    [ExportGroup("Identity")]
    [Export] public string EnemyId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "Enemy";

    [ExportGroup("Stats")]
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public float MoveSpeed { get; set; } = 3.0f;
    [Export] public float DetectionRange { get; set; } = 10f;
    [Export] public float AttackRange { get; set; } = 2f;
    [Export] public float AttackCooldown { get; set; } = 1.5f;

    [ExportGroup("Loot")]
    [Export] public int MinGold { get; set; }
    [Export] public int MaxGold { get; set; }
    [Export] public LootEntry[] LootTable { get; set; }

    [ExportGroup("Visuals")]
    [Export] public PackedScene Model { get; set; }
    [Export] public PackedScene DeathEffect { get; set; }
    [Export] public Color TintColor { get; set; } = Colors.White;
}

// Nested resource for loot entries
[GlobalClass]
public partial class LootEntry : Resource
{
    [Export] public string ItemId { get; set; } = "";
    [Export(PropertyHint.Range, "0,100,1")] public float DropChance { get; set; } = 10f;
    [Export] public int MinQuantity { get; set; } = 1;
    [Export] public int MaxQuantity { get; set; } = 1;
}
```

---

### Ability Data Resource

```csharp
// scripts/resources/AbilityData.cs
using Godot;

[GlobalClass]
public partial class AbilityData : Resource
{
    [Export] public string AbilityId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "Ability";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    [ExportGroup("Cost")]
    [Export] public float StaminaCost { get; set; }
    [Export] public float ManaCost { get; set; }
    [Export] public float Cooldown { get; set; } = 1.0f;

    [ExportGroup("Effect")]
    [Export] public float Damage { get; set; }
    [Export] public float HealAmount { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public float Radius { get; set; }
    [Export] public AbilityTargetType TargetType { get; set; }

    [ExportGroup("Visuals")]
    [Export] public PackedScene CastEffect { get; set; }
    [Export] public PackedScene ImpactEffect { get; set; }
    [Export] public AudioStream CastSound { get; set; }
}

public enum AbilityTargetType { Self, SingleTarget, AoE, Projectile, Cone }
```

---

### Using Resources in Components (Composition)

```csharp
// The Enemy scene uses EnemyData to configure itself
public partial class Enemy : CharacterBody3D
{
    [Export] public EnemyData Data { get; set; }

    private HealthComponent _health;

    public override void _Ready()
    {
        if (Data == null)
        {
            GD.PrintErr("Enemy has no EnemyData assigned!");
            return;
        }

        // Configure composed components from resource data
        _health = GetNode<HealthComponent>("HealthComponent");
        _health.SetMaxHealth(Data.MaxHealth, true);

        var hitbox = GetNode<HitboxComponent>("HitboxComponent");
        hitbox.Damage = Data.AttackDamage;

        var detection = GetNode<CollisionShape3D>("DetectionArea/CollisionShape3D");
        if (detection.Shape is SphereShape3D sphere)
            sphere.Radius = Data.DetectionRange;

        // Apply visual tint
        var mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        mesh.Modulate = Data.TintColor;
    }
}
```

**In the TSCN:**
```ini
[ext_resource type="Resource" path="res://resources/data/enemies/goblin_data.tres" id="goblin_data"]

[node name="Goblin" parent="Enemies" instance=ExtResource("enemy_scene")]
Data = ExtResource("goblin_data")
position = Vector3(5, 0, 3)
```

---

### Resource Registry Pattern

Load all resources of a type at startup:

```csharp
public partial class ItemRegistry : Node
{
    public static ItemRegistry Instance { get; private set; }
    private Dictionary<string, ItemData> _items = new();

    public override void _Ready()
    {
        Instance = this;
        LoadAllItems("res://resources/data/items/");
    }

    private void LoadAllItems(string path)
    {
        var dir = DirAccess.Open(path);
        if (dir == null) return;

        dir.ListDirBegin();
        string fileName;
        while ((fileName = dir.GetNext()) != "")
        {
            if (fileName.EndsWith(".tres"))
            {
                var item = ResourceLoader.Load<ItemData>($"{path}{fileName}");
                if (item != null)
                    _items[item.ItemId] = item;
            }
        }
        dir.ListDirEnd();
    }

    public ItemData GetItem(string id) => _items.GetValueOrDefault(id);
    public bool HasItem(string id) => _items.ContainsKey(id);
}
```

## Key Takeaways
1. **[GlobalClass]** makes the resource visible in Godot's resource picker
2. **Export groups** organize properties in the inspector
3. **Resources separate data from behavior** — change stats without touching code
4. **.tres files are version-control friendly** — human-readable text
5. **Resources can reference other resources** (LootEntry[] inside EnemyData)
6. **Scenes consume resources via [Export]** — drag and drop in the editor
