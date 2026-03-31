# PROJECT KERNEX — Development Roadmap

> *This roadmap is a living document and subject to change based on community feedback and development priorities.*
> *Each phase builds on the previous one. MVP milestones mark playable checkpoints.*

---

## Phase 0 — Foundation 🏗️ `CURRENT`

**Goal:** Establish project structure, game design, community infrastructure, and technical architecture.

- [x] Define core game concept
- [x] Create repository structure
- [x] Write LICENSE, CLA, CONTRIBUTING guidelines
- [x] Draft Game Design Document (v0.2)
- [x] Research reference games (Dark Orbit, OGame, Factorio, CoC, NMS, Star Citizen)
- [x] Choose game engine and tech stack — **Godot 4.6+ Mono (C# / .NET 8+)**
- [ ] Define command/event architecture (offline-first → online-ready)
- [ ] Set up development environment documentation
- [ ] Set up CI/CD pipeline
- [ ] Create Discord server for community
- [ ] Define art style guide (isometric)
- [ ] Define audio guidelines

---

## Phase 1 — Core Engine & Isometric World 🌍

**Goal:** Isometric renderer, chunked coordinate system, and basic player interaction.

- [ ] Isometric tile rendering engine
- [ ] Chunked coordinate system (64-bit sector ID + 32-bit local offset)
- [ ] Camera system (pan, zoom, multi-level zoom: station → sector → system → galaxy)
- [ ] Chunk manager (load/unload around camera, spatial hash map)
- [ ] Basic player entity in isometric space
- [ ] Mouse + keyboard input handling
- [ ] Basic UI framework (HUD, panels, tooltips)
- [ ] Scene management (menus, gameplay, transitions)

**MVP 1:** Player can navigate an isometric space environment with camera controls.

---

## Phase 2 — Station Core 🏠

**Goal:** Build the base — station modules, resources, and the building loop.

- [ ] Three-resource economy (Metallum, Crystallis, Deuterium)
- [ ] Resource data model (types, quantities, storage, overflow decay)
- [ ] Building-as-levels system (upgrade, not duplicate)
- [ ] Core buildings: Extractors, Refineries, Solar Array, Storage
- [ ] Module placement on station grid (adjacency system)
- [ ] Build/upgrade UI (requirements, timers, costs)
- [ ] Power system (generation, consumption, balance)
- [ ] Basic production chain (ore → alloy → component)
- [ ] Branching production interdependencies (Circuit Board = Wafer + Wire)

**MVP 2:** Player builds a station, mines resources, processes production chains.

---

## Phase 3 — Fleet & Combat ⚔️

**Goal:** Ships, drones, and real-time isometric combat.

- [ ] Ship data model (classes, equipment slots, stats)
- [ ] Fleet composition system with hard counters (rapid-fire mechanic)
- [ ] Ship building in Shipyard (queued construction)
- [ ] Basic ship movement and navigation in isometric space
- [ ] Lock-on targeting and auto-fire combat
- [ ] Combat drones (orbiting flagship, equipment slots, formations)
- [ ] PvE enemies: pirates, rogue AI, alien creatures
- [ ] Loot drops and cargo collection
- [ ] Equipment system (weapons, shields, generators, ammo types)
- [ ] Equipment upgrade path (levels 1-16)

**MVP 3:** Player builds a fleet and fights enemies in real-time isometric combat.

---

## Phase 4 — Defense & Threats 🛡️

**Goal:** Station defense, the activity-threat feedback loop, and attack/defense gameplay.

- [ ] Threat system: activity generates energy signature → attracts threats
- [ ] Environmental threats (asteroids, radiation storms, solar flares)
- [ ] Hostile threats (pirate raids, rogue AI attacks)
- [ ] Defense structures: turrets (point, splash, anti-air), shields, traps
- [ ] Defense placement affecting coverage (kill zones, blind spots)
- [ ] Bulkhead system (damage isolation between modules)
- [ ] 70% auto-repair after attacks
- [ ] Threat cooldown after major attacks
- [ ] Post-attack event log
- [ ] Early warning system (sensor array, preparation window)

**MVP 4:** Station is attacked, player defends with layered defenses, reviews attack logs.

---

## Phase 5 — Exploration & Procedural World 🗺️

**Goal:** Infinite procedurally generated universe with exploration mechanics.

- [ ] Seed-based deterministic sector generation
- [ ] Galaxy → Region → Constellation → System → Sector hierarchy
- [ ] Hex grid sector map (isometric-friendly)
- [ ] Fog of war (unknown → scanned → explored)
- [ ] Intel decay over time
- [ ] Sector archetypes (resource-rich, hazardous, derelict, deep void)
- [ ] Anomaly system (data caches, derelict ships, void rifts, ancient structures)
- [ ] Hyperspace lanes between systems (strategic chokepoints)
- [ ] Multi-zoom galaxy map (sector → system → galaxy views)
- [ ] Scout drones for remote exploration

**MVP 5:** Player explores an infinite universe, discovers anomalies, finds rare resources in deep sectors.

---

## Phase 6 — Terminal & Automation 🖥️

**Goal:** In-game terminal (vsh) and scripting system as the power-user layer.

- [ ] Terminal emulator overlay (toggleable, resizable)
- [ ] Command parser and execution engine
- [ ] Core commands: `help`, `status`, `scan`, `drone`, `refinery`, `build`, `defense`, `map`, `log`
- [ ] Piping system (`|`) and command chaining
- [ ] Aliases (`alias mine="drone deploy --type mining"`)
- [ ] `.vsh` scripting language (loops, conditionals, variables)
- [ ] Cron jobs (scheduled automation)
- [ ] `watch` command (real-time monitoring)
- [ ] Script editor (in-game)
- [ ] Command output ↔ isometric view sync (commands trigger visual feedback)

**MVP 6:** Power users can automate their entire station through terminal scripts.

---

## Phase 7 — KIRA AI 🤖

**Goal:** Local LLM companion that equalizes automation access for all players.

- [ ] KIRA dialogue engine (scripted fallback for low-spec hardware)
- [ ] Local LLM integration (llama.cpp or similar, 7B quantized model)
- [ ] Game state context injection into LLM prompts
- [ ] KIRA evolution stages (0-4) with unlock progression
- [ ] KIRA writes `.vsh` scripts on player request
- [ ] Proactive alerts and suggestions based on game state
- [ ] KIRA personality system (evolves with stages)
- [ ] Story fragments delivered through KIRA dialogue
- [ ] Hardware detection and automatic fallback (LLM vs scripted)

**MVP 7:** Non-dev player asks KIRA "automate mining in sector 7" and KIRA writes, deploys, and monitors the script.

---

## Phase 8 — Tech Tree & Progression 📊

**Goal:** Research system, equipment progression, and station tiers.

- [ ] Tech tree with prerequisites (Energy → Computing → Materials → Combat → Utility)
- [ ] Research consumes refined products (forces working production chains)
- [ ] Station tier progression (Outpost → Station → Complex → Hub → Nexus)
- [ ] Ship class unlocks through research
- [ ] Propulsion tech (Chemical → Ion → Hyperspace drives)
- [ ] Combat tech (+10% damage/shields/armor per level)
- [ ] Mutually exclusive research paths (encourage different playstyles)

---

## Phase 9 — Factions & Lore 🏛️

**Goal:** Three-faction system, narrative, and world-building.

- [ ] Faction selection at game start (HELIX, NOVA, VOID)
- [ ] Faction-specific starting locations, bonuses, and aesthetics
- [ ] Lore entries discoverable through exploration
- [ ] KIRA story arc (fragments across evolution stages)
- [ ] 50+ unique anomaly events
- [ ] Crew logs, ancient messages, data caches with narrative
- [ ] Tutorial / onboarding flow

---

## Phase 10 — Polish & Release 🎨

**Goal:** Game feel, content, and Steam launch preparation.

- [ ] Sound design (ambient space, station ops, combat, KIRA voice)
- [ ] Music (atmospheric, procedurally layered by game state)
- [ ] Visual polish (particles, lighting, UI animations)
- [ ] Full isometric art pass (station modules, ships, environments)
- [ ] Achievement system
- [ ] Monolith of Contributors (credits)
- [ ] Steam integration (achievements, cloud saves)
- [ ] Playtesting and balancing
- [ ] Steam store page, trailer, press kit
- [ ] **Steam Early Access Release**

---

## Phase 11 — Online MVP 🌐

**Goal:** First multiplayer features — transition from offline to online.

- [ ] Server infrastructure (connection server, game state service)
- [ ] Player authentication (OAuth)
- [ ] Persistent world state database (stations, territory, discoveries)
- [ ] Inter-player communication (comms system)
- [ ] Resource trading between players
- [ ] Per-system instancing (each system = instance)
- [ ] Basic PvP (raiding, fleet combat, bashing limits)
- [ ] Shield/cooldown system after attacks

---

## Phase 12 — MMO Features 🌍

**Goal:** Full MMO experience — territory, clans, warfare.

- [ ] Clan system (creation, management, chat, tags)
- [ ] Alliance system (NAP, trade agreements, full alliance)
- [ ] Territory sovereignty (claim, maintain, contest sectors)
- [ ] Activity Defense Multiplier (active occupation strengthens defense)
- [ ] Clan Wars (preparation → execution cycle)
- [ ] Alliance Combat System (combined fleets)
- [ ] Global faction map (territory visualization)
- [ ] Shared script repository (`vpm` — void package manager)
- [ ] Leaderboards (individual, clan, faction)
- [ ] Server meshing for high player density

---

## Phase 13 — Endgame & Expansion 🚀

**Goal:** Deep endgame content and continued development.

- [ ] Void Gates (instanced wave-based PvE, exclusive rewards)
- [ ] Unknown Entities (endgame threat from deep void)
- [ ] Ancient alien technology research
- [ ] Capital ships (massive fleet flagships)
- [ ] Cross-region warfare events
- [ ] Expansion DLCs (new regions, lore, mechanics)
- [ ] Community-driven events and content

---

*Want to contribute to a specific phase? Check the [Issues](../../issues) tab for tasks tagged with the relevant phase label.*
