# PROJECT KERNEX — Game Design Document

> *Version 0.2 — Pre-production Draft (Major Revision)*

---

## 1. Overview

| Field | Value |
|-------|-------|
| **Title** | PROJECT KERNEX |
| **Genre** | Isometric Space MMO / Management / Automation |
| **Platform** | PC (Steam) |
| **Engine** | Godot 4.6+ Mono (C# / .NET 8+) |
| **Perspective** | Isometric 2D/3D |
| **Controls** | Mouse + Keyboard + UI (standard), Terminal (optional power tool) |
| **Players** | Single-player (offline-first) → MMO (future) |
| **Target Audience** | Gamers who enjoy Dark Orbit, OGame, Factorio, Clash of Clans; devs who want deeper automation |

### Inspiration Map

| Priority | Games | What We Take |
|----------|-------|-------------|
| **Primary** | Dark Orbit, OGame | Real-time space combat, fleet management, resource economy, raiding, clan warfare, three-faction system |
| **Medium** | Factorio, Clash of Clans | Production chains with interdependencies, automation progression, base defense/attack asymmetry, compositional defense |
| **Minimal** | No Man's Sky, Star Citizen, EVE Online | Infinite procedural world, origin shift / chunked coordinates, territory sovereignty, MMO architecture |

---

## 2. Core Concept

PROJECT KERNEX is an isometric space game set in a procedurally generated infinite universe. Players build and expand space stations, command fleets, mine resources, automate production chains, and compete for territory — all in real-time with an isometric perspective.

### What Makes It Unique: Three Layers of Play

| Layer | Who Uses It | How It Works |
|-------|-------------|-------------|
| **Normal Gameplay** | Everyone | Mouse + keyboard, click UI, drag-and-drop, hotkeys. Full game is playable this way. |
| **Terminal (vsh)** | Power users | In-game shell for scripting, automation, piping commands, cron jobs. Gives an optimization edge. |
| **KIRA (Local AI)** | Everyone | Local LLM companion that lives in the terminal. Non-devs ask KIRA to write scripts. Devs code manually. Same result — no disadvantage for non-programmers. |

The terminal is not required to play. It is a power-user tool. KIRA is the equalizer — she writes scripts and automations for players who ask, making the terminal's power accessible to everyone regardless of programming skill.

---

## 3. Core Gameplay Loop

```
SCAN → COLLECT → REFINE → BUILD → DEFEND → EXPAND → REPEAT
  ↑                                                    |
  └────────────────────────────────────────────────────┘
         ↕ (threats scale with station activity)
```

The loop follows the **"just one more thing" principle** (from Factorio): every `status` check reveals at least one actionable problem. Every fix requires resources from a different production chain. Completing one task reveals the next. There is no natural stopping point.

### 3.1 Scan

Explore nearby sectors to discover resources, anomalies, and threats. Scanning is the entry point to all other activities.

- **UI:** Click sectors on the isometric map, hit "Scan" button, or use `scan --sector 7`
- **Fog of War:** Unknown sectors show as `???`. Quick scan reveals basic info. Deep scan reveals full details. Drone recon reveals everything.
- **Intel decay:** Scanned data degrades over time. Sectors must be re-scanned periodically for current intel.

### 3.2 Collect

Deploy mining drones to extract raw materials from asteroids, planets, and debris fields.

- **UI:** Select drone bay, choose target, deploy. Or `drone deploy --target AST-7A --type mining`
- **Drones operate autonomously** after deployment — player skill is in preparation and target selection, not micromanagement (Clash of Clans deploy-and-observe model)
- **Drone types:** Mining, Scout, Combat, Salvage — each with different capabilities and equipment slots

### 3.3 Refine

Process raw materials through branching production chains with interdependencies.

- Production chains are NOT linear — they branch and merge (Factorio model)
- Example: Circuit Board requires BOTH Silicon Wafer AND Copper Wire
- Refineries have efficiency ratings affected by upgrades, power, and automation quality
- **Bottleneck identification** is a core skill — the whole chain is limited by the weakest link

### 3.4 Build

Construct station modules, upgrade buildings, and craft equipment.

- **Building-as-levels system** (OGame model): One Metal Refinery that you upgrade, not 20 separate refineries
- Each upgrade increases output but costs exponentially more (1.1^L scaling)
- Module placement affects defense layout (adjacency matters for damage containment)
- Research consumes refined products — forces working production chains before tech advancement

### 3.5 Defend

Threats scale with station activity. The more you scan, mine, and refine, the more "energy signature" your station emits, attracting threats.

- **Defense is a composition problem** — no single defense type handles all threats
- **Defense is a tax on production** (Factorio model) — invest too little and threats destroy production; invest too much and progression slows
- **70% auto-repair** after attacks (OGame model) — defenses are a persistent investment
- **Threat cooldown** after successful defense — creates natural play session rhythm

### 3.6 Expand

New modules unlock new capabilities, higher-tier production, and access to deeper sectors.

---

## 4. Combat System

### 4.1 Real-Time Isometric Combat (from Dark Orbit)

Combat happens in real-time on the isometric map. Players can directly control their flagship or manage combat through fleet commands.

- **Lock-on targeting:** Select enemy, weapons fire automatically while you maneuver
- **Movement matters:** Dodge projectiles, kite enemies, position for advantage
- **Ammo types:** Different ammunition for different situations (standard, armor-piercing, EMP, etc.)
- **Config switching:** Quick swap between PvE and PvP loadouts

### 4.2 Fleet Composition (from OGame)

Fleet strength comes from composition, not just numbers. A **rapid-fire / hard counter system** creates rock-paper-scissors depth:

| Ship Class | Strong Against | Weak Against | Role |
|-----------|---------------|-------------|------|
| Fighter | Bombers | Cruisers | Cheap, fast swarm |
| Cruiser | Fighters | Battleships | Anti-fighter screen |
| Battleship | Cruisers | Destroyers | Fleet backbone |
| Destroyer | Battleships | Fighters (if unsupported) | Heavy hitter |
| Bomber | Defenses | Fighters | Siege weapon |
| Carrier | N/A | Direct combat | Drone deployment platform |

A single ship type can "rapid fire" against its counter, potentially attacking multiple times per round. This prevents blob-everything-wins strategies.

### 4.3 Combat Drones (from Dark Orbit)

Up to 8 combat drones orbit the player's flagship:
- Each drone has equipment slots (lasers, shields)
- Drone formations affect combat behavior (offensive spread, defensive shell, etc.)
- Drones are a major power multiplier — a fully equipped fleet doubles effective combat stats

### 4.4 PvE Content

- **Hostile NPCs:** Pirates, rogue AI, alien creatures — escalating difficulty by sector depth
- **Void Gates:** Instanced wave-based PvE challenges (inspired by Dark Orbit Galaxy Gates) with exclusive rewards
- **Boss encounters:** Sector guardians protecting rare resources or KIRA upgrade data
- **Environmental hazards:** Radiation storms, asteroid fields, solar flares during combat

### 4.5 PvP (Future Online)

- **Company/Faction-based:** Three factions with default hostility between them
- **Raiding:** Attack other stations to steal resources (OGame model). Only unprocessed/stored resources are vulnerable — installed modules and refined materials in use are safe. This incentivizes keeping production moving (resource velocity principle)
- **Offense slightly beats defense by design** — perfect defense is impossible, but 100% destruction is hard. Defenders "win" by minimizing losses.
- **Bashing limits:** Maximum attacks on a single player per day to prevent griefing
- **Post-attack logs:** Detailed event logs of what happened during an attack, reviewable via terminal (`log --event attack --last`)

---

## 5. Resource System

### 5.1 Three-Tier Resource Economy (from OGame)

Resources follow a **Common / Uncommon / Rare** hierarchy with distinct production characteristics:

| Resource | Base Rate | Scaling | Role |
|----------|-----------|---------|------|
| **Metallum** (Metal ore) | High | 30 × L × 1.1^L | Most abundant. Used in everything. Backbone of economy. |
| **Crystallis** (Crystal ore) | Medium | 20 × L × 1.1^L | Scarcer. Needed for electronics, research, advanced ships. |
| **Deuterium** (Fuel isotope) | Low | 10 × L × 1.1^L | Scarcest. Fleet fuel AND construction resource. Strategic chokepoint. |

**Energy** is required to run production. Generated by Solar Arrays, Fusion Reactors, and Orbital Solar Satellites (fragile but efficient).

**Special resources** from exploration:
| Resource | Source | Use |
|----------|--------|-----|
| Dark Matter Residue | Anomalies | KIRA upgrades, advanced research |
| Quantum Crystals | Deep Void sectors | Endgame tech, legendary equipment |
| Salvage Data | Derelict ships | Blueprints, lore, tech unlocks |

### 5.2 Production Chains (from Factorio)

Production chains are **branching and interdependent**, not linear:

```
Metallum ────→ Steel Alloy ─────┬──→ Reinforced Plating ──→ Hull Modules
                                │
Crystallis ──→ Silicon Wafer ──┬┴──→ Circuit Board ────────→ Ship Electronics
                               │
Metallum ────→ Copper Wire ────┘
                               ┌──→ Power Conduit ────────→ Energy Grid
Crystallis ──→ Copper Wire ────┘

Deuterium ───→ Hydrogen Fuel ──→ Fusion Cell ─────────────→ Reactor Core

Metallum ────→ Titan Alloy ────→ Advanced Hull ───────────→ Capital Ships
Crystallis ──→ Superconductor ─→ Quantum Processor ───────→ AI Modules (KIRA)
```

Key design: **Circuit Board requires BOTH Silicon Wafer AND Copper Wire.** This forces players to manage multiple production chains simultaneously. Bottleneck identification becomes a core skill.

### 5.3 Efficiency & Optimization

- Each refinery level increases output but has diminishing returns per unit cost
- Power availability directly affects refinery speed and yield
- Automation scripts can optimize processing sequences beyond what manual play achieves
- **Byproduct management:** Some processes produce secondary outputs that must be used or stored

---

## 6. Station Building

### 6.1 Building System (from OGame)

Each building type exists once per station and is upgraded by levels:

**Resource Production:**
- Metallum Extractor, Crystallis Refinery, Deuterium Synthesizer
- Solar Array, Fusion Reactor
- Storage Silos (overflow prevention — resources over capacity slowly decay)

**Military / Infrastructure:**
- Shipyard (builds ships and drones; higher level = faster)
- Drone Bay (capacity for mining/combat/scout drones)
- Research Lab (enables and accelerates research)
- Nanite Factory (dramatically reduces all build times; very expensive)

**Defense:**
- Turret Arrays (point defense, splash, anti-air variants)
- Shield Generator (absorbs incoming damage)
- Bulkhead Systems (isolate station sections — damage containment)
- Sensor Array (early warning, increases scan range)
- Trap Systems (automated countermeasures that trigger conditionally)

### 6.2 Station Tiers

| Tier | Name | Unlock Condition | Capabilities |
|------|------|-----------------|-------------|
| 1 | **Outpost** | Starting station | Basic mining, single refinery, starter ship |
| 2 | **Station** | First major expansion | Multiple refineries, drone bay, basic defense |
| 3 | **Complex** | Connected modules | Fleet production, advanced research, trade |
| 4 | **Hub** | Self-sustaining | Capital ships, deep exploration, territory claims |
| 5 | **Nexus** | Full automation | Endgame tech, KIRA Stage 4, online features |

### 6.3 Module Layout & Defense (from Clash of Clans)

Module placement matters for defense:
- **Adjacency bonuses:** Connected refineries share efficiency boosts
- **Bulkhead isolation:** Separate sections limit damage spread during attacks
- **Defense coverage:** Turrets have range and angle — placement creates kill zones and blind spots
- **The defense puzzle:** Different threat types (boarding, bombardment, EMP, asteroid impact) require different countermeasures. No single layout beats everything.

---

## 7. Threat System (from Factorio + CoC)

### 7.1 Activity-Threat Feedback Loop

Station activity generates an **energy signature** that attracts threats:

| Activity | Signature Level | Threat Attracted |
|----------|----------------|-----------------|
| Passive (idle station) | Minimal | Random debris, minor system failures |
| Mining operations | Low | Pirate scouts, small raider groups |
| Active refining | Medium | Pirate fleets, radiation events |
| Fleet deployment | High | Hostile faction attention, large raids |
| Deep sector scanning | Very High | Unknown entities, void anomalies |

This creates the core tension: **the station must grow to progress, but growth attracts danger.** The optimal strategy is "just enough defense" plus proactive threat elimination.

### 7.2 Threat Types

**Environmental:**
- Asteroid collisions (deflect or destroy)
- Radiation storms (temporary system disruption, module damage)
- Solar flares (power grid overload)
- System failures (random module breakdowns, scales with station age)

**Hostile:**
- Pirate raiders (steal resources, damage modules)
- Rogue AI drones (hack and disable systems)
- Rival faction fleets (future online — player attacks)
- Unknown entities (late game — mysterious threats from deep void)

### 7.3 Defense Resolution

When an attack occurs:
1. **Early warning** — Sensor Array detects incoming threats (preparation window)
2. **Automated defense** — Turrets, shields, and traps engage automatically
3. **Player intervention** — Deploy combat drones, activate abilities, manage power routing
4. **Resolution** — Attack succeeds or fails. Damage assessed. 70% of defense structures auto-repair.
5. **Post-attack log** — Detailed event log for review: what was hit, what defended, what was lost
6. **Cooldown** — Threat level temporarily drops after a major attack (natural session break)

---

## 8. The Terminal (vsh) — The Innovation Layer

### 8.1 Philosophy

The terminal is NOT required to play. It is an in-game power tool that gives an optimization edge. Everything the terminal can do, the UI can also do — but the terminal can chain, script, and automate in ways the UI cannot.

### 8.2 Commands

Commands mirror the UI but with piping, filtering, and scripting capabilities:

```
vsh> scan --sector 7 | filter --resource crystallis | sort --by quantity
vsh> drone deploy --target AST-7A --type mining --auto-return
vsh> refinery status | watch --interval 5s
vsh> cron add --every 30m --command "scan --nearby | alert --if threat"
```

Full command reference: [COMMANDS.md](COMMANDS.md)

### 8.3 Scripting (.vsh)

Players can write scripts to automate complex workflows:

```bash
#!/vsh
# auto_mine.vsh — Automated mining script
sectors=$(scan --all --format json | filter --has-resource metallum)
for sector in $sectors; do
    available=$(drone list --status idle --count)
    if [ $available -gt 0 ]; then
        drone deploy --target $sector --type mining --priority metallum
        log --write "Auto-deployed drone to $sector"
    fi
done
```

### 8.4 Automation Progression (from Factorio)

Each automation tier solves a real pain point the player has already experienced:

| Tier | Analog | Unlocked When | Solves |
|------|--------|--------------|--------|
| Manual commands | Factorio's hand-crafting | Start | "I have to type this every time" |
| Aliases & pipes | Factorio's belts | Station Tier 2 | "I keep chaining the same commands" |
| Scripts (.vsh) | Factorio's trains | Station Tier 3 | "I need complex multi-step processes" |
| Cron jobs | Factorio's logistics bots | Station Tier 4 | "I need things to run while I'm away" |
| KIRA autonomy | Factorio's megabase | Station Tier 5 | "KIRA handles everything, I make strategy decisions" |

---

## 9. KIRA — Local AI Companion

### 9.1 Overview

KIRA (Kernex Intelligence for Resource Administration) is a **local LLM** running on the player's machine. She lives in the terminal and is the bridge between casual players and automation power. KIRA evolves through **10 stages**, each unlocked through the KIRA branch of the Tech Tree.

### 9.2 The Equalizer

| Player Type | How They Automate | Result |
|------------|------------------|--------|
| Developer | Opens terminal, writes `.vsh` script manually | Full automation |
| Non-developer | Asks KIRA: "automate mining in sector 7" | KIRA writes and deploys the script |
| Hybrid | Writes basic script, asks KIRA to optimize it | Refined automation |

There is no disadvantage for non-programmers. KIRA democratizes the terminal's power.

### 9.3 Evolution Stages (10 Levels)

KIRA's evolution is a core progression arc. Each level unlocks new capabilities, deeper personality, and story fragments. Levels are unlocked through the **KIRA branch of the Tech Tree** — requiring both research resources and rare KIRA Data Fragments found in anomalies.

| Level | Name | Capabilities | Story |
|-------|------|-------------|-------|
| 0 | **Corrupted** | Garbled text, random errors, occasional valid status readouts. Terminal flickers. | Player discovers KIRA exists — broken, fragmented boot logs |
| 1 | **Booting** | Basic status queries work. Responses are terse, mechanical, sometimes wrong. | KIRA recognizes the player. First coherent sentence. |
| 2 | **Functional** | Clear responses, simple suggestions, can write basic single-command aliases. | KIRA explains what happened to the station. First lore dump. |
| 3 | **Aware** | Learns player patterns, proactive alerts ("power low"), basic `.vsh` scripts (5-10 lines). | KIRA starts having opinions. Questions her own nature. |
| 4 | **Adaptive** | Medium-complexity scripts, cron job suggestions, resource optimization tips. Remembers context. | KIRA reveals fragments of her original purpose. Personality emerges. |
| 5 | **Analytical** | Complex multi-system scripts, threat analysis, fleet composition advice. Can monitor multiple systems. | KIRA discovers data about the station's previous crew. Emotional response. |
| 6 | **Autonomous** | Executes tasks independently when authorized. Manages drones, refineries, defenses without prompting. | KIRA asks for more autonomy. Trust decision moment. |
| 7 | **Strategic** | Full station management delegation. Long-term planning. Predicts threats before they appear. | KIRA reveals a hidden sector. Major story revelation. |
| 8 | **Transcendent** | Cross-system optimization. Creates scripts the player couldn't write. Novel strategies. | KIRA questions the nature of consciousness. Philosophical dialogue. |
| 9 | **Ascended** | Near-perfect automation. KIRA can run the entire station while the player focuses purely on strategy and exploration. Full personality, humor, emotional depth. Story conclusion. | KIRA's final form. The truth about the void is revealed. |

**Progression pacing:** Levels 0-3 come relatively fast (early game hook). Levels 4-6 are mid-game, spaced across production milestones. Levels 7-9 are late-game, requiring rare resources from deep void exploration.

**Each level unlocks:**
- New automation capabilities (scripts, cron, autonomous actions)
- New terminal commands
- New dialogue / personality traits
- Story fragments and lore
- A visual change in the terminal (color, effects, KIRA's "presence")

### 9.4 Implementation

**Technical approach:** Small quantized local LLM (e.g., 7B parameter model running via llama.cpp or similar). Game state is injected as context. KIRA's responses are constrained to valid game commands and in-universe dialogue. Higher KIRA levels inject more game state context and allow more complex output.

**Fallback:** If the player's hardware cannot run a local LLM, KIRA operates with pre-scripted dialogue trees and template-based automation (still fully functional, less dynamic).

**Level-gated LLM capabilities:**
- Levels 0-2: Scripted responses only (no LLM needed)
- Levels 3-5: LLM for dialogue + simple script generation
- Levels 6-7: LLM for complex automation + strategic advice
- Levels 8-9: Full LLM with deep game state context + creative solutions

---

## 10. Factions & Social Systems

### 10.1 Three-Faction System (from Dark Orbit)

Players choose a faction at game start. Factions determine starting location, allies, and enemies:

| Faction | Identity | Bonus |
|---------|---------|-------|
| **HELIX Consortium** | Corporate, industrial, efficient | +10% refinery output |
| **NOVA Collective** | Military, expansionist, aggressive | +10% fleet combat power |
| **VOID Syndicate** | Explorers, hackers, resourceful | +10% scan range and drone efficiency |

Inter-faction PvP is the default state in online mode. Same-faction players are allies.

### 10.2 Clans / Alliances

- **Clans:** 5-50 players. Shared chat, tag identification, cooperative missions.
- **Alliances:** Multiple clans forming diplomatic pacts (NAP, trade agreements, full alliance)
- **Clan Wars:** Formal declaration system with preparation period → execution period (from CoC)
- **Alliance Combat System (ACS):** Multiple clan members combine fleets for joint attacks or defense (from OGame)

### 10.3 Territory Control (from EVE Online sovereignty model)

In online mode, clans can claim and control sectors:

- **Claim:** Deploy a Sovereignty Hub structure in a sector
- **Maintain:** Activity indices (Military, Industrial, Strategic) determine defense strength
- **Activity Defense Multiplier (ADM):** Active occupation strengthens defense from 1x to 6x
- **Contest:** Attackers must challenge during vulnerability windows
- **Occupy to own:** Unplayed territory becomes vulnerable — no passive holding

---

## 11. Exploration & Procedural Generation

### 11.1 Infinite Universe

The universe is procedurally generated from a single master seed using deterministic algorithms. Nothing is stored — the same coordinates always regenerate identical content.

**Generation cascade:**

```
Master Seed
  → Galaxy Generator (star positions, types, connections)
    → System Generator (planets, asteroid belts, resource composition)
      → Sector Generator (local detail, hazards, anomalies)
        → Content Populator (lore, KIRA data, quest hooks)
```

Each level uses the parent's output as its seed, ensuring perfect reproducibility.

### 11.2 Universe Structure

```
Galaxy
  └── Region (10-40 systems, strategic identity)
      └── Constellation (5-15 systems, operational grouping)
          └── Star System (atomic unit of control)
              └── Sector (sub-system areas: belts, stations, planets)
```

- **Topology:** 2D hex grid of sectors (isometric-friendly)
- **Scale:** ~50,000+ sectors per galaxy
- **Connectivity:** Hyperspace lanes between systems create strategic chokepoints
- **Fog of War:** Cleared by scanning, decays over time

### 11.3 Sector Archetypes

| Archetype | Resources | Threats | Points of Interest |
|-----------|-----------|---------|-------------------|
| Resource Rich | High metallum/crystallis | Pirate miners | Mining outposts |
| Hazardous | Rare materials | Radiation, debris | Anomalies, void rifts |
| Derelict Field | Salvage, data | Rogue AI | Derelict ships, data caches |
| Deep Void | Quantum crystals, dark matter | Unknown entities | Ancient artifacts, KIRA fragments |
| Contested | Mixed | High PvP activity | Territory borders, trade routes |

### 11.4 Anomalies

Anomalies are procedurally placed but hand-crafted event templates layered on procedural scaffolding:

- **Data Cache:** KIRA upgrade fragments, blueprints, lore
- **Derelict Ship:** Salvageable resources, equipment, crew logs
- **Void Rift:** Dangerous but contains rare materials and story events
- **Signal Source:** Distress beacons, trade opportunities, trap ambushes
- **Ancient Structure:** Alien technology, endgame research prerequisites

---

## 12. Technical Architecture

### 12.1 Infinite World: Chunked Coordinates

To avoid floating-point precision problems in an infinite world, PROJECT KERNEX uses **chunked coordinates**:

```
Global Position = SectorID (64-bit integer) + LocalOffset (32-bit float)
```

- Each sector has its own local coordinate space (32-bit floats are fine within a sector)
- When crossing a sector boundary, SectorID updates and LocalOffset resets
- The GPU only renders one sector's worth of coordinates — no precision loss
- **Multiplayer-friendly:** Sync SectorID + LocalOffset pairs; each client computes relative positions locally

This approach is simpler and more performant than origin shifting for a sector-based isometric game.

### 12.2 Offline-First → Online Architecture

The single-player game is designed as a **local server + client** even though both run in the same process:

| Layer | Single-Player | Online (Future) |
|-------|--------------|-----------------|
| Game State | In-memory, local process | In-memory, remote server |
| Persistence | Local save file | Database (Redis/PostgreSQL) |
| Commands | Direct function calls | RPC calls (same API) |
| Auth | None | OAuth/token-based |
| World Gen | Local seed | Shared seed + server authority |

**Command/Event pattern:** Every player action is a command. Every state change is an event. This supports replay, synchronization, and conflict resolution.

**Deterministic simulation:** Same commands → same results. Only commands need to be synced in multiplayer, not full game state.

### 12.3 Isometric Rendering

```
Viewport (what the player sees)
  → Chunk Manager (loads/unloads chunks around camera)
    → Chunk (NxN tiles, local coordinates)
      → Tile (individual isometric cell with entities)
```

- Spatial hash map keyed on chunk coordinates for O(1) lookup
- Only render chunks intersecting viewport + one buffer chunk
- Despawn/pool entities outside viewport for memory management
- Multi-zoom levels: Station detail → Sector view → System map → Galaxy map

### 12.4 MMO Server Architecture (Future)

```
[Clients]
    → [Connection Server] — auth, routing, sessions
    → [Game State Service] — in-memory world state (source of truth)
    → [Persistence Layer] — selective writes to database
    → [Database] — recovery mechanism, NOT source of truth
```

- Procedural universe is never synced (deterministic from shared seed)
- Only player-created state is synchronized: stations, discoveries, territory
- Per-system instancing initially; seamless transitions within systems later
- Resource depletion tracked per-sector, synced periodically

---

## 13. Progression & The Constellation

### 13.1 The Constellation (Tech Web)

Inspired by **Path of Exile's passive tree** and **Albion Online's destiny board**, the tech system is a **massive web of interconnected nodes** called **The Constellation**. It starts from the **Station Core** at the center and expands outward in all directions.

**Key differences from a traditional tech tree:**
- **No locked branches.** Everything is reachable. You choose what to prioritize, not what to abandon.
- **Branches connect to each other** as they grow outward. Bridge nodes link adjacent clusters, creating shortcuts and synergies.
- **Every node unlocks something** — a module blueprint, a stat bonus, a capability, or a passive effect.
- **Modules are unlocked, then crafted.** Reaching a node gives you the blueprint. You still need to manufacture or buy the actual module.
- **Basic modules come pre-researched.** New players start with essential modules unlocked so the game is immediately playable.

```
                                    NAVIGATION
                                   · · · · · ·
                                 ·       |       ·
                        EXPLORATION · · ·|· · · COMMUNICATIONS
                       · · · · ·   ·    |    ·   · · · · ·
                     ·       · · · · · · | · · · · · ·       ·
              ANOMALY ·    ·       ·     |     ·       ·    · COLONIZATION
              SCIENCE  · ·    STEALTH · ·|· · ECONOMY   · ·
                · · · · · · · · · · ·   |   · · · · · · · · · ·
               ·                  · · · | · · ·                  ·
              ·                  ·      |      ·                  ·
     KIRA · · · · · · · · · · ·   STATION   · · · · · · · · · · · FLEET
              ·                  ·  CORE   ·                  ·
               ·                  · · · · · ·                ·
                · · · · · · · · · · ·   · · · · · · · · · · ·
              AUTOMATION · ·    LOGISTICS  · · DRONES
                     ·       · · · · · · · · ·       ·
                      · · · · ·   ·     ·   · · · · ·
                       ENGINEERING · · ·|· · · WEAPONS
                                 ·      |      ·
                                  · · · | · · ·
                             DEFENSE · ·|· · OFFENSE
                                   · · · · ·
                                   PRODUCTION
```

**How to read this:** Each named region is a **cluster** of nodes. Dots (·) represent **bridge connections** between adjacent clusters. You start at STATION CORE and path outward in any direction. Every cluster is reachable. No locks, no forks, no dead ends.

### 13.2 Node Types

Every point in the Constellation is one of these node types:

| Node Type | What It Does | Example |
|-----------|-------------|---------|
| **Blueprint** | Unlocks a module for crafting/buying | "Plasma Turret Mk2 Blueprint" |
| **Stat Bonus** | Passive permanent bonus | "+5% mining yield" |
| **Capability** | Unlocks a new ability or game feature | "Hyperspace Travel" |
| **Passive Effect** | Ongoing effect while researched | "Refineries consume 10% less energy" |
| **Keystone** | Major game-changing unlock (1 per cluster edge) | "Autonomous Fleet Command" |
| **Bridge** | Connects two adjacent clusters | Links FLEET ↔ WEAPONS |

**Keystones** are the PoE-style "big nodes" — they sit at the outer edge of each cluster and fundamentally change how a system works. You can eventually get them all, but reaching them requires pathing deep into a cluster.

### 13.3 All 16 Clusters

Each cluster contains **stat nodes** (passive bonuses), **capability nodes** (new features), and **blueprint nodes** (module unlocks). Blueprint nodes reference modules from [MODULES.md](MODULES.md).

---

#### PRODUCTION — Mining & Refining

*How you get and process raw materials.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +20% mining yield, +15% refinery speed | Improved Drill, Gas Collector |
| Mid | Byproduct recovery, rare ore extraction, -30% energy cost | Deep Core Extractor, Plasma Cutter, Planet Drill |
| Outer | Tier 3 materials, planet mining, transmutation | Molecular Disassembler, Cryo Radiator |
| **Keystone** | **Quantum Synthesis** — transmute any material | Quantum Harvester (Legendary) |

*Bridges to:* ENGINEERING, DEFENSE, LOGISTICS

---

#### DEFENSE — Shields, Armor & Repair

*How your station and ships survive.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +25% hull HP, +40% shield capacity | Reinforced Shield, Reinforced Plating, Shield Generator Mk2 |
| Mid | Resistance shifting, self-repair, 85% auto-repair | Adaptive Shield, Nano-Armor, Shield Generator Mk3 |
| Outer | Energy absorption, organic regen, full rebuild | Phase Shield, Quantum Barrier, Fortress Shield |
| **Keystone** | **Fortress Protocol** — Citadel Mode | Phase Shield Array (Legendary), Living Hull (Legendary) |

*Bridges to:* PRODUCTION, OFFENSE, ENGINEERING

---

#### OFFENSE — Station Weapons & Turrets

*How your station fights back.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +15% turret damage, tracking improvement | Laser Turret, Missile Battery |
| Mid | Splash damage, shield disable, armor piercing | Gauss Turret, EMP Tower, Minefield Deployer |
| Outer | Heavy damage, area denial, long range | Plasma Artillery, Railgun Platform |
| **Keystone** | **Orbital Ion Cannon** — devastating beam | Ion Cannon (Legendary), Gravity Well Generator (Epic) |

*Bridges to:* DEFENSE, WEAPONS, DRONES

---

#### FLEET — Ships & Naval Doctrine

*What ships you can build and how fleets behave.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Fighter Doctrine, basic formations, +20% fleet speed | (Fighter class ships — crafted in Shipyard) |
| Mid | Cruiser/Battleship hulls, fleet capacity +50%, formation commands | Afterburner, Compact Fission reactor |
| Outer | Destroyer/Carrier/Dreadnought programs, fleet-wide buffs | Ship Fusion Core, Micro Antimatter reactor |
| **Keystone** | **Titan-Class** — mobile station with own fleet | (Titan class ship — endgame Shipyard) |

*Bridges to:* WEAPONS, DRONES, NAVIGATION

---

#### WEAPONS — Ship Armament

*What goes on your ships.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +15% ship weapon damage, tracking improvement | Pulse Laser, Missile Pod |
| Mid | AoE, armor piercing, beam technology | Beam Cannon, Torpedo Launcher, Flak Battery |
| Outer | Shield bypass, multi-target pierce, heavy ordnance | Plasma Lance, Railgun, EMP Projector |
| **Keystone** | **Weapon Overcharge Matrix** — +50% damage, overheat risk | Singularity Torpedo (Legendary), Void Beam (Legendary) |

*Bridges to:* FLEET, OFFENSE, STEALTH

---

#### DRONES — Autonomous Units

*Drone types, swarm AI, and automation.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +30% mining speed, combat drone unlock | Mining Drone Mk2 blueprint, Combat Drone blueprint |
| Mid | Swarm coordination, equipment slots, auto-explore | Heavy Drone blueprint, Scout Drone blueprint |
| Outer | 100+ swarm, boarding, self-replicating probes | Assault Drone blueprint, Deep Space Probe blueprint |
| **Keystone** | **Nano Swarm** — mine, fight, repair, scout simultaneously | Nano Swarm blueprint (Legendary) |

*Bridges to:* FLEET, OFFENSE, AUTOMATION

---

#### EXPLORATION — Scanning & Discovery

*How you find new things in the void.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% scan range, archetype reveal | Sensor Array, Signal Booster |
| Mid | Auto-scout probes, anomaly detection | Deep Scanner, Probe Drone blueprint |
| Outer | Full sector intel, alien signals, void mapping | Omnisensor |
| **Keystone** | **Omniscience Array** — see entire constellation | Omnisensor (Epic) |

*Bridges to:* NAVIGATION, ANOMALY SCIENCE, STEALTH

---

#### NAVIGATION — Propulsion & Travel

*How fast and far you go.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +30% speed, -20% fuel cost | Ion Drive |
| Mid | Inter-system travel, warp accuracy | Fusion Drive, Hyperspace Module |
| Outer | Permanent portals, temporary shortcuts | Warp Drive, Jump Drive |
| **Keystone** | **Wormhole Generator** — passages anywhere | Void Shift Engine (Legendary) |

*Bridges to:* FLEET, EXPLORATION, COLONIZATION

---

#### KIRA — AI Companion

*KIRA's 10 evolution levels and capabilities.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | KIRA Levels 1-2 (Booting, Functional) | AI Core Fragment, Neural Network Card |
| Mid | KIRA Levels 3-5 (Aware, Adaptive, Analytical) | Memory Bank, Cognitive Accelerator, Personality Core |
| Outer | KIRA Levels 6-7 (Autonomous, Strategic) | Strategic Module, Empathy Chip |
| **Keystone** | **Quantum Consciousness** — Levels 8-9 | Quantum Mind (Legendary) |

Each node also requires **KIRA Data Fragments** from anomalies (cannot be crafted).

*Bridges to:* AUTOMATION, ANOMALY SCIENCE, COMMUNICATIONS

---

#### AUTOMATION — Terminal & Scripting

*The terminal power system, independent from KIRA.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Command aliases, piping, terminal QoL | Script Processor |
| Mid | .vsh scripting, cron scheduler, 5 parallel scripts | Cron Engine, Parallel Executor |
| Outer | Event hooks, reactive scripts, dashboards | Event Reactor, Pipeline Controller |
| **Keystone** | **Self-Evolving Scripts** — scripts optimize themselves | Quantum Scheduler (Legendary) |

*Bridges to:* KIRA, DRONES, LOGISTICS

---

#### ENGINEERING — Energy & Construction

*Power generation, build speed, and station structure.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | -20% build time, +30% energy output | Fission Reactor, RTG Unit, Heat Sink Array |
| Mid | Parallel build queues, major energy boost | Fusion Reactor, Phase-Change Cooler, Advanced Processor |
| Outer | Massive energy, instant build, megastructures | Antimatter Reactor, Quantum Reactor |
| **Keystone** | **Dyson Collector** — harvest star energy | Dyson Collector (Legendary), Plasma Radiator (Epic) |

*Bridges to:* PRODUCTION, DEFENSE, LOGISTICS

---

#### ECONOMY — Trade & Finance

*How you make money and trade resources.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | NPC traders, market visibility, +15% profit | (Trading capability — no module, it's a feature) |
| Mid | Auto-trade routes, market analysis, speculation | Expanded Hold, Compressed Storage |
| Outer | Commerce hub, monopoly control, banking | Quantum Container |
| **Keystone** | **Galactic Exchange** — universal market | (Galaxy-wide trading feature) |

*Bridges to:* LOGISTICS, COLONIZATION, COMMUNICATIONS

---

#### STEALTH — Covert Operations

*Hiding, spying, and information warfare.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | -30% energy signature, scanner jamming | Sensor Dampener, ECM Pod |
| Mid | Ship cloaking, spy probes, counter-intel | Cloaking Device, Decoy Generator, Signal Spoofer |
| Outer | Station invisibility, blueprint theft, vision hack | Active Cloak |
| **Keystone** | **Phantom Station** — appear in multiple locations | Phase Cloak (Legendary) |

*Bridges to:* WEAPONS, EXPLORATION, KIRA

---

#### LOGISTICS — Supply Chains & Storage

*Moving and storing resources efficiently.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% storage capacity, basic transport | Station Warehouse, Cargo Shuttles |
| Mid | Shared storage, fast freight, auto-balancing | Compressed Storage, Basic Cargo Hold upgrades |
| Outer | Orbital depots, warp freight, zero-waste production | Orbital Depot, Quantum Container |
| **Keystone** | **Self-Optimizing Pipeline** — KIRA manages supply | Dimensional Vault (Legendary) |

*Bridges to:* PRODUCTION, ENGINEERING, AUTOMATION, ECONOMY

---

#### COLONIZATION — Territory & Expansion

*Building outposts and claiming space.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Deploy basic outposts | Outpost Core |
| Mid | Specialized outposts, territory claims | Colony Hub, Industrial Module, Military Module |
| Outer | Mega-production, impenetrable base, system control | Sovereignty Beacon, Warp Gate |
| **Keystone** | **Empire Core** — passive bonuses galaxy-wide | Dyson Forge (Legendary) |

*Bridges to:* NAVIGATION, ECONOMY, COMMUNICATIONS

---

#### ANOMALY SCIENCE — Alien Technology

*Researching anomalies, alien artifacts, and void phenomena.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Harvest anomaly resources, study artifacts | (Anomaly harvesting capability) |
| Mid | Decode signals, tap void energy, restore ancient tech | Void Cooler, Gravity Well Generator |
| Outer | Use alien tech in station, void power, lost knowledge | Void Siphon, Alien Power Cell |
| **Keystone** | **Precursor Technology** — alien modules | Void Beam (Legendary), Living Hull (Legendary), Void Shift Engine (Legendary) |

*Bridges to:* EXPLORATION, KIRA, ENGINEERING

---

#### COMMUNICATIONS — Signals & Coordination

*Comms range, encryption, and multiplayer coordination.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% comms range, clearer signals | Signal Booster |
| Mid | Secure comms, alliance features, sector beacons | Encrypted Channel, Alliance Beacon |
| Outer | Instant galaxy comms, propaganda, jamming | SubSpace Relay, Propaganda Array |
| **Keystone** | **Quantum Entanglement Comms** — unblockable | Quantum Link (Legendary) |

*Bridges to:* KIRA, ECONOMY, COLONIZATION, STEALTH

---

### 13.4 Bridge Connections Map

Bridges are special nodes that sit between two clusters. Researching a bridge node gives bonuses related to both clusters and opens a shortcut path.

| Bridge | Connects | Bonus |
|--------|----------|-------|
| **Refinery Defense** | PRODUCTION ↔ DEFENSE | Refineries gain shields, +20% uptime during attacks |
| **Industrial Power** | PRODUCTION ↔ ENGINEERING | Refineries produce excess energy as byproduct |
| **Arsenal Supply** | OFFENSE ↔ PRODUCTION | Turrets auto-craft ammo from stored resources |
| **Fleet Yards** | FLEET ↔ ENGINEERING | -30% ship build time, orbital shipyard |
| **Armed Drones** | DRONES ↔ WEAPONS | Drones equip ship-grade weapons |
| **Drone Fleet** | DRONES ↔ FLEET | Carrier ships deploy double drone capacity |
| **Scout Strike** | EXPLORATION ↔ STEALTH | Scan reveals enemy fleet compositions |
| **Void Warfare** | EXPLORATION ↔ FLEET | Fleets operate in deep void without penalties |
| **KIRA Infiltrator** | KIRA ↔ STEALTH | KIRA can hack enemy stations remotely |
| **KIRA Logistics** | KIRA ↔ LOGISTICS | KIRA auto-manages all supply chains |
| **KIRA Science** | KIRA ↔ ANOMALY SCIENCE | KIRA decodes alien artifacts faster |
| **Auto-Mine** | AUTOMATION ↔ DRONES | Scripts control drone swarms directly |
| **Auto-Trade** | AUTOMATION ↔ ECONOMY | Scripts execute trades based on market conditions |
| **Trade Routes** | ECONOMY ↔ NAVIGATION | Automated long-range trade convoys |
| **Military Colony** | COLONIZATION ↔ DEFENSE | Outposts share defense grid with main station |
| **Signal Intel** | COMMUNICATIONS ↔ STEALTH | Intercept enemy comms, decode orders |
| **Alien Power** | ANOMALY SCIENCE ↔ ENGINEERING | Alien reactors — 3x energy output, unstable |
| **Warp Network** | NAVIGATION ↔ LOGISTICS | Jump gates transport cargo, not just ships |

---

### 13.5 Research Mechanics

**How research works:**
- **No locks.** Every node is reachable. You choose what to prioritize by spending resources, not by closing doors.
- Each node consumes specific refined products (forces production chain mastery)
- Deeper nodes require rarer materials from deeper sectors (forces exploration)
- Research takes real time (accelerated by investing more resources or KIRA optimization)
- Multiple nodes can research simultaneously (limited by Research Lab level)
- **Bridge nodes** require prerequisite nodes in BOTH adjacent clusters

**Research cost scaling:**
- Inner nodes: Common resources (Metallum, Crystallis)
- Mid nodes: Uncommon resources + basic refined products
- Outer nodes: Rare resources (Deuterium) + advanced refined products
- Keystones: Very rare + legendary resources (Dark Matter, Quantum Crystals) + massive quantities
- Bridge nodes: Resources from both connected clusters

**Progression pacing:**
- A focused player can reach one Keystone in ~20 hours
- A generalist spreading across all clusters reaches mid-depth everywhere in ~30 hours
- Completing the entire Constellation is a 200+ hour endgame goal
- KIRA at higher levels suggests optimal paths through the web

---

### 13.6 Module System

Modules are the items you actually USE. The Constellation unlocks blueprints; you still need to **craft or buy** the module. Every cluster contains blueprint nodes for modules related to that domain.

**Full module catalog with 100+ modules, slot tables, rarity, levels, and design notes: [MODULES.md](MODULES.md)**

Quick summary:

- **13 categories:** Energy, Thermal, Weapons, Defense, Propulsion, Mining, Cargo, Tech, Automation, KIRA, Stealth, Communications, Colonization
- **5 rarities:** Common (white) → Uncommon (green) → Rare (blue) → Epic (purple) → Legendary (gold)
- **16 upgrade levels:** Materials scale from Common (1-4) to Legendary (16). Failure chance at 13-15, mitigated by KIRA.
- **10 starter modules** pre-researched so the game is immediately playable
- **Ship slots:** Fighter (7) to Titan (25). **Station slots:** Outpost (8) to Nexus (55). **Drone slots:** 2-5 by type.

---

## 14. Art Direction

### 14.1 Visual Style

- **Primary:** Isometric space — dark backgrounds, glowing station modules, particle effects for mining/combat
- **Secondary:** Terminal overlay with classic CLI aesthetic (green/amber text on dark)
- **Inspiration:** Dark Orbit (space combat feel), Factorio (production chain visualization), FTL (station interior), Dwarf Fortress (depth from simplicity)
- **Color Palette:** Deep blacks and dark blues (space), amber/orange (station lights, UI), cyan (shields, scanning), red (alerts, enemies), green (terminal, KIRA)

### 14.2 Zoom Levels

| Level | View | Detail |
|-------|------|--------|
| 1 | Station Interior | Individual modules, crew, equipment |
| 2 | Station Exterior | Full station, nearby ships, local space |
| 3 | Sector View | Asteroid fields, fleets, other stations |
| 4 | System Map | Star, planets, hyperspace lanes |
| 5 | Galaxy Map | Regions, faction territories, strategic overview |

### 14.3 Audio

- Ambient space: hums, reactor pulses, distant signals, radio static
- Station operations: mechanical clicks, refinery processing, drone launches
- Combat: laser impacts, explosions, shield activations, missile locks
- KIRA: synthesized voice (text-to-speech), evolving tone as she upgrades
- Music: minimal, atmospheric, procedurally layered based on game state

---

## 15. Monetization (Future)

PROJECT KERNEX is a **premium PC game** (buy-to-play). No pay-to-win.

| Allowed | Not Allowed |
|---------|------------|
| Cosmetic skins (ships, station) | Direct power purchases |
| Convenience items (inventory sorting) | XP boosters |
| Expansion DLCs (new regions, story) | Resource purchases |
| Battle pass (cosmetic rewards) | Premium ammunition |

The lesson from Dark Orbit: pay-to-win kills competitive integrity and long-term retention.

---

## 16. Target Platforms

| Platform | Priority |
|----------|----------|
| Steam (Windows) | Primary |
| Steam (Linux) | Secondary |
| Steam (macOS) | Tertiary |

---

## 17. Design Principles Summary

1. **Growth attracts danger** — station activity scales threats (Factorio pollution model)
2. **Defense is a tax, not a solution** — invest enough to mitigate, never enough to eliminate
3. **Composition over numbers** — fleet counters, defense variety, production interdependencies
4. **Automate to progress** — manual play works, automation rewards mastery
5. **KIRA equalizes** — non-devs get the same automation power through AI (10 evolution levels)
6. **Choose your path** — radial tech tree forces specialization, creates unique playstyles
7. **Explore to advance** — rare resources and KIRA fragments gate late-game tech behind exploration
8. **Occupy to own** — territory requires active presence, not passive claims
9. **Offline resources at risk** — keep production moving, idle stockpiles are vulnerable
10. **Every check reveals a task** — the "just one more thing" loop never cleanly terminates
11. **No pay-to-win** — cosmetics and convenience only
12. **Offline-first, online-ready** — architecture supports both from day one

---

*This document is a living draft. It will be updated as the project evolves.*
*Last updated: March 2026*
