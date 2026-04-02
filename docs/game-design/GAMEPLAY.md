# PROJECT KERNEX — Core Gameplay Loop

---

## Core Gameplay Loop

```
SCAN → COLLECT → REFINE → BUILD → DEFEND → EXPAND → REPEAT
  ↑                                                    |
  └────────────────────────────────────────────────────┘
         ↕ (threats scale with station activity)
```

The loop follows the **"just one more thing" principle** (from Factorio): every `status` check reveals at least one actionable problem. Every fix requires resources from a different production chain. Completing one task reveals the next. There is no natural stopping point.

### Scan

Explore nearby sectors to discover resources, anomalies, and threats. Scanning is the entry point to all other activities.

- **UI:** Click sectors on the isometric map, hit "Scan" button, or use `scan --sector 7`
- **Fog of War:** Unknown sectors show as `???`. Quick scan reveals basic info. Deep scan reveals full details. Drone recon reveals everything.
- **Intel decay:** Scanned data degrades over time. Sectors must be re-scanned periodically for current intel.

### Collect

Deploy mining drones to extract raw materials from asteroids, planets, and debris fields.

- **UI:** Select drone bay, choose target, deploy. Or `drone deploy --target AST-7A --type mining`
- **Drones operate autonomously** after deployment — player skill is in preparation and target selection, not micromanagement (Clash of Clans deploy-and-observe model)
- **Drone types:** Mining, Scout, Combat, Salvage — each with different capabilities and equipment slots

### Refine

Process raw materials through branching production chains with interdependencies.

- Production chains are NOT linear — they branch and merge (Factorio model)
- Example: Circuit Board requires BOTH Silicon Wafer AND Copper Wire
- Refineries have efficiency ratings affected by upgrades, power, and automation quality
- **Bottleneck identification** is a core skill — the whole chain is limited by the weakest link

### Build

Construct station modules, upgrade buildings, and craft equipment.

- **Building-as-levels system** (OGame model): One Metal Refinery that you upgrade, not 20 separate refineries
- Each upgrade increases output but costs exponentially more (1.1^L scaling)
- Module placement affects defense layout (adjacency matters for damage containment)
- Research consumes refined products — forces working production chains before tech advancement

### Defend

Threats scale with station activity. The more you scan, mine, and refine, the more "energy signature" your station emits, attracting threats.

- **Defense is a composition problem** — no single defense type handles all threats
- **Defense is a tax on production** (Factorio model) — invest too little and threats destroy production; invest too much and progression slows
- **70% auto-repair** after attacks (OGame model) — defenses are a persistent investment
- **Threat cooldown** after successful defense — creates natural play session rhythm

### Expand

New modules unlock new capabilities, higher-tier production, and access to deeper sectors.

### Perspectives

The core gameplay loop happens primarily in **isometric view** (2.5D top-down). All phases — scan, collect, refine, build, defend, expand — are playable from isometric.

In later phases, players can toggle into a **first-person cockpit view** inside ships for a more immersive navigation and combat experience. The cockpit instruments mirror the isometric HUD — same data, different presentation. The isometric HUD design is inspired by cockpit instrument aesthetics, so both feel cohesive.

Future: **walkable station interiors** in first-person, where modules appear as rooms the player can physically enter and inspect.

---

*Last updated: April 2026*
