# PROJECT KERNEX

> *You are the root user of the void.*

**PROJECT KERNEX** is a hybrid 2.5D/3D space game where you build stations, command fleets, automate production chains, and conquer territory in a procedurally generated infinite universe. Isometric view for exploration and building, first-person cockpit inside ships, and walkable station interiors — blending perspectives to create depth without requiring full 3D where it's not needed. An in-game shell lets power users script automation, while the local AI companion **AXIA** writes those scripts for everyone else — so non-programmers get the same power as devs.

Built with AI assistance. Built by the community.

---

## About the Game

You awaken as the sole operator of **Station XARA-7**, a derelict outpost on the edge of known space. A broken AI named AXIA flickers to life in your shell. Mine resources, refine materials, build your fleet, research through a massive tech web, and expand into the void — while threats scale with your ambition.

### Core Pillars

| Pillar | Description |
|--------|-------------|
| **Hybrid 2.5D/3D Space** | Isometric world for exploration, mining, combat, and base building. First-person cockpit view inside ships. Walk inside your station (future). Fake 3D where the player can't tell the difference. |
| **Shell (vsh)** | Optional in-game shell. Script automation, chain commands, schedule cron jobs. Not required — but powerful. |
| **AXIA (Local AI)** | Local LLM on your machine. 10 evolution levels. Writes scripts for non-devs. The equalizer. |
| **The Constellation** | PoE-style research web. 16 interconnected clusters, 18 bridges. No locks — unlock everything. |
| **100+ Modules** | 13 categories, 5 rarities, 16 upgrade levels. Unlocked through the Constellation, then crafted. |
| **Fleet Combat** | Real-time with hard counters (OGame/Dark Orbit). Composition beats numbers. Drone formations. |
| **Station Defense** | Layered defense (CoC). Activity attracts threats (Factorio). Growth has a cost. |
| **Production Chains** | Branching, interdependent (Factorio). Circuit Board = Wafer + Wire. Bottlenecks matter. |
| **Infinite Universe** | Seed-based procedural generation. Chunked coordinates with origin shift. Billions of sectors. |
| **Three Factions** | HELIX (industrial), NOVA (military), VOID (explorers). Clan warfare. Territory sovereignty. |
| **Offline → MMO** | Ships as single-player. Transitions to online after MVPs. No pay-to-win. |

### Planned Features

- 🌍 Infinite procedural universe with origin shift (no floating-point issues at any distance)
- ⚔️ Real-time fleet combat with rapid-fire hard counters and drone formations
- 🏗️ Station building with defense layout strategy and module adjacency
- ⛏️ Branching production chains with interdependencies
- 🌳 The Constellation — 16-cluster research web with 18 bridge connections
- 🔧 100+ modules across 13 categories (energy, weapons, defense, AXIA, stealth...)
- 🖥️ In-game shell (`vsh`) with scripting, piping, cron jobs
- 🤖 AXIA — 10-level local AI that writes scripts, manages stations, reveals story
- 🛡️ Activity-threat feedback loop (growth attracts danger)
- 🏛️ Three factions with clan wars and territory sovereignty
- 🌐 Offline-first → MMO transition

---

## Project Status

> **Phase 0 — Foundation** `CURRENT`

Game design complete (15 design docs). Godot 4.6 Mono project initialized. CI/CD with 10 PR checks. Branch protection active. Next: Shell + AXIA (Phase 1).

Check the [roadmap](docs/ROADMAP.md) for all phases and MVPs.

---

## Game Design Documentation

All design docs live in [`docs/game-design/`](docs/game-design/INDEX.md):

| Document | What It Covers |
|----------|---------------|
| [Overview](docs/game-design/OVERVIEW.md) | Vision, concept, art direction, monetization, design principles |
| [Gameplay](docs/game-design/GAMEPLAY.md) | Core loop: scan → collect → refine → build → defend → expand |
| [Combat](docs/game-design/COMBAT.md) | Fleet combat, drones, PvE, PvP |
| [Resources](docs/game-design/RESOURCES.md) | Three-tier economy, production chains |
| [Station](docs/game-design/STATION.md) | Building system, tiers, module layout |
| [Threats](docs/game-design/THREATS.md) | Activity-threat loop, defense resolution |
| [Shell](docs/game-design/TERMINAL.md) | vsh shell, scripting, automation |
| [AXIA](docs/game-design/AXIA.md) | 10 evolution levels, LLM integration |
| [Factions](docs/game-design/FACTIONS.md) | Three factions, clans, territory control |
| [Exploration](docs/game-design/EXPLORATION.md) | Infinite universe, procedural gen, anomalies |
| [Architecture](docs/game-design/ARCHITECTURE.md) | Chunked coords, offline-first, MMO architecture |
| [Constellation](docs/game-design/CONSTELLATION.md) | Research web (16 clusters, 18 bridges) |
| [Modules](docs/game-design/MODULES.md) | 100+ modules, rarity, levels, slots |
| [Commands](docs/game-design/COMMANDS.md) | Shell commands reference |

