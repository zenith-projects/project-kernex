# Official Godot Best Practices (from Godot 4.6 docs)

Best practices from official Godot documentation: data preferences, logic preferences, and object-oriented principles.

---

## Applying Object-Oriented Principles in Godot

### How Scripts Work in the Engine

Godot scripts are NOT true classes in the traditional sense. They are **resources that tell the engine a sequence of initializations to perform** on one of the engine's built-in classes.

The engine maintains a **ClassDB** database that tracks all classes with their:
- Properties
- Methods
- Constants
- Signals

When objects perform operations like property access or method calls, they "check the database's records and the object's base types' records" to verify support. Attaching a script extends the available methods, properties, and signals from this database.

**Key rule**: Scripts without an explicit `extends` statement implicitly inherit from `RefCounted`, making them instantiable from code but unable to attach to `Node` objects.

### Scenes as Conceptual Classes

Scenes function similarly to classes -- they are **reusable, instantiable, and inheritable groups of nodes**. A scene paired with a scripted root node effectively combines declarative structure with imperative behavior.

Scenes define:
- Available nodes for the script
- Node organization
- Initialization sequence
- Signal connections

**The scene is always an extension of the script attached to its root node.** This means object-oriented principles (single responsibility, encapsulation) apply to scenes just as they apply to code.

Since scene instances are objects, it is appropriate to interpret the scene as a component of a class definition -- the scene provides the structure, the script provides the behavior.

---

## Logic Preferences

### Adding Nodes and Changing Properties: Which First?

**Best practice: Change values on a node BEFORE adding it to the scene tree.**

Property setters contain code updating corresponding values, and this can impact performance -- particularly in procedurally-generated content scenarios. Setting initial values before adding to the tree prevents unnecessary update cascades.

```csharp
// GOOD: Configure before adding to tree
var enemy = enemyScene.Instantiate<Enemy>();
enemy.MaxHealth = 200;
enemy.MoveSpeed = 150f;
enemy.GlobalPosition = spawnPoint;  // Exception: global position can't be set before tree
GetTree().CurrentScene.AddChild(enemy);

// BAD: Adding then configuring triggers updates for each property
GetTree().CurrentScene.AddChild(enemy);
enemy.MaxHealth = 200;       // Triggers setter update
enemy.MoveSpeed = 150f;      // Triggers setter update
```

**Exception**: Some values CANNOT be set before being added to the scene tree, such as `global_position` / `GlobalPosition`.

### Loading vs. Preloading

GDScript's `preload()` loads resources as early as possible to front-load the loading operations. The `load()` method loads resources only when the statement executes.

**C# (there is NO preload equivalent — and that's fine, C# has better patterns):**
```csharp
using Godot;

public partial class MyBuildings : Node
{
    // Loaded when class is initialized (closest to preload)
    public readonly PackedScene Building = ResourceLoader.Load<PackedScene>("res://building.tscn");

    public PackedScene ABuilding;

    public override void _Ready()
    {
        // Loaded at runtime (equivalent to load())
        ABuilding = GD.Load<PackedScene>("res://Office.tscn");
    }
}
```

**When to use preload vs. load:**

1. **Unexpected load times**: Preloading without knowing script load timing creates variable load durations
2. **Exported properties override**: When scene exports override values, preloading becomes meaningless
3. **Class imports**: Preloaded constants work well for importing scripts/scenes, EXCEPT:
   - If the imported class may change at runtime (use `@export` or `load()`)
   - If managing many dependencies requiring runtime unloading (set properties to `null`)
4. **Memory management**: Resources loaded as properties can be set to `null` to unload from memory

### Large Levels: Static vs. Dynamic

**Static approach** (load everything at once):
- Pro: Simple
- Con: Excessive memory consumption causes slowdowns and crashes

**Dynamic approach** (load pieces as needed):
- Pro: Memory efficient
- Con: Increased complexity introduces bugs and technical debt

**Core recommendation**: Break larger scenes into smaller ones to aid in reusability of assets.

**Strategy recommendations:**
1. **Small games**: Use static levels (load everything)
2. **Medium/large games with resources**: Develop reusable management libraries or plugins handling resource creation/deletion
3. **Medium/large games with limited time**: Use dynamic logic when coding skills exist but time/resources are limited

---

## Data Preferences

### Big O Notation Quick Reference

When choosing data structures, consider operation complexity:
- **Constant-time O(1)**: Runtime does not increase with data size
- **Logarithmic-time O(log n)**: Runtime increases slowly
- **Linear-time O(n)**: Runtime increases proportionally to data size

