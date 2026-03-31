# Godot 4.6 C# Skill

Skill completa para desenvolvimento em Godot 4.6 com C# (.NET). Baseada na doc oficial https://docs.godotengine.org/en/4.6/

## Cobertura

- **Scenes & Composition**: Arquitetura de cenas, formato TSCN/TRES, composicao, instanciamento, 10 patterns, DI oficial
- **C# Scripting**: Sinais, lifecycle, exports, [GlobalClass], Variant types, style guide (doc oficial 4.6)
- **3D Development**: Transform3D, CharacterBody3D, navigation, physics, lighting, environment
- **Animation**: AnimationPlayer, AnimationTree, BlendSpace, StateMachine, root motion, RESET animation
- **Audio**: Bus routing, efeitos, 3D spatial audio, AudioStreamRandomizer
- **Input**: Pipeline de 8 passos, actions, gamepad, deadzone, remapping
- **UI System**: Control nodes, themes, layouts responsivos, padroes de UI
- **2D Development**: Movement patterns, TileMap, parallax, 2D lights, particles, sprite animation
- **Networking**: Multiplayer RPC, HTTP requests, WebSocket client/server
- **I/O & Rendering**: Save system, file paths, viewports, multiple resolutions, scene management
- **Export & Debug**: Export presets, feature tags, profiler, CPU/GPU optimization, notifications
- **Shaders**: Shader language completo, spatial/canvas/particle built-ins, compute shaders, post-processing
- **Navigation Advanced**: NavigationServer API, agents, obstacles, links, chunk baking, debug tools
- **Physics Advanced**: RigidBody, ragdoll, SoftBody, collision shapes, Jolt, interpolation
- **Plugins**: Editor plugins, import/inspector/gizmo plugins — tudo em C#
- **i18n**: PO/gettext workflow, pseudolocalization, RTL
- **Math**: Vetores, dot/cross, interpolation, random, Bezier, SAT collision
- **3D Advanced**: Materials, lights/shadows, MultiMesh, LOD, occlusion, CSG, GridMaps, particles, fog
- **Export**: Windows, Linux, Android, Web, dedicated servers — setup completo
- **Asset Pipeline**: glTF/blend/FBX import, imagens, audio, Server API optimization
- **Best Practices**: OOP oficial, data/logic preferences, multithreading, anti-patterns
- **Project Architecture**: Estrutura de projeto, naming conventions, organizacao

## Estrutura

```
godot/
├── SKILL.md                          ← Prompt principal (coracao da skill)
├── README.md                         ← Este arquivo
├── examples/                         ← 8 exemplos detalhados input → output
│   ├── 01-player-character-2d.md
│   ├── 02-enemy-state-machine.md    ← 3D + state machine + composicao
│   ├── 03-tscn-manual-editing.md    ← edicao direta de .tscn
│   ├── 04-scene-composition.md      ← 3D RPG com 11 scenes compostas
│   ├── 05-csharp-signals.md
│   ├── 06-ui-inventory.md
│   ├── 07-resource-system.md
│   ├── 08-component-pattern.md      ← 3D hitbox/hurtbox/health reusavel
│   ├── 09-shaders-i18n-raycasting.md ← Shaders, i18n, ray-casting
│   └── 10-game-architectures.md     ← 9 jogos completos (65KB)
├── guidelines/                       ← Regras detalhadas por area
│   ├── scene_composition.md         ← 10 patterns + DI oficial + decision tree
│   ├── tscn_format_reference.md     ← Formato 4.6 (load_steps deprecated!)
│   ├── csharp_scripting.md          ← Referencia completa C# 4.6 + style guide
│   ├── 3d_development.md            ← Transform3D, physics, nav, lighting
│   ├── animation_system.md          ← AnimationPlayer, AnimationTree, BlendSpace
│   ├── audio_system.md              ← Buses, 3D audio, randomizer
│   ├── input_system.md              ← Pipeline 8 passos, gamepad, remapping
│   ├── ui_system.md
│   ├── project_structure.md
│   ├── 2d_development.md            ← Movement, TileMap, parallax, lights
│   ├── networking.md                ← Multiplayer, HTTP, WebSocket
│   ├── io_and_rendering.md          ← Save system, viewports, resolutions
│   ├── export_debug_optimization.md ← Export, debug, CPU/GPU optimization
│   ├── navigation_advanced.md       ← 1793 linhas: server API, agents, obstacles, debug
│   ├── physics_advanced.md          ← 1026 linhas: RigidBody, ragdoll, Jolt, interpolation
│   ├── shaders_advanced.md          ← 1820 linhas: spatial/canvas/particle/compute shaders
│   ├── plugins_i18n_math.md         ← 2371 linhas: editor plugins, gettext, vector math
│   ├── 3d_export_assets_advanced.md ← 1604 linhas: materials, lights, LOD, export, assets
│   ├── quick_recipes.md             ← 50+ snippets copy-paste
│   ├── gdscript_to_csharp.md        ← Cheat sheet de conversao
│   ├── chickensoft_ecosystem.md     ← AutoInject, LogicBlocks, 3-layer arch
│   ├── best_practices_official.md   ← OOP, data/logic prefs, threading
│   └── performance_and_antipatterns.md ← GC, pooling, multithreading
```
