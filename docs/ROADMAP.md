# PROJECT KERNEX — Development Roadmap

> *This roadmap is a living document and subject to change based on community feedback and development priorities.*

---

## Phase 0 — Foundation 🏗️ `CURRENT`

**Goal:** Establish project structure, game design, and community infrastructure.

- [x] Define core game concept
- [x] Create repository structure
- [x] Write LICENSE, CLA, CONTRIBUTING guidelines
- [x] Draft Game Design Document
- [ ] Choose game engine and tech stack
- [ ] Set up development environment documentation
- [ ] Set up CI/CD pipeline
- [ ] Create Discord server for community
- [ ] Define art style guide
- [ ] Define audio guidelines

---

## Phase 1 — Terminal Prototype 🖥️

**Goal:** Build a functional terminal emulator with basic commands.

- [ ] Implement `vsh` (Void Shell) terminal emulator
- [ ] Basic command parser and execution engine
- [ ] Implement core commands: `help`, `status`, `clear`, `log`
- [ ] Command history and autocomplete
- [ ] Piping system (`|`)
- [ ] Basic scripting support (`.vsh` files)
- [ ] Terminal UI (font, colors, cursor, scrollback)

---

## Phase 2 — Resource Core ⛏️

**Goal:** Implement the resource collection and refinery pipeline.

- [ ] Resource data model (types, quantities, storage)
- [ ] `scan` command — sector scanning with results
- [ ] `drone` command — deploy, recall, status
- [ ] `inventory` command — view stored resources
- [ ] `refinery` command — process raw materials
- [ ] Basic refinery chain (ore → alloy → component)
- [ ] Time-based processing with real-time updates
- [ ] Power system (generation, consumption, balance)

---

## Phase 3 — Station Building 🚀

**Goal:** Enable station expansion through construction.

- [ ] `build` command — construct modules
- [ ] Module system (types, requirements, effects)
- [ ] Station layout / slot system
- [ ] `upgrade` command — improve existing modules
- [ ] `repair` command — fix damaged modules
- [ ] Tech tree implementation
- [ ] Visual station overview (2D map or ASCII art)

---

## Phase 4 — KIRA AI 🤖

**Goal:** Implement the AI companion system.

- [ ] KIRA dialogue engine (scripted)
- [ ] Context-aware responses based on game state
- [ ] KIRA evolution stages (0-4)
- [ ] `kira` command interface
- [ ] Proactive alerts and suggestions
- [ ] Story fragments delivered through KIRA
- [ ] (Optional) Local LLM integration prototype

---

## Phase 5 — Exploration & Threats 🗺️

**Goal:** Add procedural sector generation and threat systems.

- [ ] Procedural sector generator
- [ ] `map` command — sector visualization
- [ ] Fog of war system
- [ ] Anomaly system (data caches, derelicts, rifts)
- [ ] Threat system (asteroids, radiation, failures)
- [ ] `defense` command — manage defenses
- [ ] Event system for random encounters

---

## Phase 6 — Automation & Scripting 📜

**Goal:** Deep scripting and automation capabilities.

- [ ] Full `.vsh` scripting language
- [ ] `cron` command — scheduled tasks
- [ ] `alias` command — custom shortcuts
- [ ] `watch` command — real-time monitoring
- [ ] Script editor (in-game)
- [ ] Performance metrics for automation evaluation
- [ ] Tutorial missions teaching scripting

---

## Phase 7 — Polish & Content 🎨

**Goal:** Game feel, content, and release preparation.

- [ ] Sound design and ambient audio
- [ ] Music (atmospheric, minimal)
- [ ] Full tutorial / onboarding flow
- [ ] 50+ unique anomaly events
- [ ] Lore entries and world-building text
- [ ] Achievement system
- [ ] Monolith of Contributors (credits)
- [ ] Steam store page preparation
- [ ] Playtesting and balancing

---

## Phase 8 — Release 🎮

**Goal:** Launch on Steam.

- [ ] Steam integration (achievements, cloud saves)
- [ ] Final QA pass
- [ ] Launch trailer
- [ ] Press kit
- [ ] **Steam Early Access or Full Release**

---

## Future — Online Concepts 🌐

> *Post-release development. No timeline yet.*

- [ ] Station-to-station networking
- [ ] Trade protocol system
- [ ] Shared script repository (`vpm`)
- [ ] Cooperative sector exploration
- [ ] Competitive sector control
- [ ] Leaderboards

---

*Want to contribute to a specific phase? Check the [Issues](../../issues) tab for tasks tagged with the relevant phase label.*