For 3 million data points in a single frame, linear-time algorithms become impractical. But for small datasets or infrequent operations, linear-time is often acceptable.

### Array vs. Dictionary vs. Object

Godot stores all variables in the **Variant** class. Arrays are `Vector<Variant>`, Dictionaries are `HashMap<Variant, Variant>`.

**Array performance:**
| Operation | Speed | Notes |
|-----------|-------|-------|
| Iterate | Fastest | Increments counter to access next record |
| Insert/Erase/Move | Position-dependent | Fast at end, slow at beginning |
| Get/Set by index | Fastest | Single addition from array start |
| Find by value | Slowest | Requires iteration and comparison |

**Dictionary performance:**
| Operation | Speed | Notes |
|-----------|-------|-------|
| Iterate | Fast | Iterates over internal hash vector |
| Insert/Erase | Fastest | Constant-time hashing plus lookup |
| Get/Set by key | Fastest | Hash-based lookup |
| Find by value | Slowest | Must iterate all records |

**Object performance:**
Objects query multiple data sources (scripts, ClassDB) through inheritance hierarchies. Slower than Arrays or Dictionaries due to chain-of-queries complexity.

**When to use Objects over simpler structures:**
1. **Control**: Need sophisticated abstractions and signals
2. **Clarity**: Need to verify property existence at design time
3. **Convenience**: Extending existing engine classes

### TreeNode Example (Objects as Data Structures)

**C#:**
```csharp
using Godot;
using System.Collections.Generic;

public partial class TreeNode : GodotObject
{
    private TreeNode _parent = null;
    private List<TreeNode> _children = [];

    public override void _Notification(int what)
    {
        switch (what)
        {
            case NotificationPredelete:
                foreach (TreeNode child in _children)
                {
                    child.Free();
                }
                break;
        }
    }
}
```

### Enumerations: int vs. string

**Integer comparisons** are constant-time (O(1)), while **string comparisons** are linear-time (O(n) on string length).

However, GDScript prioritizes usability over performance:
- Integers print as numbers, not meaningful names
- Strings are immediately readable and self-documenting
- Strings avoid requiring separate Dictionary mappings for display

**Recommendation**: Use string enumerations for readability in GDScript. In C#, use proper `enum` types which give you both performance (integer comparison) and readability (named values).

### Animation System Comparison

| System | Type | Best For |
|--------|------|----------|
| **AnimatedTexture** | Resource (not Node) | Looping animated sequences in TileMap, minimal engine logic |
| **AnimatedSprite2D** | Node | Frame-based 2D animations with SpriteFrames, controlling speed/offset/orientation |
| **AnimationPlayer** | Node (required) | Triggering side effects (particles, function calls), cut-out animations, 2D mesh animations, complex multi-property keyframe animation |
| **AnimationTree** | Node | Combining animations for blending, managing hierarchical animation structures with smooth transitions |

**Decision guide:**
- Need simple sprite cycling? -> AnimatedSprite2D
- Need to animate multiple properties + trigger effects? -> AnimationPlayer
- Need blend trees, state machines, smooth transitions? -> AnimationTree
- Need animated texture in TileMap with minimal overhead? -> AnimatedTexture

---

## Documentation Topic Indexes (from Godot 4.6 docs)

### Export Topics
1. Exporting projects
2. Exporting PCK files
3. Feature tags
4. Exporting for Windows
5. Exporting for Linux
6. Exporting for macOS
7. Exporting for Android
8. Exporting for iOS
9. Exporting for visionOS
10. Exporting for Web
11. Changing application icon for Windows
12. Running on macOS
13. Android Gradle build
14. One-click deploy
15. Exporting for dedicated servers

### Networking Topics
1. High-level multiplayer
2. HTTP request class
3. HTTP client class
4. SSL certificates
5. WebSocket
6. WebRTC

### Plugin Topics
**Editor plugins:**
1. Installing plugins
2. Making plugins
3. Making main screen plugins
4. Import plugins
5. 3D gizmos
6. Inspector plugins
7. Visual shader plugins

**Other:**
- Running code in the editor

### Performance Topics
**Common:**
- General optimization
- Using servers

**CPU:**
- CPU optimization

**GPU:**
- GPU optimization
- Using MultiMesh
- Pipeline compilations

**3D:**
- Optimizing 3D performance
- Vertex animation

**Threads:**
- Using multiple threads
- Thread safe APIs
