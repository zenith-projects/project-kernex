# Example 05: C# Signal Patterns

## User Input
> "Show me all the ways to use signals in Godot C# with practical examples"

## Expected Output

### Pattern 1: Basic Custom Signal

```csharp
public partial class Door : Node3D
{
    [Signal] public delegate void OpenedEventHandler();
    [Signal] public delegate void ClosedEventHandler();

    private bool _isOpen;

    public void Toggle()
    {
        _isOpen = !_isOpen;
        EmitSignal(_isOpen ? SignalName.Opened : SignalName.Closed);
    }
}

// In the parent scene script:
public override void _Ready()
{
    var door = GetNode<Door>("Door");
    door.Opened += () => GD.Print("Door opened!");
    door.Closed += () => GD.Print("Door closed!");
}
```

### Pattern 2: Signals with Parameters

```csharp
public partial class ScoreManager : Node
{
    [Signal] public delegate void ScoreChangedEventHandler(int newScore, int delta);
    [Signal] public delegate void HighScoreReachedEventHandler(int score);
    [Signal] public delegate void ComboEventHandler(int comboCount, string comboType);

    private int _score;
    private int _highScore;

    public void AddScore(int points, string reason = "")
    {
        int oldScore = _score;
        _score += points;
        EmitSignal(SignalName.ScoreChanged, _score, points);

        if (_score > _highScore)
        {
            _highScore = _score;
            EmitSignal(SignalName.HighScoreReached, _highScore);
        }
    }

    public void TriggerCombo(int count, string type)
    {
        EmitSignal(SignalName.Combo, count, type);
    }
}

// Connecting with typed handler:
public override void _Ready()
{
    var scoreManager = GetNode<ScoreManager>("/root/ScoreManager");
    scoreManager.ScoreChanged += OnScoreChanged;
    scoreManager.HighScoreReached += OnHighScore;
    scoreManager.Combo += OnCombo;
}

private void OnScoreChanged(int newScore, int delta)
{
    _scoreLabel.Text = $"Score: {newScore} (+{delta})";
}

private void OnHighScore(int score)
{
    _highScoreLabel.Text = $"NEW HIGH SCORE: {score}!";
}

private void OnCombo(int count, string type)
{
    _comboLabel.Text = $"{count}x {type} COMBO!";
}
```

### Pattern 3: Connecting Built-in Signals

```csharp
public override void _Ready()
{
    // Area signals
    var area = GetNode<Area3D>("DetectionArea");
    area.BodyEntered += OnBodyEntered;
    area.BodyExited += OnBodyExited;
    area.AreaEntered += OnAreaEntered;

    // Button signals
    var button = GetNode<Button>("UI/Button");
    button.Pressed += OnButtonPressed;
    button.MouseEntered += OnButtonHover;

    // Timer signals
    var timer = GetNode<Timer>("CooldownTimer");
    timer.Timeout += OnCooldownFinished;

    // AnimationPlayer signals
    var anim = GetNode<AnimationPlayer>("AnimationPlayer");
    anim.AnimationFinished += OnAnimationFinished;

    // Visibility
    var notifier = GetNode<VisibleOnScreenNotifier3D>("Notifier");
    notifier.ScreenEntered += OnBecameVisible;
    notifier.ScreenExited += OnBecameInvisible;

    // Viewport
    GetViewport().SizeChanged += OnWindowResized;
}

private void OnBodyEntered(Node3D body) { }
private void OnBodyExited(Node3D body) { }
private void OnAreaEntered(Area3D area) { }
private void OnButtonPressed() { }
private void OnButtonHover() { }
private void OnCooldownFinished() { }
private void OnAnimationFinished(StringName animName) { }
private void OnBecameVisible() { }
private void OnBecameInvisible() { }
private void OnWindowResized() { }
```

### Pattern 4: Signal Bus (Global Event System)

When you absolutely need global events (use sparingly):

```csharp
// scripts/autoload/EventBus.cs — Autoload singleton
public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }

    // Game events
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void LevelCompletedEventHandler(int level);
    [Signal] public delegate void EnemyKilledEventHandler(Vector3 position, string enemyType);
    [Signal] public delegate void ItemPickedUpEventHandler(string itemId, int quantity);

    // UI events
    [Signal] public delegate void ShowDialogueEventHandler(string speaker, string text);
    [Signal] public delegate void ShowNotificationEventHandler(string message);

    public override void _Ready()
    {
        Instance = this;
    }
}

// Usage — anywhere in the project:

// Emitting:
EventBus.Instance.EmitSignal(EventBus.SignalName.EnemyKilled, position, "goblin");

// Listening:
public override void _Ready()
{
    EventBus.Instance.EnemyKilled += OnEnemyKilled;
}

public override void _ExitTree()
{
    // CRITICAL: Always disconnect from global signals!
    EventBus.Instance.EnemyKilled -= OnEnemyKilled;
}
```

