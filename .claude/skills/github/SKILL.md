---
name: github
description: Git workflow, conventional commits, PR creation with gh CLI, branch strategy, and code review patterns for PROJECT KERNEX.
allowed-tools:
  - Read
  - Write
  - Edit
  - Bash
  - Glob
  - Grep
---

# Git & GitHub Workflow — PROJECT KERNEX

## Branch Strategy

```
main          ← Stable releases only. Never commit directly.
  └── develop ← Integration branch. All PRs target here.
      ├── feature/*  ← New features (feature/chunk-system, feature/axia-ai)
      ├── fix/*      ← Bug fixes (fix/origin-shift-jitter)
      ├── docs/*     ← Documentation (docs/update-roadmap)
      ├── art/*      ← Art/assets (art/station-textures)
      ├── audio/*    ← Audio (audio/ambient-engine-hum)
      ├── refactor/* ← Refactoring (refactor/extract-production-chain)
      ├── perf/*     ← Performance (perf/chunk-loading-optimization)
      └── test/*     ← Tests (test/combat-system-unit-tests)
```

### Rules
- **Always branch from `develop`**, never from `main`.
- **Never push directly to `main` or `develop`** — always via PR.
- **Delete branches after merge.**
- Branch names: `type/short-description` in kebab-case.

---

## Conventional Commits

Every commit message follows [Conventional Commits](https://www.conventionalcommits.org/).

### Format
```
<type>(<optional scope>): <description>

<optional body>

<optional footer>
```

### Types

| Type | When to Use | Example |
|------|-------------|---------|
| `feat` | New feature or capability | `feat(combat): add rapid-fire fleet mechanics` |
| `fix` | Bug fix | `fix(chunks): correct origin shift jitter at sector boundary` |
| `docs` | Documentation only | `docs: update game design with faction system` |
| `refactor` | Code restructure, no behavior change | `refactor(production): extract chain resolver into own class` |
| `perf` | Performance improvement | `perf(world): add spatial hash for O(1) chunk lookup` |
| `test` | Adding or fixing tests | `test(combat): add unit tests for rapid-fire resolver` |
| `art` | Art/visual assets | `art: add isometric station module sprites` |
| `audio` | Audio assets | `audio: add ambient reactor hum loop` |
| `chore` | Build, CI, tooling, dependencies | `chore: update Godot to 4.6.1` |
| `style` | Code formatting, no logic change | `style: fix whitespace in StationModule.cs` |
| `ci` | CI/CD pipeline changes | `ci: add build workflow for Godot Mono` |
| `revert` | Revert a previous commit | `revert: revert "feat(combat): add rapid-fire"` |

### Scopes (Optional)

Use when the change is clearly scoped to one system:

| Scope | Area |
|-------|------|
| `combat` | Combat system, fleet, weapons |
| `station` | Station building, modules, defense |
| `world` | Chunks, sectors, origin shift, procedural gen |
| `production` | Resources, refinery, production chains |
| `shell` | vsh shell, commands, scripting |
| `axia` | AXIA AI system |
| `ui` | HUD, menus, panels |
| `exploration` | Scanning, fog of war, anomalies |
| `faction` | Factions, clans, reputation |
| `economy` | Trade, market |
| `progression` | Tech tree, research, station tiers |
| `network` | Online/MMO features |

### Rules
- **Subject line**: imperative mood, lowercase, no period, max 72 chars.
- **Body** (optional): explain WHY, not WHAT (the diff shows what).
- **Breaking changes**: add `BREAKING CHANGE:` in footer or `!` after type.
- **Issue references**: `Closes #123`, `Relates to #456` in footer.

### Examples

```
feat(world): implement 3-level chunk system with origin shift

Add Cell, Sector, and Quadrant coordinate types with diamond lattice
geometry. Origin shift triggers at sector boundaries, keeping the player
near (0,0) for float precision safety.

Closes #12
```

```
fix(combat): prevent double-damage on rapid-fire overflow

RapidFire resolver was counting the initial hit as a bonus hit,
effectively doubling damage on the first target. Now correctly
starts the chain after the initial hit.

Closes #45
```

```
perf(world): reduce chunk load time by 60% with spatial hash

Replace linear search in EntityRegistry.GetEntitiesInCell() with
Dictionary<CellAddress, HashSet<long>> spatial index. Measured:
chunk load drops from ~5ms to ~2ms for 3-cell radius.
```

---

## Pull Requests

### Creating a PR with `gh`

```bash
# 1. Ensure branch is pushed
git push -u origin feature/my-feature

# 2. Create PR targeting develop
gh pr create --base develop --title "feat(scope): short description" --body "$(cat <<'EOF'
## Description

Brief description of what this PR does and why.

## Type of Change

- [x] 🎮 Gameplay / Systems

## Related Issues

Closes #123

## Changes Made

- Added X
- Modified Y
- Removed Z

## Testing

- [x] Compiles without errors
- [x] Runs without crashes
- [x] Existing tests pass

## AI Disclosure

- [x] This PR contains AI-generated content (Claude Code used for implementation)

## CLA Agreement

- [x] **I have read and agree to the [Contributor License Agreement](../CLA.md).**
EOF
)"
```

### PR Title Convention

Same as commit convention: `type(scope): description`

```
feat(combat): add rapid-fire fleet mechanics
fix(world): correct origin shift at sector boundaries
docs: update roadmap with Phase 2 milestones
refactor(station): extract building-as-levels system
```

### PR Description Rules

1. **Description**: 1-3 sentences on WHAT and WHY.
2. **Type of Change**: Check the matching checkbox.
3. **Related Issues**: Always link issues if they exist.
4. **Changes Made**: Bullet list of key changes (not every file, just the meaningful ones).
5. **Testing**: Describe how you tested. Check applicable boxes.
6. **AI Disclosure**: Always disclose if AI was used. Describe what it generated.
7. **CLA Agreement**: Required checkbox.

### PR Review Checklist

Before approving a PR, verify:

- [ ] Follows conventional commits in title
- [ ] No GDScript (C# only)
- [ ] Composition pattern used (no deep inheritance)
- [ ] No magic numbers (uses [Export] or constants)
- [ ] Max 500 lines per .cs file
- [ ] Scenes used for layout, code for logic only
- [ ] Signal up, call down
- [ ] No `GetParent()` or `GetNode("..")`
- [ ] Performance-sensitive code profiled
- [ ] Compiles and runs without errors

---

## Common `gh` Commands

```bash
# List open PRs
gh pr list

# View a specific PR
gh pr view 123

# Check PR status (CI, reviews)
gh pr checks 123

# Merge a PR (squash)
gh pr merge 123 --squash --delete-branch

# Create an issue
gh issue create --title "[FEATURE] description" --label feature

# List issues
gh issue list --label "good-first-issue"

# Close an issue
gh issue close 123

# View repo in browser
gh repo view --web
```

---

## Git Hygiene

- **Commit often, push when ready.** Small, focused commits are better than large ones.
- **Never force push to `develop` or `main`.**
- **Rebase feature branches** on develop before creating PR: `git rebase develop`
- **Squash merge** PRs into develop for a clean history.
- **Write meaningful commit messages.** The body explains WHY, the diff shows WHAT.
- **Tag releases** on main: `git tag -a v0.1.0 -m "Phase 1 MVP"`
