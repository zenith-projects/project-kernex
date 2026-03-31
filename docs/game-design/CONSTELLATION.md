# PROJECT KERNEX — The Constellation (Research Web)

> *Version 0.1 — Pre-production Draft*
> *Inspired by Path of Exile's passive tree and Albion Online's destiny board.*

---

## Overview

The Constellation is PROJECT KERNEX's research system — a **massive web of interconnected nodes** radiating from the Station Core at the center. **16 clusters** spread outward like a constellation map, connected by **18 bridge nodes** that link adjacent clusters.

**Core rules:**
- **No locked branches.** Everything is reachable. You choose what to prioritize, not what to abandon.
- **Branches connect to each other** as they grow outward. Bridge nodes link adjacent clusters, creating shortcuts and synergies.
- **Every node unlocks something** — a module blueprint, a stat bonus, a capability, or a passive effect.
- **Modules are unlocked, then crafted.** Reaching a node gives you the blueprint. You still need to manufacture or buy the actual module. See [MODULES.md](MODULES.md) for the full catalog.
- **Basic modules come pre-researched.** New players start with essential modules unlocked so the game is immediately playable.

---

## The Map

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

---

## Node Types

Every point in the Constellation is one of these:

| Node Type | What It Does | Example |
|-----------|-------------|---------|
| **Blueprint** | Unlocks a module for crafting/buying | "Plasma Turret Mk2 Blueprint" |
| **Stat Bonus** | Passive permanent bonus | "+5% mining yield" |
| **Capability** | Unlocks a new ability or game feature | "Hyperspace Travel" |
| **Passive Effect** | Ongoing effect while researched | "Refineries consume 10% less energy" |
| **Keystone** | Major game-changing unlock (1 per cluster edge) | "Autonomous Fleet Command" |
| **Bridge** | Connects two adjacent clusters | Links FLEET ↔ WEAPONS |

**Keystones** are the PoE-style "big nodes" — they sit at the outer edge of each cluster and fundamentally change how a system works. You can eventually get them all, but reaching them requires pathing deep into a cluster.

---

## All 16 Clusters

Each cluster contains **stat nodes** (passive bonuses), **capability nodes** (new features), and **blueprint nodes** (module unlocks). Blueprint nodes reference modules from [MODULES.md](MODULES.md).

---

### PRODUCTION — Mining & Refining

*How you get and process raw materials.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +20% mining yield, +15% refinery speed | Improved Drill, Gas Collector |
| Mid | Byproduct recovery, rare ore extraction, -30% energy cost | Deep Core Extractor, Plasma Cutter, Planet Drill |
| Outer | Tier 3 materials, planet mining, transmutation | Molecular Disassembler, Cryo Radiator |
| **Keystone** | **Quantum Synthesis** — transmute any material | Quantum Harvester (Legendary) |

*Bridges to:* ENGINEERING, DEFENSE, LOGISTICS

---

### DEFENSE — Shields, Armor & Repair

*How your station and ships survive.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +25% hull HP, +40% shield capacity | Reinforced Shield, Reinforced Plating, Shield Generator Mk2 |
| Mid | Resistance shifting, self-repair, 85% auto-repair | Adaptive Shield, Nano-Armor, Shield Generator Mk3 |
| Outer | Energy absorption, organic regen, full rebuild | Phase Shield, Quantum Barrier, Fortress Shield |
| **Keystone** | **Fortress Protocol** — Citadel Mode | Phase Shield Array (Legendary), Living Hull (Legendary) |

*Bridges to:* PRODUCTION, OFFENSE, ENGINEERING

---

### OFFENSE — Station Weapons & Turrets

*How your station fights back.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +15% turret damage, tracking improvement | Laser Turret, Missile Battery |
| Mid | Splash damage, shield disable, armor piercing | Gauss Turret, EMP Tower, Minefield Deployer |
| Outer | Heavy damage, area denial, long range | Plasma Artillery, Railgun Platform |
| **Keystone** | **Orbital Ion Cannon** — devastating beam | Ion Cannon (Legendary), Gravity Well Generator (Epic) |