### Pattern 5: Awaiting Signals (Async)

```csharp
// Wait for a specific signal
public async void PlayCutscene()
{
    var anim = GetNode<AnimationPlayer>("CutsceneAnimation");

    // Play intro
    anim.Play("intro");
    await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

    // Show dialogue
    var dialogue = GetNode<DialogueBox>("DialogueBox");
    dialogue.ShowDialogue("NPC", "Welcome, hero!");
    await ToSignal(dialogue, DialogueBox.SignalName.DialogueFinished);

    // Play outro
    anim.Play("outro");
    await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

    GD.Print("Cutscene complete!");
}

// Wait for a timer
public async void SpawnWave()
{
    for (int i = 0; i < 5; i++)
    {
        SpawnEnemy();
        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
    }
}

// Wait with timeout
public async void WaitForPlayerInput(float timeout)
{
    var timer = GetTree().CreateTimer(timeout);

    // Race between player input and timeout
    // Use a flag pattern
    bool inputReceived = false;

    void OnInput()
    {
        inputReceived = true;
    }

    // This is a simplified pattern — in practice use a TaskCompletionSource
    await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

    if (!inputReceived)
        GD.Print("Player timed out!");
}
```

### Pattern 6: One-Shot Connection (Connect Once)

```csharp
// Using lambda that disconnects itself
public void ConnectOnce()
{
    var area = GetNode<Area3D>("TriggerZone");

    // Method: disable after first trigger
    void OnFirstEntry(Node3D body)
    {
        if (body.IsInGroup("player"))
        {
            TriggerEvent();
            area.BodyEntered -= OnFirstEntry; // Disconnect after first call
        }
    }

    area.BodyEntered += OnFirstEntry;
}

// Using Connect with OneShot flag
public void ConnectOnceAlternative()
{
    var area = GetNode<Area3D>("TriggerZone");
    area.Connect(Area3D.SignalName.BodyEntered,
        new Callable(this, MethodName.OnTriggerBodyEntered),
        (uint)GodotObject.ConnectFlags.OneShot);
}
```

### Pattern 7: Component Communication via Signals

The most important pattern — how composed components talk to each other:

```csharp
// Parent orchestrates component communication
public partial class Player : CharacterBody3D
{
    public override void _Ready()
    {
        var health = GetNode<HealthComponent>("HealthComponent");
        var hurtbox = GetNode<HurtboxComponent>("HurtboxComponent");
        var movement = GetNode<MovementComponent3D>("MovementComponent3D");
        var audio = GetNode<AudioComponent>("AudioComponent");
        var hud = GetNode<HUD>("/root/Main/UI/HUD");

        // Wire components together via signals
        // Hurtbox → Health (damage flows from detection to health)
        hurtbox.Hurt += (damage) => health.TakeDamage(damage);

        // Health → HUD (health changes update UI)
        health.HealthChanged += (current, max) => hud.UpdateHealth(current, max);

        // Health → Audio (play sound on damage)
        health.DamageTaken += (amount, source) => audio.PlayHitSound();

        // Health → Death
        health.Died += OnDied;

        // Movement → Audio (footstep sounds)
        movement.Moved += (vel) =>
        {
            if (IsOnFloor()) audio.PlayFootstep();
        };
    }
}
```

---

## Deep Dive: Event Bus Pattern

### When to Use Event Bus

The Event Bus is an Autoload singleton that only holds signals. It decouples emitters from listeners across distant scenes. Use it **only** when:

- The emitter and listener have no shared ancestor (different scene trees)
- Multiple unrelated systems need to react to the same event
- The event is truly global (player died, level completed, settings changed)

**Never use Event Bus for**: component-to-component communication within the same entity. Use parent orchestration instead.

### Why Event Bus Helps

Without Event Bus, connecting distant nodes requires either:
- Hard-coded node paths (fragile, breaks on refactor)
- Passing references through every intermediate node (bloated)
- Autoloads that do too much (god objects)

With Event Bus, you replace 4 code locations (emitter finds listener, stores reference, connects, disconnects) with 2 (emit signal, connect to bus).

### Complete Event Bus Implementation

