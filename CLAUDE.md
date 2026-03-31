# CLAUDE.md — Project Conventions for PROJECT KERNEX

## Core Philosophy — The 5-Step Process

Before writing any code, building any system, or adding any feature, follow these steps **in order**:

1. **Question the requirement.** Is this actually needed? Who asked for it and why? The most common source of bad code is a requirement that shouldn't exist. Delete dumb requirements before implementing them.
2. **Delete.** Remove any part, process, system, or abstraction that doesn't directly serve the player experience. If you're not occasionally adding things back that you deleted, you're not deleting enough.
3. **Simplify.** Only after steps 1-2. Reduce complexity. Fewer nodes, fewer scripts, fewer abstractions. The best code is no code. The second-best is simple code. Never simplify a step that shouldn't exist (step 2).
4. **Optimize.** Only after steps 1-3. Make it faster, cheaper, lighter. Profile first, then act. Never optimize a system that should have been deleted or simplified.
5. **Automate.** Only after steps 1-4. Automate what's proven to work. Never automate a broken or overcomplicated process — you'll just produce broken results faster.

**The order matters.** Most engineers jump to step 3-5 immediately. The biggest gains come from steps 1-2.

---

## Engine & Language

- **Engine:** Godot 4.6+ Mono (.NET)
- **Language:** C# exclusively. **GDScript is STRICTLY PROHIBITED.** Zero GDScript files, zero GDScript snippets, zero exceptions.
- **Target:** .NET 8+ SDK
- **Format:** TSCN format=3 (load_steps deprecated since 4.6, omit it)

## Architecture Rules

### Composition — Mandatory, Not Optional

- **Every component = `.tscn` + `.cs` in the same folder.** No loose scripts. No exceptions.
- **Entities have NO script.** An entity (ship, station, drone) is a scene that instances components as children. All behavior comes from components.
- **Scenes are the unit of reuse.** Every reusable piece of functionality is its own scene. C# is for logic, NOT for mounting node trees via code.
- **Max 500 lines per .cs file.** Split into partial classes or extract sub-components.

### Component Folder Structure
```
components/
  └── {category}/
      └── {name}/
          ├── ComponentName.cs
          ├── ComponentName.tscn
          └── (optional: sub-scripts, shaders)
```

### Node Communication
- **"Call Down, Signal Up"** — Parents call methods on children. Children emit signals upward.
- **Component → Parent:** `GetParent<ParentType>()`
- **Component → Sibling:** `parent.GetNodeOrNull<T>("SiblingName")`
- **HUD → Entity:** Find by group (`GetTree().GetNodesInGroup("player")`)
- **Lazy init always:** `_ref ??= GetNodeOrNull<T>("Name")`
- **Never `GetNode("../../..")`** — scenes must work without knowing their parent.

### Scene Composition
```
Map (Node2D)
├── Environment       ← background, decorations, star layers
├── Entities          ← instances of ships, stations, drones
│   └── Camera       ← child of main entity
└── HUD (CanvasLayer) ← all UI, separated from game world
```

**HUD is NEVER a child of an entity.** HUD belongs to the scene/map. If the entity dies, the HUD stays alive.

### Other Rules
- **Command/Event pattern** — Every player action is a command, every state change is an event. This supports future offline → online transition.
- **[Export] everything configurable.** No magic numbers in scripts. Tuning without recompilation.
- **If it can be configured in the editor, configure it in the editor.** Scenes define structure, scripts define logic.

## Optimization Philosophy

**"What the player sees is what matters, not reality."**

- Fake everything you can. If the player can't tell the difference, use the cheaper approach.
- Object pooling for anything spawned frequently (bullets, particles, drones, enemies).
- Disable `_Process` / `_PhysicsProcess` on nodes that don't need per-frame updates.
- Use `VisibleOnScreenNotifier2D/3D` to disable processing for off-screen entities.
- Chunk-based loading — only render what's in the viewport + one buffer chunk.
- Profile before and after every significant change. Use Godot monitors and external .NET profilers.
- Minimize allocations in hot paths. Cache references in `_Ready()`, never in `_Process()`.
- Cooldowns by timestamp (`Time.GetTicksMsec()`) — zero allocation per frame.
- Static/reusable lists for temporary operations — avoid `new` per frame.
- MultiMeshInstance for repeated geometry. LOD for distant objects.
- Spatial hash maps for O(1) chunk lookup.
- **If it's not visible, it doesn't exist.** Despawn/pool entities outside the viewport.
- One collision processed per frame when possible (`break` after processing).

