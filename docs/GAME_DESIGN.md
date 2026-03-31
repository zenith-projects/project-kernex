# PROJECT KERNEX — Game Design Document

> *Version 0.1 — Pre-production Draft*

---

## 1. Overview

| Field | Value |
|-------|-------|
| **Title** | PROJECT KERNEX |
| **Genre** | Space Management / Automation / Simulation |
| **Platform** | PC (Steam) |
| **Engine** | TBD |
| **Perspective** | Terminal-based with optional 2D/3D visualization |
| **Players** | Single-player (online concepts planned for future) |
| **Target Audience** | Developers, sysadmins, gamers who enjoy Factorio, Zachtronics, Hacknet |

---

## 2. Concept

The player is the sole operator of a derelict space station. All interaction happens through a terminal interface — a custom shell called `vsh` (Void Shell). The core loop involves scanning space for resources, deploying automated drones, processing raw materials through increasingly complex refinery chains, and expanding the station's capabilities.

What makes PROJECT KERNEX unique: **programming is not just a theme — it's the gameplay**. Players write actual scripts to automate tasks, creating a progression curve where manual operations give way to sophisticated automation pipelines.

---

## 3. Core Gameplay Loop

```
SCAN → COLLECT → REFINE → BUILD → EXPAND → REPEAT
  ↑                                           |
  └───────────────────────────────────────────┘
```

### 3.1 Scan

The player uses the terminal to scan nearby sectors for resources, anomalies, and threats.

```
vsh> scan --sector 7 --depth full
[SCAN] Sector 7 — 3 asteroid fields detected
  ├── AST-7A: Iron (high), Silicon (medium)
  ├── AST-7B: Titanium (low), Rare Earth (trace)
  └── AST-7C: Ice (high), Helium-3 (medium)
[WARN] Radiation anomaly detected at coordinates 7.42.18
```

### 3.2 Collect

Deploy mining drones to extract raw materials from asteroids, planets, or debris.

```
vsh> drone deploy --target AST-7A --type mining --priority iron
[DRONE-04] Deployed to AST-7A
[DRONE-04] ETA: 12 minutes
[DRONE-04] Estimated yield: ~450 units iron ore
```

### 3.3 Refine

Process raw materials through the station's refinery system.

```
vsh> refinery start --input iron_ore --output steel_alloy --quantity 200
[REFINERY-A] Processing 200 units iron_ore
[REFINERY-A] Output: ~180 units steel_alloy (90% efficiency)
[REFINERY-A] Time remaining: 8 minutes
[REFINERY-A] Power consumption: 45 kW
```

### 3.4 Build

Use refined materials to build new station modules, upgrade systems, or craft equipment.

```
vsh> build module --type solar_array --location bay-3
[BUILD] Requirements:
  ├── Steel Alloy: 50/50 ✓
  ├── Silicon Wafer: 30/30 ✓
  └── Copper Wire: 20/20 ✓
[BUILD] Construction started in Bay 3
[BUILD] Time remaining: 25 minutes
```

### 3.5 Expand

New modules unlock new commands, capabilities, and areas to explore.

---

## 4. The Terminal (vsh)

### 4.1 Built-in Commands

The terminal provides a set of built-in commands that mimic real Unix/Linux utilities:

**Navigation & Info**
- `status` — Station overview (power, resources, modules, alerts)
- `scan` — Scan sectors for resources and anomalies
- `map` — Display known sector map
- `help` — Command reference
- `man <command>` — Detailed command manual
- `log` — View station event log
- `clear` — Clear terminal

**Resources & Production**
- `drone` — Manage mining/scout/combat drones
- `refinery` — Manage refinery operations
- `inventory` — View stored resources
- `build` — Construct modules and equipment
- `craft` — Craft smaller items and components

**Station Management**
- `power` — Power grid management
- `systems` — View and manage station systems
- `repair` — Repair damaged modules
- `upgrade` — Upgrade existing modules

**Communication & AI**
- `kira` — Interact with AI companion
- `comms` — Communication array (future: online features)
- `beacon` — Manage distress/trade beacons

**Scripting & Automation**
- `cron` — Schedule automated tasks
- `script` — Run/manage scripts
- `pipe` — Chain command outputs
- `alias` — Create command shortcuts
- `watch` — Monitor values in real time

### 4.2 Piping and Chaining

Commands can be piped together like a real shell:

```
vsh> scan --sector 7 | filter --resource titanium | drone deploy --auto
```

### 4.3 Scripting

Players can write `.vsh` scripts to automate complex workflows:

```bash
#!/vsh
# auto_mine.vsh — Automated mining script

sectors=$(scan --all --format json | filter --has-resource iron)

for sector in $sectors; do
    available=$(drone list --status idle --count)
    if [ $available -gt 0 ]; then
        drone deploy --target $sector --type mining --priority iron
        log --write "Auto-deployed drone to $sector"
    fi
done
```

### 4.4 Cron Jobs

Schedule recurring tasks:

```
vsh> cron add --every 30m --command "scan --nearby | alert --if threat"
[CRON] Job #3 added: scan+alert every 30 minutes
```

---

## 5. AI Companion — KIRA

### 5.1 Overview

KIRA (Kinetic Intelligence for Resource Administration) is the station's AI system. She starts broken — only capable of basic responses — and is gradually repaired and upgraded throughout the game.

### 5.2 Interaction

KIRA is accessed through the terminal:

```
vsh> kira "What's our power status?"
[KIRA] Power grid is at 73% capacity. Solar arrays generating
       420 kW. Refinery A consuming 45 kW. Recommend shutting
       down non-essential systems during night cycle.
```

### 5.3 Evolution Stages

