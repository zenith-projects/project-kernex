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

## Phase 1 — Shell & KIRA (Core Innovation) 🖥️🤖 `IN PROGRESS`

**Goal:** Build the shell and KIRA first — they are the core innovation and differentiate the game from everything else. Even before the isometric world exists, the shell + KIRA should be functional and impressive.

### Shell (vsh) — Terminal Emulator

- [x] **Shell emulator overlay** — `ShellOverlay` (CanvasLayer) with `ShellDisplay` (scrollable RichTextLabel output) + `ShellInput` (LineEdit with command history via Up/Down arrows). Components follow project composition rules: each is `.tscn` + `.cs` in `src/Components/HUD/`.
- [x] **Command parser and execution engine** — `CommandParser` (stateless tokenizer with quote support), `CommandRegistry` (name + alias lookup), `ParsedCommand` (value object with positional args + flags), `CommandResult` (factory with typed outputs: Success, Error, Info, Kira, Clear). All pure C# in `src/Systems/Shell/`.
- [x] **Core commands** — `help`, `status`, `clear`, `whoami`, `uptime`, `kira`. Each implements `ICommand` interface. `HelpCommand` lists all registered commands. `KiraCommand` bridges to `KiraEngine`. Located in `src/Systems/Shell/Commands/`.
- [x] **Shell test scene** — `ShellTestScene.tscn` in `src/Screens/DevTools/`, F6-runnable standalone scene with dark background, welcome banner, all commands registered.
- [ ] Piping system (`|`) and command chaining
- [ ] Aliases (`alias mine="drone deploy --type mining"`)
- [ ] Basic `.vsh` scripting (loops, conditionals, variables)
- [ ] Cron jobs (scheduled task execution)

### KIRA — AI Companion

