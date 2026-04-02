# PROJECT KERNEX — Exploration & Procedural Generation

---

## Exploration & Procedural Generation

### Infinite Universe

The universe is procedurally generated from a single master seed using deterministic algorithms. Nothing is stored — the same coordinates always regenerate identical content.

**Generation cascade:**

```
Master Seed
  → Galaxy Generator (star positions, types, connections)
    → System Generator (planets, asteroid belts, resource composition)
      → Sector Generator (local detail, hazards, anomalies)
        → Content Populator (lore, AXIA data, quest hooks)
```

Each level uses the parent's output as its seed, ensuring perfect reproducibility.

### Universe Structure

```
Galaxy
  └── Region (10-40 systems, strategic identity)
      └── Constellation (5-15 systems, operational grouping)
          └── Star System (atomic unit of control)
              └── Sector (sub-system areas: belts, stations, planets)
```

- **Topology:** 2D hex grid of sectors (isometric-friendly)
- **Scale:** ~50,000+ sectors per galaxy
- **Connectivity:** Hyperspace lanes between systems create strategic chokepoints
- **Fog of War:** Cleared by scanning, decays over time

### Sector Archetypes

| Archetype | Resources | Threats | Points of Interest |
|-----------|-----------|---------|-------------------|
| Resource Rich | High metallum/crystallis | Pirate miners | Mining outposts |
| Hazardous | Rare materials | Radiation, debris | Anomalies, void rifts |
| Derelict Field | Salvage, data | Rogue AI | Derelict ships, data caches |
| Deep Void | Quantum crystals, dark matter | Unknown entities | Ancient artifacts, AXIA fragments |
| Contested | Mixed | High PvP activity | Territory borders, trade routes |

### Anomalies

Anomalies are procedurally placed but hand-crafted event templates layered on procedural scaffolding:

- **Data Cache:** AXIA upgrade fragments, blueprints, lore
- **Derelict Ship:** Salvageable resources, equipment, crew logs
- **Void Rift:** Dangerous but contains rare materials and story events
- **Signal Source:** Distress beacons, trade opportunities, trap ambushes
- **Ancient Structure:** Alien technology, endgame research prerequisites

---

*Last updated: March 2026*
