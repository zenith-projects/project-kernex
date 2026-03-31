# Project Structure Guidelines

Best practices for organizing Godot 4 C# projects.

---

## Recommended Folder Structure

```
project/
├── project.godot                  # Project configuration
├── .csproj                        # C# project file
│
├── scenes/                        # All .tscn scene files
│   ├── characters/                # Player, enemies, NPCs
│   │   ├── Player.tscn
│   │   ├── enemies/
│   │   │   ├── BaseEnemy.tscn
│   │   │   ├── Goblin.tscn
│   │   │   └── Orc.tscn
│   │   └── npcs/
│   │       └── Shopkeeper.tscn
│   │
│   ├── components/                # Reusable component scenes
│   │   ├── HealthComponent.tscn
│   │   ├── HitboxComponent.tscn
│   │   ├── HurtboxComponent.tscn
│   │   ├── MovementComponent.tscn
│   │   └── StateMachine.tscn
│   │
│   ├── levels/                    # Level scenes
│   │   ├── Level1.tscn
│   │   ├── Level2.tscn
│   │   └── common/               # Shared level elements
│   │       ├── Door.tscn
│   │       └── Chest.tscn
│   │
│   ├── ui/                        # UI scenes
│   │   ├── MainMenu.tscn
│   │   ├── PauseMenu.tscn
│   │   ├── HUD.tscn
│   │   ├── InventoryUI.tscn
│   │   └── DialogueBox.tscn
│   │
│   ├── projectiles/               # Bullet, arrow, spell scenes
│   │   ├── Bullet.tscn
│   │   └── Arrow.tscn
│   │
│   └── vfx/                       # Visual effects
│       ├── Explosion.tscn
│       ├── HitSpark.tscn
│       └── DamageNumber.tscn
│
├── scripts/                       # All .cs script files
│   ├── autoload/                  # Singleton/autoload scripts
│   │   ├── GameManager.cs
│   │   ├── AudioManager.cs
│   │   └── SaveManager.cs
│   │
│   ├── characters/                # Character scripts
│   │   ├── Player.cs
│   │   ├── enemies/
│   │   │   ├── BaseEnemy.cs
│   │   │   ├── Goblin.cs
│   │   │   └── Orc.cs
│   │   └── npcs/
│   │       └── Shopkeeper.cs
│   │
│   ├── components/                # Component scripts
│   │   ├── HealthComponent.cs
│   │   ├── HitboxComponent.cs
│   │   ├── HurtboxComponent.cs
│   │   ├── MovementComponent.cs
│   │   ├── State.cs
│   │   └── StateMachine.cs
│   │
│   ├── resources/                 # Custom Resource classes
│   │   ├── EnemyData.cs
│   │   ├── ItemData.cs
│   │   ├── WeaponData.cs
│   │   └── DialogueLine.cs
│   │
│   ├── systems/                   # Game systems
│   │   ├── CombatSystem.cs
│   │   ├── InventorySystem.cs
│   │   └── QuestSystem.cs
│   │
│   ├── ui/                        # UI scripts
│   │   ├── MainMenu.cs
│   │   ├── HUD.cs
│   │   ├── InventoryUI.cs
│   │   └── DialogueBox.cs
│   │
│   └── utils/                     # Utility classes
│       ├── Extensions.cs
│       └── Constants.cs
│
├── assets/                        # Raw art/audio/font assets
│   ├── sprites/
│   │   ├── characters/
│   │   ├── items/
│   │   ├── environment/
│   │   └── ui/
│   ├── audio/
│   │   ├── music/
│   │   └── sfx/
│   ├── fonts/
│   └── shaders/
│
└── resources/                     # .tres resource files
    ├── themes/
    │   └── MainTheme.tres
    ├── data/
    │   ├── enemies/
    │   │   ├── goblin_data.tres
    │   │   └── orc_data.tres
    │   ├── items/
    │   │   ├── sword_data.tres
    │   │   └── potion_data.tres
    │   └── dialogue/
    └── materials/
```

---

## Naming Conventions

### Files and Folders

