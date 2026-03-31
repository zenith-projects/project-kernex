# CLAUDE.md — Project Conventions for PROJECT KERNEX

## Engine & Language

- **Engine:** Godot 4.6+ Mono (.NET)
- **Language:** C# exclusively. **GDScript is STRICTLY PROHIBITED.** Zero GDScript files, zero GDScript snippets, zero exceptions.
- **Target:** .NET 8+ SDK
- **Format:** TSCN format=3 (load_steps deprecated since 4.6, omit it)

## Architecture Rules

- **Composition is MANDATORY, not optional.** Build complex objects from small, reusable component scenes. Never use deep inheritance chains.
- **"Call Down, Signal Up"** — Parents call methods on children. Children emit signals upward. Never use `GetParent()` or `GetNode("..")`.
- **Scenes are the unit of reuse.** Every reusable piece of functionality is its own scene.
- **Command/Event pattern** — Every player action is a command, every state change is an event. This supports future offline → online transition.
- **Max 500 lines per .cs file.** Split into partial classes or extract sub-components.

## Optimization Philosophy

**"What the player sees is what matters, not reality."**

- Fake everything you can. If the player can't tell the difference, use the cheaper approach.
- Object pooling for anything spawned frequently (bullets, particles, drones, enemies).
- Disable `_Process` / `_PhysicsProcess` on nodes that don't need per-frame updates.
- Use `VisibleOnScreenNotifier2D/3D` to disable processing for off-screen entities.
- Chunk-based loading — only render what's in the viewport + one buffer chunk.
- Profile before and after every significant change. Use Godot monitors and external .NET profilers.
- Minimize allocations in hot paths. Cache references in `_Ready()`, never in `_Process()`.
- MultiMeshInstance for repeated geometry. LOD for distant objects.
- Spatial hash maps for O(1) chunk lookup.
- **If it's not visible, it doesn't exist.** Despawn/pool entities outside the viewport.

## Project Structure

```
project-kernex/
├── project.godot
├── ProjectKernex.csproj
├── scenes/
│   ├── core/              # Core game scenes (main, game loop)
│   ├── station/           # Station modules, buildings
│   ├── fleet/             # Ships, drones, combat
│   ├── world/             # Sectors, chunks, map
│   ├── ui/                # HUD, menus, panels
│   ├── terminal/          # Terminal emulator overlay
│   ├── components/        # Reusable component scenes
│   └── vfx/               # Visual effects, particles
├── scripts/
│   ├── autoload/          # Singletons (GameManager, AudioManager, etc.)
│   ├── components/        # Component scripts
│   ├── resources/         # Custom Resource classes
│   ├── systems/           # Game systems (resource, combat, threat, etc.)
│   ├── terminal/          # Terminal/vsh engine
│   └── ai/                # KIRA AI system
├── assets/
│   ├── audio/
│   ├── textures/
│   ├── fonts/
│   └── shaders/
├── resources/
│   ├── themes/            # UI themes (.tres)
│   └── data/              # Game data resources (.tres)
└── tests/
```

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

## Commit Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):
```
feat: add asteroid mining drill mechanic
fix: correct resource calculation overflow
docs: update terminal command reference
art: add new space station hull textures
audio: add ambient engine hum sound effect
refactor: extract resource pipeline logic
test: add unit tests for refinery system
perf: optimize chunk loading with spatial hash
```

## Key Technical Decisions

- **Chunked coordinates:** 64-bit integer SectorID + 32-bit float LocalOffset for infinite world
- **Isometric rendering:** Chunk-based tile rendering with spatial hash map
- **Deterministic procedural generation:** Seed-based, nothing stored, same coords = same content
- **Offline-first → MMO:** Game logic separated from I/O via abstraction layer
- **Local AI (KIRA):** llama.cpp or similar, 7B quantized model, with scripted fallback

## What NOT To Do

- Never write GDScript. Ever.
- Never use deep inheritance chains. Use composition.
- Never hardcode pixel positions for UI. Use containers.
- Never put game logic in UI scripts. UI reads state and emits signals.
- Never create nodes in `_Process()`. Pool them.
- Never assume an API exists. Check Godot 4.6 docs first.
- Never build entire UI hierarchies in code. Use scenes.
- Never modify scene tree from background threads. Use `CallDeferred()`.
