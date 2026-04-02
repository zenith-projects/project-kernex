# PROJECT KERNEX — Development Roadmap

> *This roadmap is a living document and subject to change based on community feedback and development priorities.*
> *Each phase builds on the previous one. MVP milestones mark playable checkpoints.*

---

## Phase 0 — Foundation 🏗️ `COMPLETE`

**Goal:** Project structure, game design, community infrastructure, and technical architecture.

- [x] Define core game concept
- [x] Create repository structure
- [x] Write LICENSE, CLA, CONTRIBUTING guidelines
- [x] Game design documentation (15 docs across all systems)
- [x] Research reference games (Dark Orbit, OGame, Factorio, CoC, NMS, Star Citizen)
- [x] Choose game engine — Godot 4.6+ Mono (C# / .NET 8+)
- [x] Set up CI/CD pipeline (10 PR checks)
- [x] Branch protection rules (rebase only, approval required)
- [x] Claude Code agents and skills for the project
- [ ] Define command/event architecture (offline-first → online-ready)
- [ ] Create Discord server for community
- [ ] Define art style guide (isometric)

---

## Phase 1 — Shell & AXIA (Core Innovation) 🖥️🤖 `IN PROGRESS`

**Goal:** Build the shell and AXIA first — they are the core innovation and differentiate the game from everything else. Even before the isometric world exists, the shell + AXIA should be functional and impressive.

### Shell (vsh) — Terminal Emulator

- [x] **Shell emulator overlay** — `ShellOverlay` (CanvasLayer) with `ShellDisplay` (scrollable RichTextLabel output) + `ShellInput` (LineEdit with command history via Up/Down arrows). Components follow project composition rules: each is `.tscn` + `.cs` in `src/Components/HUD/`.
- [x] **Command parser and execution engine** — `CommandParser` (stateless tokenizer with quote support), `CommandRegistry` (name + alias lookup), `ParsedCommand` (value object with positional args + flags), `CommandResult` (factory with typed outputs: Success, Error, Info, Axia, Clear). All pure C# in `src/Systems/Shell/`.
- [x] **Core commands** — `help`, `status`, `clear`, `whoami`, `uptime`, `axia`. Each implements `ICommand` interface. `HelpCommand` lists all registered commands. `AxiaCommand` bridges to `AxiaEngine`. Located in `src/Systems/Shell/Commands/`.
- [x] **Shell test scene** — `ShellTestScene.tscn` in `src/Screens/DevTools/`, F6-runnable standalone scene with dark background, welcome banner, all commands registered.
- [ ] Piping system (`|`) and command chaining
- [ ] Aliases (`alias mine="drone deploy --type mining"`)
- [ ] Basic `.vsh` scripting (loops, conditionals, variables)
- [ ] Cron jobs (scheduled task execution)

### AXIA — AI Companion

- [x] **AXIA scripted dialogue engine (Levels 0-2)** — `ScriptedDialogueProvider` with keyword-based response banks per level. Level 0 (Corrupted): 70% garbled via `GlitchTextGenerator` (character corruption, dropouts, insertions), 30% glitched scripted response. Level 1 (Booting): terse mechanical responses, 15% chance of `[SIGNAL LOST]` append. Level 2 (Functional): clear helpful responses, no glitching.
- [x] **AXIA game state context injection** — `AxiaPromptBuilder` constructs system prompts with station state (hull, power, drones, threat, etc.) filtered by `AxiaLevelData.AllowedContextKeys`. Level 0 sees nothing, Level 2 sees 5 keys, Level 5+ sees everything. Context always appended to prompt, even with custom prompts.
- [x] **Local LLM integration** — `LlamaSharpProvider` wraps LLamaSharp 0.26.0 (C# bindings for llama.cpp). Uses `InteractiveExecutor` with `MemoryClear()` before each call for fresh context. ChatML format (`<|im_start|>`) for Qwen compatibility. Configurable: Temperature, TopP, TopK, MinP, MaxTokens, RepeatPenalty, FrequencyPenalty, PresencePenalty, Seed, Mirostat (mode/tau/eta), AntiPrompts. Response cleaning pipeline strips role prefixes, artifacts, and asterisk actions.
- [x] **10-level evolution system** — `AxiaLevelData` Resource (`.tres`) for all 10 levels (Corrupted → Ascended). Each defines: personality notes, response delay, glitch intensity, allowed context keys, LLM toggle, output color. Levels 0-2 always scripted, 3+ use LLM with scripted fallback.
- [x] **Emotion system** — `AxiaResponse.Parse()` extracts `[EMOTION]` tags from LLM output. 11 emotions: NEUTRAL, CURIOUS, HAPPY, SAD, ANGRY, CRAZY, CRYING, CAUTIOUS, RELAX, SHY, VANISHED. Emotion drives portrait changes with crossfade transition (0.25s Sine tween). AXIA waifu portraits (10 emotion variants + VANISHED empty screen) in `src/Assets/Textures/Axia/`.
- [x] **Persistent memory system** — `AxiaMemoryStore` saves all messages to JSON (`user://axia_memory/messages.json`). Features: keyword search (stopword filtering, relevance ranking), long-term summaries via 3B model (batches of 20 messages → 3-5 bullet points stored in `summaries.json`). Prompt injection: summaries always included + keyword matches for relevant past messages. Chat restored on scene reload (last 30 messages).
- [x] **Proactive messaging engine** — `AxiaProactiveEngine` uses the 3B model for autonomous AXIA messages. Escalating behavior: 15s (casual/curious) → +20s (needy/shy) → +30s (emotional/dramatic) → gives up. Fresh chat: AXIA introduces herself. Visual indicator: portrait border gold flash (2-pulse tween). State persisted (gave_up, attempt count). Resets when player speaks.
- [x] **Dual-model architecture** — Two Qwen2.5 models loaded simultaneously:
  - **7B** (`qwen2.5-7b-instruct-q4_k_m.gguf`, 4.4 GB) — Main chat, higher quality responses
  - **3B** (`qwen2.5-3b-instruct-q4_k_m.gguf`, 2.0 GB) — Proactive decisions, memory summarization, fast inference
  - Both Apache 2.0 license, GGUF Q4_K_M quantization, ~6.5 GB total RAM
- [x] **Debug/test scene** — `AxiaTestScene.tscn` with comprehensive controls:
  - Left: AxiaPanel (chat with AXIA, emotion portrait with crossfade)
  - Right: Collapsible debug sections (AXIA, MODEL, INFERENCE, SYSTEM PROMPT, GAME CONTEXT, OUTPUT, PROACTIVE, MEMORY)
  - All parameters adjustable via sliders with tooltips
  - Editable system prompt with anti-prompts
  - Mock game context (hull, power, drones, storage, threat, sector, faction)
  - Config persisted in `user://axia_debug_config.tres` (auto-save on every change)
  - JetBrains Mono font, dark+gold glassmorphism theme (`src/Assets/Themes/Axia/axia_debug_theme.tres`), animated nebula background shader (`src/Assets/Themes/Axia/Shaders/`)
- [ ] AXIA writes `.vsh` scripts on player request
- [ ] Hardware detection and automatic fallback (LLM vs scripted)

### AI Models — Technical Details

| Model | File | Size | License | Use |
|-------|------|------|---------|-----|
| Qwen2.5-7B-Instruct | `models/qwen2.5-7b-instruct-q4_k_m.gguf` | 4.4 GB | Apache 2.0 | Main chat (Level 3+) |
| Qwen2.5-3B-Instruct | `models/qwen2.5-3b-instruct-q4_k_m.gguf` | 2.0 GB | Apache 2.0 | Proactive + summarization |

**Runtime:** LLamaSharp 0.26.0 (NuGet) with CPU backend. GPU offload supported via `GpuLayerCount`.

**Qwen2.5 supports tool/function calling** natively via ChatML format. This opens the path for AXIA to execute game commands directly (scan sectors, deploy drones, manage refineries) — planned for the `.vsh` script generation feature.

### Next Steps (Phase 1 remaining)

1. **Tool calling integration** — AXIA calls game functions directly instead of just injecting state. Enables: `get_hull_status()`, `deploy_drone(sector)`, `scan_sector(id)`. Foundation for AXIA writing `.vsh` scripts.
2. **Piping and aliases** — Shell command chaining (`scan | filter | deploy`) and shortcut system.
3. **`.vsh` scripting** — Basic script interpreter with loops, conditionals, variables.
4. **Hardware detection** — Auto-detect RAM/VRAM and select 3B vs 7B model. Fallback to scripted-only if insufficient hardware.

### UI Foundation & Design System

- [x] **Design system components** — Custom UI components with `[GlobalClass]` + `[Tool]` (visible in Godot "Add Node" dialog, render in editor viewport):
  - `KernexButton` — Ghost-style button (text-only, no background). Hover: text color tween + scale-up (1.12x). Moonhouse font with FontVariation embolden.
  - `KernexLabel` — Label with 6 style variants (Title, Subtitle, Body, Caption, Monospace, Logo). Shadow support via LabelSettings. Moonhouse display font for titles, JetBrains Mono for body text.
  - `KernexPanel` — Styled panel with configurable border, alpha, header, corner radius.
  - `KernexSeparator` — Horizontal separator with accent color.
  - `KernexToggle` — Animated toggle switch with pill track and sliding knob.
  - `KernexSlider` — Labeled slider with track fill, circular grabber, and value display.
  - `KernexDropdown` — Labeled dropdown with styled popup and hover states.
  - `KernexCorner` — L-bracket decorative drawn as filled polygon via `_Draw()`.
  - `KernexLine` — Horizontal/vertical line with Edge alignment and Padding, drawn as polygon.
  - `KernexHatch` — Diagonal line pattern for sci-fi decorative detail.
- [x] **Screen infrastructure** — `ScreenManager` autoload with fade transitions (ColorRect + Tween). Root scene as entry point loading MainMenu. Configured in `project.godot`.
- [x] **Main Menu** — Background image with `bg_enhance` shader (zoom breathing + barrel distortion + vignette). Logo (Moonhouse font) with "PROJECT" / "KERNEX" / tagline. 5 ghost buttons with stagger slide-in animation. Decorative frame: corners, lines, thin accent lines, hatch patterns — all anchor-responsive for window resize. Settings as internal panel (show/hide, no screen transition).
- [x] **Settings** — Graphics tab (resolution dropdown, fullscreen/vsync toggles), Audio tab (master/music/sfx sliders), Controls tab (placeholder). All using Kernex custom components. Persists to `user://settings.cfg` via ConfigFile.
- [x] **Background shader** — `bg_enhance.gdshader`: zoom breathing (oscillating scale from center), barrel distortion (intensity synced with breath), vignette, contrast/brightness grading.
- [x] **Rendering quality** — Viewport 1920x1080, MSAA 2D 4x, FXAA enabled.
- [ ] Loading screen
- [ ] Credits / Monolith of Contributors scroll

**MVP 1:** Shell works, AXIA responds with personality and memory, player can type commands and AXIA generates context-aware responses with emotions. Main menu and settings functional with custom design system. ✅ *Partially achieved — shell + AXIA + UI foundation functional, scripting pending.*

---

## Phase 2 — Core Engine & Isometric World 🌍

**Goal:** Isometric renderer, chunked coordinate system, and basic player interaction. This is the primary rendering perspective — all core gameplay starts here.

- [ ] Isometric tile rendering engine (2.5D with fake 3D depth via layering)
- [ ] 3-level chunked coordinate system (Cell / Sector / Quadrant with origin shift)
- [ ] Camera system (pan, zoom, multi-level: station → sector → system → galaxy)
- [ ] Chunk manager (load/unload around camera, spatial hash map)
- [ ] Entity registry (source of truth for all entities, live + off-screen)
- [ ] Basic player ship in isometric space
- [ ] Mouse + keyboard input handling
- [ ] HUD framework inspired by cockpit instrument design (panels, tooltips, gauges)
- [ ] Shell integration with isometric view (commands trigger visual feedback)

**MVP 2:** Player navigates an infinite isometric space with origin shift. Shell commands affect the world.

> **Rendering philosophy:** The game uses hybrid 2.5D/3D. Isometric view is the primary perspective for all gameplay. First-person cockpit and station interior views come in later phases. Fake 3D (pre-rendered sprites, SubViewport 3D, parallax depth) is preferred wherever the player can't distinguish it from real 3D.

---

## Phase 3 — Station Core 🏠

**Goal:** Station building, resources, and production chains.

- [ ] Three-resource economy (Metallum, Crystallis, Deuterium)
- [ ] Building-as-levels system (upgrade, not duplicate)
- [ ] Core station modules: Extractors, Refineries, Solar Array, Storage
- [ ] Module placement on station grid (adjacency bonuses)
- [ ] Power system (generation, consumption, balance)
- [ ] Branching production chains (Circuit Board = Wafer + Wire)
- [ ] Module rarity system (Common → Legendary)
- [ ] Module upgrade system (levels 1-16)
- [ ] AXIA Level 3 (Aware) — proactive alerts, basic optimization tips

**MVP 3:** Player builds a station, mines resources, processes production chains. Modules can be crafted and upgraded.

---

## Phase 4 — Fleet & Combat ⚔️

**Goal:** Ships, drones, and real-time isometric combat.

- [ ] Ship classes (Fighter → Cruiser → Battleship → Destroyer → Carrier)
- [ ] Fleet composition with rapid-fire hard counters
- [ ] Ship module slots (weapons, shields, armor, engine, power, utility)
- [ ] Combat drones (orbiting flagship, equipment slots, formations)
- [ ] Lock-on targeting and auto-fire combat
- [ ] PvE enemies (pirates, rogue AI, alien creatures)
- [ ] Loot drops and cargo collection
- [ ] AXIA Level 4 (Adaptive) — fleet composition advice

**MVP 4:** Player builds a fleet and fights enemies in real-time. Fleet composition matters.

---

## Phase 5 — Defense & Threats 🛡️

**Goal:** Station defense and the activity-threat feedback loop.

- [ ] Activity → energy signature → attracts threats
- [ ] Defense modules: turrets, shields, traps, bulkheads
- [ ] Thermal system (radiators, overheating mechanic)
- [ ] 70% auto-repair after attacks
- [ ] Post-attack event logs (reviewable via shell)
- [ ] AXIA Level 5 (Analytical) — threat analysis, defense suggestions

**MVP 5:** Station gets attacked, player defends with layered systems, AXIA analyzes the results.

---

## Phase 6 — Exploration & Procedural Universe 🗺️

**Goal:** Infinite universe with procedural generation and anomalies.

- [ ] Seed-based deterministic sector generation
- [ ] Galaxy → Region → Constellation → System → Sector hierarchy
- [ ] Fog of war with intel decay
- [ ] Sector archetypes (resource-rich, hazardous, derelict, deep void)
- [ ] Anomaly system (data caches, derelict ships, void rifts, AXIA fragments)
- [ ] Hyperspace lanes (strategic chokepoints)
- [ ] AXIA Level 6 (Autonomous) — independent exploration, AXIA Data Fragment discovery

**MVP 6:** Player explores an infinite universe, discovers anomalies, finds rare resources and AXIA fragments.

---

## Phase 7 — The Constellation (Research Web) 🌳

**Goal:** The full PoE-style research web with 16 clusters and module unlocks.

- [ ] Constellation UI (radial web, zoom, pan, node selection)
- [ ] 16 clusters with Inner / Mid / Outer / Keystone depths
- [ ] 18 bridge nodes connecting adjacent clusters
- [ ] Blueprint nodes unlock module crafting recipes
- [ ] Research consumes refined products
- [ ] Cross-cluster synergies
- [ ] AXIA Level 7 (Strategic) — suggests optimal research paths

**MVP 7:** Player researches through the Constellation, unlocks module blueprints, and crafts new equipment.

---

## Phase 8 — Factions & Narrative 🏛️

**Goal:** Three-faction system, lore, and AXIA's story arc.

- [ ] Faction selection (HELIX, NOVA, VOID) with bonuses and aesthetics
- [ ] Lore entries discoverable through exploration
- [ ] AXIA story arc (fragments across Levels 0-9)
- [ ] 50+ unique anomaly events
- [ ] AXIA Levels 8-9 (Transcendent → Ascended) — story conclusion
- [ ] Tutorial / onboarding flow

---

## Phase 9 — Cockpit View 🚀

**Goal:** First-person cockpit perspective inside ships — a second way to experience the game.

- [ ] Cockpit 3D scene (SubViewport or fake 3D with pre-rendered panels)
- [ ] Instrument panels: navigation, ship status, threat radar, resource overview
- [ ] AXIA holographic interface inside cockpit (3D model or animated sprite)
- [ ] Viewport into space (see what's outside from cockpit perspective)
- [ ] Cockpit UI and isometric HUD share visual language (same design system)
- [ ] Toggle between isometric ↔ cockpit with smooth transition
- [ ] Cockpit state reflects isometric world (same data, different presentation)

**MVP 9:** Player can toggle into cockpit view, see instruments, interact with AXIA hologram, and navigate from first-person. Isometric remains the primary interface.

> The cockpit doesn't replace isometric gameplay — it's an immersion layer. Players who prefer top-down keep using isometric. Players who want immersion switch to cockpit. Both views show the same game state.

---

## Phase 10 — Polish & Release 🎨

**Goal:** Game feel, content, and Steam launch.

- [ ] Sound design (ambient, station ops, combat, AXIA voice)
- [ ] Music (atmospheric, procedurally layered)
- [ ] Visual polish (particles, lighting, UI animations)
- [ ] Full isometric art pass + cockpit art pass
- [ ] Achievement system
- [ ] Monolith of Contributors (credits)
- [ ] Steam integration (achievements, cloud saves)
- [ ] Playtesting and balancing
- [ ] **Steam Early Access Release**

---

## Phase 11 — Online MVP 🌐

**Goal:** First multiplayer features — offline to online transition.

- [ ] Server infrastructure (connection server, game state service)
- [ ] Player authentication
- [ ] Persistent world state (stations, territory, discoveries)
- [ ] Resource trading between players
- [ ] Basic PvP (raiding, fleet combat, bashing limits)

---

## Phase 12 — MMO & Endgame 🌍

**Goal:** Full MMO — territory, clans, warfare, endgame content.

- [ ] Clan and alliance system
- [ ] Territory sovereignty (claim, maintain, contest)
- [ ] Clan Wars and Alliance Combat System
- [ ] Void Gates (instanced wave-based PvE)
- [ ] Unknown Entities (endgame threats)
- [ ] Shared script repository (`vpm`)
- [ ] Server meshing for scale
- [ ] Expansion DLCs

---

## Phase 13 — Station Interior 🏠

**Goal:** First-person walkable station interiors — see your station from the inside.

- [ ] First-person navigation inside station (3D or pre-rendered corridors)
- [ ] Modules as rooms/bays (refinery room, drone bay, research lab)
- [ ] AXIA holographic terminal in station common area
- [ ] Visual link: isometric module layout maps to interior room layout
- [ ] Interact with systems from interior (alternative to isometric management)
- [ ] Station damage visible in interior (hull breaches, power failures)

> Station interiors are a long-term immersion feature, not a gameplay replacement. All station management works from isometric view. Interiors add atmosphere and a new way to experience the station.

---

*Want to contribute to a specific phase? Check the [Issues](../../issues) tab for tasks tagged with the relevant phase label.*