```csharp
// scripts/autoload/EventBus.cs
// Register as Autoload in Project Settings > Globals
using Godot;

public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }

    // ═══════════════════════════════════════════
    // GAME LIFECYCLE
    // ═══════════════════════════════════════════
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void GamePausedEventHandler(bool isPaused);
    [Signal] public delegate void GameOverEventHandler(bool won);
    [Signal] public delegate void LevelLoadedEventHandler(int levelIndex, string levelName);

    // ═══════════════════════════════════════════
    // PLAYER EVENTS
    // ═══════════════════════════════════════════
    [Signal] public delegate void PlayerSpawnedEventHandler(Vector3 position);
    [Signal] public delegate void PlayerDiedEventHandler(Vector3 position, string causeOfDeath);
    [Signal] public delegate void PlayerHealthChangedEventHandler(int current, int max);
    [Signal] public delegate void PlayerLeveledUpEventHandler(int newLevel);

    // ═══════════════════════════════════════════
    // COMBAT EVENTS
    // ═══════════════════════════════════════════
    [Signal] public delegate void EnemyKilledEventHandler(Vector3 position, string enemyType, int scoreValue);
    [Signal] public delegate void DamageDealtEventHandler(Vector3 position, int amount, bool isCritical);
    [Signal] public delegate void BossPhaseChangedEventHandler(int phase, string bossName);

    // ═══════════════════════════════════════════
    // ECONOMY / PROGRESSION
    // ═══════════════════════════════════════════
    [Signal] public delegate void ScoreChangedEventHandler(int newScore);
    [Signal] public delegate void CurrencyChangedEventHandler(int newAmount);
    [Signal] public delegate void ItemPickedUpEventHandler(string itemId, int quantity);
    [Signal] public delegate void AchievementUnlockedEventHandler(string achievementId);

    // ═══════════════════════════════════════════
    // UI REQUESTS
    // ═══════════════════════════════════════════
    [Signal] public delegate void ShowDialogueEventHandler(string speaker, string text);
    [Signal] public delegate void ShowNotificationEventHandler(string message, float duration);
    [Signal] public delegate void ShowDamageNumberEventHandler(Vector3 worldPos, int amount, bool isCritical);
    [Signal] public delegate void ShowTooltipEventHandler(string title, string description);
    [Signal] public delegate void HideTooltipEventHandler();

    // ═══════════════════════════════════════════
    // AUDIO REQUESTS
    // ═══════════════════════════════════════════
    [Signal] public delegate void PlaySfxEventHandler(string sfxName, Vector3 position);
    [Signal] public delegate void PlayMusicEventHandler(string trackName, float fadeDuration);

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}
```

### Event Bus: Emitter Side

Emitters fire and forget. They don't know or care who listens:

```csharp
// Enemy.cs — when killed, emits to bus
public partial class Enemy : CharacterBody3D
{
    [Export] public string EnemyType { get; set; } = "goblin";
    [Export] public int ScoreValue { get; set; } = 100;

    private void OnDied()
    {
        // Emit to Event Bus — HUD, ScoreManager, AchievementTracker all listen
        EventBus.Instance.EmitSignal(
            EventBus.SignalName.EnemyKilled,
            GlobalPosition, EnemyType, ScoreValue
        );

        // Emit damage number request
        EventBus.Instance.EmitSignal(
            EventBus.SignalName.ShowDamageNumber,
            GlobalPosition + Vector3.Up, ScoreValue, false
        );

        QueueFree();
    }
}

// Player.cs — emits health changes to bus for anyone who cares
private void OnHealthChanged(int current, int max)
{
    EventBus.Instance.EmitSignal(
        EventBus.SignalName.PlayerHealthChanged, current, max
    );
}
```

### Event Bus: Listener Side

Listeners connect in `_Ready` and **MUST disconnect in `_ExitTree`**:

