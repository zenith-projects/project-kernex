# PROJECT KERNEX — Module Catalog

> *Version 0.1 — Pre-production Draft*
> *Modules are the building blocks of everything. Your ship, station, drones — all are defined by the modules installed.*

---

## How Modules Work

1. **Unlock** — Progress through The Constellation to unlock blueprints
2. **Craft or Buy** — Use materials to manufacture the module (or buy from NPC traders / market)
3. **Install** — Equip the module in a compatible slot (ship, station, drone)
4. **Upgrade** — Spend materials to level the module from 1 to 16
5. **Swap** — Modules can be uninstalled and reinstalled freely

### Rarity

| Rarity | Color | Drop/Unlock Source | Stat Multiplier |
|--------|-------|--------------------|-----------------|
| **Common** | White | Starter / inner Constellation nodes | 1.0x (baseline) |
| **Uncommon** | Green | Mid Constellation nodes, basic crafting | 1.2-1.4x |
| **Rare** | Blue | Outer Constellation nodes, anomaly drops | 1.5-1.8x |
| **Epic** | Purple | Keystone unlocks, deep void anomalies | 2.0-2.5x |
| **Legendary** | Gold | Bridge synergies, boss drops, Precursor tech | 3.0x + unique effect |

Higher rarity modules have better base stats AND may have bonus passive effects (e.g., a Legendary Shield might also regenerate hull).

### Upgrade Levels (1-16)

