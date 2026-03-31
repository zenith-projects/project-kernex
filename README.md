# PROJECT KERNEX

> *You are the root user of the void.*

**PROJECT KERNEX** is an isometric space game where you build stations, command fleets, automate production chains, and conquer territory in a procedurally generated infinite universe. Play with mouse and keyboard like any normal game — or open the in-game terminal for scripting and automation. Your AI companion KIRA runs locally and writes scripts for you, so everyone has access to automation power regardless of coding skill.

Built with AI assistance. Built by the community.

---

## 🎮 About the Game

You awaken as the sole operator of **Station XARA-7**, a derelict outpost on the edge of known space. Mine resources, refine materials, build your fleet, and expand into the void — while threats scale with your ambition.

### Core Pillars

**Isometric Space Gameplay** — Build and manage your station in a real-time isometric world. Mine asteroids, deploy fleets, defend your base, and explore an infinite procedurally generated universe using mouse, keyboard, and UI.

**In-Game Terminal (vsh)** — An optional power tool inside the game. Chain commands, write scripts, schedule automation. Not required to play, but gives a massive edge to those who use it.

**AI Companion (KIRA)** — A local LLM running on your machine. KIRA lives in the terminal, writes automation scripts on request, provides strategic advice, and evolves through 5 stages. Non-programmers ask KIRA; programmers code manually — same result, no disadvantage.

**Deep Production Chains** — Branching, interdependent resource processing inspired by Factorio. Circuit Boards need both Silicon Wafers AND Copper Wire. Bottleneck identification is a core skill.

**Fleet Combat** — Real-time isometric combat with fleet composition depth inspired by OGame/Dark Orbit. Hard counters, rapid-fire mechanics, and drone formations. Composition beats numbers.

**Defend Your Station** — Layered defense inspired by Clash of Clans. Different threat types need different countermeasures. Station activity attracts threats — growth has a cost.

**Infinite Universe** — Procedurally generated from a single seed. Billions of sectors to explore. Chunked coordinates eliminate floating-point precision issues at any distance.

### Planned Features

- 🌍 Isometric infinite procedural universe
- ⚔️ Real-time fleet combat with hard counters and drone systems
- 🏗️ Station building with defense layout strategy
- ⛏️ Branching production chains with interdependencies
- 🖥️ In-game terminal emulator (`vsh`) for power users
- 🤖 Local AI companion (KIRA) — writes scripts and automates for you
- 🛡️ Activity-threat feedback loop (growth attracts danger)
- 🏛️ Three-faction system with clan warfare
- 🌐 Offline-first → MMO transition (future)

---

## 🏗️ Project Status

> **Pre-production** — Currently in game design and prototyping phase.

This project is in active development. We're defining core mechanics, building prototypes, and establishing the technical foundation. Check the [roadmap](docs/ROADMAP.md) for planned milestones.

---

## 🤝 Contributing

This project is **community-driven**. Everyone can contribute — from code and art to game design and documentation.

**Important:** This is NOT an open-source project. The code is publicly visible to enable collaboration, but all rights are reserved. Please read:

- [CONTRIBUTING.md](CONTRIBUTING.md) — How to contribute
- [CLA.md](CLA.md) — Contributor License Agreement (required)
- [LICENSE](LICENSE) — Project license (proprietary)

### Quick Start

```bash
git clone https://github.com/Matrix2100/project-kernex.git
cd project-kernex
git checkout develop
# See docs/SETUP.md for environment setup
```

### Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Stable releases only |
| `develop` | Integration branch — PRs go here |
| `feature/*` | Individual feature branches |
| `fix/*` | Bug fix branches |
| `docs/*` | Documentation branches |

---

## 📁 Project Structure

```
project-kernex/
├── docs/                    # Game design documents, guides, lore
│   ├── ROADMAP.md           # Development roadmap
│   ├── GAME_DESIGN.md       # Core game design document
│   ├── SETUP.md             # Development environment setup
│   ├── COMMANDS.md          # In-game terminal commands reference
│   ├── ART_STYLE.md         # Visual style guide
│   └── AUDIO_GUIDE.md       # Audio guidelines
├── src/                     # Source code
│   ├── core/                # Core game engine
│   ├── terminal/            # Terminal emulator and shell
│   ├── ai/                  # AI companion system (KIRA)
│   ├── resources/           # Resource management systems
│   ├── world/               # Procedural generation
│   └── ui/                  # User interface
├── assets/                  # Game assets
│   ├── audio/               # Sound effects and music
│   ├── textures/            # Visual assets
│   ├── fonts/               # Terminal and UI fonts
│   └── data/                # Game data files (JSON/YAML)
├── tests/                   # Test suites
├── tools/                   # Development tools and scripts
├── LICENSE                  # Proprietary license
├── CLA.md                   # Contributor License Agreement
├── CONTRIBUTING.md          # Contribution guidelines
├── CODE_OF_CONDUCT.md       # Community guidelines
└── README.md                # This file
```

---

## 🛠️ Tech Stack

> **Under evaluation** — Leading candidates:

- **Engine:** Godot 4 (leading) / Unity / Custom
- **Language:** C# / GDScript / Rust
- **Rendering:** Isometric 2D/3D with chunked coordinate system
- **AI:** Local LLM (llama.cpp, 7B quantized) + scripted fallback
- **Architecture:** Command/Event pattern (offline-first → MMO-ready)
- **World Gen:** Seed-based deterministic procedural generation
- **Audio:** FMOD / Wwise / Native
- **Build:** CI/CD with GitHub Actions

Community input on tech stack decisions is welcome — see the related issue in the [Issues](../../issues) tab.

---

## 📜 License

This project is **proprietary software**. The source code is publicly visible to enable community contribution, but **all rights are reserved** by the project owner.

- ❌ No redistribution
- ❌ No permanent forks
- ❌ No commercial use
- ❌ No derivative works
- ✅ Viewing and studying the code
- ✅ Contributing via Pull Requests

See [LICENSE](LICENSE) for full terms.

---

## 🏛️ Credits — The Monolith of Contributors

Every person who contributes to PROJECT KERNEX will be immortalized in the **Monolith of Contributors** — a credits monument displayed at the end of the game.

Your name. Your legacy. Carved in the void forever.

*See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution tiers and recognition details.*

---

## ⚠️ AI Disclosure

This project is developed **with AI assistance**. AI tools (such as GitHub Copilot, Claude, ChatGPT, and others) are used for:

- Code generation and review
- Game design brainstorming
- Documentation drafting
- Asset prototyping

All AI-generated content is reviewed, curated, and modified by human contributors. Contributors must disclose when submitting AI-generated work.

---

## 📬 Contact

- **Maintainer:** Matrix2100
- **Email:** zenithprojects.dev@gmail.com
- **Discord:** https://discord.gg/PbYGBZJZCv

---

<p align="center">
  <strong>PROJECT KERNEX</strong> — <em>root@void:~$ _</em>
</p>