*Bridges to:* DEFENSE, WEAPONS, DRONES

---

### FLEET — Ships & Naval Doctrine

*What ships you can build and how fleets behave.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Fighter Doctrine, basic formations, +20% fleet speed | (Fighter class ships — crafted in Shipyard) |
| Mid | Cruiser/Battleship hulls, fleet capacity +50%, formation commands | Afterburner, Compact Fission reactor |
| Outer | Destroyer/Carrier/Dreadnought programs, fleet-wide buffs | Ship Fusion Core, Micro Antimatter reactor |
| **Keystone** | **Titan-Class** — mobile station with own fleet | (Titan class ship — endgame Shipyard) |

*Bridges to:* WEAPONS, DRONES, NAVIGATION

---

### WEAPONS — Ship Armament

*What goes on your ships.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +15% ship weapon damage, tracking improvement | Pulse Laser, Missile Pod |
| Mid | AoE, armor piercing, beam technology | Beam Cannon, Torpedo Launcher, Flak Battery |
| Outer | Shield bypass, multi-target pierce, heavy ordnance | Plasma Lance, Railgun, EMP Projector |
| **Keystone** | **Weapon Overcharge Matrix** — +50% damage, overheat risk | Singularity Torpedo (Legendary), Void Beam (Legendary) |

*Bridges to:* FLEET, OFFENSE, STEALTH

---

### DRONES — Autonomous Units

*Drone types, swarm AI, and automation.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +30% mining speed, combat drone unlock | Mining Drone Mk2 blueprint, Combat Drone blueprint |
| Mid | Swarm coordination, equipment slots, auto-explore | Heavy Drone blueprint, Scout Drone blueprint |
| Outer | 100+ swarm, boarding, self-replicating probes | Assault Drone blueprint, Deep Space Probe blueprint |
| **Keystone** | **Nano Swarm** — mine, fight, repair, scout simultaneously | Nano Swarm blueprint (Legendary) |

*Bridges to:* FLEET, OFFENSE, AUTOMATION

---

### EXPLORATION — Scanning & Discovery

*How you find new things in the void.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% scan range, archetype reveal | Sensor Array, Signal Booster |
| Mid | Auto-scout probes, anomaly detection | Deep Scanner, Probe Drone blueprint |
| Outer | Full sector intel, alien signals, void mapping | Omnisensor |
| **Keystone** | **Omniscience Array** — see entire constellation | Omnisensor (Epic) |

*Bridges to:* NAVIGATION, ANOMALY SCIENCE, STEALTH

---

### NAVIGATION — Propulsion & Travel

*How fast and far you go.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +30% speed, -20% fuel cost | Ion Drive |
| Mid | Inter-system travel, warp accuracy | Fusion Drive, Hyperspace Module |
| Outer | Permanent portals, temporary shortcuts | Warp Drive, Jump Drive |
| **Keystone** | **Wormhole Generator** — passages anywhere | Void Shift Engine (Legendary) |

*Bridges to:* FLEET, EXPLORATION, COLONIZATION

---

### KIRA — AI Companion

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

### AUTOMATION — Terminal & Scripting

*The terminal power system, independent from KIRA.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Command aliases, piping, terminal QoL | Script Processor |
| Mid | .vsh scripting, cron scheduler, 5 parallel scripts | Cron Engine, Parallel Executor |
| Outer | Event hooks, reactive scripts, dashboards | Event Reactor, Pipeline Controller |
| **Keystone** | **Self-Evolving Scripts** — scripts optimize themselves | Quantum Scheduler (Legendary) |

*Bridges to:* KIRA, DRONES, LOGISTICS

---

### ENGINEERING — Energy & Construction

*Power generation, build speed, and station structure.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | -20% build time, +30% energy output | Fission Reactor, RTG Unit, Heat Sink Array |
| Mid | Parallel build queues, major energy boost | Fusion Reactor, Phase-Change Cooler, Advanced Processor |
| Outer | Massive energy, instant build, megastructures | Antimatter Reactor, Quantum Reactor |
| **Keystone** | **Dyson Collector** — harvest star energy | Dyson Collector (Legendary), Plasma Radiator (Epic) |

