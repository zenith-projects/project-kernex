# Navigation System — Advanced Reference

> Godot 4.6 documentation. All code examples are C# only.

---

## Table of Contents

1. [Using NavigationServer](#1-using-navigationserver)
2. [Using NavigationMaps](#2-using-navigationmaps)
3. [Using NavigationRegions](#3-using-navigationregions)
4. [Using Navigation Meshes](#4-using-navigation-meshes)
5. [Using NavigationAgents](#5-using-navigationagents)
6. [Using NavigationObstacles](#6-using-navigationobstacles)
7. [Using NavigationLinks](#7-using-navigationlinks)
8. [Using NavigationLayers](#8-using-navigationlayers)
9. [Connecting Navigation Meshes](#9-connecting-navigation-meshes)
10. [Support Different Actor Types](#10-support-different-actor-types)
11. [Support Different Actor Locomotion](#11-support-different-actor-locomotion)
12. [Navigation Debug Tools](#12-navigation-debug-tools)

---

## 1. Using NavigationServer

2D and 3D version of the NavigationServer are available as `NavigationServer2D` and `NavigationServer3D` respectively.

### Communicating with the NavigationServer

To work with the NavigationServer means to prepare parameters for a **query** that can be sent to the NavigationServer for updates or requesting data.

To reference the internal NavigationServer objects like maps, regions and agents RIDs are used as identification numbers. Every navigation related node in the scene tree has a function that returns the RID for this node.

### Threading and Synchronization

The NavigationServer does not update every change immediately but waits until the end of the **physics frame** to synchronize all the changes together.

Waiting for synchronization is required to apply changes to all maps, regions and agents. Synchronization is done because some updates like a recalculation of the entire navigation map are very expensive and require updated data from all other objects. Also the NavigationServer uses a **threadpool** by default for some functionality like avoidance calculation between agents.

Waiting is not required for most `get()` functions that only request data from the NavigationServer without making changes. Note that not all data will account for changes made in the same frame. E.g. if an avoidance agent changed the navigation map this frame the `agent_get_map()` function will still return the old map before the synchronization. The exception to this are nodes that store their values internally before sending the update to the NavigationServer. When a getter on a node is used for a value that was updated in the same frame it will return the already updated value stored on the node.

The NavigationServer is **thread-safe** as it places all API calls that want to make changes in a queue to be executed in the synchronization phase. Synchronization for the NavigationServer happens in the middle of the physics frame after scene input from scripts and nodes are all done.

> **Note:** The important takeaway is that most NavigationServer changes take effect after the next physics frame and not immediately. This includes all changes made by navigation related nodes in the scene tree or through scripts.

> **Note:** All setters and delete functions require synchronization.

### 2D and 3D NavigationServer Differences

NavigationServer2D and NavigationServer3D are equivalent in functionality for their dimension.

Technically it is possible to use the tools for creating navigation meshes in one dimension for the other dimension, e.g. baking a 2D navigation mesh with the 3D NavigationMesh when using flat 3D source geometry or creating 3D flat navigation meshes with the polygon outline draw tools of NavigationRegion2D and NavigationPolygons.

### Waiting for Synchronization

At the start of the game, a new scene or procedural navigation changes any path query to a NavigationServer will return empty or wrong.

The navigation map is still empty or not updated at this point. All nodes from the scene tree need to first upload their navigation related data to the NavigationServer. Each added or changed map, region or agent need to be registered with the NavigationServer. Afterward the NavigationServer requires a **physics frame** for synchronization to update the maps, regions and agents.

One workaround is to make a deferred call to a custom setup function (so all nodes are ready). The setup function makes all the navigation changes, e.g. adding procedural stuff. Afterwards the function waits for the next physics frame before continuing with path queries.

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    public override void _Ready()
    {
        // Use call deferred to make sure the entire scene tree nodes are setup
        // else await on 'physics_frame' in a _Ready() might get stuck.
        CallDeferred(MethodName.CustomSetup);
    }

    private async void CustomSetup()
    {
        // Create a new navigation map.
        Rid map = NavigationServer3D.MapCreate();
        NavigationServer3D.MapSetUp(map, Vector3.Up);
        NavigationServer3D.MapSetActive(map, true);

        // Create a new navigation region and add it to the map.
        Rid region = NavigationServer3D.RegionCreate();
        NavigationServer3D.RegionSetTransform(region, Transform3D.Identity);
        NavigationServer3D.RegionSetMap(region, map);

        // Create a procedural navigation mesh for the region.
        var newNavigationMesh = new NavigationMesh()
        {
            Vertices =
            [
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(9.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 9.0f),
            ],
        };
        int[] polygon = [0, 1, 2];
        newNavigationMesh.AddPolygon(polygon);
        NavigationServer3D.RegionSetNavigationMesh(region, newNavigationMesh);

        // Wait for NavigationServer sync to adapt to made changes.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Query the path from the navigation server.
        var startPosition = new Vector3(0.1f, 0.0f, 0.1f);
        var targetPosition = new Vector3(1.0f, 0.0f, 1.0f);

        Vector3[] path = NavigationServer3D.MapGetPath(map, startPosition, targetPosition, optimize: true);

        GD.Print("Found a path!");
        GD.Print((Variant)path);
    }
}
```

### Server Avoidance Callbacks

If RVO avoidance agents are registered for avoidance callbacks the NavigationServer dispatches their `velocity_computed` signals just before the PhysicsServer synchronization.

The simplified order of execution for NavigationAgents that use avoidance:

- Physics frame starts.
- `_PhysicsProcess(delta)`.
- `Velocity` property is set on NavigationAgent Node.
- Agent sends velocity and position to NavigationServer.
- NavigationServer waits for synchronization.
- NavigationServer synchronizes and computes avoidance velocities for all registered avoidance agents.
- NavigationServer sends safe velocity vector with signals for each registered avoidance agents.
- Agents receive the signal and move their parent e.g. with `MoveAndSlide()` or `LinearVelocity`.
- PhysicsServer synchronizes.
- Physics frame ends.

Therefore moving a physicsbody actor in the callback function with the safe velocity is perfectly thread- and physics-safe as all happens inside the same physics frame before the PhysicsServer commits to changes and does its own calculations.

---

## 2. Using NavigationMaps

A NavigationMap is an abstract navigation world on the NavigationServer identified by a NavigationServer `Rid`.

A map can hold and connect a near infinite number of navigation regions with navigation meshes to build the traversable areas of a game world for pathfinding.

A map can contain avoidance agents. Collision avoidance will be calculated based on the agents present in the map.

> **Note:** Different NavigationMaps are completely isolated from each other but navigation regions and avoidance agents can switch between different maps. Switches will become effective on NavigationServer synchronization.

### Default Navigation Maps

By default Godot creates a navigation map for each `World2D` and `World3D` of the root viewport.

The 2D default navigation map RID can be obtained with `GetWorld2D().NavigationMap` from any Node2D inheriting Node.

The 3D default navigation map RID can be obtained with `GetWorld3D().NavigationMap` from any Node3D inheriting Node.

**2D:**

```csharp
public partial class MyNode2D : Node2D
{
    public override void _Ready()
    {
        Rid defaultNavigationMapRid = GetWorld2D().NavigationMap;
    }
}
```

**3D:**

```csharp
public partial class MyNode3D : Node3D
{
    public override void _Ready()
    {
        Rid defaultNavigationMapRid = GetWorld3D().NavigationMap;
    }
}
```

### Creating New Navigation Maps

The NavigationServer can create and support as many navigation maps as required for specific gameplay. Additional navigation maps are created and handled by using the NavigationServer API directly e.g. to support different avoidance agent or actor locomotion types.

Each navigation map individually synchronizes queued changes to its navigation regions and avoidance agents. A navigation map that has not received changes will consume little to no processing time. Navigation regions and avoidance agents can only be part of a single navigation map but they can switch map at any time.

> **Note:** A navigation map switch will take effect only after the next NavigationServer synchronization.

**2D:**

```csharp
public partial class MyNode2D : Node2D
{
    public override void _Ready()
    {
        Rid newNavigationMap = NavigationServer2D.MapCreate();
        NavigationServer2D.MapSetActive(newNavigationMap, true);
    }
}
```

**3D:**

```csharp
public partial class MyNode3D : Node3D
{
    public override void _Ready()
    {
        Rid newNavigationMap = NavigationServer3D.MapCreate();
        NavigationServer3D.MapSetActive(newNavigationMap, true);
    }
}
```

> **Note:** There is no difference between navigation maps created with the NavigationServer2D API or the NavigationServer3D API.

---

## 3. Using NavigationRegions

NavigationRegions are the visual Node representation of a **region** of the navigation **map** on the NavigationServer. Each NavigationRegion node holds a resource for the navigation mesh data.

Both 2D and 3D version are available as `NavigationRegion2D` and `NavigationRegion3D` respectively.

Individual NavigationRegions upload their 2D NavigationPolygon or 3D NavigationMesh resource data to the NavigationServer. The NavigationServer map turns this information into a combined navigation map for pathfinding.

To create a navigation region using the scene tree add a `NavigationRegion2D` or `NavigationRegion3D` node to the scene. All regions require a navigation mesh resource to function.

NavigationRegions will automatically push `global_transform` changes to the region on the NavigationServer which makes them suitable for moving platforms. The NavigationServer will attempt to connect the navigation meshes of individual regions when they are close enough. To connect NavigationRegions over arbitrary distances see the NavigationLinks section to learn how to create and use `NavigationLinks`.

> **Warning:** While changing the transform of a NavigationRegion node does update the region position on the NavigationServer, changing the scale does not. A navigation mesh resource has no scale and needs to be fully updated when source geometry changes scale.

Regions can be enabled / disabled and if disabled will not contribute to future pathfinding queries.

> **Note:** Existing paths will not be automatically updated when a region gets enabled / disabled.

### Creating New Navigation Regions

New NavigationRegion nodes will automatically register to the default world navigation map for their 2D/3D dimension.

The region RID can then be obtained from NavigationRegion Nodes with `GetRid()`.

**2D:**

```csharp
public partial class MyNavigationRegion2D : NavigationRegion2D
{
    public override void _Ready()
    {
        Rid navigationServerRegionRid = GetRid();
    }
}
```

**3D:**

```csharp
public partial class MyNavigationRegion3D : NavigationRegion3D
{
    public override void _Ready()
    {
        Rid navigationServerRegionRid = GetRid();
    }
}
```

New regions can also be created with the NavigationServer API and added to any existing map. If regions are created with the NavigationServer API directly they need to be assigned a navigation map manually.

**2D:**

```csharp
public partial class MyNode2D : Node2D
{
    public override void _Ready()
    {
        Rid newRegionRid = NavigationServer2D.RegionCreate();
        Rid defaultMapRid = GetWorld2D().NavigationMap;
        NavigationServer2D.RegionSetMap(newRegionRid, defaultMapRid);
    }
}
```

**3D:**

```csharp
public partial class MyNode3D : Node3D
{
    public override void _Ready()
    {
        Rid newRegionRid = NavigationServer3D.RegionCreate();
        Rid defaultMapRid = GetWorld3D().NavigationMap;
        NavigationServer3D.RegionSetMap(newRegionRid, defaultMapRid);
    }
}
```

> **Note:** Navigation regions can only be assigned to a single navigation map. If an existing region is assigned to a new navigation map it will leave the old map.

---

## 4. Using Navigation Meshes

2D and 3D versions of the navigation mesh are available as `NavigationPolygon` and `NavigationMesh` respectively.

> **Note:** A navigation mesh only describes a traversable area for an agent's center position. Any radius values an agent may have are ignored. If you want pathfinding to account for an agent's (collision) size you need to shrink the navigation mesh accordingly.

Navigation works independently from other engine parts like rendering or physics. Navigation meshes are the only things considered when doing pathfinding, e.g. visuals and collision shapes for example are completely ignored by the navigation system. If you need to take other data (like visuals for example) into account when doing pathfinding, you need to adapt your navigation meshes accordingly. The process of factoring in navigation restrictions in navigation meshes is commonly referred to as navigation mesh baking.

A navigation mesh describes a surface that an agent can stand on safely with its center compared to physics shapes that describe outer collision bounds.

If you experience clipping or collision problems while following navigation paths, always remember that you need to tell the navigation system what your intentions are through an appropriate navigation mesh. By itself the navigation system will never know "this is a tree / rock / wall collision shape or visual mesh" because it only knows that "here I was told I can path safely because it is on a navigation mesh".

### Baking a Navigation Mesh with a NavigationRegion

The navigation mesh baking is made more accessible with the NavigationRegion node. When baking with a NavigationRegion node, the individual parsing, baking, and region update steps are all combined into one function.

The nodes are available in 2D and 3D as `NavigationRegion2D` and `NavigationRegion3D` respectively.

> **Tip:** The navigation mesh `source_geometry_mode` can be switched to parse specific node group names so nodes that should be baked can be placed anywhere in the scene.

#### Baking with a NavigationRegion2D

When a NavigationRegion2D node is selected in the Editor, bake options as well as polygon draw tools appear in the top bar of the Editor.

In order for the region to work a `NavigationPolygon` resource needs to be added.

The properties to parse and bake a navigation mesh are then part of the used resource and can be found in the resource Inspector.

The result of the source geometry parsing can be influenced with the following properties:

- The `parsed_geometry_type` that filters if visual objects or physics objects or both should be parsed from the SceneTree. For more details on what objects are parsed and how, see the section about parsing source geometry below.
- The `collision_mask` filters which physics collision objects are included when the `parsed_geometry_type` includes static colliders.
- The `source_geometry_mode` that defines on which node(s) to start the parsing, and how to traverse the SceneTree.
- The `source_geometry_group_name` is used when only a certain node group should be parsed. Depends on the selected `source_geometry_mode`.

With the source geometry added, the result of the baking can be controlled with the following properties:

- The `cell_size` sets the rasterization grid size and should match the navigation map size.
- The `agent_radius` shrinks the baked navigation mesh to have enough margin for the agent (collision) size.

The NavigationRegion2D baking can also be used at runtime with scripts:

```csharp
bool onThread = true;
BakeNavigationPolygon(onThread);
```

To quickly test the 2D baking with default settings:

1. Add a `NavigationRegion2D`.
2. Add a `NavigationPolygon` resource to the NavigationRegion2D.
3. Add a `Polygon2D` below the NavigationRegion2D.
4. Draw 1 NavigationPolygon outline with the selected NavigationRegion2D draw tool.
5. Draw 1 Polygon2D outline inside the NavigationPolygon outline with the selected Polygon2D draw tool.
6. Hit the Editor bake button and a navigation mesh should appear.

#### Baking with a NavigationRegion3D

When a NavigationRegion3D node is selected in the Editor, bake options appear in the top bar of the Editor.

In order for the region to work a `NavigationMesh` resource needs to be added.

The properties to parse and bake a navigation mesh are then part of the used resource and can be found in the resource Inspector.

The result of the source geometry parsing can be influenced with the following properties:

- The `parsed_geometry_type` that filters if visual objects or physics objects or both should be parsed from the SceneTree.
- The `collision_mask` filters which physics collision objects are included when the `parsed_geometry_type` includes static colliders.
- The `source_geometry_mode` that defines on which node(s) to start the parsing, and how to traverse the SceneTree.
- The `source_geometry_group_name` is used when only a certain node group should be parsed. Depends on the selected `source_geometry_mode`.

With the source geometry added, the result of the baking can be controlled with the following properties:

- The `cell_size` and `cell_height` sets the rasterization voxel grid size and should match the navigation map size.
- The `agent_radius` shrinks the baked navigation mesh to have enough margin for the agent (collision) size.
- The `agent_height` excludes areas from the navigation mesh where the agent is too tall to fit in.
- The `agent_max_climb` and `agent_max_slope` removes areas where the height difference between neighboring voxels is too large, or where their surface is too steep.

> **Warning:** A too small `cell_size` or `cell_height` can create so many voxels that it has the potential to freeze the game or even crash.

The NavigationRegion3D baking can also be used at runtime with scripts:

```csharp
bool onThread = true;
BakeNavigationMesh(onThread);
```

To quickly test the 3D baking with default settings:

1. Add a `NavigationRegion3D`.
2. Add a `NavigationMesh` resource to the NavigationRegion3D.
3. Add a `MeshInstance3D` below the NavigationRegion3D.
4. Add a `PlaneMesh` to the MeshInstance3D.
5. Hit the Editor bake button and a navigation mesh should appear.

### Baking a Navigation Mesh with the NavigationServer

The `NavigationServer2D` and `NavigationServer3D` have API functions to call each step of the navigation mesh baking process individually.

- `ParseSourceGeometryData()` can be used to parse source geometry to a reusable and serializable resource.
- `BakeFromSourceGeometryData()` can be used to bake a navigation mesh from already parsed data e.g. to avoid runtime performance issues with (redundant) parsing.
- `BakeFromSourceGeometryDataAsync()` is the same but bakes the navigation mesh deferred with threads, not blocking the main thread.

Compared to a NavigationRegion, the NavigationServer offers finer control over the navigation mesh baking process. In turn it is more complex to use but also provides more advanced options.

Some other advantages of the NavigationServer over a NavigationRegion are:

- The server can parse source geometry without baking, e.g. to cache it for later use.
- The server allows selecting the root node at which to start the source geometry parsing manually.
- The server can accept and bake from procedurally generated source geometry data.
- The server can bake multiple navigation meshes in sequence while (re)using the same source geometry data.

To bake navigation meshes with the NavigationServer, source geometry is required. Source geometry is geometry data that should be considered in a navigation mesh baking process. Both navigation meshes for 2D and 3D are created by baking them from source geometry.

2D and 3D versions of the source geometry resources are available as `NavigationMeshSourceGeometryData2D` and `NavigationMeshSourceGeometryData3D` respectively.

Source geometry can be geometry parsed from visual meshes, from physics collision, or procedural created arrays of data, like outlines (2D) and triangle faces (3D). For convenience, source geometry is commonly parsed directly from node setups in the SceneTree. For runtime navigation mesh (re)bakes, be aware that the geometry parsing always happens on the main thread.

> **Note:** The SceneTree is not thread-safe. Parsing source geometry from the SceneTree can only be done on the main thread.

> **Warning:** The data from visual meshes and polygons needs to be received from the GPU, stalling the RenderingServer in the process. For runtime (re)baking prefer using physics shapes as parsed source geometry.

Source geometry is stored inside resources so the created geometry can be reused for multiple bakes. E.g. baking multiple navigation meshes for different agent sizes from the same source geometry. This also allows to save source geometry to disk so it can be loaded later, e.g. to avoid the overhead of parsing it again at runtime.

The geometry data should be in general kept very simple. As many edges as are required but as few as possible. Especially in 2D duplicated and nested geometry should be avoided as it forces polygon hole calculation that can result in flipped polygons. An example for nested geometry would be a smaller StaticBody2D shape placed completely inside the bounds of another StaticBody2D shape.

### Baking Navigation Mesh Chunks for Large Worlds

To avoid misaligned edges between different region chunks the navigation meshes have two important properties for the navigation mesh baking process: the baking bound and the border size. Together they can be used to ensure perfectly aligned edges between region chunks.

The baking bound, which is an axis-aligned `Rect2` for 2D and `AABB` for 3D, limits the used source geometry by discarding all the geometry that is outside of the bounds.

The `NavigationPolygon` properties `baking_rect` and `baking_rect_offset` can be used to create and place the 2D baking bound.

The `NavigationMesh` properties `filter_baking_aabb` and `filter_baking_aabb_offset` can be used to create and place the 3D baking bound.

With only the baking bound set another problem still exists. The resulting navigation mesh will inevitably be affected by necessary offsets like the `agent_radius` which makes the edges not align properly.

This is where the `border_size` property for navigation mesh comes in. The border size is an inward margin from the baking bound. The important characteristic of the border size is that it is unaffected by most offsets and postprocessing like the `agent_radius`.

Instead of discarding source geometry, the border size discards parts of the final surface of the baked navigation mesh. If the baking bound is large enough the border size can remove the problematic surface parts so that only the intended chunk size is left.

> **Note:** The baking bounds need to be large enough to include a reasonable amount of source geometry from all the neighboring chunks.

> **Warning:** In 3D the functionality of the border size is limited to the xz-axis.

### Navigation Mesh Baking Common Problems

There are some common user problems and important caveats to consider when creating or baking navigation meshes.

**Navigation mesh baking creates frame rate problems at runtime:**

The navigation mesh baking is by default done on a background thread, so as long as the platform supports threads, the actual baking is rarely the source of any performance issues (assuming a reasonably sized and complex geometry for runtime rebakes).

The common source for performance issues at runtime is the parsing step for source geometry that involves nodes and the SceneTree. The SceneTree is not thread-safe so all the nodes need to be parsed on the main thread. Some nodes with a lot of data can be very heavy and slow to parse at runtime, e.g. a TileMap has one or more polygons for every single used cell and TileMapLayer to parse. Nodes that hold meshes need to request the data from the RenderingServer stalling the rendering in the process.

To improve performance, use more optimized shapes, e.g. collision shapes over detailed visual meshes, and merge and simplify as much geometry as possible upfront. If nothing helps, don't parse the SceneTree and add the source geometry procedural with scripts. If only pure data arrays are used as source geometry, the entire baking process can be done on a background thread.

**Navigation mesh creates unintended holes in 2D:**

The navigation mesh baking in 2D is done by doing polygon clipping operations based on outline paths. Polygons with "holes" are a necessary evil to create more complex 2D polygons but can become unpredictable for users with many complex shapes involved.

To avoid any unexpected problems with polygon hole calculations, avoid nesting any outlines inside other outlines of the same type (traversable / obstruction). This includes the parsed shapes from nodes. E.g. placing a smaller StaticBody2D shape inside a larger StaticBody2D shape can result in the resulting polygon being flipped.

**Navigation mesh appears inside geometry in 3D:**

The navigation mesh baking in 3D has no concept of "inside". The voxel cells used to rasterize the geometry are either occupied or not. Remove the geometry that is on the ground inside the other geometry. If that is not possible, add smaller "dummy" geometry inside with as few triangles as possible so the cells are occupied with something.

A `NavigationObstacle3D` shape set to bake with navigation mesh can be used to discard geometry as well.

### Navigation Mesh Script Templates

The following script uses the NavigationServer to parse source geometry from the scene tree, bakes a navigation mesh, and updates a navigation region with the updated navigation mesh.

**2D:**

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    private NavigationPolygon _navigationMesh;
    private NavigationMeshSourceGeometryData2D _sourceGeometry;
    private Callable _callbackParsing;
    private Callable _callbackBaking;
    private Rid _regionRid;

    public override void _Ready()
    {
        _navigationMesh = new NavigationPolygon();
        _navigationMesh.AgentRadius = 10.0f;
        _sourceGeometry = new NavigationMeshSourceGeometryData2D();
        _callbackParsing = Callable.From(OnParsingDone);
        _callbackBaking = Callable.From(OnBakingDone);
        _regionRid = NavigationServer2D.RegionCreate();

        // Enable the region and set it to the default navigation map.
        NavigationServer2D.RegionSetEnabled(_regionRid, true);
        NavigationServer2D.RegionSetMap(_regionRid, GetWorld2D().NavigationMap);

        // Some mega-nodes like TileMap are often not ready on the first frame.
        // Also the parsing needs to happen on the main-thread.
        // So do a deferred call to avoid common parsing issues.
        CallDeferred(MethodName.ParseSourceGeometry);
    }

    private void ParseSourceGeometry()
    {
        _sourceGeometry.Clear();
        Node2D rootNode = this;

        // Parse the obstruction outlines from all child nodes of the root node by default.
        NavigationServer2D.ParseSourceGeometryData(
            _navigationMesh,
            _sourceGeometry,
            rootNode,
            _callbackParsing
        );
    }

    private void OnParsingDone()
    {
        // If we did not parse a TileMap with navigation mesh cells we may now only
        // have obstruction outlines so add at least one traversable outline
        // so the obstructions outlines have something to "cut" into.
        _sourceGeometry.AddTraversableOutline(
        [
            new Vector2(0.0f, 0.0f),
            new Vector2(500.0f, 0.0f),
            new Vector2(500.0f, 500.0f),
            new Vector2(0.0f, 500.0f),
        ]);

        // Bake the navigation mesh on a thread with the source geometry data.
        NavigationServer2D.BakeFromSourceGeometryDataAsync(_navigationMesh, _sourceGeometry, _callbackBaking);
    }

    private void OnBakingDone()
    {
        // Update the region with the updated navigation mesh.
        NavigationServer2D.RegionSetNavigationPolygon(_regionRid, _navigationMesh);
    }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    private NavigationMesh _navigationMesh;
    private NavigationMeshSourceGeometryData3D _sourceGeometry;
    private Callable _callbackParsing;
    private Callable _callbackBaking;
    private Rid _regionRid;

    public override void _Ready()
    {
        _navigationMesh = new NavigationMesh();
        _navigationMesh.AgentRadius = 0.5f;
        _sourceGeometry = new NavigationMeshSourceGeometryData3D();
        _callbackParsing = Callable.From(OnParsingDone);
        _callbackBaking = Callable.From(OnBakingDone);
        _regionRid = NavigationServer3D.RegionCreate();

        // Enable the region and set it to the default navigation map.
        NavigationServer3D.RegionSetEnabled(_regionRid, true);
        NavigationServer3D.RegionSetMap(_regionRid, GetWorld3D().NavigationMap);

        // Some mega-nodes like GridMap are often not ready on the first frame.
        // Also the parsing needs to happen on the main-thread.
        // So do a deferred call to avoid common parsing issues.
        CallDeferred(MethodName.ParseSourceGeometry);
    }

    private void ParseSourceGeometry()
    {
        _sourceGeometry.Clear();
        Node3D rootNode = this;

        // Parse the geometry from all mesh child nodes of the root node by default.
        NavigationServer3D.ParseSourceGeometryData(
            _navigationMesh,
            _sourceGeometry,
            rootNode,
            _callbackParsing
        );
    }

    private void OnParsingDone()
    {
        // Bake the navigation mesh on a thread with the source geometry data.
        NavigationServer3D.BakeFromSourceGeometryDataAsync(_navigationMesh, _sourceGeometry, _callbackBaking);
    }

    private void OnBakingDone()
    {
        // Update the region with the updated navigation mesh.
        NavigationServer3D.RegionSetNavigationMesh(_regionRid, _navigationMesh);
    }
}
```

The following script uses the NavigationServer to update a navigation region with procedurally generated navigation mesh data.

**2D:**

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    private NavigationPolygon _navigationMesh;
    private Rid _regionRid;

    public override void _Ready()
    {
        _navigationMesh = new NavigationPolygon();
        _regionRid = NavigationServer2D.RegionCreate();

        // Enable the region and set it to the default navigation map.
        NavigationServer2D.RegionSetEnabled(_regionRid, true);
        NavigationServer2D.RegionSetMap(_regionRid, GetWorld2D().NavigationMap);

        // Add vertices for a convex polygon.
        _navigationMesh.Vertices =
        [
            new Vector2(0, 0),
            new Vector2(100.0f, 0),
            new Vector2(100.0f, 100.0f),
            new Vector2(0, 100.0f),
        ];

        // Add indices for the polygon.
        _navigationMesh.AddPolygon([0, 1, 2, 3]);

        NavigationServer2D.RegionSetNavigationPolygon(_regionRid, _navigationMesh);
    }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    private NavigationMesh _navigationMesh;
    private Rid _regionRid;

    public override void _Ready()
    {
        _navigationMesh = new NavigationMesh();
        _regionRid = NavigationServer3D.RegionCreate();

        // Enable the region and set it to the default navigation map.
        NavigationServer3D.RegionSetEnabled(_regionRid, true);
        NavigationServer3D.RegionSetMap(_regionRid, GetWorld3D().NavigationMap);

        // Add vertices for a convex polygon.
        _navigationMesh.Vertices =
        [
            new Vector3(-1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 0.0f, -1.0f),
            new Vector3(-1.0f, 0.0f, -1.0f),
        ];

        // Add indices for the polygon.
        _navigationMesh.AddPolygon([0, 1, 2, 3]);

        NavigationServer3D.RegionSetNavigationMesh(_regionRid, _navigationMesh);
    }
}
```

---

## 5. Using NavigationAgents

NavigationAgents are helper nodes that combine functionality for pathfinding, path following and agent avoidance for a Node2D/3D inheriting parent node. They facilitate common calls to the NavigationServer API on behalf of the parent actor node in a more convenient manner for beginners.

2D and 3D version of NavigationAgents are available as `NavigationAgent2D` and `NavigationAgent3D` respectively.

New NavigationAgent nodes will automatically join the default navigation map on the `World2D`/`World3D`.

NavigationAgent nodes are optional and not a hard requirement to use the navigation system. Their entire functionality can be replaced with scripts and direct calls to the NavigationServer API.

> **Tip:** For more advanced uses consider NavigationPathQueryObjects over NavigationAgent nodes.

### NavigationAgent Pathfinding

NavigationAgents query a new navigation path on their current navigation map when their `target_position` is set with a global position.

The result of the pathfinding can be influenced with the following properties:

- The `navigation_layers` bitmask can be used to limit the navigation meshes that the agent can use.
- The `pathfinding_algorithm` controls how the pathfinding travels through the navigation mesh polygons in the path search.
- The `path_postprocessing` sets if or how the raw path corridor found by the pathfinding is altered before it is returned.
- The `path_metadata_flags` enable the collection of additional path point meta data returned by the path.
- The `simplify_path` and `simplify_epsilon` properties can be used to remove less critical points from the path.

> **Warning:** Disabling path meta flags will disable related signal emissions on the agent.

### NavigationAgent Pathfollowing

After a `target_position` has been set for the agent, the next position to follow in the path can be retrieved with the `GetNextPathPosition()` function.

Once the next path position is received, move the parent actor node of the agent towards this path position with your own movement code.

> **Note:** The navigation system never moves the parent node of a NavigationAgent. The movement is entirely in the hands of users and their custom scripts.

NavigationAgents have their own internal logic to proceed with the current path and call for updates.

The `GetNextPathPosition()` function is responsible for updating many of the agent's internal states and properties. The function should be repeatedly called *once* every `_PhysicsProcess` until `IsNavigationFinished()` tells that the path is finished. The function should not be called after the target position or path end has been reached as it can make the agent jitter in place due to the repeated path updates. Always check very early in script with `IsNavigationFinished()` if the path is already finished.

The following distance properties influence the path following behavior:

- At `path_desired_distance` from the next path position, the agent advances its internal path index to the subsequent next path position.
- At `target_desired_distance` from the target path position, the agent considers the target position to be reached and the path at its end.
- At `path_max_distance` from the ideal path to the next path position, the agent requests a new path because it was pushed too far off.

The important updates are all triggered with the `GetNextPathPosition()` function when called in `_PhysicsProcess()`.

NavigationAgents can be used with `_Process` but are still limited to a single update that happens in `_PhysicsProcess`.

### Pathfollowing Common Problems

There are some common user problems and important caveats to consider when writing agent movement scripts.

**The path is returned empty:**
If an agent queries a path before the navigation map synchronisation, e.g. in a `_Ready()` function, the path might return empty. In this case the `GetNextPathPosition()` function will return the same position as the agent parent node and the agent will consider the path end reached. This is fixed by making a deferred call or using a callback e.g. waiting for the navigation map changed signal.

**The agent is stuck dancing between two positions:**
This is usually caused by very frequent path updates every single frame, either deliberate or by accident (e.g. max path distance set too short). The pathfinding needs to find the closest position that are valid on navigation mesh. If a new path is requested every single frame the first path positions might end up switching constantly in front and behind the agent's current position, causing it to dance between the two positions.

**The agent is backtracking sometimes:**
If an agent moves very fast it might overshoot the `path_desired_distance` check without ever advancing the path index. This can lead to the agent backtracking to the path point now behind it until it passes the distance check to increase the path index. Increase the desired distances accordingly for your agent speed and update rate usually fixes this as well as a more balanced navigation mesh polygon layout with not too many polygon edges cramped together in small spaces.

**The agent is sometimes looking backwards for a frame:**
Same as with stuck dancing agents between two positions, this is usually caused by very frequent path updates every single frame. Depending on your navigation mesh layout, and especially when an agent is directly placed over a navigation mesh edge or edge connection, expect path positions to be sometimes slightly "behind" your actors current orientation. This happens due to precision issues and can not always be avoided. This is usually only a visible problem if actors are instantly rotated to face the current path position.

### NavigationAgent Avoidance

This section explains how to use the navigation avoidance specific to NavigationAgents.

In order for NavigationAgents to use the avoidance feature the `avoidance_enabled` property must be set to `true`.

The `velocity_computed` signal of the NavigationAgent node must be connected to receive the safe velocity calculation result.

Set the `Velocity` of the NavigationAgent node in `_PhysicsProcess()` to update the agent with the current velocity of the agent's parent node.

While avoidance is enabled on the agent the `safe_velocity` vector will be received with the `VelocityComputed` signal every physics frame. This velocity vector should be used to move the NavigationAgent's parent node in order to avoid collision with other avoidance using agents or avoidance obstacles.

> **Note:** Only other agents on the same map that are registered for avoidance themselves will be considered in the avoidance calculation.

The following NavigationAgent properties are relevant for avoidance:

- The property `height` is available in 3D only. The height together with the current global y-axis position of the agent determines the vertical placement of the agent in the avoidance simulation. Agents using the 2D avoidance will automatically ignore other agents or obstacles that are below or above them.
- The property `radius` controls the size of the avoidance circle, or in case of 3D sphere, around the agent. This area describes the agents body and not the avoidance maneuver distance.
- The property `neighbor_distance` controls the search radius of the agent when searching for other agents that should be avoided. A lower value reduces processing cost.
- The property `max_neighbors` controls how many other agents are considered in the avoidance calculation if they all have overlapping radius. A lower value reduces processing cost but a too low value may result in agents ignoring the avoidance.
- The properties `time_horizon_agents` and `time_horizon_obstacles` control the avoidance prediction time for other agents or obstacles in seconds. When agents calculate their safe velocities they choose velocities that can be kept for this amount of seconds without colliding with another avoidance object. The prediction time should be kept as low as possible as agents will slow down their velocities to avoid collision in that timeframe.
- The property `max_speed` controls the maximum velocity allowed for the agents avoidance calculation. If the agents parents moves faster than this value the avoidance `safe_velocity` might not be accurate enough to avoid collision.
- The property `use_3d_avoidance` switches the agent between the 2D avoidance (xz axis) and the 3D avoidance (xyz axis) on the next update. Note that 2D avoidance and 3D avoidance run in separate avoidance simulations so agents split between them do not affect each other.
- The properties `avoidance_layers` and `avoidance_mask` are bitmasks similar to e.g. physics layers. Agents will only avoid other avoidance objects that are on an avoidance layer that matches at least one of their own avoidance mask bits.
- The `avoidance_priority` makes agents with a higher priority ignore agents with a lower priority. This can be used to give certain agents more importance in the avoidance simulation, e.g. important non-playable characters, without constantly changing their entire avoidance layers or mask.

Avoidance exists in its own space and has no information from navigation meshes or physics collision. Behind the scene avoidance agents are just circles with different radius on a flat 2D plane or spheres in an otherwise empty 3D space. NavigationObstacles can be used to add some environment constraints to the avoidance simulation.

> **Note:** Avoidance does not affect the pathfinding. It should be seen as an additional option for constantly moving objects that cannot be (re)baked to a navigation mesh efficiently in order to move around them.

> **Note:** RVO avoidance makes implicit assumptions about natural agent behavior. E.g. that agents move on reasonable passing sides that can be assigned when they encounter each other. This means that very clinical avoidance test scenarios will commonly fail. E.g. agents moved directly against each other with perfect opposite velocities will fail because the agents can not get their passing sides assigned.

Using the NavigationAgent `avoidance_enabled` property is the preferred option to toggle avoidance. The following code snippets can be used to toggle avoidance on agents, create or delete avoidance callbacks or switch avoidance modes.

**2D:**

```csharp
using Godot;

public partial class MyNavigationAgent2D : NavigationAgent2D
{
    public override void _Ready()
    {
        Rid agent = GetRid();
        // Enable avoidance
        NavigationServer2D.AgentSetAvoidanceEnabled(agent, true);
        // Create avoidance callback
        NavigationServer2D.AgentSetAvoidanceCallback(agent, Callable.From(AvoidanceDone));

        // Disable avoidance
        NavigationServer2D.AgentSetAvoidanceEnabled(agent, false);
        // Delete avoidance callback
        NavigationServer2D.AgentSetAvoidanceCallback(agent, default);
    }

    private void AvoidanceDone() { }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNavigationAgent3D : NavigationAgent3D
{
    public override void _Ready()
    {
        Rid agent = GetRid();
        // Enable avoidance
        NavigationServer3D.AgentSetAvoidanceEnabled(agent, true);
        // Create avoidance callback
        NavigationServer3D.AgentSetAvoidanceCallback(agent, Callable.From(AvoidanceDone));
        // Switch to 3D avoidance
        NavigationServer3D.AgentSetUse3DAvoidance(agent, true);

        // Disable avoidance
        NavigationServer3D.AgentSetAvoidanceEnabled(agent, false);
        // Delete avoidance callback
        NavigationServer3D.AgentSetAvoidanceCallback(agent, default);
        // Switch to 2D avoidance
        NavigationServer3D.AgentSetUse3DAvoidance(agent, false);
    }

    private void AvoidanceDone() { }
}
```

### NavigationAgent Script Templates

The following sections provide script templates for nodes commonly used with NavigationAgents.

#### 2D Node2D

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent2D _navigationAgent;
    private float _movementDelta;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector2 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer2D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        _movementDelta = MovementSpeed * (float)delta;
        Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector2 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * _movementDelta;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + safeVelocity, _movementDelta);
    }
}
```

#### 2D CharacterBody2D

```csharp
using Godot;

public partial class MyCharacterBody2D : CharacterBody2D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent2D _navigationAgent;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector2 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer2D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector2 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
    }
}
```

#### 2D RigidBody2D

```csharp
using Godot;

public partial class MyRigidBody2D : RigidBody2D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent2D _navigationAgent;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector2 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer2D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector2 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        LinearVelocity = safeVelocity;
    }
}
```

#### 3D Node3D

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent3D _navigationAgent;
    private float _movementDelta;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector3 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        _movementDelta = MovementSpeed * (float)delta;
        Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * _movementDelta;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + safeVelocity, _movementDelta);
    }
}
```

#### 3D CharacterBody3D

```csharp
using Godot;

public partial class MyCharacterBody3D : CharacterBody3D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent3D _navigationAgent;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector3 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
    }
}
```

#### 3D RigidBody3D

```csharp
using Godot;

public partial class MyRigidBody3D : RigidBody3D
{
    [Export]
    public float MovementSpeed { get; set; } = 4.0f;
    NavigationAgent3D _navigationAgent;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    private void SetMovementTarget(Vector3 movementTarget)
    {
        _navigationAgent.TargetPosition = movementTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
        {
            return;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            return;
        }

        Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
        Vector3 newVelocity = GlobalPosition.DirectionTo(nextPathPosition) * MovementSpeed;
        if (_navigationAgent.AvoidanceEnabled)
        {
            _navigationAgent.Velocity = newVelocity;
        }
        else
        {
            OnVelocityComputed(newVelocity);
        }
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        LinearVelocity = safeVelocity;
    }
}
```

---

## 6. Using NavigationObstacles

2D and 3D versions of NavigationObstacles nodes are available as `NavigationObstacle2D` and `NavigationObstacle3D` respectively.

Navigation obstacles are dual purpose in that they can affect both the navigation mesh baking, and the agent avoidance.

- With `affect_navigation_mesh` enabled the obstacle will affect navigation mesh when baked.
- With `avoidance_enabled` the obstacle will affect avoidance agents.

> **Tip:** Avoidance is enabled by default. If the obstacle is not used for avoidance disable `avoidance_enabled` to save performance.

### Obstacles and Navigation Mesh

For navigation mesh baking, obstacles can be used to discard parts of all other source geometry inside the obstacle shape.

This can be used to stop navigation meshes being baked in unwanted places, e.g. inside "solid" geometry like thick walls or on top of other geometry that should not be included for gameplay like roofs.

An obstacle does not add geometry in the baking process, it only removes geometry. It does so by nullifying all the (voxel) cells with rasterized source geometry that are within the obstacle shape. As such its effect and shape detail is limited to the cell resolution used by the baking process.

The property `affect_navigation_mesh` makes the obstacle contribute to the navigation mesh baking. It will be parsed or unparsed like all other node objects in a navigation mesh baking process.

The `carve_navigation_mesh` property makes the shape unaffected by offsets of the baking, e.g. the offset added by the navigation mesh `agent_radius`. It will basically act as a stencil and cut into the already offset navigation mesh surface. It will still be affected by further postprocessing of the baking process like edge simplification.

The obstacle shape and placement is defined with the `height` and `vertices` properties, and the `global_position` of the obstacle. The y-axis value of any Vector3 used for the vertices is ignored as the obstacle is projected on a flat horizontal plane.

When baking navigation meshes in scripts obstacles can be added procedurally as a projected obstruction. Obstacles are not involved in the source geometry parsing so adding them just before baking is enough.

**2D:**

```csharp
Vector2[] obstacleOutline =
[
    new Vector2(-50, -50),
    new Vector2(50, -50),
    new Vector2(50, 50),
    new Vector2(-50, 50),
];

var navigationMesh = new NavigationPolygon();
var sourceGeometry = new NavigationMeshSourceGeometryData2D();

NavigationServer2D.ParseSourceGeometryData(navigationMesh, sourceGeometry, GetNode<Node2D>("MyTestRootNode"));

bool obstacleCarve = true;

sourceGeometry.AddProjectedObstruction(obstacleOutline, obstacleCarve);
NavigationServer2D.BakeFromSourceGeometryData(navigationMesh, sourceGeometry);
```

**3D:**

```csharp
Vector3[] obstacleOutline =
[
    new Vector3(-5, 0, -5),
    new Vector3(5, 0, -5),
    new Vector3(5, 0, 5),
    new Vector3(-5, 0, 5),
];

var navigationMesh = new NavigationMesh();
var sourceGeometry = new NavigationMeshSourceGeometryData3D();

NavigationServer3D.ParseSourceGeometryData(navigationMesh, sourceGeometry, GetNode<Node3D>("MyTestRootNode"));

float obstacleElevation = GetNode<Node3D>("MyTestObstacleNode").GlobalPosition.Y;
float obstacleHeight = 50.0f;
bool obstacleCarve = true;

sourceGeometry.AddProjectedObstruction(obstacleOutline, obstacleElevation, obstacleHeight, obstacleCarve);
NavigationServer3D.BakeFromSourceGeometryData(navigationMesh, sourceGeometry);
```

### Obstacles and Agent Avoidance

For avoidance navigation obstacles can be used either as static or dynamic obstacles to affect avoidance controlled agents.

- When used statically NavigationObstacles constrain avoidance controlled agents outside or inside a polygon defined area.
- When used dynamically NavigationObstacles push away avoidance controlled agents in a radius around them.

#### Static Avoidance Obstacles

An avoidance obstacle is considered static when its `vertices` property is populated with an outline array of positions to form a polygon.

- Static obstacles act as hard do-not-cross boundaries for avoidance using agents, e.g. similar to physics collision but for avoidance.
- Static obstacles define their boundaries with an array of outline `vertices` (positions), and in case of 3D with an additional `height` property.
- Static obstacles only work for agents that use the 2D avoidance mode.
- Static obstacles define through winding order of the vertices if agents are pushed out or sucked in.
- Static obstacles can not change their position. They can only be warped to a new position and rebuilt from scratch. Static obstacles as a result are ill-suited for usages where the position is changed every frame, as the constant rebuild has a high performance cost.
- Static obstacles that are warped to another position can not be predicted by agents. This creates the risk of getting agents stuck should a static obstacle be warped on top of agents.

When the 2D avoidance is used in 3D the y-axis of Vector3 vertices is ignored. Instead, the global y-axis position of the obstacle is used as the elevation level. Agents will ignore static obstacles in 3D that are below or above them. This is automatically determined by global y-axis position of both obstacle and agent as the elevation level as well as their respective height properties.

#### Dynamic Avoidance Obstacles

An avoidance obstacle is considered dynamic when its `radius` property is greater than zero.

- Dynamic obstacles act as a soft please-move-away-from-me object for avoidance using agents, e.g. similar to how they avoid other agents.
- Dynamic obstacles define their boundaries with a single `radius` for a 2D circle, or in case of 3D avoidance a sphere shape.
- Dynamic obstacles can change their position every frame without additional performance cost.
- Dynamic obstacles with a set velocity can be predicted in their movement by agents.
- Dynamic obstacles are not a reliable way to constrain agents in crowded or narrow spaces.

While both static and dynamic properties can be active at the same time on the same obstacle this is not recommended for performance. Ideally when an obstacle is moving the static vertices are removed and instead the radius activated. When the obstacle reaches the new final position it should gradually enlarge its radius to push all other agents away. With enough created safe space around the obstacle it should add the static vertices again and remove the radius. This helps avoid getting agents stuck in the suddenly appearing static obstacle when the rebuilt static boundary is finished.

Similar to agents the obstacles can make use of the `avoidance_layers` bitmask. All agents with a matching bit on their own avoidance mask will avoid the obstacle.

### Procedural Obstacles

New obstacles can be created in a script without a Node by using the NavigationServer directly.

Obstacles created with scripts require at least a `map` and a `position`. For dynamic use a `radius` is required. For static use an array of `vertices` is required.

**2D:**

```csharp
// Create a new "obstacle" and place it on the default navigation map.
Rid newObstacleRid = NavigationServer2D.ObstacleCreate();
Rid defaultMapRid = GetWorld2D().NavigationMap;

NavigationServer2D.ObstacleSetMap(newObstacleRid, defaultMapRid);
NavigationServer2D.ObstacleSetPosition(newObstacleRid, GlobalPosition);

// Use obstacle dynamic by increasing radius above zero.
NavigationServer2D.ObstacleSetRadius(newObstacleRid, 5.0f);

// Use obstacle static by adding a square that pushes agents out.
Vector2[] outline =
[
    new Vector2(-100, -100),
    new Vector2(100, -100),
    new Vector2(100, 100),
    new Vector2(-100, 100),
];
NavigationServer2D.ObstacleSetVertices(newObstacleRid, outline);

// Enable the obstacle.
NavigationServer2D.ObstacleSetAvoidanceEnabled(newObstacleRid, true);
```

**3D:**

```csharp
// Create a new "obstacle" and place it on the default navigation map.
Rid newObstacleRid = NavigationServer3D.ObstacleCreate();
Rid defaultMapRid = GetWorld3D().NavigationMap;

NavigationServer3D.ObstacleSetMap(newObstacleRid, defaultMapRid);
NavigationServer3D.ObstacleSetPosition(newObstacleRid, GlobalPosition);

// Use obstacle dynamic by increasing radius above zero.
NavigationServer3D.ObstacleSetRadius(newObstacleRid, 5.0f);

// Use obstacle static by adding a square that pushes agents out.
Vector3[] outline =
[
    new Vector3(-5, 0, -5),
    new Vector3(5, 0, -5),
    new Vector3(5, 0, 5),
    new Vector3(-5, 0, 5),
];
NavigationServer3D.ObstacleSetVertices(newObstacleRid, outline);
// Set the obstacle height on the y-axis.
NavigationServer3D.ObstacleSetHeight(newObstacleRid, 1.0f);

// Enable the obstacle.
NavigationServer3D.ObstacleSetAvoidanceEnabled(newObstacleRid, true);
```

---

## 7. Using NavigationLinks

NavigationLinks are used to connect navigation mesh polygons from `NavigationRegion2D` and `NavigationRegion3D` over arbitrary distances for pathfinding.

NavigationLinks are also used to consider movement shortcuts in pathfinding available through interacting with gameplay objects e.g. ladders, jump pads or teleports.

2D and 3D versions of NavigationLink nodes are available as `NavigationLink2D` and `NavigationLink3D` respectively.

Different NavigationRegions can connect their navigation meshes without the need for a NavigationLink as long as they have overlapping edges or edges that are within navigation map `edge_connection_margin`. As soon as the distance becomes too large, building valid connections becomes a problem - a problem that NavigationLinks can solve.

NavigationLinks share many properties with NavigationRegions like `navigation_layers`. NavigationLinks add a single connection between two positions over an arbitrary distance compared to NavigationRegions that add a more local traversable area with a navigation mesh resource.

NavigationLinks have a `start_position` and `end_position` and can go in both directions when `bidirectional` is enabled. When placed a NavigationLink connects the navigation mesh polygons closest to its `start_position` and `end_position` within search radius for pathfinding.

The polygon search radius can be configured globally in the ProjectSettings under `navigation/2d_or_3d/default_link_connection_radius` or set for each navigation **map** individually using the `NavigationServer.MapSetLinkConnectionRadius()` function.

Both `start_position` and `end_position` have debug markers in the Editor. The arrows indicate which direction the link can be travelled across, and the visible radius of a position shows the polygon search radius. All navigation mesh polygons inside are compared and the closest is picked for the edge connection. If no valid polygon is found within the search radius the navigation link gets disabled.

The link debug visuals can be changed in the Editor ProjectSettings under `debug/shapes/navigation`. The visibility of the debug can also be controlled in the Editor 3D Viewport gizmo menu.

A navigation link does not provide any specialized movement through the link. Instead, when an agent reaches the position of a link, game code needs to react (e.g. through area triggers) and provide means for the agent to move through the link to end up at the links other position (e.g. through teleport or animation). Without that an agent will attempt to move itself along the path of the link. You could end up with an agent walking over a bottomless pit instead of waiting for a moving platform, or walking through a teleporter and proceeding through a wall.

### Navigation Link Script Templates

The following script uses the NavigationServer to create a new navigation link.

**2D:**

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    private Rid _linkRid;
    private Vector2 _linkStartPosition;
    private Vector2 _linkEndPosition;

    public override void _Ready()
    {
        _linkRid = NavigationServer2D.LinkCreate();

        ulong linkOwnerId = GetInstanceId();
        float linkEnterCost = 1.0f;
        float linkTravelCost = 1.0f;
        uint linkNavigationLayers = 1;
        bool linkBidirectional = true;

        NavigationServer2D.LinkSetOwnerId(_linkRid, linkOwnerId);
        NavigationServer2D.LinkSetEnterCost(_linkRid, linkEnterCost);
        NavigationServer2D.LinkSetTravelCost(_linkRid, linkTravelCost);
        NavigationServer2D.LinkSetNavigationLayers(_linkRid, linkNavigationLayers);
        NavigationServer2D.LinkSetBidirectional(_linkRid, linkBidirectional);

        // Enable the link and set it to the default navigation map.
        NavigationServer2D.LinkSetEnabled(_linkRid, true);
        NavigationServer2D.LinkSetMap(_linkRid, GetWorld2D().NavigationMap);

        // Move the 2 link positions to their intended global positions.
        NavigationServer2D.LinkSetStartPosition(_linkRid, _linkStartPosition);
        NavigationServer2D.LinkSetEndPosition(_linkRid, _linkEndPosition);
    }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    private Rid _linkRid;
    private Vector3 _linkStartPosition;
    private Vector3 _linkEndPosition;

    public override void _Ready()
    {
        _linkRid = NavigationServer3D.LinkCreate();

        ulong linkOwnerId = GetInstanceId();
        float linkEnterCost = 1.0f;
        float linkTravelCost = 1.0f;
        uint linkNavigationLayers = 1;
        bool linkBidirectional = true;

        NavigationServer3D.LinkSetOwnerId(_linkRid, linkOwnerId);
        NavigationServer3D.LinkSetEnterCost(_linkRid, linkEnterCost);
        NavigationServer3D.LinkSetTravelCost(_linkRid, linkTravelCost);
        NavigationServer3D.LinkSetNavigationLayers(_linkRid, linkNavigationLayers);
        NavigationServer3D.LinkSetBidirectional(_linkRid, linkBidirectional);

        // Enable the link and set it to the default navigation map.
        NavigationServer3D.LinkSetEnabled(_linkRid, true);
        NavigationServer3D.LinkSetMap(_linkRid, GetWorld3D().NavigationMap);

        // Move the 2 link positions to their intended global positions.
        NavigationServer3D.LinkSetStartPosition(_linkRid, _linkStartPosition);
        NavigationServer3D.LinkSetEndPosition(_linkRid, _linkEndPosition);
    }
}
```

---

## 8. Using NavigationLayers

NavigationLayers are an optional feature to further control which navigation meshes are considered in a path query. They work similar to how physics layers control collision between collision objects or how visual layers control what is rendered to the Viewport.

NavigationLayers can be named in the **ProjectSettings** the same as physics layers or visual layers.

If a region has not a single compatible navigation layer with the `navigation_layers` parameter of a path query this regions navigation mesh will be skipped in pathfinding.

NavigationLayers are a single `int` value that is used as a **bitmask**. Many navigation related nodes have `SetNavigationLayerValue()` and `GetNavigationLayerValue()` functions to set and get a layer number directly without the need for more complex bitwise operations.

In scripts the following helper functions can be used to work with the `navigation_layers` bitmask.

**2D:**

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    private Rid _map;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    private void ChangeLayers()
    {
        NavigationRegion2D region = GetNode<NavigationRegion2D>("NavigationRegion2D");
        // Enables the 4th layer for this region.
        region.NavigationLayers = EnableBitmaskInx(region.NavigationLayers, 4);
        // Disables the 1st layer for this region.
        region.NavigationLayers = DisableBitmaskInx(region.NavigationLayers, 1);

        NavigationAgent2D agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        // Make future path queries of this agent ignore regions with the 4th layer.
        agent.NavigationLayers = DisableBitmaskInx(agent.NavigationLayers, 4);

        uint pathQueryNavigationLayers = 0;
        pathQueryNavigationLayers = EnableBitmaskInx(pathQueryNavigationLayers, 2);
        // Get a path that only considers 2nd layer regions.
        Vector2[] path = NavigationServer2D.MapGetPath(
            _map,
            _startPosition,
            _targetPosition,
            true,
            pathQueryNavigationLayers
        );
    }

    private static bool IsBitmaskInxEnabled(uint bitmask, int index)
    {
        return (bitmask & (1 << index)) != 0;
    }

    private static uint EnableBitmaskInx(uint bitmask, int index)
    {
        return bitmask | (1u << index);
    }

    private static uint DisableBitmaskInx(uint bitmask, int index)
    {
        return bitmask & ~(1u << index);
    }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    private Rid _map;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private void ChangeLayers()
    {
        NavigationRegion3D region = GetNode<NavigationRegion3D>("NavigationRegion3D");
        // Enables the 4th layer for this region.
        region.NavigationLayers = EnableBitmaskInx(region.NavigationLayers, 4);
        // Disables the 1st layer for this region.
        region.NavigationLayers = DisableBitmaskInx(region.NavigationLayers, 1);

        NavigationAgent3D agent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        // Make future path queries of this agent ignore regions with the 4th layer.
        agent.NavigationLayers = DisableBitmaskInx(agent.NavigationLayers, 4);

        uint pathQueryNavigationLayers = 0;
        pathQueryNavigationLayers = EnableBitmaskInx(pathQueryNavigationLayers, 2);
        // Get a path that only considers 2nd layer regions.
        Vector3[] path = NavigationServer3D.MapGetPath(
            _map,
            _startPosition,
            _targetPosition,
            true,
            pathQueryNavigationLayers
        );
    }

    private static bool IsBitmaskInxEnabled(uint bitmask, int index)
    {
        return (bitmask & (1 << index)) != 0;
    }

    private static uint EnableBitmaskInx(uint bitmask, int index)
    {
        return bitmask | (1u << index);
    }

    private static uint DisableBitmaskInx(uint bitmask, int index)
    {
        return bitmask & ~(1u << index);
    }
}
```

Changing navigation layers for path queries is a performance friendly alternative to enabling / disabling entire navigation regions. Compared to region changes a navigation path query with different navigation layers does not trigger large scale updates on the NavigationServer.

Changing the navigation layers of NavigationAgent nodes will have an immediate effect on the next path query. Changing the navigation layers of regions will have an effect after the next NavigationServer sync.

---

## 9. Connecting Navigation Meshes

Different NavigationMeshes are automatically merged by the NavigationServer when at least two vertex positions of one edge exactly overlap.

To connect over arbitrary distances see the NavigationLinks section.

The same is true for multiple NavigationPolygon resources. As long as their outline points overlap exactly the NavigationServer will merge them. NavigationPolygon outlines must be from different NavigationPolygon resources to connect.

Overlapping or intersecting outlines on the same NavigationPolygon will fail the navigation mesh creation. Overlapping or intersecting outlines from different NavigationPolygons will often fail to create the navigation region edge connections on the NavigationServer and should be avoided.

> **Warning:** Exactly means exactly for the vertex position merge. Small float errors that happen quite regularly with imported meshes will prevent a successful vertex merge.

Alternatively navigation meshes are not merged but still considered as **connected** by the NavigationServer when their edges are nearly parallel and within distance to each other. The connection distance is defined by the `edge_connection_margin` for each navigation map. In many cases navigation mesh edges cannot properly connect when they partly overlap. Better avoid any navigation mesh overlap at all time for a consistent merge behavior.

If navigation debug is enabled and the NavigationServer active the established navigation mesh connections will be visualized.

The default 2D `edge_connection_margin` can be changed in the ProjectSettings under `navigation/2d/default_edge_connection_margin`.

The default 3D `edge_connection_margin` can be changed in the ProjectSettings under `navigation/3d/default_edge_connection_margin`.

The edge connection margin value of any navigation map can also be changed at runtime with the NavigationServer API.

**2D:**

```csharp
using Godot;

public partial class MyNode2D : Node2D
{
    public override void _Ready()
    {
        // 2D margins are designed to work with 2D "pixel" values.
        Rid defaultMapRid = GetWorld2D().NavigationMap;
        NavigationServer2D.MapSetEdgeConnectionMargin(defaultMapRid, 50.0f);
    }
}
```

**3D:**

```csharp
using Godot;

public partial class MyNode3D : Node3D
{
    public override void _Ready()
    {
        // 3D margins are designed to work with 3D world unit values.
        Rid defaultMapRid = GetWorld3D().NavigationMap;
        NavigationServer3D.MapSetEdgeConnectionMargin(defaultMapRid, 0.5f);
    }
}
```

> **Note:** Changing the edge connection margin will trigger a full update of all navigation mesh connections on the NavigationServer.

---

## 10. Support Different Actor Types

To support different actor types due to e.g. their sizes each type requires its own navigation map and navigation mesh baked with an appropriated agent radius and height. The same approach can be used to distinguish between e.g. landwalking, swimming or flying agents.

> **Note:** Agents are exclusively defined by a radius and height value for baking navigation meshes, pathfinding and avoidance. More complex shapes are not supported.

```csharp
// Create a navigation mesh resource for each actor size.
NavigationMesh navigationMeshStandardSize = new NavigationMesh();
NavigationMesh navigationMeshSmallSize = new NavigationMesh();
NavigationMesh navigationMeshHugeSize = new NavigationMesh();

// Set appropriated agent parameters.
navigationMeshStandardSize.AgentRadius = 0.5f;
navigationMeshStandardSize.AgentHeight = 1.8f;
navigationMeshSmallSize.AgentRadius = 0.25f;
navigationMeshSmallSize.AgentHeight = 0.7f;
navigationMeshHugeSize.AgentRadius = 1.5f;
navigationMeshHugeSize.AgentHeight = 2.5f;

// Get the root node to parse geometry for the baking.
Node3D rootNode = GetNode<Node3D>("NavigationMeshBakingRootNode");

// Create the source geometry resource that will hold the parsed geometry data.
NavigationMeshSourceGeometryData3D sourceGeometryData = new NavigationMeshSourceGeometryData3D();

// Parse the source geometry from the scene tree on the main thread.
// The navigation mesh is only required for the parse settings so any of the three will do.
NavigationServer3D.ParseSourceGeometryData(navigationMeshStandardSize, sourceGeometryData, rootNode);

// Bake the navigation geometry for each agent size from the same source geometry.
// If required for performance this baking step could also be done on background threads.
NavigationServer3D.BakeFromSourceGeometryData(navigationMeshStandardSize, sourceGeometryData);
NavigationServer3D.BakeFromSourceGeometryData(navigationMeshSmallSize, sourceGeometryData);
NavigationServer3D.BakeFromSourceGeometryData(navigationMeshHugeSize, sourceGeometryData);

// Create different navigation maps on the NavigationServer.
Rid navigationMapStandard = NavigationServer3D.MapCreate();
Rid navigationMapSmall = NavigationServer3D.MapCreate();
Rid navigationMapHuge = NavigationServer3D.MapCreate();

// Set the new navigation maps as active.
NavigationServer3D.MapSetActive(navigationMapStandard, true);
NavigationServer3D.MapSetActive(navigationMapSmall, true);
NavigationServer3D.MapSetActive(navigationMapHuge, true);

// Create a region for each map.
Rid navigationRegionStandard = NavigationServer3D.RegionCreate();
Rid navigationRegionSmall = NavigationServer3D.RegionCreate();
Rid navigationRegionHuge = NavigationServer3D.RegionCreate();

// Add the regions to the maps.
NavigationServer3D.RegionSetMap(navigationRegionStandard, navigationMapStandard);
NavigationServer3D.RegionSetMap(navigationRegionSmall, navigationMapSmall);
NavigationServer3D.RegionSetMap(navigationRegionHuge, navigationMapHuge);

// Set navigation mesh for each region.
NavigationServer3D.RegionSetNavigationMesh(navigationRegionStandard, navigationMeshStandardSize);
NavigationServer3D.RegionSetNavigationMesh(navigationRegionSmall, navigationMeshSmallSize);
NavigationServer3D.RegionSetNavigationMesh(navigationRegionHuge, navigationMeshHugeSize);

// Create start and end position for the navigation path query.
Vector3 startPos = new Vector3(0.0f, 0.0f, 0.0f);
Vector3 endPos = new Vector3(2.0f, 0.0f, 0.0f);
bool useCorridorFunnel = true;

// Query paths for each agent size.
var pathStandardAgent = NavigationServer3D.MapGetPath(navigationMapStandard, startPos, endPos, useCorridorFunnel);
var pathSmallAgent = NavigationServer3D.MapGetPath(navigationMapSmall, startPos, endPos, useCorridorFunnel);
var pathHugeAgent = NavigationServer3D.MapGetPath(navigationMapHuge, startPos, endPos, useCorridorFunnel);
```

---

## 11. Support Different Actor Locomotion

To support different actor locomotion like crouching and crawling, a similar map setup as supporting different actor types is required.

Bake different navigation meshes with an appropriate height for crouched or crawling actors so they can find paths through those narrow sections in your game world.

When an actor changes locomotion state, e.g. stands up, starts crouching or crawling, query the appropriate map for a path.

If the avoidance behavior should also change with the locomotion e.g. only avoid while standing or only avoid other agents in the same locomotion state, switch the actor's avoidance agent to another avoidance map with each locomotion change.

```csharp
private void UpdatePath()
{
    if (_actorStanding)
    {
        _path = NavigationServer3D.MapGetPath(_standingNavigationMapRid, _startPosition, _targetPosition, true);
    }
    else if (_actorCrouching)
    {
        _path = NavigationServer3D.MapGetPath(_crouchedNavigationMapRid, _startPosition, _targetPosition, true);
    }
    else if (_actorCrawling)
    {
        _path = NavigationServer3D.MapGetPath(_crawlingNavigationMapRid, _startPosition, _targetPosition, true);
    }
}

private void ChangeAgentAvoidanceState()
{
    if (_actorStanding)
    {
        NavigationServer3D.AgentSetMap(_avoidanceAgentRid, _standingNavigationMapRid);
    }
    else if (_actorCrouching)
    {
        NavigationServer3D.AgentSetMap(_avoidanceAgentRid, _crouchedNavigationMapRid);
    }
    else if (_actorCrawling)
    {
        NavigationServer3D.AgentSetMap(_avoidanceAgentRid, _crawlingNavigationMapRid);
    }
}
```

> **Note:** While a path query can be executed immediately for multiple maps, the avoidance agent map switch will only take effect after the next server synchronization.

---

## 12. Navigation Debug Tools

> **Note:** The debug tools, properties and functions are only available in Godot debug builds. Do not use any of them in code that will be part of a release build.

### Enabling Navigation Debug

The navigation debug visualizations are enabled by default inside the editor. To visualize navigation meshes and connections at runtime too, enable the option **Visible Navigation** in the editor **Debug** menu.

In Godot debug builds the navigation debug can also be toggled through the NavigationServer singletons from scripts.

```csharp
NavigationServer2D.SetDebugEnabled(false);
NavigationServer3D.SetDebugEnabled(true);
```

Debug visualizations are currently based on Nodes in the SceneTree. If the `NavigationServer2D` or `NavigationServer3D` APIs are used exclusively then changes will not be reflected by the debug navigation tools.

### Navigation Debug Settings

The appearance of navigation debug can be changed in the ProjectSettings under `debug/shapes/navigation`. Certain debug features can also be enabled or disabled at will but may require a scene restart to take effect.

### Debug Navigation Mesh Polygons

If `enable_edge_lines` is enabled, the edges of navigation mesh polygons will be highlighted. If `enable_edge_lines_xray` is also enabled, the edges of navigation meshes will be visible through geometry.

If `enable_geometry_face_random_color` is enabled, the color of each navigation mesh face will be mixed with a random color that is itself mixed with the color specified in `geometry_face_color`.

### Debug Edge Connections

When two navigation meshes are connected within `edge_connection_margin` distance, the connection is overlaid. The color of the overlay is controlled by `edge_connection_color`. The connections can be made visible through geometry with `enable_edge_connections_xray`.

> **Note:** Edge connections are only visible when the NavigationServer is active.

### Debug Performance

To measure NavigationServer performance a dedicated monitor exists that can be found within the Editor Debugger under **Debugger -> Monitors -> Navigation Process**.

Navigation Process shows how long the NavigationServer spends updating its internals this update frame in milliseconds. Navigation Process works similar to Process for visual frame rendering and Physics Process for collision and fixed updates.

Navigation Process accounts for all updates to **navigation maps**, **navigation regions** and **navigation agents** as well as all the **avoidance calculations** for the update frame.

> **Note:** Navigation Process does NOT include pathfinding performance cause pathfinding operates on the navigation map data independently from the server process update.

Navigation Process should be in general kept as low and as stable as possible for runtime performance to avoid frame rate issues. Note that since the NavigationServer process update happens in the middle of the physics update an increase in Navigation Process will automatically increase Physics Process by the same amount.

Navigation also provides more detailed statistics about the current navigation related objects and navigation map composition on the NavigationServer.

Navigation statistics shown here can not be judged as good or bad for performance as it depends entirely on the project what can be considered as reasonable or horribly excessive.

Navigation statistics help with identifying performance bottlenecks that are less obvious because the source might not always have a visible representation. E.g. pathfinding performance issues created by overly detailed navigation meshes with thousand of edges / polygons or problems caused by procedural navigation gone wrong.
