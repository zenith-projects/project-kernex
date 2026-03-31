# PROJECT KERNEX

> *You are the root user of the void.*

**PROJECT KERNEX** is a space station management game where you operate, expand, and survive through a terminal interface. Write scripts to automate your drones, refine raw materials into advanced alloys, and explore procedurally generated star sectors — all from a command line.

Built with AI assistance. Built by the community.

---

## 🎮 About the Game

You awaken as the sole operator of **Station XARA-7**, a derelict outpost on the edge of known space. Your only tools: a terminal, a broken AI companion, and whatever resources you can scavenge from the void.

### Core Pillars

**Terminal-First Gameplay** — Every system on your station is operated through a shell interface. Scan sectors, deploy mining drones, manage power grids, and defend against threats using commands, pipes, and scripts.

**Programming as a Mechanic** — Write real scripts in an in-game scripting language to automate repetitive tasks. The more you code, the more efficient your station becomes. Chain commands, schedule cron jobs, and build monitoring dashboards.

**AI Companion (KIRA)** — Your station's AI starts broken and fragmented. Repair and upgrade her systems to unlock new capabilities, dialogue, and story progression. KIRA responds via the terminal and evolves based on your choices.

**Resource Pipeline** — Mine asteroids → Process raw ore → Refine into alloys → Fabricate components → Build station modules. Every step can be optimized through automation.

**Procedural Exploration** — Each star sector is procedurally generated with unique resource compositions, anomalies, hazards, and discoveries. No two playthroughs are the same.

### Planned Features

- 🖥️ Full terminal emulator with custom shell (`vsh`)
- 📜 In-game scripting language for automation
- 🤖 AI companion with dynamic responses (simulated + optional local LLM)
- ⛏️ Deep resource collection and refinery chains
- 🚀 Station expansion and module building
- 🗺️ Procedural star sector generation
- 🛡️ Threat detection and defense systems
- 📊 In-game monitoring and dashboards
- 🌐 Online mode concepts (future development)

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

> **To be defined** — The tech stack is under evaluation. Candidates include:

- **Engine:** Godot 4 / Unity / Custom
- **Language:** GDScript / C# / Rust
- **AI:** Local LLM integration (optional) + scripted dialogue system
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
