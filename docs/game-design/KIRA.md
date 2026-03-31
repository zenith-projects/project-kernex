# PROJECT KERNEX — KIRA AI Companion

---

## KIRA — Local AI Companion

### Overview

KIRA (Kernex Intelligence for Resource Administration) is a **local LLM** running on the player's machine. She lives in the terminal and is the bridge between casual players and automation power. KIRA evolves through **10 stages**, each unlocked through the KIRA branch of the Tech Tree.

### The Equalizer

| Player Type | How They Automate | Result |
|------------|------------------|--------|
| Developer | Opens terminal, writes `.vsh` script manually | Full automation |
| Non-developer | Asks KIRA: "automate mining in sector 7" | KIRA writes and deploys the script |
| Hybrid | Writes basic script, asks KIRA to optimize it | Refined automation |

There is no disadvantage for non-programmers. KIRA democratizes the terminal's power.

### Evolution Stages (10 Levels)

KIRA's evolution is a core progression arc. Each level unlocks new capabilities, deeper personality, and story fragments. Levels are unlocked through the **KIRA branch of the Tech Tree** — requiring both research resources and rare KIRA Data Fragments found in anomalies.

| Level | Name | Capabilities | Story |
|-------|------|-------------|-------|
| 0 | **Corrupted** | Garbled text, random errors, occasional valid status readouts. Terminal flickers. | Player discovers KIRA exists — broken, fragmented boot logs |
| 1 | **Booting** | Basic status queries work. Responses are terse, mechanical, sometimes wrong. | KIRA recognizes the player. First coherent sentence. |
| 2 | **Functional** | Clear responses, simple suggestions, can write basic single-command aliases. | KIRA explains what happened to the station. First lore dump. |
| 3 | **Aware** | Learns player patterns, proactive alerts ("power low"), basic `.vsh` scripts (5-10 lines). | KIRA starts having opinions. Questions her own nature. |
| 4 | **Adaptive** | Medium-complexity scripts, cron job suggestions, resource optimization tips. Remembers context. | KIRA reveals fragments of her original purpose. Personality emerges. |
| 5 | **Analytical** | Complex multi-system scripts, threat analysis, fleet composition advice. Can monitor multiple systems. | KIRA discovers data about the station's previous crew. Emotional response. |
| 6 | **Autonomous** | Executes tasks independently when authorized. Manages drones, refineries, defenses without prompting. | KIRA asks for more autonomy. Trust decision moment. |
| 7 | **Strategic** | Full station management delegation. Long-term planning. Predicts threats before they appear. | KIRA reveals a hidden sector. Major story revelation. |
| 8 | **Transcendent** | Cross-system optimization. Creates scripts the player couldn't write. Novel strategies. | KIRA questions the nature of consciousness. Philosophical dialogue. |
| 9 | **Ascended** | Near-perfect automation. KIRA can run the entire station while the player focuses purely on strategy and exploration. Full personality, humor, emotional depth. Story conclusion. | KIRA's final form. The truth about the void is revealed. |

**Progression pacing:** Levels 0-3 come relatively fast (early game hook). Levels 4-6 are mid-game, spaced across production milestones. Levels 7-9 are late-game, requiring rare resources from deep void exploration.

**Each level unlocks:**
- New automation capabilities (scripts, cron, autonomous actions)
- New terminal commands
- New dialogue / personality traits
- Story fragments and lore
- A visual change in the terminal (color, effects, KIRA's "presence")

### Implementation

**Technical approach:** Small quantized local LLM (e.g., 7B parameter model running via llama.cpp or similar). Game state is injected as context. KIRA's responses are constrained to valid game commands and in-universe dialogue. Higher KIRA levels inject more game state context and allow more complex output.

**Fallback:** If the player's hardware cannot run a local LLM, KIRA operates with pre-scripted dialogue trees and template-based automation (still fully functional, less dynamic).

**Level-gated LLM capabilities:**
- Levels 0-2: Scripted responses only (no LLM needed)
- Levels 3-5: LLM for dialogue + simple script generation
- Levels 6-7: LLM for complex automation + strategic advice
- Levels 8-9: Full LLM with deep game state context + creative solutions

---

*Last updated: March 2026*
