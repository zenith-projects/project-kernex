# Contributing to PROJECT KERNEX

First off, thank you for your interest in contributing to **PROJECT KERNEX**! This project is built by its community, and every contribution matters — from fixing a typo to implementing a major game system.

## ⚠️ Before You Start

### Read the CLA

All contributions require agreement to our [Contributor License Agreement (CLA)](CLA.md). By submitting a Pull Request, you assign all rights to the project owner. In return, you will be credited in the **Monolith of Contributors** in the game's credits.

### Understand the License

This is **NOT** an open-source project. The code is publicly visible to enable community collaboration, but all rights are reserved. Please read the [LICENSE](LICENSE) file carefully. Forks are only permitted for the purpose of creating Pull Requests — permanent forks or independent derivatives are prohibited.

---

## How to Contribute

### 1. Find Something to Work On

- Check the [Issues](../../issues) tab for open tasks
- Issues labeled `good-first-issue` are great starting points
- Issues labeled `help-wanted` are actively seeking contributors
- Issues labeled `feature` describe new features in the game design
- If you want to work on something not listed, **open an issue first** to discuss it

### 2. Set Up Your Environment

```bash
# Clone the repository (do NOT create a permanent fork)
git clone https://github.com/Matrix2100/project-kernex.git
cd project-kernex

# Create a feature branch from develop
git checkout develop
git checkout -b feature/your-feature-name

# Follow the setup instructions in docs/SETUP.md
```

### 3. Development Workflow

- **Always** branch from `develop`, never from `main`
- `main` contains only stable, release-ready code
- `develop` is the integration branch for ongoing work
- Use descriptive branch names: `feature/`, `fix/`, `docs/`, `art/`, `audio/`

### 4. Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add asteroid mining drill mechanic
fix: correct resource calculation overflow
docs: update terminal command reference
art: add new space station hull textures
audio: add ambient engine hum sound effect
refactor: extract resource pipeline logic
test: add unit tests for refinery system
```

### 5. Submit a Pull Request

- Target the `develop` branch
- Fill out the PR template completely
- Include the CLA agreement statement:
  > **I have read and agree to the [Contributor License Agreement](CLA.md).**
- Describe what your PR does and why
- Link related issues using `Closes #123` or `Relates to #456`
- Include screenshots or GIFs for visual changes
- Ensure your code compiles and runs without errors

### 6. Code Review

- At least **one maintainer** must approve your PR
- Address review feedback promptly
- PRs that go stale (no activity for 30 days) may be closed

---

## Contribution Categories

### 🎮 Gameplay & Systems
Game mechanics, terminal commands, AI behavior, resource systems, progression, etc.

### 🎨 Art & Assets
Textures, models, UI elements, visual effects, shaders. Follow the art style guide in `docs/ART_STYLE.md`.

### 🔊 Audio
Sound effects, ambient audio, music. All audio must be original or properly licensed. See `docs/AUDIO_GUIDE.md`.

### 📝 Writing & Lore
In-game text, terminal messages, AI dialogue, lore entries, tutorial text.

### 🧪 Testing
Unit tests, integration tests, bug reports with reproduction steps.

### 📚 Documentation
README improvements, setup guides, code comments, wiki pages.

---

## Rules & Guidelines

### Do
- Write clean, documented code
- Follow existing code style and patterns
- Test your changes before submitting
- Be respectful and constructive in discussions
- Ask questions if you're unsure about something

### Don't
- Submit code copied from other projects without proper licensing verification
- Include any AI-generated assets without disclosure
- Submit large PRs without prior discussion
- Break existing functionality without discussion
- Introduce dependencies without maintainer approval

---

## Recognition

Every accepted contribution earns you a place in the **Monolith of Contributors** — a permanent credits monument at the end of the game. Your name (or chosen pseudonym) will be displayed alongside your contribution category.

### Contribution Tiers (displayed in credits)

| Tier | Criteria |
|------|----------|
| 🏛️ **Architect** | Major systems, core features, sustained contribution |
| ⚙️ **Engineer** | Significant features, important bug fixes |
| 🔧 **Technician** | Bug fixes, improvements, quality-of-life changes |
| 📡 **Operator** | Documentation, testing, issue triage |
| 🛰️ **Scout** | First-time contributors, small fixes |

---

## Code of Conduct

Be respectful. Be constructive. Be kind. We're all here to build something cool together.

Harassment, discrimination, or toxic behavior of any kind will result in immediate removal from the project.

---

## Questions?

- Open an issue with the `question` label
- Contact the maintainer at **zenithprojects.dev@gmail.com**

---

*Thank you for helping build PROJECT KERNEX. See you in the void.* 🚀