## Project Structure

Inspired by the proven v2 architecture from project-space-lanes.

`project.godot`, `.csproj`, and `.sln` live at the repo root (Godot and IDEs expect this).
All game code and assets live inside `src/`. Godot paths: `res://src/Assets/...`, `res://src/Components/...`, etc.

```
project-kernex/
├── project.godot              ← Godot project root
├── Project Kernex.csproj      ← .NET project file
├── Project Kernex.sln         ← Solution file (IDE opens this)
├── icon.svg
│
├── src/                       ← ALL game code and assets
│   ├── Addons/                ← Third-party Godot plugins
│   │
│   ├── Assets/                ← Raw assets only, no logic
│   │   ├── Audio/
│   │   │   ├── Music/         ← BGM by context (combat/, ambient/, menu/)
│   │   │   ├── SFX/
│   │   │   │   ├── Definitive/← Final sounds
│   │   │   │   └── Placeholder/← WIP sounds
│   │   │   └── Voice/         ← KIRA voice lines
│   │   ├── Fonts/
│   │   ├── Materials/
│   │   ├── Models/            ← 3D models (.glb) for SubViewport 3D
│   │   ├── Particles/
│   │   ├── Shaders/           ← Global reusable shaders
│   │   ├── Textures/
│   │   │   ├── Isometric/     ← Tilesets, floor tiles, isometric grid
│   │   │   ├── Ships/
│   │   │   ├── Drones/        ← Mining, combat, scout, salvage drone sprites
│   │   │   ├── Station/
│   │   │   ├── Environment/   ← Asteroids, nebulae, derelicts, anomalies
│   │   │   ├── UI/
│   │   │   ├── Effects/
│   │   │   └── Icons/
│   │   └── Themes/            ← Global UI theme (.tres)
│   │
│   ├── Autoloads/             ← Global singletons (max 5-6, thin coordinators)
│   │
│   ├── Components/            ← Reusable behavior units (scene + script, same folder)
│   │   ├── Core/              ← HealthComponent, MovementComponent, HitboxComponent
│   │   ├── Combat/            ← TargetingComponent, WeaponComponent, ShieldComponent
│   │   ├── Station/           ← BuildingComponent, RefineryComponent, DefenseComponent
│   │   ├── World/             ← ChunkLoader, SectorField, StarLayer
│   │   ├── Ships/             ← Thruster, RCS, Banking, DamageSystem, CameraFollow
│   │   ├── Drones/            ← DroneBehavior, MiningAI, CombatAI, ScoutAI, SalvageAI
│   │   ├── Automation/        ← ScriptRunner, CronHook, AutomationBridge (terminal↔world)
│   │   ├── Visuals/           ← VFX, particles, shaders per component
│   │   └── HUD/               ← Minimap, WorldMap, SpeedMeter, DebugPanel, etc.
│   │
│   ├── Nodes/                 ← Entities and compositions used inside Screens
│   │   ├── Ships/             ← PlayerShip, EnemyShip entities
│   │   ├── Drones/            ← MiningDrone, CombatDrone, ScoutDrone, SalvageDrone
│   │   ├── Fleet/             ← FleetFormation, FleetCommand (fleet compositions)
│   │   ├── Station/           ← StationBase, Modules, Turrets
│   │   ├── Environment/       ← Asteroids, Derelicts, Anomalies
│   │   ├── Projectiles/       ← Laser, Missile, etc.
│   │   ├── Terminal/          ← Terminal overlay, vsh emulator, command output
│   │   └── UI/                ← Menus, Panels, Popups, Tooltips
│   │
│   ├── Screens/               ← Full independent scenes (game states)
│   │   ├── Root/              ← Main scene, screen transitions
│   │   ├── MainMenu/
│   │   ├── Game/              ← Main game orchestrator
│   │   └── DevTools/          ← Debug/dev tools, balance editor
│   │
│   ├── Core/                  ← Pure C# types — NO Godot scenes
│   │   ├── Interfaces/        ← IShip, IStation, IDamageable, IAutomatable, IDrone
│   │   ├── Coordinates/       ← CellAddress, SectorAddress, QuadrantAddress, CoordConvert
│   │   ├── Enums/             ← FactionType, ResourceType, ThreatLevel, DroneType, etc.
│   │   ├── Constants/         ← GameConstants.cs (compile-time)
│   │   └── Utils/             ← MathUtil, ListUtils
│   │
│   ├── Systems/               ← Game logic + behavior strategies
│   │   ├── Production/        ← ProductionSystem, RefinerySystem, ProductionChain
│   │   ├── Combat/            ← CombatSystem, FleetSystem, RapidFireResolver
│   │   ├── Threat/            ← ThreatSystem, SignatureCalculator, DefenseResolver
│   │   ├── Exploration/       ← SectorGenerator, AnomalySystem, FogOfWar
│   │   ├── Terminal/          ← CommandParser, ScriptEngine, CronScheduler
│   │   ├── AI/                ← KiraEngine, LlmBridge, DialogueSystem
│   │   ├── Building/          ← BuildSystem, UpgradeSystem, TechTree
│   │   ├── Progression/       ← ResearchSystem, StationTierSystem
│   │   ├── Faction/           ← FactionSystem, ReputationTracker, FactionBonus
│   │   ├── Economy/           ← TradeSystem, MarketPrices, ResourceVelocity
│   │   └── Network/           ← (future) NetworkManager, SyncService, RpcLayer
│   │
│   ├── Resources/             ← .tres data files + C# Resource class definitions
│   │   ├── Ships/             ← ShipDefinition .tres files
│   │   ├── Drones/            ← DroneDefinition .tres files
│   │   ├── Buildings/         ← BuildingDefinition .tres files
│   │   ├── Research/          ← TechDefinition .tres files
│   │   ├── Enemies/           ← EnemyDefinition .tres files
│   │   ├── Factions/          ← FactionDefinition .tres files
│   │   ├── Balance/           ← BalanceConfig.tres (runtime-tunable via DevTools)
│   │   └── World/             ← SectorArchetype .tres files
│   │
│   └── Tests/                 ← Test scenes for isolated component testing
│
├── .claude/                   ← Agents, skills (auto-detected by Claude Code)
├── .github/                   ← Issue/PR templates, CI workflows
├── docs/                      ← Game design, roadmap, commands reference
├── CLAUDE.md                  ← Project conventions (this file)
├── README.md
├── LICENSE
├── CLA.md
├── CODE_OF_CONDUCT.md
└── CONTRIBUTING.md
```