- [x] **KIRA scripted dialogue engine (Levels 0-2)** — `ScriptedDialogueProvider` with keyword-based response banks per level. Level 0 (Corrupted): 70% garbled via `GlitchTextGenerator` (character corruption, dropouts, insertions), 30% glitched scripted response. Level 1 (Booting): terse mechanical responses, 15% chance of `[SIGNAL LOST]` append. Level 2 (Functional): clear helpful responses, no glitching.
- [x] **KIRA game state context injection** — `KiraPromptBuilder` constructs system prompts with station state (hull, power, drones, threat, etc.) filtered by `KiraLevelData.AllowedContextKeys`. Level 0 sees nothing, Level 2 sees 5 keys, Level 5+ sees everything. Context always appended to prompt, even with custom prompts.
- [x] **Local LLM integration** — `LlamaSharpProvider` wraps LLamaSharp 0.26.0 (C# bindings for llama.cpp). Uses `InteractiveExecutor` with `MemoryClear()` before each call for fresh context. ChatML format (`<|im_start|>`) for Qwen compatibility. Configurable: Temperature, TopP, TopK, MinP, MaxTokens, RepeatPenalty, FrequencyPenalty, PresencePenalty, Seed, Mirostat (mode/tau/eta), AntiPrompts. Response cleaning pipeline strips role prefixes, artifacts, and asterisk actions.
- [x] **10-level evolution system** — `KiraLevelData` Resource (`.tres`) for all 10 levels (Corrupted → Ascended). Each defines: personality notes, response delay, glitch intensity, allowed context keys, LLM toggle, output color. Levels 0-2 always scripted, 3+ use LLM with scripted fallback.
- [x] **Emotion system** — `KiraResponse.Parse()` extracts `[EMOTION]` tags from LLM output. 11 emotions: NEUTRAL, CURIOUS, HAPPY, SAD, ANGRY, CRAZY, CRYING, CAUTIOUS, RELAX, SHY, VANISHED. Emotion drives portrait changes with crossfade transition (0.25s Sine tween). KIRA waifu portraits (10 emotion variants + VANISHED empty screen) in `src/Assets/Textures/Kira/`.
- [x] **Persistent memory system** — `KiraMemoryStore` saves all messages to JSON (`user://kira_memory/messages.json`). Features: keyword search (stopword filtering, relevance ranking), long-term summaries via 3B model (batches of 20 messages → 3-5 bullet points stored in `summaries.json`). Prompt injection: summaries always included + keyword matches for relevant past messages. Chat restored on scene reload (last 30 messages).
- [x] **Proactive messaging engine** — `KiraProactiveEngine` uses the 3B model for autonomous KIRA messages. Escalating behavior: 15s (casual/curious) → +20s (needy/shy) → +30s (emotional/dramatic) → gives up. Fresh chat: KIRA introduces herself. Visual indicator: portrait border gold flash (2-pulse tween). State persisted (gave_up, attempt count). Resets when player speaks.
- [x] **Dual-model architecture** — Two Qwen2.5 models loaded simultaneously:
  - **7B** (`qwen2.5-7b-instruct-q4_k_m.gguf`, 4.4 GB) — Main chat, higher quality responses
  - **3B** (`qwen2.5-3b-instruct-q4_k_m.gguf`, 2.0 GB) — Proactive decisions, memory summarization, fast inference
  - Both Apache 2.0 license, GGUF Q4_K_M quantization, ~6.5 GB total RAM
- [x] **Debug/test scene** — `KiraTestScene.tscn` with comprehensive controls:
  - Left: KiraPanel (chat with KIRA, emotion portrait with crossfade)
  - Right: Collapsible debug sections (KIRA, MODEL, INFERENCE, SYSTEM PROMPT, GAME CONTEXT, OUTPUT, PROACTIVE, MEMORY)
  - All parameters adjustable via sliders with tooltips
  - Editable system prompt with anti-prompts
  - Mock game context (hull, power, drones, storage, threat, sector, faction)
  - Config persisted in `user://kira_debug_config.tres` (auto-save on every change)
  - JetBrains Mono font, dark+gold glassmorphism theme (`src/Assets/Themes/Kira/kira_debug_theme.tres`), animated nebula background shader (`src/Assets/Themes/Kira/Shaders/`)
- [ ] KIRA writes `.vsh` scripts on player request
- [ ] Hardware detection and automatic fallback (LLM vs scripted)

### AI Models — Technical Details

| Model | File | Size | License | Use |
|-------|------|------|---------|-----|
| Qwen2.5-7B-Instruct | `models/qwen2.5-7b-instruct-q4_k_m.gguf` | 4.4 GB | Apache 2.0 | Main chat (Level 3+) |
| Qwen2.5-3B-Instruct | `models/qwen2.5-3b-instruct-q4_k_m.gguf` | 2.0 GB | Apache 2.0 | Proactive + summarization |

**Runtime:** LLamaSharp 0.26.0 (NuGet) with CPU backend. GPU offload supported via `GpuLayerCount`.

**Qwen2.5 supports tool/function calling** natively via ChatML format. This opens the path for KIRA to execute game commands directly (scan sectors, deploy drones, manage refineries) — planned for the `.vsh` script generation feature.

### Next Steps (Phase 1 remaining)

1. **Tool calling integration** — KIRA calls game functions directly instead of just injecting state. Enables: `get_hull_status()`, `deploy_drone(sector)`, `scan_sector(id)`. Foundation for KIRA writing `.vsh` scripts.
2. **Piping and aliases** — Shell command chaining (`scan | filter | deploy`) and shortcut system.
3. **`.vsh` scripting** — Basic script interpreter with loops, conditionals, variables.
4. **Hardware detection** — Auto-detect RAM/VRAM and select 3B vs 7B model. Fallback to scripted-only if insufficient hardware.

**MVP 1:** Shell works, KIRA responds with personality and memory, player can type commands and KIRA generates context-aware responses with emotions. ✅ *Partially achieved — shell + KIRA chat functional, scripting pending.*

---

## Phase 2 — Core Engine & Isometric World 🌍

**Goal:** Isometric renderer, chunked coordinate system, and basic player interaction.

- [ ] Isometric tile rendering engine
- [ ] 3-level chunked coordinate system (Cell / Sector / Quadrant with origin shift)
- [ ] Camera system (pan, zoom, multi-level: station → sector → system → galaxy)
- [ ] Chunk manager (load/unload around camera, spatial hash map)
- [ ] Entity registry (source of truth for all entities, live + off-screen)
- [ ] Basic player ship in isometric space
- [ ] Mouse + keyboard input handling
- [ ] Basic UI framework (HUD, panels, tooltips)
- [ ] Shell integration with isometric view (commands trigger visual feedback)

**MVP 2:** Player navigates an infinite isometric space with origin shift. Shell commands affect the world.

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
- [ ] KIRA Level 3 (Aware) — proactive alerts, basic optimization tips

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
- [ ] KIRA Level 4 (Adaptive) — fleet composition advice

**MVP 4:** Player builds a fleet and fights enemies in real-time. Fleet composition matters.

---

## Phase 5 — Defense & Threats 🛡️

**Goal:** Station defense and the activity-threat feedback loop.

- [ ] Activity → energy signature → attracts threats
- [ ] Defense modules: turrets, shields, traps, bulkheads
- [ ] Thermal system (radiators, overheating mechanic)
- [ ] 70% auto-repair after attacks
- [ ] Post-attack event logs (reviewable via shell)
- [ ] KIRA Level 5 (Analytical) — threat analysis, defense suggestions

**MVP 5:** Station gets attacked, player defends with layered systems, KIRA analyzes the results.

---

## Phase 6 — Exploration & Procedural Universe 🗺️

**Goal:** Infinite universe with procedural generation and anomalies.

- [ ] Seed-based deterministic sector generation
- [ ] Galaxy → Region → Constellation → System → Sector hierarchy
- [ ] Fog of war with intel decay
- [ ] Sector archetypes (resource-rich, hazardous, derelict, deep void)
- [ ] Anomaly system (data caches, derelict ships, void rifts, KIRA fragments)
- [ ] Hyperspace lanes (strategic chokepoints)
- [ ] KIRA Level 6 (Autonomous) — independent exploration, KIRA Data Fragment discovery

**MVP 6:** Player explores an infinite universe, discovers anomalies, finds rare resources and KIRA fragments.

---

## Phase 7 — The Constellation (Research Web) 🌳

**Goal:** The full PoE-style research web with 16 clusters and module unlocks.

- [ ] Constellation UI (radial web, zoom, pan, node selection)
- [ ] 16 clusters with Inner / Mid / Outer / Keystone depths
- [ ] 18 bridge nodes connecting adjacent clusters
- [ ] Blueprint nodes unlock module crafting recipes
- [ ] Research consumes refined products
- [ ] Cross-cluster synergies
- [ ] KIRA Level 7 (Strategic) — suggests optimal research paths

**MVP 7:** Player researches through the Constellation, unlocks module blueprints, and crafts new equipment.

---

## Phase 8 — Factions & Narrative 🏛️

**Goal:** Three-faction system, lore, and KIRA's story arc.

- [ ] Faction selection (HELIX, NOVA, VOID) with bonuses and aesthetics
- [ ] Lore entries discoverable through exploration
- [ ] KIRA story arc (fragments across Levels 0-9)
- [ ] 50+ unique anomaly events
- [ ] KIRA Levels 8-9 (Transcendent → Ascended) — story conclusion
- [ ] Tutorial / onboarding flow

---

## Phase 9 — Polish & Release 🎨

**Goal:** Game feel, content, and Steam launch.

- [ ] Sound design (ambient, station ops, combat, KIRA voice)
- [ ] Music (atmospheric, procedurally layered)
- [ ] Visual polish (particles, lighting, UI animations)
- [ ] Full isometric art pass
- [ ] Achievement system
- [ ] Monolith of Contributors (credits)
- [ ] Steam integration (achievements, cloud saves)
- [ ] Playtesting and balancing
- [ ] **Steam Early Access Release**

---

## Phase 10 — Online MVP 🌐

**Goal:** First multiplayer features — offline to online transition.

- [ ] Server infrastructure (connection server, game state service)
- [ ] Player authentication
- [ ] Persistent world state (stations, territory, discoveries)
- [ ] Resource trading between players
- [ ] Basic PvP (raiding, fleet combat, bashing limits)

---

## Phase 11 — MMO & Endgame 🌍

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

*Want to contribute to a specific phase? Check the [Issues](../../issues) tab for tasks tagged with the relevant phase label.*
