# Chickensoft Ecosystem for Godot C# Development

Chickensoft is an open-source community providing production-grade tooling for Godot 4 + C#. This reference covers all major packages, architectural patterns, and working code examples.

---

## Table of Contents

1. [3-Layer Architecture](#3-layer-architecture)
2. [AutoInject — Dependency Injection](#autoinject)
3. [LogicBlocks — State Machines](#logicblocks)
4. [AutoProp — Reactive Properties](#autoprop)
5. [GodotGame Template — Project Structure](#godotgame-template)
6. [Testing Patterns](#testing-patterns)
7. [NuGet Package Setup](#nuget-package-setup)
8. [Complete Working Examples](#complete-working-examples)

---

## 3-Layer Architecture

Chickensoft recommends a strict 3-layer architecture where objects in one layer can only be strongly coupled with objects in the layer directly below them.

### Layer 1: Visual Layer (Godot Nodes)

Nodes are thin. They own no business logic. They forward user input to their LogicBlock and react to outputs by updating visuals.

```csharp
[Meta(typeof(IAutoNode))]
public partial class WinMenu : Control, IWinMenu {
  public override void _Notification(int what) => this.Notify(what);

  #region Nodes
  [Node]
  public IButton MainMenuButton { get; set; } = default!;
  #endregion Nodes

  #region Signals
  [Signal]
  public delegate void MainMenuEventHandler();
  #endregion Signals

  public void OnReady() => MainMenuButton.Pressed += OnMainMenuPressed;

  public void OnExitTree() => MainMenuButton.Pressed -= OnMainMenuPressed;

  public void OnMainMenuPressed() => EmitSignal(SignalName.MainMenu);
}
```

### Layer 2: Game Logic Layer (LogicBlocks + Repositories)

Split into two sub-layers:

**Visual Game Logic** — LogicBlocks (state machines) that drive a specific node's behavior. Each node that does more than simple visualization gets its own LogicBlock.

**Pure Game Logic** — Repository classes that encapsulate domain rules not specific to any single visual component (e.g., coin counting, scoring, game-over conditions).

```csharp
// Repository — Pure Game Logic
public class AppRepo : IAppRepo {
  public IAutoProp<int> NumCoinsCollected => _numCoinsCollected;
  private readonly AutoProp<int> _numCoinsCollected = new(0);

  public IAutoProp<int> NumCoinsAtStart => _numCoinsAtStart;
  private readonly AutoProp<int> _numCoinsAtStart = new(0);

  public event Action? CoinCollected;

  private int _coinsBeingCollected;

  public void StartCoinCollection(ICoin coin) {
    _coinsBeingCollected++;
    _numCoinsCollected.OnNext(_numCoinsCollected.Value + 1);
    CoinCollected?.Invoke();
  }

  public void OnFinishCoinCollection(ICoin coin) {
    _coinsBeingCollected--;
    if (
      _coinsBeingCollected == 0 &&
      _numCoinsCollected.Value >= _numCoinsAtStart.Value
    ) {
      OnGameEnded(GameOverReason.PlayerWon);
    }
  }
}
```

### Layer 3: Data Layer

Networking clients, persistence, file I/O. Repositories in Layer 2 depend on these.

### Data Flow

```
User Input → Visual Node → LogicBlock.Input(...)
                                ↓
                          State handles input
                                ↓
                          State reads from Repository (Layer 2)
                                ↓
                          State produces Output(...)
                                ↓
Visual Node ← Binding.Handle<Output>(...) updates visuals
```

---

## AutoInject

Tree-based, reflection-free dependency injection for Godot C# nodes. Providers higher in the scene tree supply dependencies to Dependents lower in the tree.

### NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="..." />
  <PackageReference Include="Chickensoft.Introspection" Version="..." />
  <PackageReference Include="Chickensoft.Introspection.Generator"
    Version="..." PrivateAssets="all" OutputItemType="analyzer" />
  <PackageReference Include="Chickensoft.AutoInject"
    Version="..." PrivateAssets="all" />
  <!-- Optional static analyzers -->
  <PackageReference Include="Chickensoft.AutoInject.Analyzers"
    Version="..." PrivateAssets="all" OutputItemType="analyzer" />
</ItemGroup>

<PropertyGroup>
  <WarningsAsErrors>CS9057</WarningsAsErrors>
</PropertyGroup>
```

### Provider Pattern — Supplying Dependencies

A node implements `IProvide<T>` for each type it provides. It MUST call `this.Provide()` once values are initialized.

```csharp
[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D, IPlayer,
  IProvide<IPlayerLogic> {

  public override void _Notification(int what) => this.Notify(what);

  #region Provisions
  IPlayerLogic IProvide<IPlayerLogic>.Value() => PlayerLogic;
  #endregion Provisions

  public IPlayerLogic PlayerLogic { get; set; } = default!;

  public void OnReady() {
    PlayerLogic = new PlayerLogic(/* dependencies */);
    this.Provide(); // Makes IPlayerLogic available to descendants
  }

  // Called after this.Provide() completes
  public void OnProvided() { }
}
```

### Dependent Pattern — Consuming Dependencies

Descendant nodes use `[Dependency]` attribute and `this.DependOn<T>()` to request values from ancestor providers.

```csharp
[Meta(typeof(IAutoNode))]
public partial class PlayerModel : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies
  [Dependency]
  public IPlayerLogic PlayerLogic => this.DependOn<IPlayerLogic>();
  #endregion Dependencies

  public void OnResolved() {
    // Called when ALL dependencies are available.
    // Safe to use PlayerLogic here.
    var binding = PlayerLogic.Bind();
    binding
      .Handle<PlayerLogic.Output.Animations.Idle>(
        (output) => AnimationStateMachine.Travel("idle")
      )
      .Handle<PlayerLogic.Output.Animations.Move>(
        (output) => AnimationStateMachine.Travel("move")
      );
  }
}
```

### Dependency with Fallback

Fallbacks execute when no ancestor provider is found (useful for testing or standalone scenes).

```csharp
[Dependency]
public string MyDependency => this.DependOn<string>(() => "fallback_value");
```

### Resolution Lifecycle

When providers call `Provide()` from `_Ready`:

1. Dependent node `_Ready`
2. Provider node `_Ready` (calls `Provide()`)
3. Dependent `OnResolved()` — called when the **slowest** provider finishes
4. Frame 1 `_Process`

Resolution is O(n) where n = height of the tree above the dependent node.

### How Resolution Works

- Dependent searches ancestors starting from itself, then parent, upward
- If a provider hasn't initialized yet, the dependent subscribes to its `OnInitialized` event
- `OnResolved()` fires only after ALL `[Dependency]` properties are satisfied

### Node as Both Provider and Dependent

A node can consume dependencies from above and provide different ones to below:

```csharp
[Meta(typeof(IAutoNode))]
public partial class GameWorld : Node3D,
  IProvide<IGameRepo>, IProvide<IPlayerLogic> {

  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public IAppRepo AppRepo => this.DependOn<IAppRepo>();

  IGameRepo IProvide<IGameRepo>.Value() => GameRepo;
  IPlayerLogic IProvide<IPlayerLogic>.Value() => PlayerLogic;

  public IGameRepo GameRepo { get; set; } = default!;
  public IPlayerLogic PlayerLogic { get; set; } = default!;

  public void OnResolved() {
    // AppRepo is now available from an ancestor.
    // Create our own objects using it, then provide them downstream.
    GameRepo = new GameRepo(AppRepo);
    PlayerLogic = new PlayerLogic(GameRepo);
    this.Provide(); // Safe to call from OnResolved
  }
}
```

### Mixin Setup

The `[Meta(typeof(IAutoNode))]` attribute applies all mixins at once. For granular control:

```csharp
[Meta(
  typeof(IAutoOn),       // .NET-style notification handlers (OnReady, OnProcess)
  typeof(IAutoConnect),  // [Node] attribute for automatic scene tree binding
  typeof(IAutoInit),     // Lifecycle hooks (Setup, OnResolved)
  typeof(IProvider),     // IProvide<T> support
  typeof(IDependent)     // [Dependency] support
)]
public partial class MyNode : Node {
  public override void _Notification(int what) => this.Notify(what);
}
```

**Critical requirement:** Every node using AutoInject MUST override `_Notification` and call `this.Notify(what)`.

### [Node] Attribute — Automatic Scene Tree Binding

```csharp
[Meta(typeof(IAutoNode))]
public partial class HUD : Control {
  public override void _Notification(int what) => this.Notify(what);

  [Node]
  public ILabel ScoreLabel { get; set; } = default!;  // Binds to child named "ScoreLabel"

  [Node("%HealthBar")]
  public IProgressBar HealthBar { get; set; } = default!;  // Unique name lookup
}
```

### Testing with Faked Dependencies

```csharp
[Test]
public void FakesDependency() {
  var dependent = new MyNode();
  var fakeValue = "I'm fake!";
  dependent.FakeDependency(fakeValue); // Overrides any provider/fallback

  TestScene.AddChild(dependent);
  dependent._Notification((int)Node.NotificationReady);

  dependent.OnResolvedCalled.ShouldBeTrue();
  dependent.MyDependency.ShouldBe(fakeValue);

  TestScene.RemoveChild(dependent);
}
```

### Best Practices

- Keep dependency trees simple and free from asynchronous initialization
- Call `Provide()` synchronously during `_Ready` or `OnResolved` to avoid deadlocks
- Exiting and re-entering the scene tree retriggers dependency resolution
- Subscribe to events in `OnResolved`, clean up in `OnExitTree`

---

## LogicBlocks

Serializable, hierarchical state machines for C#. Works with AOT. States are self-contained records that use the state pattern.

### NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.LogicBlocks" Version="..." />
  <PackageReference Include="Chickensoft.Introspection.Generator"
    Version="..." PrivateAssets="all" OutputItemType="analyzer" />
  <!-- Optional: generates UML state diagrams as *.g.puml files -->
  <PackageReference Include="Chickensoft.LogicBlocks.DiagramGenerator"
    Version="..." PrivateAssets="all" OutputItemType="analyzer" />
</ItemGroup>
```

### Anatomy of a LogicBlock

```csharp
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class LightSwitch : LogicBlock<LightSwitch.State> {

  // Required: set the initial state
  public override Transition GetInitialState() => To<State.PoweredOff>();

  // Inputs — readonly record structs to minimize allocations
  public static class Input {
    public readonly record struct Toggle;
  }

  // Outputs — side-effect signals consumed by visual layer
  public static class Output {
    public readonly record struct StatusChanged(bool IsOn);
  }

  // States — abstract base record inheriting StateLogic<State>
  public abstract record State : StateLogic<State> {

    public record PoweredOn : State, IGet<Input.Toggle> {
      public PoweredOn() {
        this.OnEnter(() => Output(new Output.StatusChanged(IsOn: true)));
      }
      public Transition On(in Input.Toggle input) => To<PoweredOff>();
    }

    public record PoweredOff : State, IGet<Input.Toggle> {
      public PoweredOff() {
        this.OnEnter(() => Output(new Output.StatusChanged(IsOn: false)));
      }
      public Transition On(in Input.Toggle input) => To<PoweredOn>();
    }
  }
}
```

### Key Concepts

| Concept | Description |
|---------|-------------|
| `IGet<TInput>` | Interface a state implements to declare it handles that input type |
| `On(in TInput input)` | Handler method; returns a `Transition` |
| `To<TState>()` | Transition to a new state |
| `ToSelf()` | Stay in the current state (re-enter triggers OnAttach/OnDetach but NOT OnEnter/OnExit) |
| `Output(...)` | Produce a side-effect output for the visual layer |
| `Get<T>()` | Read shared data from the blackboard |
| `this.OnEnter(...)` | Callback when entering this state (respects hierarchy) |
| `this.OnExit(...)` | Callback when exiting this state (respects hierarchy) |
| `this.OnAttach(...)` | Callback when state instance changes (practical setup) |
| `this.OnDetach(...)` | Callback when state instance changes (practical cleanup) |

### OnEnter/OnExit vs OnAttach/OnDetach

- **OnEnter/OnExit** respect the state type hierarchy. If state A extends Powered and you transition to state B which also extends Powered, the OnEnter for Powered fires only once. Use for theoretically-correct behavior (producing outputs).
- **OnAttach/OnDetach** fire every time the state instance changes. Use for practical housekeeping (subscribing/unsubscribing to events).

### The Blackboard — Sharing Data Between States

The blackboard is a dictionary that lets the LogicBlock and all its states share data. Set values from the LogicBlock constructor; read them from states via `Get<T>()`.

```csharp
[LogicBlock(typeof(State), Diagram = true), Meta]
public partial class VendingMachine : LogicBlock<VendingMachine.State> {

  // Shared data record — stored on the blackboard
  public partial record Data {
    public ItemType Type { get; set; }
    public int Price { get; set; }
    public int AmountReceived { get; set; }
  }

  public static class Input {
    public readonly record struct SelectionEntered(ItemType Type);
    public readonly record struct PaymentReceived(int Amount);
    public readonly record struct TransactionTimedOut;
    public readonly record struct VendingCompleted;
  }

  public static class Output {
    public readonly record struct Dispensed(ItemType Type);
    public readonly record struct TransactionStarted;
    public readonly record struct TransactionCompleted(
      ItemType Type, int Price, TransactionStatus Status, int AmountPaid
    );
    public readonly record struct RestartTransactionTimeOutTimer;
    public readonly record struct ClearTransactionTimeOutTimer;
    public readonly record struct MakeChange(int Amount);
    public readonly record struct BeginVending;
  }

  public static readonly Dictionary<ItemType, int> Prices = new() {
    [ItemType.Juice] = 4,
    [ItemType.Water] = 2,
    [ItemType.Candy] = 6
  };

  public override Transition GetInitialState() => To<Idle>();

  public VendingMachine() {
    Set(new Data()); // Place Data on the blackboard
  }
}
```

### Hierarchical State Definitions (VendingMachine Example)

States can form an inheritance hierarchy. Shared input handling goes in base states.

```csharp
// Base state
public abstract partial record State : StateLogic<State>;

// Abstract intermediate: any state where the user can edit their selection
public abstract partial record SelectionEditable : State,
  IGet<Input.SelectionEntered> {

  public Transition On(in Input.SelectionEntered input) {
    var data = Get<Data>();
    Output(new Output.RestartTransactionTimeOutTimer());

    if (Get<VendingMachineStock>().HasItem(input.Type)) {
      data.Type = input.Type;
      data.Price = Prices[input.Type];
      return To<TransactionStarted>();
    }
    return ToSelf();
  }
}

// Concrete leaf state: machine is idle
public partial record Idle : SelectionEditable,
  IGet<Input.PaymentReceived> {

  public Idle() {
    this.OnEnter(() => Output(new Output.ClearTransactionTimeOutTimer()));
  }

  public Transition On(in Input.PaymentReceived input) {
    Output(new Output.MakeChange(input.Amount)); // Eject unsolicited payment
    return ToSelf();
  }
}

// Abstract intermediate: any state with an active transaction
public abstract partial record TransactionActive : SelectionEditable,
  IGet<Input.PaymentReceived>, IGet<Input.TransactionTimedOut> {

  public TransactionActive() {
    this.OnEnter(() => Output(new Output.RestartTransactionTimeOutTimer()));
    this.OnExit(() => Get<Data>().AmountReceived = 0);
  }

  public Transition On(in Input.PaymentReceived input) {
    var data = Get<Data>();
    Output(new Output.RestartTransactionTimeOutTimer());
    data.AmountReceived += input.Amount;

    if (data.AmountReceived < data.Price) {
      return ToSelf();
    }
    if (data.AmountReceived > data.Price) {
      Output(new Output.MakeChange(data.AmountReceived - data.Price));
    }
    Output(new Output.TransactionCompleted(
      Type: data.Type,
      Price: data.Price,
      Status: TransactionStatus.Success,
      AmountPaid: data.AmountReceived
    ));
    Get<VendingMachineStock>().Vend(data.Type);
    return To<Vending>();
  }

  public Transition On(in Input.TransactionTimedOut input) {
    var data = Get<Data>();
    if (data.AmountReceived > 0) {
      Output(new Output.MakeChange(data.AmountReceived));
    }
    return To<Idle>();
  }
}

// Concrete: transaction just started
public partial record TransactionStarted : TransactionActive,
  IGet<Input.SelectionEntered> {

  public TransactionStarted() {
    this.OnEnter(() => Output(new Output.TransactionStarted()));
  }
}

// Concrete: machine is dispensing the item
public partial record Vending : State, IGet<Input.VendingCompleted> {
  public Vending() {
    this.OnEnter(() => Output(new Output.BeginVending()));
  }
  public Transition On(in Input.VendingCompleted input) => To<Idle>();
}
```

### Conditional Transitions with .With()

```csharp
public Transition On(in Input.NewGameClick input) =>
  To<FadingOut>()
    .With(state =>
      ((FadingOut)state).FadeOutFinishedAction =
        EFadeOutFinishedAction.StartGame
    );
```

### Conditional Transitions with Switch Expressions

```csharp
public Transition On(in Input.FadeOutFinished input) =>
  FadeOutFinishedAction switch {
    EFadeOutFinishedAction.StartGame => To<InGame>(),
    EFadeOutFinishedAction.BackToMainMenu => To<InMainMenu>(),
    EFadeOutFinishedAction.QuitApp => To<ClosingApplication>(),
    _ => throw new System.NotImplementedException(),
  };
```

### Sending Inputs and Using Bindings

```csharp
// Create and start the logic block
var logic = new LightSwitch();
logic.Start();

// Send input
logic.Input(new LightSwitch.Input.Toggle());

// Read current state
var state = logic.Value;

// Create a binding to react to outputs
using var binding = logic.Bind();

binding
  .Handle((in LightSwitch.Output.StatusChanged output) =>
    Console.WriteLine($"Light is {(output.IsOn ? "on" : "off")}")
  )
  .Watch((in LightSwitch.Input.Toggle input) =>
    Console.WriteLine("Toggled!")
  )
  .When((LightSwitch.State.PoweredOn _) =>
    Console.WriteLine("Powered on!")
  )
  .Catch((Exception e) => Console.WriteLine(e.Message));
```

### Binding Methods

| Method | Purpose |
|--------|---------|
| `.Handle<TOutput>(action)` | React to a specific output type |
| `.Watch<TInput>(action)` | Observe when a specific input is processed |
| `.When<TState>(action)` | React when entering a specific state |
| `.Catch(action)` | Handle exceptions from the logic block |

### Subscribing to Repository Events from States

```csharp
public partial class InGameUILogic {
  public record State : StateLogic<State>, IState {
    public State() {
      this.OnAttach(() => {
        var appRepo = Get<IAppRepo>();
        appRepo.NumCoinsCollected.Sync += OnNumCoinsCollected;
        appRepo.NumCoinsAtStart.Sync += OnNumCoinsAtStart;
      });

      this.OnDetach(() => {
        var appRepo = Get<IAppRepo>();
        appRepo.NumCoinsCollected.Sync -= OnNumCoinsCollected;
        appRepo.NumCoinsAtStart.Sync -= OnNumCoinsAtStart;
      });
    }

    public void OnNumCoinsCollected(int numCoinsCollected) {
      Output(new Output.NumCoinsChanged(
        numCoinsCollected,
        Get<IAppRepo>().NumCoinsAtStart.Value
      ));
    }

    public void OnNumCoinsAtStart(int numCoinsAtStart) {
      Output(new Output.NumCoinsChanged(
        Get<IAppRepo>().NumCoinsCollected.Value,
        numCoinsAtStart
      ));
    }
  }
}
```

---

## AutoProp

Simplified reactive property from `Chickensoft.Collections`. Like a BehaviorSubject that only fires when the value actually changes.

```csharp
using Chickensoft.Collections;

public class AppRepo : IAppRepo, IDisposable {
  // Expose read-only interface
  public IAutoProp<int> Score => _score;
  // Internal read-write version
  private readonly AutoProp<int> _score = new(0);

  public IAutoProp<bool> IsGameOver => _isGameOver;
  private readonly AutoProp<bool> _isGameOver = new(false);

  public void AddScore(int points) {
    _score.OnNext(_score.Value + points);
  }

  public void EndGame() {
    _isGameOver.OnNext(true);
  }

  public void Dispose() {
    _score.Dispose();
    _isGameOver.Dispose();
  }
}
```

### Subscribing to AutoProp Changes

```csharp
// From a LogicBlock state:
var appRepo = Get<IAppRepo>();
appRepo.Score.Sync += OnScoreChanged;  // Subscribe
appRepo.Score.Sync -= OnScoreChanged;  // Unsubscribe

void OnScoreChanged(int newScore) {
  Output(new Output.ScoreUpdated(newScore));
}

// Direct value access:
var currentScore = appRepo.Score.Value;
```

### Key Properties

- Only fires when the new value differs from the previous value
- Guarantees callback ordering even when updated from within a handler
- Deterministic and synchronous (no async)

---

## GodotGame Template

### Installation

```bash
dotnet new install Chickensoft.GodotGame
dotnet new chickengame --name "MyGameName" --param:author "My Name"
cd MyGameName
dotnet build
```

### Project Structure

```
MyGameName/
  .github/workflows/       # CI/CD: tests, spellcheck, publish, version bump
  .vscode/                 # Debug launch profiles (game, scene, tests, single test)
  addons/                  # Godot engine addons
  src/                     # Main game source code
    Main.tscn / Main.cs    # Entry point (routes to Game or Tests)
    Game.tscn / Game.cs    # Actual game root (edit this, not Main)
  test/src/                # Test source code (excluded from release builds)
  coverage/                # Coverage reports
  project.godot            # Godot project file
  global.json              # Pins .NET SDK + Godot.NET.Sdk versions
  nuget.config             # NuGet sources
```

### Key Conventions

- Scene files (.tscn) colocated with their C# scripts in the same directory
- `Main.tscn` is the entry point; in debug mode it can route to tests
- Tests are excluded from release builds via .csproj configuration
- Four VSCode launch profiles: Debug Game, Debug Current Scene, Debug Tests, Debug Current Test

### Testing Setup

```bash
# Install coverage tools globally
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run coverage
chmod +x ./coverage.sh
./coverage.sh           # Linux/macOS
.\coverage.ps1          # Windows
```

Uses **GoDotTest** (runs inside the Godot engine) with **godot-test-driver**.

---

## Testing Patterns

### Two-Phase Initialization

Separate object creation (Setup) from usage (OnResolved) so tests can inject fakes before the node starts using them.

```csharp
[Meta(typeof(IAutoNode))]
public partial class InGameUI : Control, IInGameUI {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies
  [Dependency]
  public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  #endregion Dependencies

  public IInGameUILogic InGameUILogic { get; set; } = default!;
  public InGameUILogic.IBinding InGameUIBinding { get; set; } = default!;

  // Phase 1: Create objects (called by AutoInject lifecycle)
  public void Setup() {
    InGameUILogic = new InGameUILogic(this, AppRepo);
  }

  // Phase 2: Wire everything up
  public void OnResolved() {
    InGameUIBinding = InGameUILogic.Bind();

    InGameUIBinding
      .Handle<InGameUILogic.Output.NumCoinsChanged>(
        (output) => SetCoinsLabel(output.NumCoinsCollected, output.NumCoinsAtStart)
      );

    InGameUILogic.Start();
  }

  public void SetCoinsLabel(int coins, int totalCoins) {
    CoinsLabel.Text = $"{coins}/{totalCoins}";
  }
}
```

### Faking Dependencies in Tests

```csharp
[Test]
public void UpdatesCoinsLabel() {
  var node = new InGameUI();
  var fakeAppRepo = new Mock<IAppRepo>();
  node.FakeDependency<IAppRepo>(fakeAppRepo.Object);

  TestScene.AddChild(node);
  node._Notification((int)Node.NotificationReady);

  // OnResolved fires, node creates its LogicBlock with the fake repo
  node.CoinsLabel.Text.ShouldBe("0/0");
}
```

### Testing LogicBlocks in Isolation

LogicBlocks are pure C# with no Godot dependency, so they can be tested without the engine:

```csharp
[Test]
public void LightSwitchToggles() {
  var logic = new LightSwitch();
  var outputs = new List<object>();

  using var binding = logic.Bind();
  binding.Handle((in LightSwitch.Output.StatusChanged output) =>
    outputs.Add(output)
  );

  logic.Start(); // Enters PoweredOff, outputs StatusChanged(false)
  logic.Input(new LightSwitch.Input.Toggle()); // -> PoweredOn

  logic.Value.ShouldBeOfType<LightSwitch.State.PoweredOn>();
  outputs.Count.ShouldBe(2);
}
```

### GodotNodeInterfaces for Mocking

Chickensoft generates interfaces for every GodotObject subclass (IButton, ILabel, INode3D, etc.), enabling mock-based unit testing:

```csharp
// Production code uses the interface
[Node]
public IButton StartButton { get; set; } = default!;

// Test code substitutes a mock
var mockButton = new Mock<IButton>();
node.StartButton = mockButton.Object;
```

---

## NuGet Package Setup

### Complete .csproj Configuration

```xml
<Project Sdk="Godot.NET.Sdk/4.4.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <WarningsAsErrors>CS9057</WarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Chickensoft packages -->
    <PackageReference Include="Chickensoft.AutoInject"
      Version="2.9.14" PrivateAssets="all" />
    <PackageReference Include="Chickensoft.LogicBlocks" Version="5.13.0" />
    <PackageReference Include="Chickensoft.Collections" Version="..." />
    <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="..." />
    <PackageReference Include="Chickensoft.Introspection" Version="..." />

    <!-- Source generators (build-time only) -->
    <PackageReference Include="Chickensoft.Introspection.Generator"
      Version="..." PrivateAssets="all" OutputItemType="analyzer" />
    <PackageReference Include="Chickensoft.LogicBlocks.DiagramGenerator"
      Version="..." PrivateAssets="all" OutputItemType="analyzer" />

    <!-- Optional: static analyzers -->
    <PackageReference Include="Chickensoft.AutoInject.Analyzers"
      Version="..." PrivateAssets="all" OutputItemType="analyzer" />
  </ItemGroup>
</Project>
```

---

## Complete Working Examples

### Example 1: Full App Shell with LogicBlocks + Bindings

This shows a complete application node with a state machine controlling screen flow.

```csharp
// ============================================================
// AppLogic.cs — The state machine
// ============================================================
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class AppLogic : LogicBlock<AppLogic.State> {

  public enum EFadeOutFinishedAction { StartGame, BackToMainMenu, QuitApp }

  public override Transition GetInitialState() => To<State.InMainMenu>();

  public static partial class Input {
    public partial record struct NewGameClick;
    public partial record struct RequestQuitGame;
    public partial record struct FadeOutFinished;
    public partial record struct QuitClick;
  }

  public static partial class Output {
    public partial record struct StartNewGame;
    public partial record struct RemoveGame;
    public partial record struct FadeOut;
    public partial record struct FadeIn;
    public partial record struct QuitApp;
    public partial record struct UpdateMainMenuVisibility(bool Visible);
  }

  public abstract partial record State : StateLogic<State> {
    public EFadeOutFinishedAction FadeOutFinishedAction { get; set; }

    public partial record InMainMenu : State,
      IGet<Input.NewGameClick>, IGet<Input.QuitClick> {

      public InMainMenu() {
        this.OnEnter(() => {
          Output(new Output.UpdateMainMenuVisibility(true));
          Output(new Output.FadeIn());
        });
        this.OnExit(() =>
          Output(new Output.UpdateMainMenuVisibility(false)));
      }

      public Transition On(in Input.NewGameClick input) =>
        To<FadingOut>().With(s =>
          ((FadingOut)s).FadeOutFinishedAction =
            EFadeOutFinishedAction.StartGame);

      public Transition On(in Input.QuitClick input) =>
        To<FadingOut>().With(s =>
          ((FadingOut)s).FadeOutFinishedAction =
            EFadeOutFinishedAction.QuitApp);
    }

    public partial record InGame : State, IGet<Input.RequestQuitGame> {
      public InGame() {
        this.OnEnter(() => {
          Output(new Output.StartNewGame());
          Output(new Output.FadeIn());
        });
        this.OnExit(() => Output(new Output.RemoveGame()));
      }

      public Transition On(in Input.RequestQuitGame input) =>
        To<FadingOut>().With(s =>
          ((FadingOut)s).FadeOutFinishedAction =
            EFadeOutFinishedAction.BackToMainMenu);
    }

    public partial record FadingOut : State, IGet<Input.FadeOutFinished> {
      public FadingOut() {
        this.OnEnter(() => Output(new Output.FadeOut()));
      }

      public Transition On(in Input.FadeOutFinished input) =>
        FadeOutFinishedAction switch {
          EFadeOutFinishedAction.StartGame => To<InGame>(),
          EFadeOutFinishedAction.BackToMainMenu => To<InMainMenu>(),
          EFadeOutFinishedAction.QuitApp => To<ClosingApplication>(),
          _ => throw new NotImplementedException(),
        };
    }

    public partial record ClosingApplication : State {
      public ClosingApplication() {
        this.OnEnter(() => Output(new Output.QuitApp()));
      }
    }
  }
}
```

```csharp
// ============================================================
// App.cs — The Godot visual node consuming AppLogic
// ============================================================
public partial class App : Node {
  #region State
  private AppLogic Logic { get; set; } = default!;
  private AppLogic.IBinding Binding { get; set; } = default!;
  #endregion

  #region Nodes
  [Export] private PackedScene _gameScene = default!;
  private Button _newGameButton = default!;
  private Button _quitButton = default!;
  private Game _game = default!;
  private AnimationPlayer _animPlayer = default!;
  #endregion

  public override void _Ready() {
    Logic = new AppLogic();
    Binding = Logic.Bind();

    _newGameButton = GetNode<Button>("%NewGameButton");
    _quitButton = GetNode<Button>("%QuitButton");
    _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    _newGameButton.Pressed += OnNewGameButtonPressed;
    _quitButton.Pressed += OnQuitButtonPressed;
    _animPlayer.AnimationFinished += OnAnimationFinished;

    Binding
      .Handle((in AppLogic.Output.StartNewGame _) => OnOutputStartNewGame())
      .Handle((in AppLogic.Output.RemoveGame _) => OnOutputRemoveGame())
      .Handle((in AppLogic.Output.QuitApp _) => OnOutputQuitApp())
      .Handle((in AppLogic.Output.UpdateMainMenuVisibility o) =>
        OnOutputUpdateMainMenuVisibility(o.Visible))
      .Handle((in AppLogic.Output.FadeOut _) => OnOutputFadeOut())
      .Handle((in AppLogic.Output.FadeIn _) => OnOutputFadeIn());

    Logic.Start();
  }

  public override void _UnhandledInput(InputEvent @event) {
    if (@event.IsActionPressed("ui_cancel")) {
      Logic.Input(new AppLogic.Input.RequestQuitGame());
    }
  }

  private void OnNewGameButtonPressed() =>
    Logic.Input(new AppLogic.Input.NewGameClick());

  private void OnQuitButtonPressed() =>
    Logic.Input(new AppLogic.Input.QuitClick());

  private void OnAnimationFinished(StringName animation) {
    if (animation == "fade_out") {
      Logic.Input(new AppLogic.Input.FadeOutFinished());
    }
  }

  private void OnOutputStartNewGame() {
    _game = _gameScene.Instantiate<Game>();
    AddChild(_game);
  }

  private void OnOutputRemoveGame() {
    _game?.QueueFree();
    _game = default!;
  }

  private void OnOutputQuitApp() => GetTree().Quit();

  private void OnOutputUpdateMainMenuVisibility(bool visible) =>
    GetNode<Control>("UI/MainMenu").Visible = visible;

  private void OnOutputFadeOut() => _animPlayer.Play("fade_out");
  private void OnOutputFadeIn() => _animPlayer.Play("fade_in");
}
```

### Example 2: Player Node with AutoInject + LogicBlocks

```csharp
// ============================================================
// IPlayerLogic.cs — Interface for testability
// ============================================================
public interface IPlayerLogic {
  PlayerLogic.IBinding Bind();
  void Input<TInput>(in TInput input) where TInput : struct;
  void Start();
}

// ============================================================
// Player.cs — Provider node
// ============================================================
[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D, IPlayer,
  IProvide<IPlayerLogic> {

  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies
  [Dependency]
  public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  #endregion

  #region Provisions
  IPlayerLogic IProvide<IPlayerLogic>.Value() => PlayerLogic;
  #endregion

  public IPlayerLogic PlayerLogic { get; set; } = default!;
  public PlayerLogic.IBinding PlayerBinding { get; set; } = default!;

  public void Setup() {
    PlayerLogic = new PlayerLogic(AppRepo);
  }

  public void OnResolved() {
    PlayerBinding = PlayerLogic.Bind();

    PlayerBinding
      .Handle((in PlayerLogic.Output.Moved output) => {
        Velocity = output.Velocity;
        MoveAndSlide();
      })
      .Handle((in PlayerLogic.Output.Jumped _) => {
        Velocity = new Vector3(Velocity.X, output.JumpForce, Velocity.Z);
      });

    PlayerLogic.Start();
    this.Provide(); // Now descendants can access IPlayerLogic
  }

  public override void _PhysicsProcess(double delta) {
    PlayerLogic.Input(new PlayerLogic.Input.PhysicsTick((float)delta));
  }

  public void OnExitTree() {
    PlayerBinding.Dispose();
  }
}

// ============================================================
// PlayerModel.cs — Dependent node (child of Player)
// ============================================================
[Meta(typeof(IAutoNode))]
public partial class PlayerModel : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies
  [Dependency]
  public IPlayerLogic PlayerLogic => this.DependOn<IPlayerLogic>();
  #endregion

  private PlayerLogic.IBinding _binding = default!;

  public void OnResolved() {
    _binding = PlayerLogic.Bind();

    _binding
      .Handle<PlayerLogic.Output.Animations.Idle>(
        (output) => AnimationStateMachine.Travel("idle")
      )
      .Handle<PlayerLogic.Output.Animations.Move>(
        (output) => AnimationStateMachine.Travel("move")
      )
      .Handle<PlayerLogic.Output.MoveSpeedChanged>(
        (output) => AnimationTree.Set(
          "parameters/main_animations/move/blend_position",
          output.Speed
        )
      );
  }

  public void OnExitTree() {
    _binding.Dispose();
  }
}
```

### Example 3: Repository with AutoProp + Domain Events

```csharp
public interface IAppRepo {
  IAutoProp<int> NumCoinsCollected { get; }
  IAutoProp<int> NumCoinsAtStart { get; }
  event Action? CoinCollected;
  event Action<GameOverReason>? GameEnded;
  void StartCoinCollection(ICoin coin);
  void OnFinishCoinCollection(ICoin coin);
}

public class AppRepo : IAppRepo, IDisposable {
  public IAutoProp<int> NumCoinsCollected => _numCoinsCollected;
  private readonly AutoProp<int> _numCoinsCollected = new(0);

  public IAutoProp<int> NumCoinsAtStart => _numCoinsAtStart;
  private readonly AutoProp<int> _numCoinsAtStart = new(0);

  public event Action? CoinCollected;
  public event Action<GameOverReason>? GameEnded;

  private int _coinsBeingCollected;

  public void StartCoinCollection(ICoin coin) {
    _coinsBeingCollected++;
    _numCoinsCollected.OnNext(_numCoinsCollected.Value + 1);
    CoinCollected?.Invoke();
  }

  public void OnFinishCoinCollection(ICoin coin) {
    _coinsBeingCollected--;
    if (_coinsBeingCollected == 0 &&
        _numCoinsCollected.Value >= _numCoinsAtStart.Value) {
      GameEnded?.Invoke(GameOverReason.PlayerWon);
    }
  }

  public void Dispose() {
    _numCoinsCollected.Dispose();
    _numCoinsAtStart.Dispose();
  }
}
```

---

## Summary of All Chickensoft Packages

| Package | Purpose |
|---------|---------|
| **AutoInject** | Tree-based dependency injection via `IProvide<T>` / `[Dependency]` |
| **LogicBlocks** | Hierarchical, serializable state machines with `StateLogic<T>` |
| **LogicBlocks.DiagramGenerator** | Source-generates UML state diagrams (*.g.puml) |
| **Introspection** + Generator | Source-generates mixin code (replaces reflection) |
| **GodotNodeInterfaces** | Generates interfaces for every GodotObject (IButton, ILabel, etc.) |
| **Collections** | AutoProp reactive properties, observable collections |
| **GoDotTest** | Test runner that executes inside the Godot engine |
| **godot-test-driver** | Test utilities for scene tree manipulation |
| **GodotGame** | dotnet new template with CI/CD, testing, coverage pre-configured |

---

## Architectural Rules of Thumb

1. **Nodes are thin.** If a node has logic beyond simple visualization, extract it into a LogicBlock.
2. **One LogicBlock per complex node.** The node owns it, binds to its outputs, and forwards user input.
3. **Repositories hold domain rules.** Multiple LogicBlocks subscribe to the same repository, keeping them synchronized via reactive events.
4. **Provide dependencies at the highest sensible ancestor.** Descendants request them via `[Dependency]`.
5. **Use interfaces everywhere.** IPlayerLogic, IAppRepo, IButton — enables mocking in tests.
6. **Use `readonly record struct` for Inputs/Outputs.** Minimizes heap allocations.
7. **OnAttach/OnDetach for event subscriptions.** OnEnter/OnExit for producing outputs.
8. **Blackboard for shared state.** Call `Set(data)` in the LogicBlock constructor; `Get<T>()` from any state.
9. **Dispose bindings in OnExitTree.** Prevent memory leaks.
10. **Two-phase init for testability.** Setup() creates objects, OnResolved() wires them up.
