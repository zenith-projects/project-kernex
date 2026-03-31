# PROJECT KERNEX — Technical Architecture

---

## Technical Architecture

### Infinite World: Chunked Coordinates

To avoid floating-point precision problems in an infinite world, PROJECT KERNEX uses **chunked coordinates**:

```
Global Position = SectorID (64-bit integer) + LocalOffset (32-bit float)
```

- Each sector has its own local coordinate space (32-bit floats are fine within a sector)
- When crossing a sector boundary, SectorID updates and LocalOffset resets
- The GPU only renders one sector's worth of coordinates — no precision loss
- **Multiplayer-friendly:** Sync SectorID + LocalOffset pairs; each client computes relative positions locally

This approach is simpler and more performant than origin shifting for a sector-based isometric game.

### Offline-First → Online Architecture

The single-player game is designed as a **local server + client** even though both run in the same process:

| Layer | Single-Player | Online (Future) |
|-------|--------------|-----------------|
| Game State | In-memory, local process | In-memory, remote server |
| Persistence | Local save file | Database (Redis/PostgreSQL) |
| Commands | Direct function calls | RPC calls (same API) |
| Auth | None | OAuth/token-based |
| World Gen | Local seed | Shared seed + server authority |

**Command/Event pattern:** Every player action is a command. Every state change is an event. This supports replay, synchronization, and conflict resolution.

**Deterministic simulation:** Same commands → same results. Only commands need to be synced in multiplayer, not full game state.

### Isometric Rendering

```
Viewport (what the player sees)
  → Chunk Manager (loads/unloads chunks around camera)
    → Chunk (NxN tiles, local coordinates)
      → Tile (individual isometric cell with entities)
```

- Spatial hash map keyed on chunk coordinates for O(1) lookup
- Only render chunks intersecting viewport + one buffer chunk
- Despawn/pool entities outside viewport for memory management
- Multi-zoom levels: Station detail → Sector view → System map → Galaxy map

### MMO Server Architecture (Future)

```
[Clients]
    → [Connection Server] — auth, routing, sessions
    → [Game State Service] — in-memory world state (source of truth)
    → [Persistence Layer] — selective writes to database
    → [Database] — recovery mechanism, NOT source of truth
```

- Procedural universe is never synced (deterministic from shared seed)
- Only player-created state is synchronized: stations, discoveries, territory
- Per-system instancing initially; seamless transitions within systems later
- Resource depletion tracked per-sector, synced periodically

---

*Last updated: March 2026*
