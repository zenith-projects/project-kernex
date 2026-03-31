# Godot 4.6 C# Skill

Complete skill for Godot 4.6 development with C# (.NET). Based on official docs https://docs.godotengine.org/en/4.6/

## Coverage

- **Scenes & Composition**: Scene architecture, TSCN/TRES format, composition, instancing, 10 patterns, official DI
- **C# Scripting**: Signals, lifecycle, exports, [GlobalClass], Variant types, style guide (official 4.6 docs)
- **3D Development**: Transform3D, CharacterBody3D, navigation, physics, lighting, environment
- **Animation**: AnimationPlayer, AnimationTree, BlendSpace, StateMachine, root motion, RESET animation
- **Audio**: Bus routing, effects, 3D spatial audio, AudioStreamRandomizer
- **Input**: 8-step pipeline, actions, gamepad, deadzone, remapping
- **UI System**: Control nodes, themes, responsive layouts, UI patterns
- **2D Development**: Movement patterns, TileMap, parallax, 2D lights, particles, sprite animation
- **Networking**: Multiplayer RPC, HTTP requests, WebSocket client/server
- **I/O & Rendering**: Save system, file paths, viewports, multiple resolutions, scene management
- **Export & Debug**: Export presets, feature tags, profiler, CPU/GPU optimization, notifications
- **Shaders**: Complete shader language, spatial/canvas/particle built-ins, compute shaders, post-processing
- **Navigation Advanced**: NavigationServer API, agents, obstacles, links, chunk baking, debug tools
- **Physics Advanced**: RigidBody, ragdoll, SoftBody, collision shapes, Jolt, interpolation
- **Plugins**: Editor plugins, import/inspector/gizmo plugins — all in C#
- **i18n**: PO/gettext workflow, pseudolocalization, RTL
- **Math**: Vectors, dot/cross, interpolation, random, Bezier, SAT collision
- **3D Advanced**: Materials, lights/shadows, MultiMesh, LOD, occlusion, CSG, GridMaps, particles, fog
- **Export**: Windows, Linux, Android, Web, dedicated servers — complete setup
- **Asset Pipeline**: glTF/blend/FBX import, images, audio, Server API optimization
- **Best Practices**: Official OOP, data/logic preferences, multithreading, anti-patterns
- **Project Architecture**: Project structure, naming conventions, organization

## Structure

```
godot/
├── SKILL.md                          ← Main prompt (skill core)
├── README.md                         ← This file
├── examples/                         ← 10 detailed input → output examples
│   ├── 01-player-character-2d.md
│   ├── 02-enemy-state-machine.md    ← 3D + state machine + composition
│   ├── 03-tscn-manual-editing.md    ← Direct .tscn editing
│   ├── 04-scene-composition.md      ← 3D RPG with 11 composed scenes
│   ├── 05-csharp-signals.md
│   ├── 06-ui-inventory.md
│   ├── 07-resource-system.md
│   ├── 08-component-pattern.md      ← 3D reusable hitbox/hurtbox/health
│   ├── 09-shaders-i18n-raycasting.md ← Shaders, i18n, ray-casting
│   └── 10-game-architectures.md     ← 9 complete game patterns (65KB)
├── guidelines/                       ← Detailed rules per area
│   ├── scene_composition.md         ← 10 patterns + official DI + decision tree
│   ├── tscn_format_reference.md     ← 4.6 format (load_steps deprecated!)
│   ├── csharp_scripting.md          ← Complete C# 4.6 reference + style guide
│   ├── 3d_development.md            ← Transform3D, physics, nav, lighting
│   ├── animation_system.md          ← AnimationPlayer, AnimationTree, BlendSpace
│   ├── audio_system.md              ← Buses, 3D audio, randomizer
│   ├── input_system.md              ← 8-step pipeline, gamepad, remapping
│   ├── ui_system.md
│   ├── project_structure.md
│   ├── 2d_development.md            ← Movement, TileMap, parallax, lights
│   ├── networking.md                ← Multiplayer, HTTP, WebSocket
│   ├── io_and_rendering.md          ← Save system, viewports, resolutions
│   ├── export_debug_optimization.md ← Export, debug, CPU/GPU optimization
│   ├── navigation_advanced.md       ← 1793 lines: server API, agents, obstacles, debug
│   ├── physics_advanced.md          ← 1026 lines: RigidBody, ragdoll, Jolt, interpolation
│   ├── shaders_advanced.md          ← 1820 lines: spatial/canvas/particle/compute shaders
│   ├── plugins_i18n_math.md         ← 2371 lines: editor plugins, gettext, vector math
│   ├── 3d_export_assets_advanced.md ← 1604 lines: materials, lights, LOD, export, assets
│   ├── quick_recipes.md             ← 50+ copy-paste snippets
│   ├── gdscript_to_csharp.md        ← Conversion cheat sheet
│   ├── chickensoft_ecosystem.md     ← AutoInject, LogicBlocks, 3-layer arch
│   ├── best_practices_official.md   ← OOP, data/logic prefs, threading
│   └── performance_and_antipatterns.md ← GC, pooling, multithreading
```