```csharp
// HUD.cs — listens to multiple bus events
public partial class HUD : CanvasLayer
{
    private Label _scoreLabel;
    private ProgressBar _healthBar;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("%ScoreLabel");
        _healthBar = GetNode<ProgressBar>("%HealthBar");

        // Connect to bus events
        EventBus.Instance.ScoreChanged += OnScoreChanged;
        EventBus.Instance.PlayerHealthChanged += OnPlayerHealthChanged;
        EventBus.Instance.EnemyKilled += OnEnemyKilled;
        EventBus.Instance.ShowNotification += OnShowNotification;
        EventBus.Instance.PlayerDied += OnPlayerDied;
    }

    public override void _ExitTree()
    {
        // CRITICAL: Always disconnect! If the HUD is freed but the bus persists,
        // the bus will try to call methods on a freed object → crash
        if (EventBus.Instance != null)
        {
            EventBus.Instance.ScoreChanged -= OnScoreChanged;
            EventBus.Instance.PlayerHealthChanged -= OnPlayerHealthChanged;
            EventBus.Instance.EnemyKilled -= OnEnemyKilled;
            EventBus.Instance.ShowNotification -= OnShowNotification;
            EventBus.Instance.PlayerDied -= OnPlayerDied;
        }
    }

    private void OnScoreChanged(int newScore)
    {
        _scoreLabel.Text = $"Score: {newScore}";
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        _healthBar.MaxValue = max;
        CreateTween().TweenProperty(_healthBar, "value", (double)current, 0.3);
    }

    private void OnEnemyKilled(Vector3 pos, string type, int score)
    {
        // Could show kill feed, combo counter, etc.
    }

    private void OnShowNotification(string message, float duration)
    {
        // Show temporary notification on screen
    }

    private void OnPlayerDied(Vector3 pos, string cause)
    {
        // Show game over screen
    }
}

// ScoreManager.cs — also listens to EnemyKilled
public partial class ScoreManager : Node
{
    private int _score;
    private int _killStreak;
    private float _killStreakTimer;

    public override void _Ready()
    {
        EventBus.Instance.EnemyKilled += OnEnemyKilled;
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance != null)
            EventBus.Instance.EnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(Vector3 pos, string type, int scoreValue)
    {
        _killStreak++;
        _killStreakTimer = 3f;

        int multiplier = _killStreak >= 5 ? 3 : _killStreak >= 3 ? 2 : 1;
        _score += scoreValue * multiplier;

        // Emit the processed score back to the bus
        EventBus.Instance.EmitSignal(EventBus.SignalName.ScoreChanged, _score);

        if (_killStreak == 10)
        {
            EventBus.Instance.EmitSignal(
                EventBus.SignalName.AchievementUnlocked, "kill_streak_10"
            );
        }
    }

    public override void _Process(double delta)
    {
        if (_killStreak > 0)
        {
            _killStreakTimer -= (float)delta;
            if (_killStreakTimer <= 0)
                _killStreak = 0;
        }
    }
}

// AchievementTracker.cs — also listens, completely independent
public partial class AchievementTracker : Node
{
    private int _totalKills;

    public override void _Ready()
    {
        EventBus.Instance.EnemyKilled += OnEnemyKilled;
        EventBus.Instance.ItemPickedUp += OnItemPickedUp;
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.EnemyKilled -= OnEnemyKilled;
            EventBus.Instance.ItemPickedUp -= OnItemPickedUp;
        }
    }

    private void OnEnemyKilled(Vector3 pos, string type, int score)
    {
        _totalKills++;
        if (_totalKills >= 100)
            EventBus.Instance.EmitSignal(EventBus.SignalName.AchievementUnlocked, "centurion");
        if (type == "boss")
            EventBus.Instance.EmitSignal(EventBus.SignalName.AchievementUnlocked, "boss_slayer");
    }

    private void OnItemPickedUp(string itemId, int quantity)
    {
        if (itemId == "rare_gem")
            EventBus.Instance.EmitSignal(EventBus.SignalName.AchievementUnlocked, "gem_collector");
    }
}
```

### Event Bus: Data Flow Diagram

```
Enemy.OnDied()
   │
   ├──[emits]──→ EventBus.EnemyKilled ──→ ScoreManager.OnEnemyKilled()
   │                                       │   ├── calculates combo multiplier
   │                                       │   └──[emits]──→ EventBus.ScoreChanged ──→ HUD.OnScoreChanged()
   │                                       │
   │                                       ├──→ AchievementTracker.OnEnemyKilled()
   │                                       │   └──[emits]──→ EventBus.AchievementUnlocked ──→ NotificationUI
   │                                       │
   │                                       └──→ HUD.OnEnemyKilled()
   │                                            └── shows kill feed entry
   │
   └──[emits]──→ EventBus.ShowDamageNumber ──→ DamageNumberSpawner.OnShowDamageNumber()
                                                └── spawns floating "+100" text
```

Notice: Enemy knows NOTHING about ScoreManager, AchievementTracker, HUD, or DamageNumberSpawner. It just fires events and dies.

---

### Pattern 8: Signal-Based Data Binding (Reactive UI)

Make UI automatically react to data changes without polling:

```csharp
// PlayerStats.cs — autoload holding player state
public partial class PlayerStats : Node
{
    public static PlayerStats Instance { get; private set; }

    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void ManaChangedEventHandler(float current, float max);
    [Signal] public delegate void ExperienceChangedEventHandler(int current, int toNext, int level);
    [Signal] public delegate void GoldChangedEventHandler(int amount);

    private int _health = 100;
    private int _maxHealth = 100;
    private float _mana = 50f;
    private float _maxMana = 50f;
    private int _experience;
    private int _level = 1;
    private int _gold;

    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, _maxHealth);
            EmitSignal(SignalName.HealthChanged, _health, _maxHealth);
        }
    }

    public float Mana
    {
        get => _mana;
        set
        {
            _mana = Mathf.Clamp(value, 0, _maxMana);
            EmitSignal(SignalName.ManaChanged, _mana, _maxMana);
        }
    }

    public int Gold
    {
        get => _gold;
        set
        {
            _gold = Mathf.Max(0, value);
            EmitSignal(SignalName.GoldChanged, _gold);
        }
    }

    public override void _Ready() => Instance = this;
}

// AnyUI.cs — just connect to the signal
public override void _Ready()
{
    PlayerStats.Instance.HealthChanged += (cur, max) =>
    {
        _healthBar.MaxValue = max;
        _healthBar.Value = cur;
    };
    PlayerStats.Instance.GoldChanged += (gold) => _goldLabel.Text = $"{gold}g";
}
```

### Pattern 9: Signal Chaining for Sequential Game Flow

```csharp
// LevelManager.cs — orchestrates level flow with signals
public partial class LevelManager : Node
{
    [Signal] public delegate void PhaseCompletedEventHandler(string phaseName);

    public async void StartLevel()
    {
        // Phase 1: Intro cutscene
        var cutscene = GetNode<CutscenePlayer>("Cutscene");
        cutscene.Play("level_intro");
        await ToSignal(cutscene, CutscenePlayer.SignalName.CutsceneFinished);

        // Phase 2: Spawn enemies in waves
        var spawner = GetNode<WaveSpawner>("WaveSpawner");
        spawner.StartWaves();
        await ToSignal(spawner, WaveSpawner.SignalName.AllWavesCleared);

        // Phase 3: Boss fight
        var boss = SpawnBoss();
        await ToSignal(boss.GetNode<HealthComponent>("HealthComponent"), HealthComponent.SignalName.Died);

        // Phase 4: Victory
        EventBus.Instance.EmitSignal(EventBus.SignalName.GameOver, true);

        // Phase 5: Outro cutscene
        cutscene.Play("level_outro");
        await ToSignal(cutscene, CutscenePlayer.SignalName.CutsceneFinished);

        // Load next level
        GetTree().ChangeSceneToFile($"res://scenes/levels/Level{_currentLevel + 1}.tscn");
    }
}
```

### Pattern 10: SafeConnect Helper (Prevent Leaked Connections)

```csharp
// Extension method to prevent common disconnection mistakes
public static class SignalExtensions
{
    /// <summary>
    /// Connects a signal and auto-disconnects when the listener exits the tree.
    /// Prevents the most common signal bug in Godot C#.
    /// </summary>
    public static void SafeConnect<T>(
        this T emitter,
        StringName signalName,
        Node listener,
        Callable callable) where T : GodotObject
    {
        emitter.Connect(signalName, callable);
        listener.TreeExiting += () =>
        {
            if (GodotObject.IsInstanceValid(emitter) && emitter.IsConnected(signalName, callable))
                emitter.Disconnect(signalName, callable);
        };
    }
}

// Usage:
EventBus.Instance.SafeConnect(
    EventBus.SignalName.EnemyKilled,
    this, // listener (this node)
    Callable.From<Vector3, string, int>(OnEnemyKilled)
);
// No need to disconnect in _ExitTree — it's automatic!
```

---

## Key Takeaways

1. **Signal delegates MUST end with `EventHandler`**
2. **Always disconnect** from signals in `_ExitTree` (especially Event Bus and custom signals)
3. **Use `+=` / `-=` syntax** — it's cleaner than `Connect()`
4. **Await signals** for sequential async flow (cutscenes, spawn waves, level progression)
5. **Event Bus is for cross-scene, many-to-many communication only** — never for component wiring
6. **The parent wires children together** — children never know about siblings
7. **Organize Event Bus signals by category** — game lifecycle, combat, economy, UI requests, audio
8. **Event Bus listeners MUST disconnect** — or use SafeConnect helper
9. **Signal relay** lets deep components communicate through their ancestors
10. **Data binding via signals** makes UI reactive without polling