### Folder Rules
| Folder | Contains | Rule |
|--------|----------|------|
| **Addons** | Third-party Godot plugins | Never modify plugin code directly |
| **Assets** | Raw files (images, audio, fonts, models, shaders) | No logic, no scripts |
| **Autoloads** | Global singletons | Max 5-6. Thin coordinators, not gameplay owners |
| **Components** | Reusable behavior (`.tscn` + `.cs` in same folder) | Instanced as children of entities |
| **Nodes** | Entities, UI compositions, terminal overlay | Used inside Screens. Compose from Components |
| **Screens** | Full game states | Independent scenes (MainMenu, Game, DevTools) |
| **Core** | Interfaces, structs, enums, constants, coordinates | Pure C#, no Godot scenes |
| **Systems** | Game logic, behavior strategies + factories | Strategy pattern for extensibility |
| **Resources** | `.tres` data + C# Resource class definitions | Side by side. Inspector-configurable |
| **Tests** | Isolated test scenes | Run individual scenes (F6) to test components |

## Naming Conventions

- **C# classes:** PascalCase (`StationModule`, `FleetManager`)
- **C# methods/properties:** PascalCase (`GetHealth()`, `MaxSpeed`)
- **C# local variables/params:** camelCase (`currentHealth`, `targetPosition`)
- **Signals:** Past tense PascalCase (`HealthChanged`, `ModuleBuilt`, `DroneDeployed`)
- **Scene files:** snake_case (`player_station.tscn`, `mining_drone.tscn`)
- **Script files:** PascalCase matching class name (`StationModule.cs`)
- **Folders:** snake_case (`game_systems/`, `ui_panels/`)

