# Contributing to PROJECT KERNEX

Thank you for your interest in contributing to **PROJECT KERNEX**! This project is built by its community, and every contribution matters — from fixing a typo to implementing a major game system.

## Before You Start

### Read the CLA

All contributions require agreement to our [Contributor License Agreement (CLA)](CLA.md). By submitting a Pull Request, you assign all rights to the project owner. Each contributor bears full legal responsibility for their own contribution. In return, you will be credited in the **Monolith of Contributors** in the game's credits.

### Understand the License

This is **NOT** an open-source project. The code is publicly visible to enable community collaboration, but all rights are reserved. Please read the [LICENSE](LICENSE) file carefully. Forks are only permitted for the purpose of creating Pull Requests — permanent forks or independent derivatives are prohibited.

### Technical Requirements

- **Engine:** Godot 4.6+ Mono
- **Language:** C# (.NET 8+) — **GDScript is strictly prohibited**
- **Architecture:** Composition is mandatory (see [CLAUDE.md](CLAUDE.md))

---

## How to Contribute

There are two ways to contribute, depending on your access level:

### Path A: External Contributors (anyone on the internet)

You do NOT need permission to contribute. Fork, work, and open a PR:

```bash
# 1. Fork the repository on GitHub (click the "Fork" button)

# 2. Clone YOUR fork
git clone https://github.com/YOUR-USERNAME/project-kernex.git
cd project-kernex

# 3. Add the original repo as upstream
git remote add upstream https://github.com/zenith-projects/project-kernex.git

# 4. Create a feature branch from develop
git fetch upstream
git checkout -b feature/your-feature-name upstream/develop

# 5. Make your changes, commit, and push to YOUR fork
git push origin feature/your-feature-name

# 6. Open a Pull Request from your fork → zenith-projects/project-kernex develop
```

### Path B: Team Members (developers team)

If you've been added to the `developers` team, you work directly in the repo:

```bash
# 1. Clone the repository
git clone https://github.com/zenith-projects/project-kernex.git
cd project-kernex

# 2. Create a feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# 3. Make your changes, commit, and push
git push -u origin feature/your-feature-name

# 4. Open a Pull Request targeting develop
```

### Not sure which path? Use Path A (fork). It always works.

---

## Branch Strategy

```
main           <- Stable releases only. Never commit directly.
  └── develop  <- Integration branch. All PRs target here.
      ├── feature/*   New features
      ├── fix/*       Bug fixes
      ├── docs/*      Documentation
      ├── art/*       Art/assets
      ├── audio/*     Audio
      ├── refactor/*  Refactoring
      ├── perf/*      Performance
      └── test/*      Tests
```

- **Always** branch from `develop`, never from `main`
- **Direct pushes to `main` and `develop` are blocked** — PRs are mandatory
- **Only rebase merges are allowed** (no merge commits, no squash)
- Branches are auto-deleted after merge

---

## Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description in imperative mood
```

### Types

| Type | When to Use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `refactor` | Code restructure, no behavior change |
| `perf` | Performance improvement |
| `test` | Tests |
| `art` | Visual assets |
| `audio` | Audio assets |
| `chore` | Build, CI, tooling |

### Scopes (optional)

`combat`, `station`, `world`, `production`, `shell`, `axia`, `ui`, `exploration`, `faction`, `economy`, `progression`

### Examples

```
feat(world): implement 3-level chunk system with origin shift
fix(combat): prevent double-damage on rapid-fire overflow
docs: update roadmap with Phase 2 milestones
art: add isometric station module sprites
perf(world): reduce chunk load time with spatial hash
```

---

## Pull Request Process

1. **Target the `develop` branch** (never `main`)
2. **Fill out the PR template completely** — every section
3. **Include the CLA statement** in the PR description:
   > **I have read and agree to the [Contributor License Agreement](CLA.md).**
4. **At least one maintainer must approve** before merge
5. **Address review feedback** promptly
6. PRs with no activity for 30 days may be closed

### PR Title

Same format as commits: `type(scope): description`

```
feat(combat): add rapid-fire fleet mechanics
fix(world): correct origin shift at sector boundaries
docs: update game design with faction system
```

---

## Code Requirements

### Mandatory

- **C# only** — zero GDScript, zero exceptions
- **Composition** — components as `.tscn` + `.cs` in same folder, entities have no script
- **Signal up, call down** — never `GetParent()` or `GetNode("..")`
- **Max 500 lines per .cs file** — split into partial classes if larger
- **[Export] everything configurable** — no magic numbers
- **Scenes for layout, code for logic** — never build UI hierarchies in code
- **Compiles and runs without errors** before submitting

### Recommended

- Read [CLAUDE.md](CLAUDE.md) for the full list of conventions
- Follow the 5-step philosophy: Question → Delete → Simplify → Optimize → Automate
- Profile performance-sensitive code before submitting
- AI-assisted code is welcome — just disclose it in the PR

---

## Contribution Categories

| Category | What |
|----------|------|
| **Gameplay & Systems** | Game mechanics, combat, resources, automation, progression |
| **Art & Assets** | Textures, models, UI elements, VFX, shaders |
| **Audio** | SFX, ambient audio, music (must be original or properly licensed) |
| **Writing & Lore** | In-game text, shell messages, AXIA dialogue, lore entries |
| **Testing** | Unit tests, integration tests, bug reports |
| **Documentation** | README, guides, code comments, wiki |

---

## Rules

### Do

- Write clean code following existing patterns
- Test your changes before submitting
- Disclose AI-generated content in PRs
- Ask questions via issues (label: `question`)
- Discuss large changes before implementing

### Don't

- Submit third-party code/assets without verifying the license
- Submit AI-generated assets without disclosure
- Submit large PRs without prior discussion
- Introduce dependencies without maintainer approval
- Write GDScript (seriously, don't)

---

## Recognition — Monolith of Contributors

Every accepted contribution earns you a place in the **Monolith of Contributors** — a permanent credits monument at the end of the game.

| Tier | Criteria |
|------|----------|
| **Architect** | Major systems, core features, sustained contribution |
| **Engineer** | Significant features, important bug fixes |
| **Technician** | Bug fixes, improvements, quality-of-life changes |
| **Operator** | Documentation, testing, issue triage |
| **Scout** | First-time contributors, small fixes |

---

## Questions?

- Open an issue with the `question` label
- Join the [Discord](https://discord.gg/PbYGBZJZCv)
- Email: zenithprojects.dev@gmail.com

---

*Thank you for helping build PROJECT KERNEX. See you in the void.*