*Bridges to:* PRODUCTION, DEFENSE, LOGISTICS

---

### ECONOMY — Trade & Finance

*How you make money and trade resources.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | NPC traders, market visibility, +15% profit | (Trading capability — no module, it's a feature) |
| Mid | Auto-trade routes, market analysis, speculation | Expanded Hold, Compressed Storage |
| Outer | Commerce hub, monopoly control, banking | Quantum Container |
| **Keystone** | **Galactic Exchange** — universal market | (Galaxy-wide trading feature) |

*Bridges to:* LOGISTICS, COLONIZATION, COMMUNICATIONS

---

### STEALTH — Covert Operations

*Hiding, spying, and information warfare.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | -30% energy signature, scanner jamming | Sensor Dampener, ECM Pod |
| Mid | Ship cloaking, spy probes, counter-intel | Cloaking Device, Decoy Generator, Signal Spoofer |
| Outer | Station invisibility, blueprint theft, vision hack | Active Cloak |
| **Keystone** | **Phantom Station** — appear in multiple locations | Phase Cloak (Legendary) |

*Bridges to:* WEAPONS, EXPLORATION, KIRA

---

### LOGISTICS — Supply Chains & Storage

*Moving and storing resources efficiently.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% storage capacity, basic transport | Station Warehouse, Cargo Shuttles |
| Mid | Shared storage, fast freight, auto-balancing | Compressed Storage, Basic Cargo Hold upgrades |
| Outer | Orbital depots, warp freight, zero-waste production | Orbital Depot, Quantum Container |
| **Keystone** | **Self-Optimizing Pipeline** — KIRA manages supply | Dimensional Vault (Legendary) |

*Bridges to:* PRODUCTION, ENGINEERING, AUTOMATION, ECONOMY

---

### COLONIZATION — Territory & Expansion

*Building outposts and claiming space.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Deploy basic outposts | Outpost Core |
| Mid | Specialized outposts, territory claims | Colony Hub, Industrial Module, Military Module |
| Outer | Mega-production, impenetrable base, system control | Sovereignty Beacon, Warp Gate |
| **Keystone** | **Empire Core** — passive bonuses galaxy-wide | Dyson Forge (Legendary) |

*Bridges to:* NAVIGATION, ECONOMY, COMMUNICATIONS

---

### ANOMALY SCIENCE — Alien Technology

*Researching anomalies, alien artifacts, and void phenomena.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | Harvest anomaly resources, study artifacts | (Anomaly harvesting capability) |
| Mid | Decode signals, tap void energy, restore ancient tech | Void Cooler, Gravity Well Generator |
| Outer | Use alien tech in station, void power, lost knowledge | Void Siphon, Alien Power Cell |
| **Keystone** | **Precursor Technology** — alien modules | Void Beam (Legendary), Living Hull (Legendary), Void Shift Engine (Legendary) |

*Bridges to:* EXPLORATION, KIRA, ENGINEERING

---

### COMMUNICATIONS — Signals & Coordination

*Comms range, encryption, and multiplayer coordination.*

| Depth | Stat/Capability Nodes | Module Blueprints Unlocked |
|-------|----------------------|---------------------------|
| Inner | +50% comms range, clearer signals | Signal Booster |
| Mid | Secure comms, alliance features, sector beacons | Encrypted Channel, Alliance Beacon |
| Outer | Instant galaxy comms, propaganda, jamming | SubSpace Relay, Propaganda Array |
| **Keystone** | **Quantum Entanglement Comms** — unblockable | Quantum Link (Legendary) |

*Bridges to:* KIRA, ECONOMY, COLONIZATION, STEALTH

---

## Bridge Connections Map

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

## Research Mechanics

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

*This document will expand as development progresses. Node counts, costs, and effects are design targets, not final values.*
*Last updated: March 2026*