## Collision Layers

```
Layer 1: World (static geometry, asteroids)
Layer 2: Player station/ship
Layer 3: Enemies / hostile entities
Layer 4: Player projectiles
Layer 5: Enemy projectiles
Layer 6: Pickups / loot drops
Layer 7: Triggers / detection areas
Layer 8: Drones (player)
Layer 9: Drones (enemy)
Layer 10: UI interaction zones
```

## Commit & Branch Convention

Full reference: `.claude/skills/github/SKILL.md`

### Branch Naming
```
feature/chunk-system       fix/origin-shift-jitter      docs/update-roadmap
art/station-textures       perf/chunk-loading            refactor/extract-chain
```
Always branch from `develop`. Never push directly to `main` or `develop`.

### Commit Format
```
type(scope): imperative description (max 72 chars)

Optional body: explain WHY, not WHAT.

Closes #123
```

### Types
| Type | Use |
|------|-----|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation |
| `refactor` | Code restructure, no behavior change |
| `perf` | Performance improvement |
| `test` | Tests |
| `art` | Visual assets |
| `audio` | Audio assets |
| `chore` | Build, CI, tooling |
| `style` | Formatting only |

### Scopes (optional, use when clearly one system)
`combat`, `station`, `world`, `production`, `terminal`, `kira`, `ui`, `exploration`, `faction`, `economy`, `progression`, `network`

### Examples
```
feat(world): implement 3-level chunk system with origin shift
fix(combat): prevent double-damage on rapid-fire overflow
perf(world): reduce chunk load time by 60% with spatial hash
docs: update game design with faction system
refactor(production): extract chain resolver into own class
```

### PR Convention
- Title follows same commit format: `type(scope): description`
- Always target `develop`
- Fill every section of `.github/PULL_REQUEST_TEMPLATE.md`
- Use `gh pr create --base develop` to create PRs

## Key Technical Decisions

### 3-Level Chunk System (proven in test-workspace prototype)
```
Quadrant (macro region, ~41 sectors)
  └── Sector (origin shift boundary, ~61 cells)
      └── Cell (smallest unit, entity spawn/despawn)
```
- **All world coordinates are `long` integers** — float only for small local offsets
- **Origin shift at sector boundaries** — player stays near (0,0), world shifts around them
- **Diamond lattice** with non-orthogonal lattice vectors for organic sector shapes
- **Entity registry pattern** — single source of truth for all entities (live + off-screen)
- **Off-screen simulation** — entities with velocity continue moving even when despawned
- **RigidBody sync after shift** — reassign GlobalPosition to sync PhysicsServer after origin shift

### Other Decisions
- **Isometric rendering:** Chunk-based tile rendering with spatial hash map
- **Deterministic procedural generation:** Seed-based, nothing stored, same coords = same content
- **Offline-first → MMO:** Game logic separated from I/O via abstraction layer
- **Local AI (KIRA):** llama.cpp or similar, 7B quantized model, with scripted fallback

### Reference
The `test-workspace` project (same org) contains a working prototype with this chunk system, origin shift, asteroid registry, ship physics, and 15+ HUD components. Use it as architectural reference.

## What NOT To Do

- Never write GDScript. Ever.
- Never use deep inheritance chains. Use composition.
- Never hardcode pixel positions for UI. Use containers.
- Never put game logic in UI scripts. UI reads state and emits signals.
- Never create nodes in `_Process()`. Pool them.
- Never assume an API exists. Check Godot 4.6 docs first.
- Never build entire UI hierarchies in code. Use scenes.
- Never modify scene tree from background threads. Use `CallDeferred()`.