| Stage | Name | Capabilities |
|-------|------|-------------|
| 0 | Fragmented | Basic status queries, garbled responses |
| 1 | Functional | Clear responses, basic suggestions |
| 2 | Adaptive | Learns player patterns, proactive alerts |
| 3 | Autonomous | Can execute simple tasks independently |
| 4 | Sentient | Full personality, complex dialogue, story reveals |

### 5.4 Implementation

**Phase 1 (MVP):** Pre-scripted dialogue tree with contextual responses based on game state.

**Phase 2 (Enhanced):** Template-based responses with variable injection and personality modifiers.

**Phase 3 (Advanced):** Optional local LLM integration (e.g., a small quantized model) for more dynamic and contextual responses. This remains optional and the game must be fully functional without it.

---

## 6. Resource System

### 6.1 Raw Materials

| Resource | Source | Rarity |
|----------|--------|--------|
| Iron Ore | Asteroids | Common |
| Silicon | Asteroids, Planets | Common |
| Ice | Comets, Ice Fields | Common |
| Copper | Asteroids | Moderate |
| Titanium | Dense Asteroids | Moderate |
| Helium-3 | Gas Giants, Comets | Rare |
| Rare Earth | Ancient Asteroids | Rare |
| Dark Matter Residue | Anomalies | Very Rare |
| Quantum Crystals | Deep Void | Legendary |

### 6.2 Refinery Chain

```
Iron Ore ──────→ Steel Alloy ──────→ Reinforced Plating
Silicon ───────→ Silicon Wafer ────→ Circuit Board
Copper ────────→ Copper Wire ──────→ Power Conduit
Titanium ──────→ Titan Alloy ──────→ Advanced Hull
Ice ───────────→ Water ────────────→ Hydrogen Fuel
Helium-3 ──────→ Fusion Cell ──────→ Reactor Core
Rare Earth ────→ Superconductor ───→ Quantum Processor
```

### 6.3 Efficiency & Optimization

- Refineries have efficiency ratings (percentage of input converted to output)
- Efficiency can be improved through upgrades and better automation scripts
- Players can discover optimal processing sequences through experimentation
- Power availability affects refinery speed and efficiency

---

## 7. Exploration

### 7.1 Sector Map

The game world is divided into sectors on a grid. Each sector is procedurally generated with:

- Resource composition
- Anomalies (positive and negative)
- Environmental hazards (radiation, debris fields, solar flares)
- Points of interest (derelict ships, data caches, alien artifacts)

### 7.2 Fog of War

Sectors start as unknown (`???`). Scanning reveals basic information. Deep scans reveal full details. Physical drone exploration reveals everything.

### 7.3 Anomalies

Anomalies are special events or locations that provide unique rewards or challenges:

- **Data Cache:** Contains fragments of lore, KIRA upgrade data, or blueprints
- **Derelict Ship:** Salvageable resources and equipment
- **Void Rift:** Dangerous but may contain rare materials
- **Signal Source:** Story events, future online interaction points

---

## 8. Threat System

### 8.1 Threat Types

- **Asteroids:** Collision course objects requiring deflection
- **Radiation Storms:** Temporary system disruption
- **System Failures:** Random module breakdowns
- **Power Surges:** Overloads requiring quick response
- **Unknown Entities:** (Late game) mysterious threats from deep space

### 8.2 Defense

Defense is managed through the terminal:

```
vsh> defense status
[DEFENSE] Turret Array: Online (4/6 turrets operational)
[DEFENSE] Shield Generator: 85% charge
[DEFENSE] Early Warning: Active (range: 3 sectors)

vsh> defense engage --target incoming-asteroid-12
[DEFENSE] Engaging asteroid with Turret Array
[DEFENSE] Impact probability reduced from 78% to 3%
```

---

## 9. Progression

### 9.1 Station Tiers

| Tier | Name | Unlock |
|------|------|--------|
| 1 | Outpost | Starting station |
| 2 | Station | First major expansion |
| 3 | Complex | Multiple connected modules |
| 4 | Hub | Self-sustaining base |
| 5 | Nexus | (Endgame) Full automation achieved |

### 9.2 Tech Tree

A tech tree governs what the player can build, craft, and research. Research requires specific resources and time. Some research paths are mutually exclusive, encouraging different playstyles.

---

## 10. Online Concepts (Future)

> **Note:** These are design concepts for future development. The game ships as single-player first.

### 10.1 Station Network

Players can connect their stations to form a network:

- Trade resources via in-game "protocols"
- Share automation scripts on a "package manager" (`vpm`)
- Cooperative resource gathering
- Competitive sector control

### 10.2 Communication Protocol

Inter-station communication uses an in-game protocol that simulates real networking:

```
vsh> comms send --to STATION-BRAVO --message "Trade request: 100 titanium for 50 fusion cells"
[COMMS] Message sent via SubSpace Relay
[COMMS] Ping: 2.3 ls (light-seconds)
```

---

## 11. Art Direction

### 11.1 Visual Style

- **Primary:** Terminal/CLI aesthetic — green/amber text on dark background
- **Secondary:** Minimalist 2D/3D visualizations for maps, station view, sector scans
- **Inspiration:** Hacknet, Duskers, FTL, Dwarf Fortress
- **Color Palette:** Deep blacks, terminal greens, amber warnings, cyan highlights, red alerts

### 11.2 Audio

- Ambient space sounds (hums, static, distant signals)
- Mechanical sounds for station operations
- KIRA's voice (text-to-speech or synthesized)
- Minimal, atmospheric music

---

## 12. Target Platforms

| Platform | Priority |
|----------|----------|
| Steam (Windows) | Primary |
| Steam (Linux) | Secondary |
| Steam (macOS) | Tertiary |

---

*This document is a living draft. It will be updated as the project evolves.*
*Last updated: March 2026*