| Level | Materials | Stat Boost | Notes |
|-------|-----------|-----------|-------|
| 1-4 | Common (Metallum, Crystallis) | +5% per level | Always succeeds |
| 5-8 | Uncommon (Steel Alloy, Silicon Wafer) | +7% per level | Always succeeds. Visual change at 5. |
| 9-12 | Rare (Titan Alloy, Superconductor) | +10% per level | Always succeeds. Visual change at 9. |
| 13-15 | Epic (Dark Matter components) | +15% per level | Failure chance (module doesn't break, materials lost). KIRA reduces failure. Visual change at 13. |
| 16 | Legendary (Quantum Crystal components) | +20% | Always succeeds. Max power. Unique glow effect. |

A Level 16 Legendary module is roughly **6x stronger** than a Level 1 Common module of the same type.

### Slots

Every ship, station, and drone has a fixed number of slots by category. Larger/higher-tier platforms have more slots.

---

## 1. ENERGY — Power Generation

*Where your power comes from. Without energy, nothing works.*

Energy modules go in **Station Power Slots** and **Ship Power Slots**.

### Station Power Modules

| Module | Rarity | Source Cluster | Output | Notes |
|--------|--------|---------------|--------|-------|
| **Solar Array** | Common | Starter | Low | Free energy, no fuel. Output depends on star proximity. |
| **Fission Reactor** | Common | ENGINEERING inner | Medium | Burns Deuterium. Reliable, cheap to build. |
| **RTG Unit** | Uncommon | PRODUCTION mid | Low-Med | Radioisotope generator. Zero fuel, low output, lasts forever. |
| **Fusion Reactor** | Uncommon | ENGINEERING mid | High | Burns Helium-3. Efficient, clean, mid-cost. |
| **Antimatter Reactor** | Rare | ENGINEERING outer | Very High | Burns Dark Matter Residue. Expensive fuel, massive output. |
| **Quantum Reactor** | Epic | ENGINEERING keystone path | Extreme | Burns Quantum Crystals. Near-infinite power. |
| **Void Siphon** | Legendary | ANOMALY SCIENCE outer | Extreme | Harvests energy from void anomalies. Unstable. |
| **Dyson Collector** | Legendary | ENGINEERING keystone | Infinite | Orbital star harvester. Endgame. |

### Ship Power Modules

| Module | Rarity | Source Cluster | Output | Notes |
|--------|--------|---------------|--------|-------|
| **Compact Fission** | Common | Starter | Low | Basic ship reactor. |
| **Ship Fusion Core** | Uncommon | ENGINEERING mid | Medium | Standard mid-game ship power. |
| **Micro Antimatter** | Rare | ENGINEERING outer | High | High-end ship power for capital ships. |
| **Alien Power Cell** | Legendary | ANOMALY SCIENCE keystone | Very High | Precursor tech. Unique energy signature. |

---

## 2. THERMAL — Heat Management

*Reactors, weapons, and engines generate heat. Without radiators, your systems overheat and shut down.*

Thermal modules go in **Station Utility Slots** and **Ship Utility Slots**.

| Module | Rarity | Source Cluster | Cooling | Notes |
|--------|--------|---------------|---------|-------|
| **Basic Radiator** | Common | Starter | Low | Passive heat dissipation panels. |
| **Heat Sink Array** | Common | ENGINEERING inner | Medium | Absorbs burst heat, dumps slowly. Good for combat. |
| **Cryo Radiator** | Uncommon | PRODUCTION mid | High | Active cooling with coolant loops. Consumes energy. |
| **Phase-Change Cooler** | Rare | ENGINEERING outer | Very High | Uses material phase transitions. Very efficient. |
| **Plasma Radiator** | Epic | ANOMALY SCIENCE mid | Extreme | Dumps heat as plasma jets. Also damages nearby enemies. |
| **Void Cooler** | Legendary | ANOMALY SCIENCE outer | Extreme | Dumps heat into void space. Zero radiation signature. |

**Overheating mechanic:** If heat exceeds capacity, modules shut down in priority order (lowest priority first). Players must balance power output vs cooling capacity.

---

## 3. WEAPONS — Offensive Armament

*What deals damage. Ship weapons and station turrets.*

### Ship Weapons (Ship Weapon Slots)

| Module | Rarity | Source Cluster | Type | Notes |
|--------|--------|---------------|------|-------|
| **Basic Laser** | Common | Starter | Energy | Low damage, no ammo cost, fast fire rate. |
| **Pulse Laser** | Common | WEAPONS inner | Energy | Burst fire, good vs shields. |
| **Missile Pod** | Uncommon | WEAPONS inner | Kinetic | Tracking missiles, splash damage. Consumes ammo. |
| **Beam Cannon** | Uncommon | WEAPONS mid | Energy | Continuous beam, high DPS, high energy cost. |
| **Railgun** | Rare | WEAPONS mid | Kinetic | Single shot, extreme damage, ignores shields. Slow reload. |
| **Torpedo Launcher** | Rare | WEAPONS mid | Kinetic | Heavy damage, slow, tracking. Anti-capital ship. |
| **Plasma Lance** | Epic | WEAPONS outer | Energy | Pierces multiple targets in a line. |
| **Flak Battery** | Uncommon | OFFENSE mid | Kinetic | Area denial, anti-fighter/drone. |
| **EMP Projector** | Rare | OFFENSE mid | EMP | Disables enemy shields and systems temporarily. |
| **Singularity Torpedo** | Legendary | WEAPONS keystone | Exotic | Creates micro black hole. Deletes target. |
| **Void Beam** | Legendary | ANOMALY SCIENCE outer | Exotic | Alien weapon. Damages hull directly, bypasses everything. |

### Station Turrets (Station Turret Slots)

| Module | Rarity | Source Cluster | Type | Notes |
|--------|--------|---------------|------|-------|
| **Point Defense Turret** | Common | Starter | Energy | Auto-targets missiles and small threats. |
| **Laser Turret** | Common | OFFENSE inner | Energy | Standard station defense. |
| **Missile Battery** | Uncommon | OFFENSE inner | Kinetic | Long-range, tracking, splash. |
| **Gauss Turret** | Uncommon | OFFENSE mid | Kinetic | High damage, ignores shields. |
| **EMP Tower** | Rare | OFFENSE mid | EMP | Area EMP burst, disables approaching fleets. |
| **Plasma Artillery** | Rare | OFFENSE outer | Energy | Heavy damage, slow, area effect. |
| **Railgun Platform** | Epic | OFFENSE outer | Kinetic | Extreme range, armor-piercing. |
| **Ion Cannon** | Legendary | OFFENSE keystone | Energy | Single orbital beam. Devastating. Long cooldown. |
| **Minefield Deployer** | Uncommon | OFFENSE mid | Kinetic | Deploys smart mines around station. |
| **Gravity Well Generator** | Epic | ANOMALY SCIENCE mid | Exotic | Slows all enemies in range. |

---

## 4. DEFENSE — Shields & Armor

*What keeps you alive.*

### Ship Shields (Ship Shield Slot)

| Module | Rarity | Source Cluster | Capacity | Notes |
|--------|--------|---------------|----------|-------|
| **Basic Deflector** | Common | Starter | Low | Standard shield. Recharges out of combat. |
| **Reinforced Shield** | Uncommon | DEFENSE inner | Medium | Faster recharge, higher capacity. |
| **Adaptive Shield** | Rare | DEFENSE mid | Medium-High | Shifts resistance type based on incoming damage. |
| **Phase Shield** | Epic | DEFENSE outer | High | Absorbs energy damage and converts to power. |
| **Quantum Barrier** | Legendary | DEFENSE keystone path | Very High | Reflects a % of damage back at attacker. |

### Ship Armor (Ship Armor Slot)

| Module | Rarity | Source Cluster | HP Bonus | Notes |
|--------|--------|---------------|----------|-------|
| **Standard Plating** | Common | Starter | +10% hull | Basic armor. |
| **Reinforced Plating** | Uncommon | DEFENSE inner | +25% hull | Heavier, slightly slower. |
| **Titan Alloy Armor** | Rare | PRODUCTION outer | +50% hull | Very heavy, significant speed penalty. |
| **Nano-Armor** | Epic | DEFENSE outer | +40% hull | Self-repairs slowly during combat. |
| **Living Hull** | Legendary | ANOMALY SCIENCE keystone | +60% hull | Organic alien tech. Regenerates, grows stronger after damage. |

### Station Shields (Station Shield Slot)

| Module | Rarity | Source Cluster | Capacity | Notes |
|--------|--------|---------------|----------|-------|
| **Shield Generator Mk1** | Common | Starter | Low | Basic station shield. |
| **Shield Generator Mk2** | Uncommon | DEFENSE inner | Medium | +40% capacity, faster recharge. |
| **Shield Generator Mk3** | Rare | DEFENSE mid | High | Covers entire station, slow recharge. |
| **Fortress Shield** | Epic | DEFENSE outer | Very High | Massive capacity, resists EMP. |
| **Phase Shield Array** | Legendary | DEFENSE keystone | Extreme | Absorbs energy attacks, converts to power for turrets. |

---

## 5. PROPULSION — Engines & Navigation

*How you move through space.*

Ship Engine Slot (one primary engine per ship).

| Module | Rarity | Source Cluster | Speed | Notes |
|--------|--------|---------------|-------|-------|
| **Chemical Thruster** | Common | Starter | Slow | Cheap, low fuel consumption. |
| **Ion Drive** | Common | NAVIGATION inner | Medium | Efficient, good for long range. |
| **Fusion Drive** | Uncommon | NAVIGATION mid | Fast | Standard mid-game engine. |
| **Afterburner** | Uncommon | FLEET mid | Boost | Utility module — temporary speed boost, high fuel cost. |
| **Hyperspace Module** | Rare | NAVIGATION mid | Warp | Enables inter-system travel. Not a speed boost — a travel mode. |
| **Warp Drive** | Epic | NAVIGATION outer | Very Fast | Fast in-system travel + efficient hyperspace. |
| **Jump Drive** | Legendary | NAVIGATION keystone | Instant | Short-range teleportation within a system. Cooldown. |
| **Void Shift Engine** | Legendary | ANOMALY SCIENCE outer | Instant | Alien tech. Phase through obstacles. Unpredictable. |

---

## 6. MINING — Resource Extraction

*What your mining drones and ships use to extract resources.*

Drone Equipment Slots and Ship Utility Slots.

| Module | Rarity | Source Cluster | Yield | Notes |
|--------|--------|---------------|-------|-------|
| **Basic Mining Laser** | Common | Starter | 1.0x | Standard extraction. |
| **Improved Drill** | Common | PRODUCTION inner | 1.3x | Faster, slightly more yield. |
| **Deep Core Extractor** | Uncommon | PRODUCTION mid | 1.5x | Can mine rare ores from asteroid cores. |
| **Plasma Cutter** | Rare | PRODUCTION mid | 2.0x | Fast extraction, works on dense asteroids. |
| **Molecular Disassembler** | Epic | PRODUCTION outer | 2.5x | Breaks down materials at atomic level. |
| **Quantum Harvester** | Legendary | PRODUCTION keystone | 3.0x | Extracts ALL materials from a target, nothing wasted. |
| **Planet Drill** | Rare | PRODUCTION outer | Special | Mines planetary surfaces. Requires orbit. |
| **Gas Collector** | Uncommon | PRODUCTION mid | Special | Harvests gases from gas giants (Helium-3). |

---

## 7. CARGO — Storage & Transport

*How much you carry and store.*

| Module | Rarity | Source Cluster | Capacity | Notes |
|--------|--------|---------------|----------|-------|
| **Basic Cargo Hold** | Common | Starter | 100 units | Standard ship/drone storage. |
| **Expanded Hold** | Uncommon | LOGISTICS inner | 200 units | Larger capacity, slightly heavier. |
| **Compressed Storage** | Rare | LOGISTICS mid | 400 units | Same size, double capacity via compression. |
| **Quantum Container** | Epic | LOGISTICS outer | 1000 units | Pocket dimension storage. Weightless. |
| **Station Storage Silo** | Common | Starter | 5000 units | Basic station storage module. |
| **Station Warehouse** | Uncommon | LOGISTICS inner | 15000 units | Large station storage. |
| **Orbital Depot** | Rare | LOGISTICS outer | 50000 units | Satellite storage, safe from raids. |
| **Dimensional Vault** | Legendary | LOGISTICS keystone | 200000 units | Near-infinite, unhackable, unraidable. |

---

## 8. TECH — Electronics & Computing

*Processing power, research speed, and electronic systems.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **Basic Computer** | Common | Starter | 1.0x research speed | Standard research processing. |
| **Advanced Processor** | Uncommon | ENGINEERING mid | 1.3x research speed | Faster research across all branches. |
| **Quantum Processor** | Rare | PRODUCTION outer | 1.6x research speed | Enables parallel research in same branch. |
| **Neural Processor** | Epic | KIRA mid | 2.0x research speed | Optimized for KIRA evolution research. |
| **Alien Computation Core** | Legendary | ANOMALY SCIENCE keystone | 3.0x research speed | Precursor tech. Thinks in ways humans can't. |
| **Sensor Array** | Common | EXPLORATION inner | +30% scan range | Better sensors for exploration. |
| **Deep Scanner** | Uncommon | EXPLORATION mid | +60% scan range | Reveals hidden objects and anomalies. |
| **Omnisensor** | Epic | EXPLORATION keystone path | +100% scan range | Scans entire sectors instantly. |

---

## 9. AUTOMATION — Scripting & Control

*Modules that enhance the terminal and automation systems.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **Script Processor** | Common | AUTOMATION inner | Run 2 scripts simultaneously | Basic automation hardware. |
| **Cron Engine** | Uncommon | AUTOMATION mid | Run 5 cron jobs, 1-min minimum interval | Scheduled task hardware. |
| **Parallel Executor** | Rare | AUTOMATION mid | Run 10 scripts simultaneously | Heavy automation. |
| **Event Reactor** | Rare | AUTOMATION outer | Trigger scripts on game events | Reactive automation (e.g., "on attack, activate shields"). |
| **Pipeline Controller** | Epic | AUTOMATION outer | Chain scripts together, shared variables | Complex multi-step automation. |
| **Quantum Scheduler** | Legendary | AUTOMATION keystone | Unlimited scripts, predictive scheduling | Scripts run before events happen (KIRA predicts). |

---

## 10. KIRA — AI Hardware

*Physical modules that upgrade KIRA's capabilities. Separate from Constellation research — these are the hardware KIRA runs on.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **AI Core Fragment** | Common | KIRA inner | Enables KIRA Level 1-2 | Basic AI hardware. Found in early anomalies. |
| **Neural Network Card** | Uncommon | KIRA mid | +30% KIRA response quality | Better context understanding. |
| **Memory Bank** | Uncommon | KIRA mid | KIRA remembers 2x more context | Longer conversation history. |
| **Cognitive Accelerator** | Rare | KIRA outer | KIRA processes 2x faster | Less wait time for complex scripts. |
| **Personality Core** | Rare | KIRA outer | Unlocks KIRA personality traits | Humor, opinions, emotional responses. |
| **Strategic Module** | Epic | KIRA keystone path | KIRA plans long-term strategy | Suggests research paths, warns of threats days ahead. |
| **Empathy Chip** | Epic | KIRA keystone path | KIRA develops emotional intelligence | Deeper dialogue, story reveals, debates. |
| **Quantum Mind** | Legendary | KIRA keystone | KIRA Level 9 hardware | Full AI consciousness. The final KIRA. |
| **KIRA Data Fragment** | Special | Anomalies only | Required for each KIRA level | Cannot be crafted. Must explore to find. |

---

## 11. STEALTH — Covert Systems

*Hiding, jamming, and deception modules.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **Sensor Dampener** | Common | STEALTH inner | -30% energy signature | Harder to detect. |
| **ECM Pod** | Uncommon | STEALTH inner | Jams enemy targeting for 10s | Active countermeasure. Cooldown. |
| **Cloaking Device** | Rare | STEALTH mid | Ship becomes invisible while stationary | Decloak on move or fire. |
| **Active Cloak** | Epic | STEALTH outer | Ship invisible while moving (slow) | Decloak on fire. Massive energy cost. |
| **Decoy Generator** | Uncommon | STEALTH mid | Deploys holographic copies of your ship | Confuses enemy targeting. |
| **Signal Spoofer** | Rare | STEALTH mid | Your station appears as a different type on scans | Enemies misjudge your defenses. |
| **Phase Cloak** | Legendary | STEALTH keystone | Station becomes undetectable for a time | Entire station vanishes from all sensors. |

---

## 12. COMMUNICATIONS — Comms & Coordination

*Range, encryption, and alliance features.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **Basic Antenna** | Common | Starter | Standard comms range | Baseline communications. |
| **Signal Booster** | Uncommon | COMMUNICATIONS inner | +50% comms range | Talk to farther stations. |
| **Encrypted Channel** | Uncommon | COMMUNICATIONS mid | Secure communications | Messages can't be intercepted (anti-STEALTH). |
| **SubSpace Relay** | Rare | COMMUNICATIONS outer | Galaxy-wide comms | Talk to anyone, anywhere. |
| **Alliance Beacon** | Rare | COMMUNICATIONS mid | Coordinate fleet movements with allies | Shared targeting, formation commands. |
| **Propaganda Array** | Epic | COMMUNICATIONS outer | Broadcast to enemy players (future PvP) | Psychological warfare, demoralize. |
| **Quantum Link** | Legendary | COMMUNICATIONS keystone | Zero-latency comms, unblockable | Cannot be jammed or intercepted. |

---

## 13. COLONIZATION — Outpost Modules

*Modules specific to outposts and territorial expansion.*

| Module | Rarity | Source Cluster | Effect | Notes |
|--------|--------|---------------|--------|-------|
| **Outpost Core** | Uncommon | COLONIZATION inner | Deploy a basic outpost | Minimal functionality, needs modules. |
| **Colony Hub** | Rare | COLONIZATION mid | Full secondary station | Significant investment, full module slots. |
| **Industrial Module** | Uncommon | COLONIZATION mid | +50% production at outpost | Specialized for resource generation. |
| **Military Module** | Uncommon | COLONIZATION mid | Defense + fleet docking at outpost | Specialized for military staging. |
| **Sovereignty Beacon** | Rare | COLONIZATION outer | Claim territory around outpost | Defines your territorial borders (future online). |
| **Warp Gate** | Epic | COLONIZATION outer | Instant travel between your stations | Network of connected outposts. |
| **Dyson Forge** | Legendary | COLONIZATION keystone | Star-powered mega-factory | Produces everything. Endgame production. |

---

## Module Slot Summary by Platform

### Player Ship (varies by ship class)

| Ship Class | Weapon | Shield | Armor | Engine | Power | Utility | Total |
|-----------|--------|--------|-------|--------|-------|---------|-------|
| Fighter | 2 | 1 | 1 | 1 | 1 | 1 | 7 |
| Cruiser | 3 | 1 | 1 | 1 | 1 | 2 | 9 |
| Battleship | 4 | 2 | 2 | 1 | 2 | 2 | 13 |
| Destroyer | 5 | 1 | 1 | 1 | 2 | 3 | 13 |
| Carrier | 2 | 2 | 2 | 1 | 3 | 4 | 14 |
| Dreadnought | 6 | 2 | 3 | 1 | 3 | 3 | 18 |
| Titan | 8 | 3 | 3 | 2 | 4 | 5 | 25 |

### Station (expands with station tier)

| Station Tier | Turret | Shield | Module | Power | Total |
|-------------|--------|--------|--------|-------|-------|
| 1 - Outpost | 2 | 1 | 4 | 1 | 8 |
| 2 - Station | 4 | 1 | 8 | 2 | 15 |
| 3 - Complex | 6 | 2 | 14 | 3 | 25 |
| 4 - Hub | 10 | 2 | 20 | 4 | 36 |
| 5 - Nexus | 16 | 3 | 30 | 6 | 55 |

### Drones (varies by drone type)

| Drone Type | Equipment Slots |
|-----------|----------------|
| Mining Drone | 2 (mining + cargo) |
| Combat Drone | 3 (weapon + shield + utility) |
| Scout Drone | 2 (sensor + engine) |
| Salvage Drone | 2 (tool + cargo) |
| Heavy Drone | 5 (weapon + weapon + shield + cargo + utility) |

---

## Design Notes

- **Modules ARE the game.** Every meaningful decision involves choosing which modules to install, upgrade, and prioritize. The Constellation unlocks options; modules are the execution.
- **Tradeoffs everywhere.** A Fission Reactor is cheap but weak. An Antimatter Reactor is powerful but burns rare fuel. Nano-Armor self-repairs but gives less HP than Titan Alloy. Every choice has a cost.
- **Rarity is not always better.** A Level 16 Common module can outperform a Level 1 Epic module. Investment matters more than luck.
- **KIRA Data Fragments are special.** They cannot be crafted, traded, or bought. You must explore anomalies to find them. This is the one resource that forces exploration regardless of playstyle.
- **Alien (Precursor) modules** are fundamentally different from human tech — unique effects that don't exist in any other module category. They come from the ANOMALY SCIENCE cluster and are always Legendary rarity.

---

*This catalog will expand as development progresses. Module stats are design targets, not final values.*
*Last updated: March 2026*
