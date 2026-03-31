# PROJECT KERNEX — Development Roadmap

> *This roadmap is a living document and subject to change based on community feedback and development priorities.*
> *Each phase builds on the previous one. MVP milestones mark playable checkpoints.*

---

## Phase 0 — Foundation 🏗️ `CURRENT`

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

## Phase 1 — Shell & KIRA (Core Innovation) 🖥️🤖

**Goal:** Build the shell and KIRA first — they are the core innovation and differentiate the game from everything else. Even before the isometric world exists, the shell + KIRA should be functional and impressive.

- [ ] Shell emulator overlay (toggleable, resizable panel)
- [ ] Command parser and execution engine
- [ ] Core commands: `help`, `status`, `clear`, `log`, `kira`
- [ ] Piping system (`|`) and command chaining
- [ ] Aliases (`alias mine="drone deploy --type mining"`)
- [ ] Basic `.vsh` scripting (loops, conditionals, variables)
- [ ] Cron jobs (scheduled task execution)
- [ ] KIRA scripted dialogue engine (Levels 0-2: Corrupted → Booting → Functional)
- [ ] KIRA game state context injection
- [ ] Local LLM integration prototype (llama.cpp, 7B quantized)
- [ ] KIRA writes `.vsh` scripts on player request
- [ ] Hardware detection and automatic fallback (LLM vs scripted)

**MVP 1:** Shell works, KIRA responds, player can type commands and KIRA writes scripts. The core innovation is playable.

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
