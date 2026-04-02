# PROJECT KERNEX — Overview & Design Pillars

---

## Overview

| Field | Value |
|-------|-------|
| **Title** | PROJECT KERNEX |
| **Genre** | Hybrid 2.5D/3D Space MMO / Management / Automation |
| **Platform** | PC (Steam) |
| **Engine** | Godot 4.6+ Mono (C# / .NET 8+) |
| **Perspective** | Isometric (exploration, building, combat) + First-person (cockpit, station interiors) |
| **Controls** | Mouse + Keyboard + UI (standard), Shell (optional power tool) |
| **Players** | Single-player (offline-first) → MMO (future) |
| **Target Audience** | Gamers who enjoy Dark Orbit, OGame, Factorio, Clash of Clans; devs who want deeper automation |

### Inspiration Map

| Priority | Games | What We Take |
|----------|-------|-------------|
| **Primary** | Dark Orbit, OGame | Real-time space combat, fleet management, resource economy, raiding, clan warfare, three-faction system |
| **Medium** | Factorio, Clash of Clans | Production chains with interdependencies, automation progression, base defense/attack asymmetry, compositional defense |
| **Minimal** | No Man's Sky, Star Citizen, EVE Online | Infinite procedural world, origin shift / chunked coordinates, territory sovereignty, MMO architecture |

---

## Core Concept

PROJECT KERNEX is a hybrid 2.5D/3D space game set in a procedurally generated infinite universe. Players build and expand space stations, command fleets, mine resources, automate production chains, and compete for territory — blending isometric and first-person perspectives. Exploration, mining, combat, and base building happen in isometric view. Ships have a first-person cockpit with instruments and a holographic AXIA interface. Station interiors are walkable in first-person (future). Fake 3D is used wherever the player can't tell the difference — the feel matters, not the technical approach.

### What Makes It Unique: Three Layers of Play

| Layer | Who Uses It | How It Works |
|-------|-------------|-------------|
| **Normal Gameplay** | Everyone | Mouse + keyboard, click UI, drag-and-drop, hotkeys. Full game is playable this way. |
| **Shell (vsh)** | Power users | In-game shell for scripting, automation, piping commands, cron jobs. Gives an optimization edge. |
| **AXIA (Local AI)** | Everyone | Local LLM companion that lives in the shell. Non-devs ask AXIA to write scripts. Devs code manually. Same result — no disadvantage for non-programmers. |

The shell is not required to play. It is a power-user tool. AXIA is the equalizer — she writes scripts and automations for players who ask, making the shell's power accessible to everyone regardless of programming skill.

---

## Art Direction

### Visual Style

- **Isometric view:** Dark space backgrounds, glowing station modules, particle effects for mining/combat. Primary gameplay perspective.
- **Cockpit view:** First-person inside ships — instrument panels, holographic AXIA display, viewport into space. UI panels in isometric mode are visually inspired by cockpit instruments, so both perspectives share the same design language.
- **Station interior:** First-person walkable station interiors (future). See your modules, storage bays, and AXIA's hologram up close.
- **Shell overlay:** Classic CLI aesthetic (gold text on dark) — consistent across all perspectives.
- **Rendering philosophy:** Fake 3D wherever possible. Pre-rendered sprites, SubViewport 3D for cockpit, 2.5D tricks for depth. If the player perceives 3D, it doesn't matter how it's achieved.
- **Inspiration:** Dark Orbit (space combat feel), Factorio (production chain visualization), FTL (ship interior), Dwarf Fortress (depth from simplicity), Star Citizen (cockpit immersion)
- **Color Palette:** Deep blacks and dark blues (space), amber/gold (station lights, UI, cockpit instruments), cyan (shields, scanning), red (alerts, enemies), green (shell, AXIA)

### Views & Perspectives

| View | Perspective | When |
|------|------------|------|
| **Isometric World** | Top-down isometric (2.5D) | Exploration, mining, combat, base building (primary) |
| **Cockpit** | First-person (3D/fake 3D) | Inside ship — instruments, navigation, AXIA hologram |
| **Station Interior** | First-person (3D/fake 3D) | Walk inside your station (future) |
| **Sector Map** | Strategic overhead | Asteroid fields, fleets, other stations |
| **System Map** | Abstract | Star, planets, hyperspace lanes |
| **Galaxy Map** | Abstract | Regions, faction territories, strategic overview |

The isometric view is the initial development focus. Cockpit and station interior views come in later phases.

### Audio

- Ambient space: hums, reactor pulses, distant signals, radio static
- Station operations: mechanical clicks, refinery processing, drone launches
- Combat: laser impacts, explosions, shield activations, missile locks
- AXIA: synthesized voice (text-to-speech), evolving tone as she upgrades
- Music: minimal, atmospheric, procedurally layered based on game state

---

## Monetization (Future)

PROJECT KERNEX is a **premium PC game** (buy-to-play). No pay-to-win.

| Allowed | Not Allowed |
|---------|------------|
| Cosmetic skins (ships, station) | Direct power purchases |
| Convenience items (inventory sorting) | XP boosters |
| Expansion DLCs (new regions, story) | Resource purchases |
| Battle pass (cosmetic rewards) | Premium ammunition |

The lesson from Dark Orbit: pay-to-win kills competitive integrity and long-term retention.

---

## Target Platforms

| Platform | Priority |
|----------|----------|
| Steam (Windows) | Primary |
| Steam (Linux) | Secondary |
| Steam (macOS) | Tertiary |

---

## Design Principles Summary

1. **Growth attracts danger** — station activity scales threats (Factorio pollution model)
2. **Defense is a tax, not a solution** — invest enough to mitigate, never enough to eliminate
3. **Composition over numbers** — fleet counters, defense variety, production interdependencies
4. **Automate to progress** — manual play works, automation rewards mastery
5. **AXIA equalizes** — non-devs get the same automation power through AI (10 evolution levels)
6. **Choose your path** — radial tech tree forces specialization, creates unique playstyles
7. **Explore to advance** — rare resources and AXIA fragments gate late-game tech behind exploration
8. **Occupy to own** — territory requires active presence, not passive claims
9. **Offline resources at risk** — keep production moving, idle stockpiles are vulnerable
10. **Every check reveals a task** — the "just one more thing" loop never cleanly terminates
11. **No pay-to-win** — cosmetics and convenience only
12. **Offline-first, online-ready** — architecture supports both from day one

---

*Last updated: March 2026*
