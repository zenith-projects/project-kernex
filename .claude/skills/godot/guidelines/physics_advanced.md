# Physics System — Advanced Reference

> Extracted from Godot 4.6 official documentation. All code examples converted to C#.

---

## Table of Contents

1. [Using RigidBody](#1-using-rigidbody)
2. [Using Area2D](#2-using-area2d)
3. [Ragdoll System](#3-ragdoll-system)
4. [Using SoftBody3D](#4-using-softbody3d)
5. [Collision Shapes (2D)](#5-collision-shapes-2d)
6. [Collision Shapes (3D)](#6-collision-shapes-3d)
7. [Large World Coordinates](#7-large-world-coordinates)
8. [Physics Interpolation — Introduction](#8-physics-interpolation--introduction)
9. [Using Physics Interpolation](#9-using-physics-interpolation)
10. [Troubleshooting Physics Issues](#10-troubleshooting-physics-issues)
11. [Using Jolt Physics](#11-using-jolt-physics)

---

## 1. Using RigidBody

### What is a rigid body?

A rigid body is one that is directly controlled by the physics engine in order to simulate the behavior of physical objects. In order to define the shape of the body, it must have one or more `Shape3D` objects assigned. Note that setting the position of these shapes will affect the body's center of mass.

### How to control a rigid body

A rigid body's behavior can be altered by setting its properties, such as mass and weight. A physics material needs to be added to the rigid body to adjust its friction and bounce, and set if it's absorbent and/or rough. These properties can be set in the Inspector or via code. See `RigidBody3D` and `PhysicsMaterial` for the full list of properties and their effects.

There are several ways to control a rigid body's movement, depending on your desired application.

If you only need to place a rigid body once, for example to set its initial location, you can use the methods provided by the `Node3D` node, such as `SetGlobalTransform()` or `LookAt()`. However, these methods cannot be called every frame or the physics engine will not be able to correctly simulate the body's state. As an example, consider a rigid body that you want to rotate so that it points towards another object. A common mistake when implementing this kind of behavior is to use `LookAt()` every frame, which breaks the physics simulation. Below, we'll demonstrate how to implement this correctly.

The fact that you can't use `SetGlobalTransform()` or `LookAt()` methods doesn't mean that you can't have full control of a rigid body. Instead, you can control it by using the `_IntegrateForces()` callback. In this method, you can add *forces*, apply *impulses*, or set the *velocity* in order to achieve any movement you desire.

### The "look at" method

As described above, using the Node3D's `LookAt()` method can't be used each frame to follow a target. Here is a custom `LookAt()` method called `LookFollow()` that will work with rigid bodies:

```csharp
using Godot;

public partial class MyRigidBody3D : RigidBody3D
{
    private float _speed = 0.1f;

    private void LookFollow(PhysicsDirectBodyState3D state, Transform3D currentTransform, Vector3 targetPosition)
    {
        Vector3 forwardLocalAxis = new Vector3(1, 0, 0);
        Vector3 forwardDir = (currentTransform.Basis * forwardLocalAxis).Normalized();
        Vector3 targetDir = (targetPosition - currentTransform.Origin).Normalized();
        float localSpeed = Mathf.Clamp(_speed, 0.0f, Mathf.Acos(forwardDir.Dot(targetDir)));
        if (forwardDir.Dot(targetDir) > 1e-4)
        {
            state.AngularVelocity = forwardDir.Cross(targetDir) * localSpeed / state.Step;
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        Vector3 targetPosition = GetNode<Node3D>("MyTargetNode3DNode").GlobalTransform.Origin;
        LookFollow(state, GlobalTransform, targetPosition);
    }
}
```

This method uses the rigid body's `AngularVelocity` property to rotate the body. The axis to rotate around is given by the cross product between the current forward direction and the direction one wants to look in. The `Clamp` is a simple method used to prevent the amount of rotation from going past the direction which is wanted to be looked in, as the total amount of rotation needed is given by the arccosine of the dot product. This method can be used with `AxisLockAngular*` as well. If more precise control is needed, solutions such as ones relying on `Quaternion` may be required, as discussed in the transforms documentation.

---

## 2. Using Area2D

### Introduction

Godot offers a number of collision objects to provide both collision detection and response. Trying to decide which one to use for your project can be confusing. You can avoid problems and simplify development if you understand how each of them works and what their pros and cons are. In this tutorial, we'll look at the `Area2D` node and show some examples of how it can be used.

> **Note:** This document assumes you're familiar with Godot's various physics bodies. Please read the Physics Introduction first.

### What is an area?

An `Area2D` defines a region of 2D space. In this space you can detect other `CollisionObject2D` nodes overlapping, entering, and exiting. Areas also allow for overriding local physics properties. We'll explore each of these functions below.

### Area properties

Areas have many properties you can use to customize their behavior.

The **Gravity**, **Linear Damp**, and **Angular Damp** sections are used to configure the area's physics override behavior. We'll look at how to use those in the *Area influence* section below.

**Monitoring** and **Monitorable** are used to enable and disable the area.

The **Audio Bus** section allows you to override audio in the area, for example to apply an audio effect when the player moves through.

Note that `Area2D` extends `CollisionObject2D`, so it also provides properties inherited from that class. The **Collision** section of `CollisionObject2D` is where you configure the area's collision layer(s) and mask(s).

### Overlap detection

Perhaps the most common use of `Area2D` nodes is for contact and overlap detection. When you need to know that two objects have touched, but don't need physical collision, you can use an area to notify you of the contact.

For example, let's say we're making a coin for the player to pick up. The coin is not a solid object — the player can't stand on it or push it — we just want it to disappear when the player touches it.

Here's the node setup for the coin:

- Area2D (Coin)
  - CollisionShape2D

To detect the overlap, we'll connect the appropriate signal on the Area2D. Which signal to use depends on the player's node type. If the player is another area, use `area_entered`. However, let's assume our player is a `CharacterBody2D` (and therefore a `CollisionObject2D` type), so we'll connect the `body_entered` signal.

> **Note:** If you're not familiar with using signals, see the Signals documentation for an introduction.

```csharp
using Godot;

public partial class Coin : Area2D
{
    private void OnCoinBodyEntered(PhysicsBody2D body)
    {
        QueueFree();
    }
}
```

Now our player can collect the coins!

Some other usage examples:

- Areas are great for bullets and other projectiles that hit and deal damage, but don't need any other physics such as bouncing.
- Use a large circular area around an enemy to define its "detect" radius. When the player is outside the area, the enemy can't "see" it.
- "Security cameras" — In a large level with multiple cameras, attach areas to each camera and activate them when the player enters.

See the "Your First 2D Game" tutorial for an example of using Area2D in a game.

### Area influence

The second major use for area nodes is to alter physics. By default, the area won't do this, but you can enable this with the **Space Override** property. When areas overlap, they are processed in **Priority** order (higher priority areas are processed first). There are four options for override:

- **Combine** — The area adds its values to what has been calculated so far.
- **Replace** — The area replaces physics properties, and lower priority areas are ignored.
- **Combine-Replace** — The area adds its gravity/damping values to whatever has been calculated so far (in priority order), ignoring any lower priority areas.
- **Replace-Combine** — The area replaces any gravity/damping calculated so far, but keeps calculating the rest of the areas.

Using these properties, you can create very complex behavior with multiple overlapping areas.

The physics properties that can be overridden are:

- **Gravity** — Gravity's strength inside the area.
- **Gravity Direction** — This vector does not need to be normalized.
- **Linear Damp** — How quickly objects stop moving — linear velocity lost per second.
- **Angular Damp** — How quickly objects stop spinning — angular velocity lost per second.

### Point gravity

The **Gravity Point** property allows you to create an "attractor". Gravity in the area will be calculated towards a point, given by the **Point Center** property. Values are relative to the Area2D, so for example using `(0, 0)` will attract objects to the center of the area.

### Examples

The example project has three areas demonstrating physics override behavior. You can download this project here: [area_2d_starter.zip](https://github.com/godotengine/godot-docs-project-starters/releases/download/latest-4.x/area_2d_starter.zip)

---

## 3. Ragdoll System

### Introduction

Ragdoll physics uses physics simulation for realistic procedural animation, commonly used in death sequences. The system uses two primary nodes:

- **PhysicalBoneSimulator3D** — The parent node controlling the overall simulation.
- **PhysicalBone3D** — Child nodes representing individual bones with physics properties.

> **Note:** You can download the Platformer 3D demo on [GitHub](https://github.com/godotengine/godot-demo-projects/tree/master/3d/platformer) or using the [Asset Library](https://godotengine.org/asset-library/asset/2748). You can also check out an example of a complete ragdoll setup in the [Ragdoll Physics demo](https://github.com/godotengine/godot-demo-projects/tree/master/3d/ragdoll_physics).

### Setting up the ragdoll

The ragdoll system requires a `PhysicalBoneSimulator3D` parent node and `PhysicalBone3D` child nodes. The setup involves creating a physical skeleton from a `Skeleton3D` node via the editor button "Create Physical Skeleton" which automatically generates `PhysicalBone3D` nodes and collision shapes for each bone in the skeleton.

### Optimization

It is recommended to remove small or utility bones to reduce performance costs. For humanoid characters, consolidating finger bones into hand representations is suggested. Remove any bones that are not essential for the ragdoll effect — this includes things like facial bones, extra twist bones, and other deformation helpers.

### Joint configuration

After generating the physical skeleton, you need to configure the joints for each `PhysicalBone3D`. The available joint types are:

- **None** — No joint constraint.
- **PinJoint** — Allows rotation in all axes around a point.
- **ConeJoint** — Limits rotation to a cone shape (good for shoulders/hips).
- **HingeJoint** — Allows rotation around a single axis (good for elbows/knees).
- **SliderJoint** — Allows linear movement along a single axis.
- **6DOFJoint** — Six degrees of freedom for complex constraints.

Start with **HingeJoint** and **ConeJoint**, as they cover most use cases. Configure joint limits by selecting each `PhysicalBone3D` node and adjusting the joint parameters in the Inspector.

### Collision shapes

After configuring joints, adjust the collision shapes. Rotating joints also rotates collision geometry, so adjustments may be necessary. Resize and reposition the collision shapes to match the body parts accurately.

### Starting the simulation

To start the ragdoll simulation, call `PhysicalBonesStartSimulation()` on the `PhysicalBoneSimulator3D` node:

```csharp
using Godot;

public partial class MyCharacter : Node3D
{
    public override void _Ready()
    {
        GetNode<PhysicalBoneSimulator3D>("PhysicalBoneSimulator3D").PhysicalBonesStartSimulation();
    }
}
```

To stop the simulation:

```csharp
using Godot;

public partial class MyCharacter : Node3D
{
    public void StopRagdoll()
    {
        GetNode<PhysicalBoneSimulator3D>("PhysicalBoneSimulator3D").PhysicalBonesStopSimulation();
    }
}
```

### Partial ragdoll simulation

You can simulate only specific bones by passing bone names as parameters. This allows selective limb simulation while maintaining normal animation elsewhere:

```csharp
using Godot;

public partial class MyCharacter : Node3D
{
    public override void _Ready()
    {
        // Only simulate the left and right arms
        GetNode<PhysicalBoneSimulator3D>("PhysicalBoneSimulator3D")
            .PhysicalBonesStartSimulation(new StringName[] { "l-arm", "r-arm" });
    }
}
```

### Collision layer and mask configuration

Proper collision layer and mask configuration is essential to prevent the ragdoll from interfering with the character's movement capsule. Make sure to configure collision layers so that the player's `CharacterBody3D` capsule does not collide with the ragdoll's `PhysicalBone3D` shapes. You can do this by placing the ragdoll physics on a separate collision layer and excluding it from the character's collision mask.

---

## 4. Using SoftBody3D

Soft bodies (or *soft-body dynamics*) simulate movement, changing shape, and other physical properties of deformable objects. For example, this can be used to simulate cloth or to create more realistic characters.

### Physics engine considerations

Support for soft bodies is generally more robust in Jolt Physics compared to GodotPhysics3D. You can switch physics engines by changing **Physics > 3D > Physics Engine** in the Project Settings. Projects created in Godot 4.6 and later use Jolt Physics by default, but existing projects will have to be switched over manually.

Additionally, physics interpolation currently does not affect soft bodies. If you want soft body simulation to look smoother at higher framerates, you'll have to increase the **Physics > Common > Physics Ticks per Second** project setting, which comes at a performance cost.

### Basic setup

A `SoftBody3D` node is used for soft body simulations. Unlike other physics body nodes like `RigidBody3D` or `StaticBody3D`, it does **not** have a `CollisionShape3D` or a `MeshInstance3D` child node. Instead, the collision shape is derived from the mesh assigned to the node. This mesh is also directly used for rendering, which means you don't need to create any child nodes for a functional, visible setup.

We will create a bouncy cube to demonstrate the setup of a soft body.

Create a new scene with a `Node3D` node as root. Then, create a `SoftBody3D` node. Add a `BoxMesh` in the **Mesh** property of the node in the inspector and increase the subdivision of the mesh for simulation.

The subdivision level determines the precision level of the deformation, with higher values allowing for smaller and more detailed deformations, at the cost of performance. In this example, we'll set it to 3 on each axis.

Now, set the parameters to obtain the type of soft body you aim for. Try to keep the **Simulation Precision** above 5; otherwise, the soft body may collapse.

> **Note:** Handle some parameters with care, as some values can lead to strange results. For example, if the shape is not completely closed and you set pressure to a value greater than `0.0`, the soft body will fly around like a plastic bag under strong wind.

> **Tip:** To improve the simulation's result, increase the **Simulation Precision**. This can give a significant improvement at the cost of performance. Alternatively, you can increase the **Physics > Common > Physics Ticks per Second** project setting, which will also affect soft body simulation quality.

### Cloak simulation

Let's make a cloak in the Platformer 3D demo.

> **Note:** You can download the Platformer 3D demo on [GitHub](https://github.com/godotengine/godot-demo-projects/tree/master/3d/platformer) or the [Asset Library](https://godotengine.org/asset-library/asset/2748).

Open the `player/player.tscn` scene, add a `SoftBody3D` node below the root node, then assign a `PlaneMesh` resource to it in its **Mesh** property.

Open the PlaneMesh's properties and set the size to `(0.5, 1.0)`, then set **Subdivide Width** and **Subdivide Depth** to `5`. Adjust the SoftBody3D node's position and rotation so that the plane appears to be close to the character's back.

> **Tip:** Subdivision generates a more tessellated mesh for better simulations. However, higher subdivision levels will impact performance. Try to find a balance between performance and quality. This depends on the number of soft body simulations that you expect to be active at a given time, as well as the distance between the camera and the soft body.

Add a `BoneAttachment3D` node under the skeleton node and select the Neck bone to attach the cloak to the character skeleton.

> **Note:** The `BoneAttachment3D` node is used to attach objects to a bone of an armature. The attached object will follow the bone's movement. For example, a character's held weapon can be attached this way.
>
> Do **not** move the SoftBody3D node under the BoneAttachment3D node as of now. Instead, we'll configure its *pinned points* to follow the BoneAttachment3D node.

To create pinned points, select the upper vertices in the SoftBody3D node. A pinned point appears blue in the 3D editor viewport.

The pinned joints can be found in SoftBody3D's **Attachments** section, which is under the **Collision** section that must be expanded first. Choose the BoneAttachment3D node as the **Spatial Attachment Path** for each pinned joint. The pinned joints are now attached to the neck.

> **Tip:** To assign the properties faster, you can drag-and-drop the BoneAttachment3D node from the scene tree dock to the **Spatial Attachment Path** property field.

Note that you may have to deselect then reselect the SoftBody3D node for the **Attachments** section to appear.

The last step is to avoid clipping by adding the CharacterBody3D `Player` (the scene's root node) to the **Parent Collision Ignore** property of the SoftBody3D.

Play the scene and the cloak should simulate correctly.

This covers the basic settings of a soft body simulation. Experiment with the parameters to achieve the effect you are aiming for when making your game.

> **Note:** The cloak will not appear when viewed from certain angles due to backface culling. To resolve this, you can disable backface culling by assigning a new `StandardMaterial3D`, then setting its cull mode to **Disabled**. This will make the material render both sides of the plane.

### Using imported meshes

The **Save to File** option in the Advanced Import Settings dialog allows you to save a mesh to a standalone resource file that you can then attach to SoftBody3D nodes.

You may also want to disable LOD generation or change the LOD generation options when importing a mesh for use with SoftBody3D. The default import settings will produce an LOD that merges adjacent faces that are nearly flat with respect to each other, even at very close render distances. This works well for static meshes, but is often undesirable for use with SoftBody3D if you want these faces to be able to bend and move with respect to each other, instead of being rendered as a single plane.

See the "Importing 3D Scenes" and "Mesh LOD" documentation for more details.

---

## 5. Collision Shapes (2D)

This guide explains:

- The types of collision shapes available in 2D in Godot.
- Using an image converted to a polygon as a collision shape.
- Performance considerations regarding 2D collisions.

Godot provides many kinds of collision shapes, with different performance and accuracy tradeoffs.

You can define the shape of a `PhysicsBody2D` by adding one or more `CollisionShape2D` or `CollisionPolygon2D` nodes as *direct* child nodes. Indirect child nodes (i.e. children of child nodes) will be ignored and won't be used as collision shapes. Also, note that you must add a `Shape2D` *resource* to collision shape nodes in the Inspector dock.

> **Note:** When you add multiple collision shapes to a single PhysicsBody2D, you don't have to worry about them overlapping. They won't "collide" with each other.

### Primitive collision shapes

Godot provides the following primitive collision shape types:

- **RectangleShape2D**
- **CircleShape2D**
- **CapsuleShape2D**
- **SegmentShape2D**
- **SeparationRayShape2D** (designed for characters)
- **WorldBoundaryShape2D** (infinite plane)

You can represent the collision of most smaller objects using one or more primitive shapes. However, for more complex objects, such as a large ship or a whole level, you may need convex or concave shapes instead. More on that below.

We recommend favoring primitive shapes for dynamic objects such as RigidBodies and CharacterBodies as their behavior is the most reliable. They often provide better performance as well.

### Convex collision shapes

> **Warning:** Godot currently doesn't offer a built-in way to create 2D convex collision shapes. This section is mainly here for reference purposes.

Convex collision shapes (`ConvexPolygonShape2D`) are a compromise between primitive collision shapes and concave collision shapes. They can represent shapes of any complexity, but with an important caveat. As their name implies, an individual shape can only represent a *convex* shape. For instance, a pyramid is *convex*, but a hollow box is *concave*. To define a concave object with a single collision shape, you need to use a concave collision shape.

Depending on the object's complexity, you may get better performance by using multiple convex shapes instead of a concave collision shape. Godot lets you use *convex decomposition* to generate convex shapes that roughly match a hollow object. Note this performance advantage no longer applies after a certain amount of convex shapes. For large and complex objects such as a whole level, we recommend using concave shapes instead.

### Concave or trimesh collision shapes

Concave collision shapes (`ConcavePolygonShape2D`), also called trimesh collision shapes, can take any form, from a few triangles to thousands of triangles. Concave shapes are the slowest option but are also the most accurate in Godot. **You can only use concave shapes within StaticBodies.** They will not work with CharacterBodies or RigidBodies unless the RigidBody's mode is Static.

> **Note:** Even though concave shapes offer the most accurate *collision*, contact reporting can be less precise than primitive shapes.

When not using TileMaps for level design, concave shapes are the best approach for a level's collision.

You can configure the `CollisionPolygon2D` node's *build mode* in the inspector. If it is set to **Solids** (the default), collisions will include the polygon and its contained area. If it is set to **Segments**, collisions will only include the polygon edges.

You can generate a concave collision shape from the editor by selecting a `Sprite2D` and using the **Sprite2D** menu at the top of the 2D viewport. The Sprite2D menu dropdown exposes an option called **Create CollisionPolygon2D Sibling**. Once you click it, it displays a menu with 3 settings:

- **Simplification:** Higher values will result in a less detailed shape, which improves performance at the cost of accuracy.
- **Shrink (Pixels):** Higher values will shrink the generated collision polygon relative to the sprite's edges.
- **Grow (Pixels):** Higher values will grow the generated collision polygon relative to the sprite's edges. Note that setting Grow and Shrink to equal values may yield different results than leaving both of them on 0.

> **Note:** If you have an image with many small details, it's recommended to create a simplified version and use it to generate the collision polygon. This can result in better performance and game feel, since the player won't be blocked by small, decorative details.
>
> To use a separate image for collision polygon generation, create another Sprite2D, generate a collision polygon sibling from it then remove the Sprite2D node. This way, you can exclude small details from the generated collision.

### Performance caveats

You aren't limited to a single collision shape per PhysicsBody. Still, we recommend keeping the number of shapes as low as possible to improve performance, especially for dynamic objects like RigidBodies and CharacterBodies. On top of that, avoid translating, rotating, or scaling CollisionShapes to benefit from the physics engine's internal optimizations.

When using a single non-transformed collision shape in a StaticBody, the engine's *broad phase* algorithm can discard inactive PhysicsBodies. The *narrow phase* will then only have to take into account the active bodies' shapes. If a StaticBody has many collision shapes, the broad phase will fail. The narrow phase, which is slower, must then perform a collision check against each shape.

If you run into performance issues, you may have to make tradeoffs in terms of accuracy. Most games out there don't have a 100% accurate collision. They find creative ways to hide it or otherwise make it unnoticeable during normal gameplay.

---

## 6. Collision Shapes (3D)

This guide explains:

- The types of collision shapes available in 3D in Godot.
- Using a convex or a concave mesh as a collision shape.
- Performance considerations regarding 3D collisions.

Godot provides many kinds of collision shapes, with different performance and accuracy tradeoffs.

You can define the shape of a `PhysicsBody3D` by adding one or more `CollisionShape3D` nodes as *direct* child nodes. Indirect child nodes (i.e. children of child nodes) will be ignored and won't be used as collision shapes. Also, note that you must add a `Shape3D` *resource* to collision shape nodes in the Inspector dock.

> **Note:** When you add multiple collision shapes to a single PhysicsBody, you don't have to worry about them overlapping. They won't "collide" with each other.

### Primitive collision shapes

Godot provides the following primitive collision shape types:

- **BoxShape3D**
- **SphereShape3D**
- **CapsuleShape3D**
- **CylinderShape3D**

You can represent the collision of most smaller objects using one or more primitive shapes. However, for more complex objects, such as a large ship or a whole level, you may need convex or concave shapes instead. More on that below.

We recommend favoring primitive shapes for dynamic objects such as RigidBodies and CharacterBodies as their behavior is the most reliable. They often provide better performance as well.

### Convex collision shapes

Convex collision shapes (`ConvexPolygonShape3D`) are a compromise between primitive collision shapes and concave collision shapes. They can represent shapes of any complexity, but with an important caveat. As their name implies, an individual shape can only represent a *convex* shape. For instance, a pyramid is *convex*, but a hollow box is *concave*. To define a concave object with a single collision shape, you need to use a concave collision shape.

Depending on the object's complexity, you may get better performance by using multiple convex shapes instead of a concave collision shape. Godot lets you use *convex decomposition* to generate convex shapes that roughly match a hollow object. Note this performance advantage no longer applies after a certain amount of convex shapes. For large and complex objects such as a whole level, we recommend using concave shapes instead.

You can generate one or several convex collision shapes from the editor by selecting a `MeshInstance3D` and using the **Mesh** menu at the top of the 3D viewport. The editor exposes two generation modes:

- **Create Single Convex Collision Sibling** uses the Quickhull algorithm. It creates one CollisionShape node with an automatically generated convex collision shape. Since it only generates a single shape, it provides good performance and is ideal for small objects.

- **Create Multiple Convex Collision Siblings** uses the V-HACD algorithm. It creates several CollisionShape nodes, each with a convex shape. Since it generates multiple shapes, it is more accurate for concave objects at the cost of performance. For objects with medium complexity, it will likely be faster than using a single concave collision shape.

### Concave or trimesh collision shapes

Concave collision shapes (`ConcavePolygonShape3D`), also called trimesh collision shapes, can take any form, from a few triangles to thousands of triangles. Concave shapes are the slowest option but are also the most accurate in Godot. **You can only use concave shapes within StaticBodies.** They will not work with CharacterBodies or RigidBodies unless the RigidBody's mode is Static.

> **Note:** Even though concave shapes offer the most accurate *collision*, contact reporting can be less precise than primitive shapes.

When not using GridMaps for level design, concave shapes are the best approach for a level's collision. That said, if your level has small details, you may want to exclude those from collision for performance and game feel. To do so, you can build a simplified collision mesh in a 3D modeler and have Godot generate a collision shape for it automatically.

Note that unlike primitive and convex shapes, a concave collision shape doesn't have an actual "volume". You can place objects both *outside* of the shape as well as *inside*.

You can generate a concave collision shape from the editor by selecting a `MeshInstance3D` and using the **Mesh** menu at the top of the 3D viewport. The editor exposes two options:

- **Create Trimesh Static Body** is a convenient option. It creates a StaticBody containing a concave shape matching the mesh's geometry.

- **Create Trimesh Collision Sibling** creates a CollisionShape node with a concave shape matching the mesh's geometry.

> **See also:** See the "Importing 3D Scenes" documentation for information on how to export models for Godot and automatically generate collision shapes on import.

### Performance caveats

You aren't limited to a single collision shape per PhysicsBody. Still, we recommend keeping the number of shapes as low as possible to improve performance, especially for dynamic objects like RigidBodies and CharacterBodies. On top of that, avoid translating, rotating, or scaling CollisionShapes to benefit from the physics engine's internal optimizations.

When using a single non-transformed collision shape in a StaticBody, the engine's *broad phase* algorithm can discard inactive PhysicsBodies. The *narrow phase* will then only have to take into account the active bodies' shapes. If a StaticBody has many collision shapes, the broad phase will fail. The narrow phase, which is slower, must then perform a collision check against each shape.

If you run into performance issues, you may have to make tradeoffs in terms of accuracy. Most games out there don't have a 100% accurate collision. They find creative ways to hide it or otherwise make it unnoticeable during normal gameplay.

---

## 7. Large World Coordinates

> **Note:** Large world coordinates are mainly useful in 3D projects; they are rarely required in 2D projects. Also, unlike 3D rendering, 2D rendering currently doesn't benefit from increased precision when large world coordinates are enabled.

### Why use large world coordinates?

In Godot, physics simulation and rendering both rely on *floating-point* numbers. However, in computing, floating-point numbers have **limited precision and range**. This can be a problem for games with huge worlds, such as space or planetary-scale simulation games.

Precision is the greatest when the value is close to `0.0`. Precision becomes gradually lower as the value increases or decreases away from `0.0`. This occurs every time the floating-point number's *exponent* increases, which happens when the floating-point number surpasses a power of 2 value (2, 4, 8, 16, ...). Every time this occurs, the number's minimum step will *increase*, resulting in a loss of precision.

In practice, this means that as the player moves away from the world origin (`Vector2(0, 0)` in 2D games or `Vector3(0, 0, 0)` in 3D games), precision will decrease.

This loss of precision can result in objects appearing to "vibrate" when far away from the world origin, as the model's position will snap to the nearest value that can be represented in a floating-point number. This can also result in physics glitches that only occur when the player is far from the world origin.

The range determines the minimum and maximum values that can be stored in the number. If the player tries to move past this range, they will simply not be able to. However, in practice, floating-point precision almost always becomes a problem before the range does.

The range and precision (minimum step between two exponent intervals) are determined by the floating-point number type. The *theoretical* range allows extremely high values to be stored in single-precision floats, but with very low precision. In practice, a floating-point type that cannot represent all integer values is not very useful. At extreme values, precision becomes so low that the number cannot even distinguish two separate *integer* values from each other.

This is the range where individual integer values can be represented in a floating-point number:

- **Single-precision float range (represent all integers):** Between -16,777,216 and 16,777,216
- **Double-precision float range (represent all integers):** Between -9 quadrillion and 9 quadrillion

### Precision table

| Range | Single step | Double step | Comment |
|-------|-------------|-------------|---------|
| [1; 2] | ~0.0000001 | ~1e-15 | Precision becomes greater near 0.0 (this table is abbreviated). |
| [2; 4] | ~0.0000002 | ~1e-15 | |
| [4; 8] | ~0.0000005 | ~1e-15 | |
| [8; 16] | ~0.000001 | ~1e-14 | |
| [16; 32] | ~0.000002 | ~1e-14 | |
| [32; 64] | ~0.000004 | ~1e-14 | |
| [64; 128] | ~0.000008 | ~1e-13 | |
| [128; 256] | ~0.000015 | ~1e-13 | |
| [256; 512] | ~0.00003 | ~1e-13 | |
| [512; 1024] | ~0.00006 | ~1e-12 | |
| [1024; 2048] | ~0.0001 | ~1e-12 | |
| [2048; 4096] | ~0.0002 | ~1e-12 | Max recommended single-precision range for a first-person 3D game without rendering artifacts or physics glitches. |
| [4096; 8192] | ~0.0005 | ~1e-12 | Max recommended single-precision range for a third-person 3D game without rendering artifacts or physics glitches. |
| [8192; 16384] | ~0.001 | ~1e-12 | |
| [16384; 32768] | ~0.0019 | ~1e-11 | Max recommended single-precision range for a top-down 3D game without rendering artifacts or physics glitches. |
| [32768; 65536] | ~0.0039 | ~1e-11 | Max recommended single-precision range for any 3D game. Double precision (large world coordinates) is usually required past this point. |
| [65536; 131072] | ~0.0078 | ~1e-11 | |
| [131072; 262144] | ~0.0156 | ~1e-10 | |
| > 262144 | > ~0.0313 | ~1e-10 (0.0000000001) | Double-precision remains far more precise than single-precision past this value. |

When using single-precision floats, it is possible to go past the suggested ranges, but more visible artifacting will occur and physics glitches will be more common (such as the player not walking straight in certain directions).

> **See also:** See the [Demystifying Floating Point Precision](https://blog.demofox.org/2017/11/21/) article for more information.

### How large world coordinates work

Large world coordinates (also known as **double-precision physics**) increase the precision level of all floating-point computations within the engine.

By default, `float` is 64-bit in GDScript, but `Vector2`, `Vector3`, and `Vector4` are 32-bit. This means that the precision of vector types is much more limited. To resolve this, we can increase the number of bits used to represent a floating-point number in a Vector type. This results in an *exponential* increase in precision, which means the final value is not just twice as precise, but potentially thousands of times more precise at high values. The maximum value that can be represented is also greatly increased by going from a single-precision float to a double-precision float.

To avoid model snapping issues when far away from the world origin, Godot's 3D rendering engine will increase its precision for rendering operations when large world coordinates are enabled. The shaders do not use double-precision floats for performance reasons, but an [alternative solution](https://github.com/godotengine/godot/pull/66178) is used to emulate double precision for rendering using single-precision floats.

> **Note:** Enabling large world coordinates comes with a performance and memory usage penalty, especially on 32-bit CPUs. Only enable large world coordinates if you actually need them.
>
> This feature is tailored towards mid-range/high-end desktop platforms. Large world coordinates may not perform well on low-end mobile devices, unless you take steps to reduce CPU usage with other means (such as decreasing the number of physics ticks per second).
>
> On low-end platforms, an *origin shifting* approach can be used instead to allow for large worlds without using double-precision physics and rendering. Origin shifting works with single-precision floats, but it introduces more complexity to game logic, especially in multiplayer games. Therefore, origin shifting is not detailed on this page.

### Who are large world coordinates for?

Large world coordinates are typically required for 3D space or planetary-scale simulation games. This extends to games that require supporting *very* fast movement speeds, but also very slow *and* precise movements at times.

On the other hand, it's important to only use large world coordinates when actually required (for performance reasons). Large world coordinates are usually **not** required for:

- 2D games, as precision issues are usually less noticeable.
- Games with small-scale or medium-scale worlds.
- Games with large worlds, but split into different levels with loading sequences in between. You can center each level portion around the world origin to avoid precision issues without a performance penalty.
- Open world games with a *playable on-foot area* not exceeding 8192x8192 meters (centered around the world origin). As shown in the above table, the level of precision remains acceptable within that range, even for a first-person game.

**If in doubt**, you probably don't need to use large world coordinates in your project. For reference, most modern AAA open world titles don't use a large world coordinates system and still rely on single-precision floats for both rendering and physics.

### Enabling large world coordinates

This process requires recompiling the editor and all export template binaries you intend to use. If you only intend to export your project in release mode, you can skip the compilation of debug export templates. In any case, you'll need to compile an editor build so you can test your large precision world without having to export the project every time.

See the Compiling section for compiling instructions for each target platform. You will need to add the `precision=double` SCons option when compiling the editor and export templates.

The resulting binaries will be named with a `.double` suffix to distinguish them from single-precision binaries (which lack any precision suffix). You can then specify the binaries as custom export templates in your project's export presets in the Export dialog.

### Compatibility between single-precision and double-precision builds

When saving a *binary* resource using the `ResourceSaver` singleton, a special flag is stored in the file if the resource was saved using a build that uses double-precision numbers. As a result, all binary resources will change on disk when you switch to a double-precision build and save over them.

Both single-precision and double-precision builds support using the `ResourceLoader` singleton on resources that use this special flag. This means single-precision builds can load resources saved using double-precision builds and vice versa. Text-based resources don't store a double-precision flag, as they don't require such a flag for correct reading.

### Known incompatibilities

- In a networked multiplayer game, the server and all clients should be using the same build type to ensure precision remains consistent across clients. Using different build types *may* work, but various issues can occur.
- The GDExtension API changes in an incompatible way in double-precision builds. This means extensions **must** be rebuilt to work with double-precision builds. On the extension developer's end, the `REAL_T_IS_DOUBLE` define is enabled when building a GDExtension with `precision=double`. `real_t` can be used as an alias for `float` in single-precision builds, and `double` in double-precision builds.

### Limitations

Since 3D rendering shaders don't actually use double-precision floats, there are some limitations when it comes to 3D rendering precision:

- **Triplanar mapping** doesn't benefit from increased precision. Materials using triplanar mapping will exhibit visible jittering when far away from the world origin.
- **GPUParticles3D** nodes with **Local Coords** disabled will not benefit from increased precision. This can cause visible particle snapping to occur when far away from the world origin. Nodes with **Local Coords** enabled, as well as `CPUParticles3D` nodes, will still benefit from increased precision.
- Shaders using the `skip_vertex_transform` or `world_vertex_coords` render modes don't benefit from increased precision.
- In double-precision builds, world space coordinates in a shader `fragment()` function can't be reconstructed from view space. For example, this will **not** work correctly:

```glsl
vec3 world = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
```

Instead, calculate the world space coordinates in the `vertex()` function and pass them using a varying:

```glsl
varying vec3 world;
void vertex() {
    world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}
```

2D rendering currently doesn't benefit from increased precision when large world coordinates are enabled. This can cause visible model snapping to occur when far away from the world origin (starting from a few million pixels at typical zoom levels). 2D physics calculations will still benefit from increased precision though.

---

## 8. Physics Interpolation — Introduction

### Physics ticks and rendered frames

One key concept to understand in Godot is the distinction between physics ticks (sometimes referred to as iterations or physics frames), and rendered frames. The physics proceeds at a fixed tick rate (set in **Project Settings > Physics > Common > Physics Ticks per Second**), which defaults to 60 ticks per second.

However, the engine does not necessarily **render** at the same rate. Although many monitors refresh at 60 Hz (cycles per second), many refresh at completely different frequencies (e.g. 75 Hz, 144 Hz, 240 Hz or more). Even though a monitor may be able to show a new frame e.g. 60 times a second, there is no guarantee that the CPU and GPU will be able to *supply* frames at this rate. For instance, when running with V-Sync, the computer may be too slow for 60 and only reach the deadlines for 30 FPS, in which case the frames you see will change at 30 FPS (resulting in stuttering).

But there is a problem here. What happens if the physics ticks do not coincide with frames? What happens if the physics tick rate is out of phase with the frame rate? Or worse, what happens if the physics tick rate is *lower* than the rendered frame rate?

This problem is easier to understand if we consider an extreme scenario. If you set the physics tick rate to 10 ticks per second, in a simple game with a rendered frame rate of 60 FPS, if we plot a graph of the positions of an object against the rendered frames, you can see that the positions will appear to "jump" every 1/10th of a second, rather than giving a smooth motion. When the physics calculates a new position for a new object, it is not rendered in this position for just one frame, but for 6 frames.

This jump can be seen in other combinations of tick / frame rate as glitches, or jitter, caused by this staircasing effect due to the discrepancy between physics tick time and rendered frame time.

### What can we do about frames and ticks being out of sync?

#### Lock the tick / frame rate together?

The most obvious solution is to get rid of the problem, by ensuring there is a physics tick that coincides with every frame. This used to be the approach on old consoles and fixed hardware computers. If you know that every player will be using the same hardware, you can ensure it is fast enough to calculate ticks and frames at e.g. 50 FPS, and you will be sure it will work great for everybody.

However, modern games are often no longer made for fixed hardware. You will often be planning to release on desktop computers, mobiles, and more. All of which have huge variations in performance, as well as different monitor refresh rates. We need to come up with a better way of dealing with the problem.

#### Adapt the tick rate?

Instead of designing the game at a fixed physics tick rate, we could allow the tick rate to scale according to the end user's hardware. We could for example use a fixed tick rate that works for that hardware, or even vary the duration of each physics tick to match a particular frame duration.

This works, but there is a problem. Physics (*and game logic*, which is often also run in the `_PhysicsProcess`) work best and most consistently when run at a **fixed**, predetermined tick rate. If you attempt to run a racing game physics that has been designed for 60 TPS (ticks per second) at e.g. 10 TPS, the physics will behave completely differently. Controls may be less responsive, collisions / trajectories can be completely different. You may test your game thoroughly at 60 TPS, then find it breaks on end users' machines when it runs at a different tick rate.

This can make quality assurance difficult with hard to reproduce bugs, especially in AAA games where problems of this sort can be very costly. This can also be problematic for multiplayer games for competitive integrity, as running the game at certain tick rates may be more advantageous than others.

#### Lock the tick rate, but use interpolation to smooth frames in between physics ticks

This has become one of the most popular approaches to deal with the problem, although it is optional and disabled by default.

We have established that the most desirable physics/game logic arrangement for consistency and predictability is a physics tick rate that is fixed at design-time. The problem is the discrepancy between the physics position recorded, and where we "want" a physics object to be shown on a frame to give smooth motion.

The answer turns out to be simple, but can be a little hard to get your head around at first.

Instead of keeping track of just the current position of a physics object in the engine, we keep track of *both the current position of the object, and the previous position* on the previous physics tick.

Why do we need the previous position *(in fact the entire transform, including rotation and scaling)*? By using a little math magic, we can use **interpolation** to calculate what the transform of the object would be between those two points, in our ideal world of smooth continuous movement.

### Linear interpolation

The simplest way to achieve this is linear interpolation, or lerping, which you may have used before.

Let us consider only the position, and a situation where we know that the previous physics tick X coordinate was 10 units, and the current physics tick X coordinate is 30 units.

> **Note:** Although the maths is explained here, you do not have to worry about the details, as this step will be performed for you. Under the hood, Godot may use more complex forms of interpolation, but linear interpolation is the easiest in terms of explanation.

### The physics interpolation fraction

If our physics ticks are happening 10 times per second (for this example), what happens if our rendered frame takes place at time 0.12 seconds? We can do some math to figure out where the object would be to obtain a smooth motion between the two ticks.

First of all, we have to calculate how far through the physics tick we want the object to be. If the last physics tick took place at 0.1 seconds, we are 0.02 seconds *(0.12 - 0.1)* through a tick that we know will take 0.1 seconds (10 ticks per second). The fraction through the tick is thus:

```csharp
float fraction = 0.02f / 0.10f;
// fraction = 0.2f
```

This is called the **physics interpolation fraction**, and is handily calculated for you by Godot. It can be retrieved on any frame by calling `Engine.GetPhysicsInterpolationFraction()`.

### Calculating the interpolated position

Once we have the interpolation fraction, we can insert it into a standard linear interpolation equation. The X coordinate would thus be:

```csharp
float xInterpolated = xPrev + ((xCurr - xPrev) * 0.2f);
```

So substituting our `xPrev` as 10, and `xCurr` as 30:

```csharp
float xInterpolated = 10 + ((30 - 10) * 0.2f);
// xInterpolated = 10 + 4
// xInterpolated = 14
```

Let's break that down:

- We know the X starts from the coordinate on the previous tick (`xPrev`) which is 10 units.
- We know that after the full tick, the difference between the current tick and the previous tick will have been added (`xCurr - xPrev`) (which is 20 units).
- The only thing we need to vary is the proportion of this difference we add, according to how far we are through the physics tick.

> **Note:** Although this example interpolates the position, the same thing can be done with the rotation and scale of objects. It is not necessary to know the details as Godot will do all this for you.

### Smoothed transformations between physics ticks?

Putting all this together shows that it should be possible to have a nice smooth estimation of the transform of objects between the current and previous physics tick.

But wait, you may have noticed something. If we are interpolating between the current and previous ticks, we are not estimating the position of the object *now*, we are estimating the position of the object in the past. To be exact, we are estimating the position of the object *between 1 and 2 ticks* into the past.

### In the past

What does this mean? This scheme does work, but it does mean we are effectively introducing a delay between what we see on the screen, and where the objects *should* be.

In practice, most people won't notice this delay, or rather, it is typically not *objectionable*. There are already significant delays involved in games, we just don't typically notice them. The most significant effect is there can be a slight delay to input, which can be a factor in fast twitch games. In some of these fast input situations, you may wish to turn off physics interpolation and use a different scheme, or use a high tick rate, which mitigates these delays.

### Why look into the past? Why not predict the future?

There is an alternative to this scheme, which is: instead of interpolating between the previous and current tick, we use maths to *extrapolate* into the future. We try to predict where the object *will be*, rather than show it where it was. This can be done and may be offered as an option in future, but there are some significant downsides:

- The prediction may not be correct, especially when an object collides with another object during the physics tick.
- Where a prediction was incorrect, the object may extrapolate into an "impossible" position, like inside a wall.
- Providing the movement speed is slow, these incorrect predictions may not be too much of a problem.
- When a prediction was incorrect, the object may have to jump or snap back onto the corrected path. This can be visually jarring.

### Fixed timestep interpolation

In Godot this whole system is referred to as physics interpolation, but you may also hear it referred to as **"fixed timestep interpolation"**, as it is interpolating between objects moved with a fixed timestep (physics ticks per second). In some ways the second term is more accurate, because it can also be used to interpolate objects that are not driven by physics.

> **Tip:** Although physics interpolation is usually a good choice, there are exceptions where you may choose not to use Godot's built-in physics interpolation (or use it in a limited fashion). An example category is internet multiplayer games. Multiplayer games often receive tick or timing based information from other players or a server and these may not coincide with local physics ticks, so a custom interpolation technique can often be a better fit.

---

## 9. Using Physics Interpolation

How do we incorporate physics interpolation into a Godot game? Are there any caveats?

We have tried to make the system as easy to use as possible, and many existing games will work with few changes. That said there are some situations which require special treatment, and these will be described.

### Turn on the physics interpolation setting

The first step is to turn on physics interpolation in **Project Settings > Physics > Common > Physics Interpolation**. You can now run your game.

It is likely that nothing looks hugely different, particularly if you are running physics at 60 TPS or a multiple of it. However, quite a bit more is happening behind the scenes.

> **Tip:** To convert an existing game to use interpolation, it is highly recommended that you temporarily set **Project Settings > Physics > Common > Physics Ticks per Second** to a low value such as `10`, which will make interpolation problems more obvious.

### Move (almost) all game logic from _Process to _PhysicsProcess

The most fundamental requirement for physics interpolation (which you may be doing already) is that you should be moving and performing game logic on your objects within `_PhysicsProcess()` (which runs at a physics tick) rather than `_Process()` (which runs on a rendered frame). This means your scripts should typically be doing the bulk of their processing within `_PhysicsProcess()`, including responding to input and AI.

Setting the transform of objects only within physics ticks allows the automatic interpolation to deal with transforms *between* physics ticks, and ensures the game will run the same whatever machine it is run on. As a bonus, this also reduces CPU usage if the game is rendering at high FPS, since AI logic (for example) will no longer run on every rendered frame.

> **Note:** If you attempt to set the transform of interpolated objects *outside* the physics tick, the calculations for the interpolated position will be incorrect, and you will get jitter. This jitter may not be visible on your machine, but it *will* occur for some players. For this reason, setting the transform of interpolated objects should be avoided outside of the physics tick. Godot will attempt to produce warnings in the editor if this case is detected.

> **Tip:** This is only a *soft rule*. There are some occasions where you might want to teleport objects outside of the physics tick (for instance when starting a level, or respawning objects). Still, in general, you should be applying transforms from the physics tick.

### Ensure that all indirect movement happens during physics ticks

Consider that in Godot, nodes can be moved not just directly in your own scripts, but also by automatic methods such as tweening, animation, and navigation. All these methods should also have their timing set to operate on the physics tick rather than each frame ("idle"), **if** you are using them to move objects (*these methods can also be used to control properties that are not interpolated*).

> **Note:** Also consider that nodes can be moved not just by moving themselves, but also by moving parent nodes in the `SceneTree`. The movement of parents should therefore also only occur during physics ticks.

### Choose a physics tick rate

When using physics interpolation, the rendering is decoupled from physics, and you can choose any value that makes sense for your game. You are no longer limited to values that are multiples of the user's monitor refresh rate (for stutter-free gameplay if the target FPS is reached).

As a rough guide:

| Low tick rates (10-30) | Medium tick rates (30-60) | High tick rates (60+) |
|------------------------|---------------------------|----------------------|
| Better CPU performance | Good physics behavior in complex scenes | Good with fast physics |
| Add some delay to input | Good for first person games | Good for racing games |
| Simple physics behaviour | | |

> **Note:** You can always change the tick rate as you develop, it is as simple as changing the project setting.

### Call ResetPhysicsInterpolation() when teleporting objects

Most of the time, interpolation is what you want between two physics ticks. However, there is one situation in which it may *not* be what you want. That is when you are initially placing objects, or moving them to a new location. Here, you don't want a smooth motion between where the object was (e.g. the origin) and the initial position — you want an instantaneous move.

The solution to this is to call the `Node.ResetPhysicsInterpolation()` function. What this function does under the hood is set the internally stored *previous transform* of the object to be equal to the *current transform*. This ensures that when interpolating between these two equal transforms, there will be no movement.

Even if you forget to call this, it will usually not be a problem in most situations (especially at high tick rates). This is something you can easily leave to the polishing phase of your game. The worst that will happen is seeing a streaking motion for a frame or so when you move them — you will know when you need it!

There are actually two ways to use `ResetPhysicsInterpolation()`:

**Standing start (e.g. player):**

```csharp
// 1) Set the initial transform
GlobalTransform = initialTransform;
// 2) Call ResetPhysicsInterpolation()
ResetPhysicsInterpolation();
```

The previous and current transforms will be identical, resulting in no initial movement.

**Moving start (e.g. bullet):**

```csharp
// 1) Set the initial transform
GlobalTransform = startTransform;
// 2) Call ResetPhysicsInterpolation()
ResetPhysicsInterpolation();
// 3) Immediately set the transform expected after the first tick of motion
GlobalTransform = startTransform.Translated(velocity * delta);
```

The previous transform will be the starting position, and the current transform will act as though a tick of simulation has already taken place. This will immediately start moving the object, instead of having a tick delay standing still.

> **Important:** Make sure you set the transform and call `ResetPhysicsInterpolation()` in the correct order as shown above, otherwise you will see unwanted "streaking".

### Testing and debugging tips

Even if you intend to run physics at 60 TPS, in order to thoroughly test your interpolation and get the smoothest gameplay, it is highly recommended to temporarily set the physics tick rate to a low value such as 10 TPS.

The gameplay may not work perfectly, but it should enable you to more easily see cases where you should be calling `ResetPhysicsInterpolation()`, or where you should be using your own custom interpolation on e.g. a `Camera3D`. Once you have these cases fixed, you can set the physics tick rate back to the desired setting.

The other great advantage to testing at a low tick rate is you can often notice other game systems that are synchronized to the physics tick and creating glitches which you may want to work around. Typical examples include setting animation blend values, which you may decide to set in `_Process()` and interpolate manually.

> **Note:** In 2D, the position of visible collision shapes shown by the **Debug > Visible Collision Shapes** option **will** take physics interpolation into account.
>
> By contrast, in 3D, the position of visible collision shapes **will not** take physics interpolation into account. This means the visible collision shapes can appear to move less smoothly and appear slightly in front of the object's visual representation when the object is moving. This is not a bug, but a consequence of how physics interpolation is implemented in 3D.

---

## 10. Troubleshooting Physics Issues

When working with a physics engine, you may encounter unexpected results. While many of these issues can be resolved through configuration, some of them are the result of engine bugs. For known issues related to the physics engine, see [open physics-related issues on GitHub](https://github.com/godotengine/godot/issues?q=is%3Aopen+is%3Aissue+label%3Atopic%3Aphysics). Looking through [closed issues](https://github.com/godotengine/godot/issues?q=+is%3Aclosed+is%3Aissue+label%3Atopic%3Aphysics) can also help answer questions related to physics engine behavior.

### Objects are passing through each other at high speeds

This is known as *tunneling*. Enabling **Continuous CD** in the RigidBody properties can sometimes resolve this issue. If this does not help, there are other solutions you can try:

- Make your static collision shapes thicker. For example, if you have a thin floor that the player can't get below in some way, you can make the collider thicker than the floor's visual representation.
- Modify your fast-moving object's collision shape depending on its movement speed. The faster the object moves, the larger the collision shape should extend outside of the object to ensure it can collide with thin walls more reliably.
- Increase **Physics Ticks per Second** in the advanced Project Settings. While this has other benefits (such as more stable simulation and reduced input lag), this increases CPU utilization and may not be viable for mobile/web platforms. Multipliers of the default value of `60` (such as `120`, `180` or `240`) should be preferred for a smooth appearance on most displays.

### Stacked objects are unstable and wobbly

Despite seeming like a simple problem, stable RigidBody simulation with stacked objects is difficult to implement in a physics engine. This is caused by integrating forces going against each other. The more stacked objects are present, the stronger the forces will be against each other. This eventually causes the simulation to become wobbly, making the objects unable to rest on top of each other without moving.

Increasing the physics simulation rate can help alleviate this issue. To do so, increase **Physics Ticks per Second** in the advanced Project Settings. Note that this increases CPU utilization and may not be viable for mobile/web platforms. Multipliers of the default value of `60` (such as `120`, `180` or `240`) should be preferred for a smooth appearance on most displays.

In 3D, switching the physics engine from the default GodotPhysics to Jolt can also improve stability. See the [Using Jolt Physics](#11-using-jolt-physics) section for more information.

### Scaled physics bodies or collision shapes do not collide correctly

Godot does not currently support scaling of physics bodies or collision shapes. As a workaround, change the collision shape's extents instead of changing its scale. If you want the visual representation's scale to change as well, change the scale of the underlying visual representation (`Sprite2D`, `MeshInstance3D`, ...) and change the collision shape's extents separately. Make sure the collision shape is not a child of the visual representation in this case.

Since resources are shared by default, you'll have to make the collision shape resource unique if you don't want the change to be applied to all nodes using the same collision shape resource in the scene. This can be done in two ways:

- In the editor, by clicking **Make Unique** in the CollisionShape resource dropdown in the inspector, then changing its size.
- In a script, by calling `Duplicate()` on the collision shape resource *before* changing its size.

```csharp
// Example: Making a collision shape unique and resizing it
var shape = GetNode<CollisionShape3D>("CollisionShape3D");
shape.Shape = (Shape3D)shape.Shape.Duplicate();
((BoxShape3D)shape.Shape).Size = new Vector3(2.0f, 1.0f, 2.0f);
```

### Thin objects are wobbly when resting on the floor

This can be due to one of two causes:

- The floor's collision shape is too thin.
- The RigidBody's collision shape is too thin.

In the first case, this can be alleviated by making the floor's collision shape thicker. For example, if you have a thin floor that the player can't get below in some way, you can make the collider thicker than the floor's visual representation.

In the second case, this can usually only be resolved by increasing the physics simulation rate (as making the shape thicker would cause a disconnect between the RigidBody's visual representation and its collision).

In both cases, increasing the physics simulation rate can also help alleviate this issue. To do so, increase **Physics Ticks per Second** in the advanced Project Settings. Note that this increases CPU utilization and may not be viable for mobile/web platforms. Multipliers of the default value of `60` (such as `120`, `180` or `240`) should be preferred for a smooth appearance on most displays.

### Cylinder collision shapes are unstable

Switching the physics engine from the default GodotPhysics to Jolt should make cylinder collision shapes more reliable. See the [Using Jolt Physics](#11-using-jolt-physics) section for more information.

During the transition from Bullet to GodotPhysics in Godot 4, cylinder collision shapes had to be reimplemented from scratch. However, cylinder collision shapes are one of the most difficult shapes to support, which is why many other physics engines don't provide any support for them. There are several known bugs with cylinder collision shapes currently.

If you are sticking to GodotPhysics, we recommend using box or capsule collision shapes for characters for now. Boxes generally provide the best reliability, but have the downside of making the character take more space diagonally. Capsule collision shapes do not have this downside, but their shape can make precision platforming more difficult.

### VehicleBody simulation is unstable, especially at high speeds

When a physics body moves at a high speed, it travels a large distance between each physics step. For instance, when using the 1 unit = 1 meter convention in 3D, a vehicle moving at 360 km/h will travel 100 units per second. With the default physics simulation rate of 60 Hz, the vehicle moves by ~1.67 units each physics tick. This means that small objects may be ignored entirely by the vehicle (due to tunneling), but also that the simulation has little data to work with in general at such a high speed.

Fast-moving vehicles can benefit a lot from an increased physics simulation rate. To do so, increase **Physics Ticks per Second** in the advanced Project Settings. Note that this increases CPU utilization and may not be viable for mobile/web platforms. Multipliers of the default value of `60` (such as `120`, `180` or `240`) should be preferred for a smooth appearance on most displays.

### Collision results in bumps when an object moves across tiles

This is a known issue in the physics engine caused by the object bumping on a shape's edges, even though that edge is covered by another shape. This can occur in both 2D and 3D.

The best way to work around this issue is to create a "composite" collider. This means that instead of individual tiles having their collision, you create a single collision shape representing the collision for a group of tiles. Typically, you should split composite colliders on a per-island basis (which means each group of touching tiles gets its own collider).

Using a composite collider can also improve physics simulation performance in certain cases. However, since the composite collision shape is much more complex, this may not be a net performance win in all cases.

> **Tip:** In Godot 4.5 and later, creating a composite collider is automatically done when using a `TileMapLayer` node. The chunk size (`16` tiles on each axis by default) can be set using the **Physics Quadrant Size** property in the TileMapLayer inspector. Larger values provide more reliable collision, at the cost of slower updates when the TileMap is changed.

### Framerate drops when an object touches another object

This is likely due to one of the objects using a collision shape that is too complex. Convex collision shapes should use a number of shapes as low as possible for performance reasons. When relying on Godot's automatic generation, it's possible that you ended up with dozens if not hundreds of shapes created for a single convex shape collision resource.

In some cases, replacing a convex collider with a couple of primitive collision shapes (box, sphere, or capsule) can deliver better performance.

This issue can also occur with StaticBodies that use very detailed trimesh (concave) collisions. In this case, use a simplified representation of the level geometry as a collider. Not only will this improve physics simulation performance significantly, but this can also improve stability by letting you remove small fixtures and crevices from being considered by collision.

In 3D, switching the physics engine from the default GodotPhysics to Jolt can also improve performance. See the [Using Jolt Physics](#11-using-jolt-physics) section for more information.

### Framerate suddenly drops to a very low value beyond a certain amount of physics simulation

This occurs because the physics engine can't keep up with the expected simulation rate. In this case, the framerate will start dropping, but the engine is only allowed to simulate a certain number of physics steps per rendered frame. This snowballs into a situation where framerate keeps dropping until it reaches a very low framerate (typically 1-2 FPS) and is called the *physics spiral of death*.

To avoid this, you should check for situations in your project that can cause excessive number of physics simulations to occur at the same time (or with excessively complex collision shapes). If these situations cannot be avoided, you can increase the **Max Physics Steps per Frame** project setting and/or reduce **Physics Ticks per Second** to alleviate this.

### Physics simulation is unreliable when far away from the world origin

This is caused by floating-point precision errors, which become more pronounced as the physics simulation occurs further away from the world origin. This issue also affects rendering, which results in wobbly camera movement when far away from the world origin. See the [Large World Coordinates](#7-large-world-coordinates) section for more information.

---

## 11. Using Jolt Physics

### Introduction

The Jolt physics engine was added as an alternative to the existing Godot Physics physics engine in 4.4. Jolt is developed by Jorrit Rouwe with a focus on games and VR applications. Previously it was available as an extension but is now built into Godot. By default, new projects will use it as the physics engine.

The existing extension is now considered in maintenance mode. That means bug fixes will be merged, and it will be kept compatible with new versions of Godot until the built-in module has feature parity with the extension. The only thing missing at this point is related joints, which you can read about on this page. The extension can be found [here on GitHub](https://github.com/godot-jolt/godot-jolt) and in Godot's asset library.

To change the 3D physics engine to be Jolt Physics, set **Project Settings > Physics > 3D > Physics Engine** to `Jolt Physics`. Once you've done that, click the **Save & Restart** button. When the editor opens again, 3D scenes should now be using Jolt for physics.

### Notable differences to Godot Physics

There are many differences between the existing Godot Physics engine and Jolt.

#### Joint properties

The current interfaces for the 3D joint nodes don't quite line up with the interface of Jolt's own joints. As such, there are a number of joint properties that are not supported, mainly ones related to configuring the joint's soft limits.

The unsupported properties are:

- **PinJoint3D:** `Bias`, `Damping`, `ImpulseClamp`
- **HingeJoint3D:** `Bias`, `Softness`, `Relaxation`
- **SliderJoint3D:** `Angular*`, `*Limit/Softness`, `*Limit/Restitution`, `*Limit/Damping`
- **ConeTwistJoint3D:** `Bias`, `Relaxation`, `Softness`
- **Generic6DOFJoint3D:** `*Limit*/Softness`, `*Limit*/Restitution`, `*Limit*/Damping`, `*Limit*/Erp`

Currently a warning is emitted if you set these properties to anything but their default values.

#### Single-body joints

You can, in Godot, omit one of the joint bodies for a two-body joint and effectively have "the world" be the other body. However, the node path that you assign your body to (`NodeA` vs `NodeB`) is ignored. Godot Physics will always behave as if you assigned it to `NodeA`, and since `NodeA` is also what defines the frame of reference for the joint limits, you end up with inverted limits and a potentially strange limit shape, especially if your limits allow both linear and angular degrees of freedom.

Jolt will behave as if you assigned the body to `NodeB` instead, with `NodeA` representing "the world". There is a project setting called **Physics > Jolt Physics 3D > Joints > World Node** that lets you toggle this behavior, if you need compatibility for an existing project.

#### Collision margins

Jolt (and other similar physics engines) uses something that Jolt refers to as "convex radius" to help improve the performance and behavior of the types of collision detection that Jolt relies on for convex shapes. Other physics engines (Godot included) might refer to these as "collision margins" instead. Godot exposes these as the `Margin` property on every `Shape3D`-derived class, but Godot Physics itself does not use them for anything.

What these collision margins sometimes do in other engines (as described in Godot's documentation) is effectively add a "shell" around the shape, slightly increasing its size while also rounding off any edges/corners. In Jolt however, these margins are first used to shrink the shape, and then the "shell" is applied, resulting in edges/corners being similarly rounded off, but without increasing the size of the shape.

To prevent having to tweak this margin property manually, since its default value can be problematic for smaller shapes, the Jolt module exposes a project setting called **Physics > Jolt Physics 3D > Collisions > Collision Margin Fraction** which is multiplied with the smallest axis of the shape's AABB to calculate the actual margin. The `Margin` property of the shape is then instead used as an upper bound.

These margins should, for most use-cases, be more or less transparent, but can sometimes result in odd collision normals when performing shape queries. You can lower the above mentioned project setting to mitigate some of this, including setting it to `0.0`, but too small of a margin can also cause odd collision results, so is generally not recommended.

#### Baumgarte stabilization

Baumgarte stabilization is a method to resolve penetrating bodies and push them to a state where they are just touching. In Godot Physics this works like a spring. This means that bodies can accelerate and may cause the bodies to overshoot and separate completely. With Jolt, the stabilization is only applied to the position and not to the velocity of the body. This means it cannot overshoot but it may take longer to resolve the penetration.

The strength of this stabilization can be tweaked using the project setting **Physics > Jolt Physics 3D > Simulation > Baumgarte Stabilization Factor**. Setting this project setting to `0.0` will turn Baumgarte stabilization off. Setting it to `1.0` will resolve penetration in 1 simulation step. This is fast but often also unstable.

#### Ghost collisions

Jolt employs two techniques to mitigate ghost collisions, meaning collisions with internal edges of shapes/bodies that result in collision normals that oppose the direction of movement.

The first technique, called **"active edge detection"**, marks edges of triangles in `ConcavePolygonShape3D` or `HeightMapShape3D` as either "active" or "inactive", based on the angle to the neighboring triangle. When a collision happens with an inactive edge the collision normal will be replaced with the triangle's normal instead, to lessen the effect of ghost collisions.

The angle threshold for this active edge detection is configurable through the project setting **Physics > Jolt Physics 3D > Collisions > Active Edge Threshold**.

The second technique, called **"enhanced internal edge removal"**, instead adds runtime checks to detect whether an edge is active or inactive, based on the contact points of the two bodies. This has the benefit of applying not only to collisions with `ConcavePolygonShape3D` and `HeightMapShape3D`, but also edges between any shapes within the same body.

Enhanced internal edge removal can be toggled on and off for the various contexts to which it's applied, using the **Physics > Jolt Physics 3D > Simulation > Use Enhanced Internal Edge Removal** project setting, and the similar settings for **queries** and **motion queries**.

Note that neither the active edge detection nor enhanced internal edge removal apply when dealing with ghost collisions between two different bodies.

#### Memory usage

Jolt uses a stack allocator for temporary allocations within its simulation step. This stack allocator requires allocating a set amount of memory up front, which can be configured using the **Physics > Jolt Physics 3D > Limits > Temporary Memory Buffer Size** project setting.

#### Ray-cast face index

The `FaceIndex` property returned in the results of `IntersectRay()` and `RayCast3D` will by default always be `-1` with Jolt. The project setting **Physics > Jolt Physics 3D > Queries > Enable Ray Cast Face Index** will enable them.

Note that enabling this setting will increase the memory requirement of `ConcavePolygonShape3D` with about 25%.

#### Kinematic RigidBody3D contacts

When using Jolt, a `RigidBody3D` frozen with `FreezeMode.Kinematic` will by default not report contacts from collisions with other static/kinematic bodies, for performance reasons, even when setting a non-zero `MaxContactsReported`. If you have many/large kinematic bodies overlapping with complex static geometry, such as `ConcavePolygonShape3D` or `HeightMapShape3D`, you can end up wasting a significant amount of CPU performance and memory without realizing it.

For this reason this behavior is opt-in through the project setting **Physics > Jolt Physics 3D > Simulation > Generate All Kinematic Contacts**.

#### Contact impulses

Due to limitations internal to Jolt, the contact impulses provided by `PhysicsDirectBodyState3D.GetContactImpulse()` are estimated ahead of time based on things like the contact manifold and velocities of the colliding bodies. This means that the reported impulses will only be accurate in cases where the two bodies in question are not colliding with any other bodies.

#### Area3D and SoftBody3D

Jolt supports the same type of interactions between `SoftBody3D` and `Area3D` as Godot Physics, such as the wind and gravity properties found on `Area3D`. Unlike Godot Physics however, Jolt also supports the various overlap signals and methods found on `Area3D`, for when a `SoftBody3D` enters or exits an overlap with it, such as `BodyEntered`.

To revert back to the behavior of Godot Physics, where no overlap signals are emitted, you need to configure the area's `CollisionMask` such that there's no overlap with the soft body's `CollisionLayer`. You can also filter out any `SoftBody3D` in the signal connection yourself.

#### WorldBoundaryShape3D

`WorldBoundaryShape3D`, which is meant to represent an infinite plane, is implemented a bit differently in Jolt compared to Godot Physics. Both engines have an upper limit for how big the effective size of this plane can be, but this size is much smaller when using Jolt, in order to avoid precision issues.

You can configure this size using the **Physics > Jolt Physics 3D > Limits > World Boundary Shape Size** project setting.

### Notable differences to the Godot Jolt extension

While the built-in Jolt module is largely a straight port of the Godot Jolt extension, there are a few things that are different.

#### Project settings

All project settings have been moved from the `physics/jolt_3d` category to `physics/jolt_physics_3d`.

On top of that, there's been some renaming and refactoring of the individual project settings as well. These include:

| Extension Setting | Module Setting |
|---|---|
| `sleep/enabled` | `simulation/allow_sleep` |
| `sleep/velocity_threshold` | `simulation/sleep_velocity_threshold` |
| `sleep/time_threshold` | `simulation/sleep_time_threshold` |
| `collisions/use_shape_margins` | `collisions/collision_margin_fraction` (value of 0 is equivalent to disabling it) |
| `collisions/use_enhanced_internal_edge_removal` | `simulation/use_enhanced_internal_edge_removal` |
| `collisions/areas_detect_static_bodies` | `simulation/areas_detect_static_bodies` |
| `collisions/report_all_kinematic_contacts` | `simulation/generate_all_kinematic_contacts` |
| `collisions/soft_body_point_margin` | `simulation/soft_body_point_radius` |
| `collisions/body_pair_cache_enabled` | `simulation/body_pair_contact_cache_enabled` |
| `collisions/body_pair_cache_distance_threshold` | `simulation/body_pair_contact_cache_distance_threshold` |
| `collisions/body_pair_cache_angle_threshold` | `simulation/body_pair_contact_cache_angle_threshold` |
| `continuous_cd/movement_threshold` | `simulation/continuous_cd_movement_threshold` (fraction instead of percentage) |
| `continuous_cd/max_penetration` | `simulation/continuous_cd_max_penetration` (fraction instead of percentage) |
| `kinematics/use_enhanced_internal_edge_removal` | `motion_queries/use_enhanced_internal_edge_removal` |
| `kinematics/recovery_iterations` | `motion_queries/recovery_iterations` (fraction instead of percentage) |
| `kinematics/recovery_amount` | `motion_queries/recovery_amount` |
| `queries/use_legacy_ray_casting` | Removed |
| `solver/position_iterations` | `simulation/position_steps` |
| `solver/velocity_iterations` | `simulation/velocity_steps` |
| `solver/position_correction` | `simulation/baumgarte_stabilization_factor` (fraction instead of percentage) |
| `solver/active_edge_threshold` | `collisions/active_edge_threshold` |
| `solver/bounce_velocity_threshold` | `simulation/bounce_velocity_threshold` |
| `solver/contact_speculative_distance` | `simulation/speculative_contact_distance` |
| `solver/contact_allowed_penetration` | `simulation/penetration_slop` |
| `limits/max_angular_velocity` | Now stored as radians instead |
| `limits/max_temporary_memory` | `limits/temporary_memory_buffer_size` |

#### Joint nodes

The joint nodes that are exposed in the Godot Jolt extension (`JoltPinJoint3D`, `JoltHingeJoint3D`, `JoltSliderJoint3D`, `JoltConeTwistJoint3D`, and `JoltGeneric6DOFJoint`) have not been included in the Jolt module.

#### Thread safety

Unlike the Godot Jolt extension, the Jolt module does have thread-safety, including support for the **Physics > 3D > Run On Separate Thread** project setting. However this has not been tested very thoroughly, so it should be considered experimental.
