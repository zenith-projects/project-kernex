# PROJECT KERNEX — Combat System

---

## Combat System

### Real-Time Isometric Combat (from Dark Orbit)

Combat happens in real-time on the isometric map. Players can directly control their flagship or manage combat through fleet commands.

- **Lock-on targeting:** Select enemy, weapons fire automatically while you maneuver
- **Movement matters:** Dodge projectiles, kite enemies, position for advantage
- **Ammo types:** Different ammunition for different situations (standard, armor-piercing, EMP, etc.)
- **Config switching:** Quick swap between PvE and PvP loadouts

### Fleet Composition (from OGame)

Fleet strength comes from composition, not just numbers. A **rapid-fire / hard counter system** creates rock-paper-scissors depth:

| Ship Class | Strong Against | Weak Against | Role |
|-----------|---------------|-------------|------|
| Fighter | Bombers | Cruisers | Cheap, fast swarm |
| Cruiser | Fighters | Battleships | Anti-fighter screen |
| Battleship | Cruisers | Destroyers | Fleet backbone |
| Destroyer | Battleships | Fighters (if unsupported) | Heavy hitter |
| Bomber | Defenses | Fighters | Siege weapon |
| Carrier | N/A | Direct combat | Drone deployment platform |

A single ship type can "rapid fire" against its counter, potentially attacking multiple times per round. This prevents blob-everything-wins strategies.

### Combat Drones (from Dark Orbit)

Up to 8 combat drones orbit the player's flagship:
- Each drone has equipment slots (lasers, shields)
- Drone formations affect combat behavior (offensive spread, defensive shell, etc.)
- Drones are a major power multiplier — a fully equipped fleet doubles effective combat stats

### PvE Content

- **Hostile NPCs:** Pirates, rogue AI, alien creatures — escalating difficulty by sector depth
- **Void Gates:** Instanced wave-based PvE challenges (inspired by Dark Orbit Galaxy Gates) with exclusive rewards
- **Boss encounters:** Sector guardians protecting rare resources or KIRA upgrade data
- **Environmental hazards:** Radiation storms, asteroid fields, solar flares during combat

### PvP (Future Online)

- **Company/Faction-based:** Three factions with default hostility between them
- **Raiding:** Attack other stations to steal resources (OGame model). Only unprocessed/stored resources are vulnerable — installed modules and refined materials in use are safe. This incentivizes keeping production moving (resource velocity principle)
- **Offense slightly beats defense by design** — perfect defense is impossible, but 100% destruction is hard. Defenders "win" by minimizing losses.
- **Bashing limits:** Maximum attacks on a single player per day to prevent griefing
- **Post-attack logs:** Detailed event logs of what happened during an attack, reviewable via terminal (`log --event attack --last`)

---

*Last updated: March 2026*