| Type | Convention | Example |
|------|-----------|---------|
| Folders | `snake_case` | `scenes/characters/enemies/` |
| Scene files | `PascalCase.tscn` | `Player.tscn`, `MainMenu.tscn` |
| C# scripts | `PascalCase.cs` | `Player.cs`, `HealthComponent.cs` |
| Resources (.tres) | `snake_case.tres` | `goblin_data.tres`, `main_theme.tres` |
| Assets (art/audio) | `snake_case` | `player_idle.png`, `hit_sound.wav` |
| Shaders | `snake_case.gdshader` | `outline_shader.gdshader` |

### Nodes

| Rule | Example |
|------|---------|
| Node names: `PascalCase` | `Player`, `HealthComponent`, `SpawnPoint` |
| Match built-in casing | `Sprite2D`, `CollisionShape2D`, `AnimationPlayer` |
| Descriptive names | `DetectionArea` not `Area2D`, `HealthBar` not `ProgressBar` |
| Groups: `snake_case` | `enemies`, `damageable`, `interactable` |

### C# Code

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | `PascalCase` | `HealthComponent`, `EnemyData` |
| Methods | `PascalCase` | `TakeDamage()`, `SpawnEnemy()` |
| Properties | `PascalCase` | `MaxHealth`, `MoveSpeed` |
| Private fields | `_camelCase` | `_currentHealth`, `_isAlive` |
| Local variables | `camelCase` | `direction`, `spawnPoint` |
| Constants | `PascalCase` | `MaxEnemies`, `DefaultSpeed` |
| Signals | `PascalCaseEventHandler` | `HealthChangedEventHandler` |
| Enums | `PascalCase` | `DamageType.Physical` |
| Events (C#) | `On` + signal name | `OnHealthChanged`, `OnDied` |

---

## Scene Organization Rules

### 1. Mirror Scene and Script Paths
Scene at `scenes/characters/Player.tscn` → Script at `scripts/characters/Player.cs`

### 2. Components Are First-Class
Components get their own folder: `scenes/components/` and `scripts/components/`

### 3. Keep Scenes Self-Contained
A scene folder should contain everything the scene exclusively needs:
```
scenes/characters/
├── Player.tscn
└── enemies/
    ├── BaseEnemy.tscn
    ├── Goblin.tscn      # Inherits BaseEnemy.tscn
    └── Orc.tscn          # Inherits BaseEnemy.tscn
```

### 4. Shared Assets Go in `assets/`
Assets used by multiple scenes go in the shared `assets/` folder. Assets exclusive to one scene can live near that scene.

### 5. Separate Data from Logic
Use Custom Resources (`.tres`) for data:
```
resources/data/enemies/goblin_data.tres   ← data
scripts/resources/EnemyData.cs             ← class definition
scripts/characters/enemies/Goblin.cs       ← behavior logic
scenes/characters/enemies/Goblin.tscn      ← scene structure
```

---

## Autoload Registration

Register autoloads in `project.godot`:

```ini
[autoload]
GameManager="*res://scripts/autoload/GameManager.cs"
AudioManager="*res://scripts/autoload/AudioManager.cs"
SaveManager="*res://scripts/autoload/SaveManager.cs"
```

The `*` prefix means it's enabled. Autoloads load before any scene.

### Keep Autoloads Minimal
Only use autoloads for:
- `GameManager` — global game state, score, current level
- `AudioManager` — background music that persists across scenes
- `SaveManager` — save/load functionality
- `SceneTransition` — fade in/out between scenes

Everything else should be a component or a scene-local system.

---

## C# Project File (.csproj)

Standard Godot 4 C# project:

```xml
<Project Sdk="Godot.NET.Sdk/4.4.0">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <TargetFramework Condition=" '$(GodotTargetPlatform)' == 'android' ">net7.0</TargetFramework>
    <TargetFramework Condition=" '$(GodotTargetPlatform)' == 'ios' ">net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Adding NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="SomeLibrary" Version="1.0.0" />
</ItemGroup>
```

**Note:** Not all NuGet packages work with Godot. Avoid packages that depend on System.Windows or ASP.NET.

---

## Version Control (.gitignore)

```gitignore
# Godot
.godot/
*.translation

# C# / .NET
.mono/
*.uid

# OS
.DS_Store
Thumbs.db

# IDE
.vs/
.idea/
*.user
*.suo
```

**Always commit:**
- `project.godot`
- All `.tscn`, `.tres`, `.cs` files
- All asset files
- `.csproj` file
- `*.import` files (Godot-generated import settings)
