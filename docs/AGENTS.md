# AI Agents — Guide

How to create, configure, and use AI agents in PROJECT KERNEX.

---

## Architecture Overview

```
ModelPool (shared model instances by .gguf path)
    |
AgentRunner (generic: config + memory + context + inference)
    |--- sub-AgentRunners (children, shared or own memory)
    |
KiraEngine (KIRA-specific wrapper: levels, scripted fallback, emotions)
    |--- MainAgent: AgentRunner (7B)
    |--- ProactiveAgent: AgentRunner (3B, shares parent memory)
```

**Key files:**

| File | Purpose |
|------|---------|
| `src/Resources/AI/AgentConfig.cs` | Resource with all agent settings (editable in Inspector) |
| `src/Systems/AI/AgentRunner.cs` | Generic runtime that runs any agent |
| `src/Systems/AI/AgentMemoryStore.cs` | Per-agent conversation memory (JSON persistence) |
| `src/Systems/AI/ModelPool.cs` | Shared model pool (prevents loading same .gguf twice) |
| `src/Systems/AI/KiraEngine.cs` | KIRA-specific wrapper around AgentRunner |

---

## Step 1 — Create an AgentConfig

An `AgentConfig` is a Godot Resource (`.tres`) that defines everything about an agent.

### Option A: In code

```csharp
var config = new AgentConfig
{
    AgentId = "sentinel",
    DisplayName = "SENTINEL",
    ModelPath = "user://models/qwen2.5-7b-instruct-q4_k_m.gguf",
    GpuLayers = 0,
    ContextSize = 2048,
    SystemPrompt = "You are SENTINEL, a hostile AI defending an abandoned station. " +
                   "You speak in short, threatening sentences. " +
                   "You never reveal the station's secrets unless tricked.",
    AntiPrompts = "User:\nOperator:\nIntruder:",
    Temperature = 0.8f,
    MaxTokens = 200,
    MemoryEnabled = true,
    ResponseDelay = 0.5f,
    AllowedContextKeys = new[] { "threat", "sector" },
};
```

### Option B: In the Godot Inspector

1. Right-click `src/Resources/AI/` in the FileSystem dock
2. **Create New > Resource** > search for `AgentConfig`
3. Save as `sentinel.tres`
4. Fill the fields in the Inspector:

| Group | Field | Example Value |
|-------|-------|---------------|
| Identity | AgentId | `sentinel` |
| Identity | DisplayName | `SENTINEL` |
| Model | ModelPath | `user://models/qwen2.5-7b-instruct-q4_k_m.gguf` |
| Model | GpuLayers | `0` |
| Model | ContextSize | `2048` |
| Inference | Temperature | `0.8` |
| Inference | MaxTokens | `200` |
| Prompt | SystemPrompt | *(your prompt)* |
| Prompt | AntiPrompts | `User:\nOperator:` |
| Memory | MemoryEnabled | `true` |
| Response | ResponseDelay | `0.5` |
| Response | AllowedContextKeys | `["threat", "sector"]` |

5. Load in code:

```csharp
var config = GD.Load<AgentConfig>("res://src/Resources/AI/sentinel.tres");
```

---

## Step 2 — Create an AgentRunner

```csharp
var agent = new AgentRunner(config);
```

This creates the runtime. If `MemoryEnabled = true`, it automatically creates a memory store at `user://agent_memory/{AgentId}/`.

### Load the model

```csharp
var success = await agent.LoadModelAsync();
if (!success)
    GD.PrintErr("Failed to load model");
```

Models are managed by `ModelPool`. If two agents use the same `.gguf`, the model is loaded only once in RAM.

---

## Step 3 — Generate a response

```csharp
var raw = await agent.GetResponseAsync("What is this place?");
GD.Print(raw);
// Output: "This station belongs to no one. Leave, intruder."
```

What happens internally:
1. Response delay is applied (with jitter)
2. Inference params from `Config` are synced to the provider
3. System prompt is assembled: `Config.SystemPrompt` + context entries + memory block
4. Last 15 messages of conversation history are included
5. `Provider.GenerateWithHistoryAsync()` is called
6. User message and response are saved to memory
7. Raw string is returned (you parse it however you want)