---

## Contributing

Anyone on the internet can contribute — no permission needed. Fork, code, open a PR.

- [CONTRIBUTING.md](CONTRIBUTING.md) — Full guide (two paths: fork or direct)
- [CLA.md](CLA.md) — Contributor License Agreement (required)
- [LICENSE](LICENSE) — Proprietary (code visible, all rights reserved)

### Quick Start

```bash
# Fork this repo, then:
git clone https://github.com/YOUR-USERNAME/project-kernex.git
cd project-kernex
git remote add upstream https://github.com/zenith-projects/project-kernex.git
git fetch upstream
git checkout -b feature/your-feature upstream/develop
# Code, commit, push to your fork, open PR targeting develop
```

### Rules

| Rule | Detail |
|------|--------|
| **PRs required** | Direct pushes to `main`/`develop` blocked |
| **10 CI checks must pass** | Build, commit format, no GDScript, file size, CLA, secrets, PR template |
| **Rebase only** | No merge commits — linear history |
| **1 admin approval** | Required before merge |
| **C# only** | GDScript strictly prohibited |
| **Conventional commits** | `type(scope): description` |

---

## Tech Stack

| Component | Choice |
|-----------|--------|
| **Engine** | Godot 4.6+ Mono |
| **Language** | C# (.NET 8+) — GDScript prohibited |
| **Architecture** | Composition mandatory, Command/Event pattern |
| **Rendering** | Hybrid 2.5D/3D — isometric world + first-person cockpit/station interiors. Fake 3D where possible. Chunked coordinates + origin shift |
| **World Gen** | Seed-based deterministic procedural generation |
| **AI** | Local LLM (llama.cpp, 7B quantized) + scripted fallback |
| **CI/CD** | GitHub Actions (10 required checks per PR) |

---

## Project Structure

```
project-kernex/
├── project.godot                  Godot project root
├── Project Kernex.csproj/.sln     .NET project (IDE opens this)
├── src/                           All game code and assets
│   ├── Assets/                    Audio, fonts, models, shaders, textures, themes
│   ├── Autoloads/                 Global singletons (GameState, EventBus, OriginShift)
│   ├── Components/                Reusable behavior (tscn + cs, same folder)
│   ├── Nodes/                     Entities (ships, drones, station, environment, UI)
│   ├── Screens/                   Game states (Root, MainMenu, Game, DevTools)
│   ├── Core/                      Pure C# (interfaces, coordinates, enums, constants)
│   ├── Systems/                   Game logic (production, combat, threat, AI, shell)
│   ├── Resources/                 .tres data + C# Resource definitions
│   └── Tests/                     Isolated test scenes
├── .claude/                       Agents and skills (auto-detected)
├── .github/                       PR templates, CI workflows (10 checks)
├── docs/                          Roadmap, setup, game-design/ (15 docs)
├── CLAUDE.md                      Project conventions
├── CONTRIBUTING.md                Contribution guide
├── CLA.md                         Contributor License Agreement
└── LICENSE                        Proprietary license
```

---

## License

**Proprietary software.** Source visible for community contribution, all rights reserved.

- ❌ No redistribution, permanent forks, commercial use, or derivatives
- ✅ Viewing, studying, and contributing via Pull Requests

See [LICENSE](LICENSE) for full terms.

---

## The Monolith of Contributors

Every contributor is immortalized in the **Monolith of Contributors** — a credits monument in the game.

| Tier | Criteria |
|------|----------|
| **Architect** | Major systems, core features, sustained contribution |
| **Engineer** | Significant features, important bug fixes |
| **Technician** | Bug fixes, improvements, QoL |
| **Operator** | Documentation, testing, issue triage |
| **Scout** | First-time contributors, small fixes |

---

## AI Disclosure

This project uses AI tools (Claude, Copilot, ChatGPT) for code generation, game design, documentation, and asset prototyping. All AI content is reviewed by humans. Contributors must disclose AI usage in PRs.

---

## Contact

- **Maintainer:** Matrix2100
- **Email:** zenithprojects.dev@gmail.com
- **Discord:** https://discord.gg/PbYGBZJZCv

---

<p align="center">
  <strong>PROJECT KERNEX</strong> — <em>root@void:~$ _</em>
</p>
