# PROJECT KERNEX — Overview & Design Pillars

---

## Overview

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

## Core Concept

PROJECT KERNEX is an isometric space game set in a procedurally generated infinite universe. Players build and expand space stations, command fleets, mine resources, automate production chains, and compete for territory — all in real-time with an isometric perspective.

### What Makes It Unique: Three Layers of Play

| Layer | Who Uses It | How It Works |
|-------|-------------|-------------|
| **Normal Gameplay** | Everyone | Mouse + keyboard, click UI, drag-and-drop, hotkeys. Full game is playable this way. |
| **Terminal (vsh)** | Power users | In-game shell for scripting, automation, piping commands, cron jobs. Gives an optimization edge. |
| **KIRA (Local AI)** | Everyone | Local LLM companion that lives in the terminal. Non-devs ask KIRA to write scripts. Devs code manually. Same result — no disadvantage for non-programmers. |

The terminal is not required to play. It is a power-user tool. KIRA is the equalizer — she writes scripts and automations for players who ask, making the terminal's power accessible to everyone regardless of programming skill.

---

## Art Direction

### Visual Style

- **Primary:** Isometric space — dark backgrounds, glowing station modules, particle effects for mining/combat
- **Secondary:** Terminal overlay with classic CLI aesthetic (green/amber text on dark)
- **Inspiration:** Dark Orbit (space combat feel), Factorio (production chain visualization), FTL (station interior), Dwarf Fortress (depth from simplicity)
- **Color Palette:** Deep blacks and dark blues (space), amber/orange (station lights, UI), cyan (shields, scanning), red (alerts, enemies), green (terminal, KIRA)

### Zoom Levels

| Level | View | Detail |
|-------|------|--------|
| 1 | Station Interior | Individual modules, crew, equipment |
| 2 | Station Exterior | Full station, nearby ships, local space |
| 3 | Sector View | Asteroid fields, fleets, other stations |
| 4 | System Map | Star, planets, hyperspace lanes |
| 5 | Galaxy Map | Regions, faction territories, strategic overview |

### Audio

- Ambient space: hums, reactor pulses, distant signals, radio static
- Station operations: mechanical clicks, refinery processing, drone launches
- Combat: laser impacts, explosions, shield activations, missile locks
- KIRA: synthesized voice (text-to-speech), evolving tone as she upgrades
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
5. **KIRA equalizes** — non-devs get the same automation power through AI (10 evolution levels)
6. **Choose your path** — radial tech tree forces specialization, creates unique playstyles
7. **Explore to advance** — rare resources and KIRA fragments gate late-game tech behind exploration
8. **Occupy to own** — territory requires active presence, not passive claims
9. **Offline resources at risk** — keep production moving, idle stockpiles are vulnerable
10. **Every check reveals a task** — the "just one more thing" loop never cleanly terminates
11. **No pay-to-win** — cosmetics and convenience only
12. **Offline-first, online-ready** — architecture supports both from day one

---

*Last updated: March 2026*