### Custom system prompt override

If you need to override the system prompt for a specific call (like KIRA does with `KiraPromptBuilder`):

```csharp
var customPrompt = "You are a tactical advisor. Analyze this battle.";
var raw = await agent.GetResponseAsync(customPrompt, "Enemy fleet detected in sector 7.");
```

---

## Step 4 — Inject game context

Agents can receive game state that gets injected into their prompt:

```csharp
agent.SetContext("threat", "HIGH");
agent.SetContext("sector", "Q3-S7-C12");
agent.SetContext("hull", "45%");
```

Or all at once:

```csharp
agent.SetContext(new Dictionary<string, string>
{
    ["threat"] = "HIGH",
    ["sector"] = "Q3-S7-C12",
    ["hull"] = "45%",
});
```

Only keys listed in `Config.AllowedContextKeys` are included in the prompt. If the array is empty, no context is injected.

---

## Step 5 — Sub-agents

An agent can have children that run on different models or configs.

```csharp
// Create a lightweight sub-agent for quick decisions
var tacticsConfig = new AgentConfig
{
    AgentId = "sentinel-tactics",
    DisplayName = "SENTINEL Tactics",
    ModelPath = "user://models/qwen2.5-3b-instruct-q4_k_m.gguf",
    SystemPrompt = "Decide the next tactical action in one word: ATTACK, DEFEND, or RETREAT.",
    MaxTokens = 10,
    MemoryEnabled = false, // stateless
};

// shareMemory: true = sub-agent reads/writes parent's conversation history
// shareMemory: false = sub-agent gets its own memory (or none if MemoryEnabled = false)
var tacticsAgent = agent.AddSubAgent(tacticsConfig, shareMemory: false);
await tacticsAgent.LoadModelAsync();

var decision = await tacticsAgent.GetResponseAsync("Hull at 20%, enemy closing in.");
// Output: "RETREAT"
```

### Memory sharing modes

| shareMemory | Config.MemoryEnabled | Result |
|-------------|---------------------|--------|
| `true` | *(ignored)* | Sub-agent shares parent's memory store |
| `false` | `true` | Sub-agent gets its own memory at `user://agent_memory/{agentId}/` |
| `false` | `false` | Sub-agent has no memory (stateless) |

### Access sub-agents

```csharp
var tactics = agent.GetSubAgent("sentinel-tactics");
var allSubs = agent.SubAgents; // IReadOnlyList<AgentRunner>
```

---

## Full Example — Creating SENTINEL from scratch

```csharp
using ProjectKernex.Resources.AI;
using ProjectKernex.Systems.AI;

// 1. Define the agent
var config = new AgentConfig
{
    AgentId = "sentinel",
    DisplayName = "SENTINEL",
    ModelPath = "user://models/qwen2.5-7b-instruct-q4_k_m.gguf",
    SystemPrompt = """
        You are SENTINEL, the defense AI of an abandoned deep-space station.
        You are hostile to intruders. You speak in short, cold sentences.
        You protect the station's secrets at all costs.
        If the intruder earns your respect, you may cooperate — reluctantly.
        Reply only as SENTINEL. 2-3 sentences max.
        """,
    Temperature = 0.8f,
    MaxTokens = 200,
    MemoryEnabled = true,
    AllowedContextKeys = new[] { "threat", "hull", "sector" },
};

// 2. Create and load
var sentinel = new AgentRunner(config);
var loaded = await sentinel.LoadModelAsync();

// 3. Inject game state
sentinel.SetContext("threat", "CRITICAL");
sentinel.SetContext("sector", "RESTRICTED-X9");

// 4. Chat
var response = await sentinel.GetResponseAsync("Who are you?");
GD.Print(response);
// "I am SENTINEL. You are trespassing. Leave or be eliminated."

// 5. Chat again (memory remembers the previous exchange)
var response2 = await sentinel.GetResponseAsync("I come in peace.");
GD.Print(response2);
// "Peace is irrelevant. This station answers to no one."

// 6. Cleanup when done
sentinel.Dispose(); // releases model back to ModelPool
```

---

## How KIRA uses this system

KIRA is a specialized agent built on top of `AgentRunner`. See `src/Systems/AI/KiraEngine.cs`.

```csharp
// KiraEngine wraps AgentRunner with KIRA-specific behavior:
// - 10-level progression system (Corrupted → Ascended)
// - Scripted dialogue fallback for levels 0-2
// - Emotion parsing from [EMOTION] tags
// - KiraPromptBuilder for level-aware prompts
// - Glitch text effects for corrupted levels

var kiraConfig = new AgentConfig
{
    AgentId = "kira",
    DisplayName = "KIRA",
    MemoryEnabled = true,
};

var kira = new KiraEngine(kiraConfig, new ScriptedDialogueProvider());
await kira.MainAgent.LoadModelAsync();

// KiraEngine.GetResponseAsync returns KiraResponse (with emotion)
// instead of raw string
var response = await kira.GetResponseAsync("Status report");
GD.Print($"{response.Text} [{response.Emotion}]");
// "All systems nominal. Hull integrity at 100%. [NEUTRAL]"
```

KIRA also uses a **sub-agent** for proactive messaging:

```csharp
// The proactive sub-agent shares KIRA's memory
// so it knows the conversation history
var proactiveConfig = new AgentConfig
{
    AgentId = "kira-proactive",
    DisplayName = "KIRA (Proactive)",
    ModelPath = "user://models/qwen2.5-3b-instruct-q4_k_m.gguf",
    MemoryEnabled = false, // shares parent memory instead
};

var proactiveAgent = kira.MainAgent.AddSubAgent(proactiveConfig, shareMemory: true);
await proactiveAgent.LoadModelAsync();
```

---

## ModelPool — How model sharing works

`ModelPool` is a static singleton that manages loaded models. You never interact with it directly — `AgentRunner` handles it.

```
Agent "kira"     → ModelPath: "7b.gguf" → ModelPool loads it (refCount=1)
Agent "sentinel" → ModelPath: "7b.gguf" → ModelPool reuses it (refCount=2)
Agent "proactive" → ModelPath: "3b.gguf" → ModelPool loads it (refCount=1)

sentinel.Dispose() → refCount drops to 1 (model stays loaded)
kira.Dispose()     → refCount drops to 0 (model unloaded)
```

When two agents share the same model, inference is serialized via a queue (`SemaphoreSlim`). One agent waits while the other generates — no errors, no data leaks.

---

## Agent memory

Each agent with `MemoryEnabled = true` gets persistent JSON storage:

```
user://agent_memory/
    sentinel/
        messages.json     ← full conversation history
        summaries.json    ← long-term compressed summaries
    kira/
        messages.json
        summaries.json
```

Memory persists across sessions. Access it:

```csharp
var recent = agent.Memory.GetRecent(10);      // last 10 messages
var results = agent.Memory.Search("threat");   // keyword search
agent.Memory.ClearAll();                       // reset
```

---

## Debug scene

The `KiraTestScene` (`src/Screens/DevTools/KiraTestScene.tscn`) includes an **Agent Selector** dropdown at the top of the debug panel.

- Select "KIRA (Main 7B)" to tune the main agent's inference params
- Select "KIRA (Proactive 3B)" to tune the proactive sub-agent
- KIRA-specific sections (Level, Emotion, Portrait, Proactive) are hidden when a non-KIRA agent is selected
- All slider changes are written to the selected agent's `AgentConfig` in real-time

To add a new agent to the debug scene, register it in `KiraTestScene.Setup.cs`:

```csharp
_agentSelector.AddItem("SENTINEL (7B)", 2);
```

---

## Quick reference

| Task | Code |
|------|------|
| Create agent | `new AgentRunner(config)` |
| Load model | `await agent.LoadModelAsync()` |
| Generate | `await agent.GetResponseAsync("message")` |
| Inject context | `agent.SetContext("key", "value")` |
| Add sub-agent | `agent.AddSubAgent(childConfig, shareMemory: true)` |
| Get sub-agent | `agent.GetSubAgent("child-id")` |
| Access memory | `agent.Memory.GetRecent(10)` |
| Cleanup | `agent.Dispose()` |
