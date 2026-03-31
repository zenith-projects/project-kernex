# Plugins, Internationalization & Math Reference

> Extracted from the Godot 4.6 official documentation. All code examples have been converted to C#.

---

## Part 1: PLUGINS

---

## 1.1 Making Plugins

### About Plugins

A plugin is a great way to extend the editor with useful tools. It can be made entirely with C# and standard scenes, without even reloading the editor. Unlike modules, you don't need to create C++ code nor recompile the engine. While this makes plugins less powerful, there are still many things you can do with them. Note that a plugin is similar to any scene you can already make, except it is created using a script to add editor functionality.

This tutorial will guide you through the creation of two plugins so you can understand how they work and be able to develop your own. The first is a custom node that you can add to any scene in the project, and the other is a custom dock added to the editor.

### Creating a Plugin

Before starting, create a new empty project wherever you want. This will serve as a base to develop and test the plugins.

The first thing you need for the editor to identify a new plugin is to create two files: a `plugin.cfg` for configuration and a tool script with the functionality. Plugins have a standard path like `addons/plugin_name` inside the project folder. Godot provides a dialog for generating those files and placing them where they need to be.

In the main toolbar, click the **Project** dropdown. Then click **Project Settings...**. Go to the **Plugins** tab and then click on the **Create New Plugin** button in the top-right.

To continue with the example, use the following values:

```ini
Plugin Name: My Custom Node
Subfolder: MyCustomNode
Description: A custom node made to extend the Godot Engine.
Author: Your Name Here
Version: 1.0.0
Language: C#
Script Name: CustomNode.cs
```

> **Warning:** In C#, the EditorPlugin script needs to be compiled, which requires building the project. After building the project the plugin can be enabled in the **Plugins** tab of **Project Settings**.

You should end up with a directory structure like this:

```
addons/
  MyCustomNode/
    plugin.cfg
    CustomNode.cs
```

`plugin.cfg` is an INI file with metadata about your plugin. The name and description help people understand what it does. Your name helps you get properly credited for your work. The version number helps others know if they have an outdated version; if you are unsure on how to come up with the version number, check out [Semantic Versioning](https://semver.org/). The main script file will instruct Godot what your plugin does in the editor once it is active.

### The Script File

Upon creation of the plugin, the dialog will automatically open the EditorPlugin script for you. The script has two requirements that you cannot change: it must be a `[Tool]` script, or else it will not load properly in the editor, and it must inherit from `EditorPlugin`.

> **Warning:** In addition to the EditorPlugin script, any other C# script that your plugin uses must *also* be a tool. Any script without `[Tool]` used by the editor will act like an empty file!

It's important to deal with initialization and clean-up of resources. A good practice is to use the virtual function `_EnterTree()` to initialize your plugin and `_ExitTree()` to clean it up. Your script should look something like this:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class CustomNode : EditorPlugin
{
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
    }
}
#endif
```

This is a good template to use when creating new plugins.

### A Custom Node

Sometimes you want a certain behavior in many nodes, such as a custom scene or control that can be reused. Instancing is helpful in a lot of cases, but sometimes it can be cumbersome, especially if you're using it in many projects. A good solution to this is to make a plugin that adds a node with a custom behavior.

For this tutorial, we'll create a button that prints a message when clicked. For that, we'll need a script that extends from `Button`. It could also extend `BaseButton` if you prefer:

```csharp
using Godot;

// Optional, add to execute in the editor.
[Tool]

// Icons are optional.
// Alternatively, you may use the UID of the icon or the absolute path.
[Icon("icon.png")]

// Automatically register the node in the Create New Node dialog
// and make it available for use with other scripts.
[GlobalClass]
public partial class MyButton : Button
{
    public override void _EnterTree()
    {
        Pressed += Clicked;
    }

    public void Clicked()
    {
        GD.Print("You clicked me!");
    }
}
```

That's it for our basic button. You may have a 16x16 icon to show in the scene tree. If you don't have one, you can grab the default one from the engine and save it in your `addons/MyCustomNode` folder as `icon.png`, or use the default Godot logo (`preload("res://icon.svg")`).

> **Tip:** SVG images that are used as custom node icons should have the **Editor > Scale With Editor Scale** and **Editor > Convert Colors With Editor Theme** import options enabled. This allows icons to follow the editor's scale and theming settings if the icons are designed with the same color palette as Godot's own icons.

With that done, the plugin should already be available in the plugin list in the **Project Settings**, so activate it as explained in the "Checking the results" section.

Then try it out by adding your new node. When you add the node, you can see that it already has the script you created attached to it. Set a text to the button, save and run the scene. When you click the button, you can see some text in the console.

### A Custom Dock

Sometimes, you need to extend the editor and add tools that are always available. An easy way to do it is to add a new dock with a plugin. Docks are just scenes based on Control, so they are created in a way similar to usual GUI scenes.

Creating a custom dock is done just like a custom node. Create a new `plugin.cfg` file in the `addons/MyCustomDock` folder, then add the following content to it:

```ini
[plugin]

name="My Custom Dock"
description="A custom dock made so I can learn how to make plugins."
author="Your Name Here"
version="1.0"
script="CustomDock.cs"
```

Then create the script `CustomDock.cs` in the same folder. Fill it with the template we've seen before to get a good start.

Since we're trying to add a new custom dock, we need to create the contents of the dock. This is nothing more than a standard Godot scene: just create a new scene in the editor then edit it.

For an editor dock, the root node **must** be a `Control` or one of its child classes. For this tutorial, you can create a single button. Don't forget to add some text to your button.

Save this scene as `MyDock.tscn`. Now, we need to grab the scene we created then add it as a dock in the editor. For this, you can rely on the function `AddDock()` from the `EditorPlugin` class.

You need to select a dock position and define the control to add (which is the scene you just created). Don't forget to **remove the dock** when the plugin is deactivated. The script could look like this:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class CustomDock : EditorPlugin
{
    private EditorDock _dock;

    public override void _EnterTree()
    {
        var dockScene = GD.Load<PackedScene>("res://addons/MyCustomDock/MyDock.tscn").Instantiate<Control>();

        // Create the dock and add the loaded scene to it.
        _dock = new EditorDock();
        _dock.AddChild(dockScene);

        _dock.Title = "My Dock";

        // Note that LeftUl means the left of the editor, upper-left dock.
        _dock.DefaultSlot = DockSlot.LeftUl;

        // Allow the dock to be on the left or right of the editor, and to be made floating.
        _dock.AvailableLayouts = DockLayout.Horizontal | DockLayout.Floating;

        AddDock(_dock);
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        // Remove the dock.
        RemoveDock(_dock);
        // Erase the control from the memory.
        _dock.QueueFree();
    }
}
#endif
```

Note that, while the dock will initially appear at its specified position, the user can freely change its position and save the resulting layout.

### Checking the Results

It's now time to check the results of your work. Open the **Project Settings** and click on the **Plugins** tab. Your plugin should be the only one on the list. You can see the plugin is not enabled. Click the **Enable** checkbox to activate the plugin. The dock should become visible before you even close the settings window. You should now have a custom dock.

### Registering Autoloads/Singletons in Plugins

It is possible for editor plugins to automatically register autoloads when the plugin is enabled. This also includes unregistering the autoload when the plugin is disabled.

This makes setting up plugins faster for users, as they no longer have to manually add autoloads to their project settings if your editor plugin requires the use of an autoload.

Use the following code to register a singleton from an editor plugin:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MyEditorPlugin : EditorPlugin
{
    // Replace this value with a PascalCase autoload name.
    private const string AutoloadName = "SomeAutoload";

    public override void _EnablePlugin()
    {
        // The autoload can be a scene or script file.
        AddAutoloadSingleton(AutoloadName, "res://addons/MyAddon/SomeAutoload.tscn");
    }

    public override void _DisablePlugin()
    {
        RemoveAutoloadSingleton(AutoloadName);
    }
}
#endif
```

### Using Sub-Plugins

Often a plugin adds multiple things, for example a custom node and a panel. In those cases it might be easier to have a separate plugin script for each of those features. Sub-plugins can be used for this.

First create all plugins and sub plugins as normal plugins. Then move the sub plugins into the main plugin folder.

Godot will hide sub-plugins from the plugin list, so that a user can't enable or disable them. Instead the main plugin script should enable and disable sub-plugins like this:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MainPlugin : EditorPlugin
{
    // The main plugin is located at res://addons/my_plugin/
    private const string PluginName = "my_plugin";

    public override void _EnablePlugin()
    {
        EditorInterface.Singleton.SetPluginEnabled(PluginName + "/node", true);
        EditorInterface.Singleton.SetPluginEnabled(PluginName + "/panel", true);
    }

    public override void _DisablePlugin()
    {
        EditorInterface.Singleton.SetPluginEnabled(PluginName + "/node", false);
        EditorInterface.Singleton.SetPluginEnabled(PluginName + "/panel", false);
    }
}
#endif
```

### Going Beyond

Now that you've learned how to make basic plugins, you can extend the editor in several ways. Lots of functionality can be added to the editor with C#; it is a powerful way to create specialized editors without having to delve into C++ modules.

You can make your own plugins to help yourself and share them in the [Asset Library](https://godotengine.org/asset-library/) so that people can benefit from your work.

---

## 1.2 Making Main Screen Plugins

### What This Tutorial Covers

Main screen plugins allow you to create new UIs in the central part of the editor, which appear next to the "2D", "3D", "Script", "Game", and "AssetLib" buttons. Such editor plugins are referred as "Main screen plugins".

This tutorial leads you through the creation of a basic main screen plugin. For the sake of simplicity, our main screen plugin will contain a single button that prints text to the console.

### Initializing the Plugin

First create a new plugin from the Plugins menu. For this tutorial, we'll put it in a folder called `main_screen`, but you can use any name you'd like.

The plugin script will come with `_EnterTree()` and `_ExitTree()` methods, but for a main screen plugin we need to add a few extra methods. Add five extra methods such that the script looks like this:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MainScreenPlugin : EditorPlugin
{
    public override void _EnterTree()
    {

    }

    public override void _ExitTree()
    {

    }

    public override bool _HasMainScreen()
    {
        return true;
    }

    public override void _MakeVisible(bool visible)
    {

    }

    public override string _GetPluginName()
    {
        return "Main Screen Plugin";
    }

    public override Texture2D _GetPluginIcon()
    {
        return EditorInterface.Singleton.GetEditorTheme().GetIcon("Node", "EditorIcons");
    }
}
#endif
```

The important part in this script is the `_HasMainScreen()` function, which is overloaded so it returns `true`. This function is automatically called by the editor on plugin activation, to tell it that this plugin adds a new center view to the editor. For now, we'll leave this script as-is and we'll come back to it later.

### Main Screen Scene

Create a new scene with a root node derived from `Control` (for this example plugin, we'll make the root node a `CenterContainer`). Select this root node, and in the viewport, click the **Layout** menu and select **Full Rect**. You also need to enable the **Expand** vertical size flag in the inspector. The panel now uses all the space available in the main viewport.

Next, let's add a button to our example main screen plugin. Add a `Button` node, and set the text to "Print Hello" or similar. Add a script to the button like this:

```csharp
using Godot;

[Tool]
public partial class PrintHello : Button
{
    private void OnPrintHelloPressed()
    {
        GD.Print("Hello from the main screen plugin!");
    }
}
```

Then connect the "pressed" signal to itself. We are done with the main screen panel. Save the scene as `main_panel.tscn`.

### Update the Plugin Script

We need to update the plugin script so the plugin instances our main panel scene and places it where it needs to be. Here is the full plugin script:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MainScreenPlugin : EditorPlugin
{
    PackedScene MainPanel = ResourceLoader.Load<PackedScene>("res://addons/main_screen/main_panel.tscn");
    Control MainPanelInstance;

    public override void _EnterTree()
    {
        MainPanelInstance = (Control)MainPanel.Instantiate();
        // Add the main panel to the editor's main viewport.
        EditorInterface.Singleton.GetEditorMainScreen().AddChild(MainPanelInstance);
        // Hide the main panel. Very much required.
        _MakeVisible(false);
    }

    public override void _ExitTree()
    {
        if (MainPanelInstance != null)
        {
            MainPanelInstance.QueueFree();
        }
    }

    public override bool _HasMainScreen()
    {
        return true;
    }

    public override void _MakeVisible(bool visible)
    {
        if (MainPanelInstance != null)
        {
            MainPanelInstance.Visible = visible;
        }
    }

    public override string _GetPluginName()
    {
        return "Main Screen Plugin";
    }

    public override Texture2D _GetPluginIcon()
    {
        // Must return some kind of Texture for the icon.
        return EditorInterface.Singleton.GetEditorTheme().GetIcon("Node", "EditorIcons");
    }
}
#endif
```

A couple of specific lines were added. `MainPanel` is a constant that holds a reference to the scene, and we instance it into `MainPanelInstance`.

The `_EnterTree()` function is called before `_Ready()`. This is where we instance the main panel scene, and add them as children of specific parts of the editor. We use `EditorInterface.GetEditorMainScreen()` to obtain the main editor screen and add our main panel instance as a child to it. We call the `_MakeVisible(false)` function to hide the main panel so it doesn't compete for space when first activating the plugin.

The `_ExitTree()` function is called when the plugin is deactivated. If the main screen still exists, we call `QueueFree()` to free the instance and remove it from memory.

The `_MakeVisible()` function is overridden to hide or show the main panel as needed. This function is automatically called by the editor when the user clicks on the main viewport buttons at the top of the editor.

The `_GetPluginName()` and `_GetPluginIcon()` functions control the displayed name and icon for the plugin's main viewport button.

Another function you can add is the `_Handles()` function, which allows you to handle a node type, automatically focusing the main screen when the type is selected. This is similar to how clicking on a 3D node will automatically switch to the 3D viewport.

### Try the Plugin

Activate the plugin in the Project Settings. You'll observe a new button next to 2D, 3D, Script above the main viewport. Clicking it will take you to your new main screen plugin, and the button in the middle will print text.

If you would like to try a finished version of this plugin, check out the plugin demos here: https://github.com/godotengine/godot-demo-projects/tree/master/plugins

If you would like to see a more complete example of what main screen plugins are capable of, check out the 2.5D demo projects here: https://github.com/godotengine/godot-demo-projects/tree/master/misc/2.5d

---

## 1.3 Import Plugins

> **Note:** This tutorial assumes familiarity with generic plugin creation (see Making Plugins above) and Godot's import system.

### Introduction

An import plugin is a special type of editor tool that allows custom resources to be imported by Godot and be treated as first-class resources. The editor includes import plugins for PNG images, Collada and glTF models, Ogg Vorbis sounds, and more.

This tutorial demonstrates creating an import plugin that loads custom text files as material resources. The text file contains three comma-separated numeric values representing RGB color channels. Example content:

```
0,0,255
```

### Configuration

Create `plugin.cfg`:

```ini
[plugin]

name="Silly Material Importer"
description="Imports a 3D Material from an external text file."
author="Yours Truly"
version="1.0"
script="MaterialImport.cs"
```

Create `MaterialImport.cs`:

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MaterialImport : EditorPlugin
{
    private EditorImportPlugin _importPlugin;

    public override void _EnterTree()
    {
        _importPlugin = new SillyMaterialImporter();
        AddImportPlugin(_importPlugin);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(_importPlugin);
        _importPlugin = null;
    }
}
#endif
```

When this plugin is activated, it will create a new instance of the import plugin and add it to the editor using the `AddImportPlugin()` method. A reference is stored in `_importPlugin` for later removal. The `RemoveImportPlugin()` method is called when the plugin is deactivated to clean up the memory and let the editor know the import plugin isn't available anymore.

> **Note:** The import plugin is a reference type, so it doesn't need to be explicitly released from memory with the `Free()` function. It will be released automatically by the engine when it goes out of scope.

### The EditorImportPlugin Class

The main character of the show is the `EditorImportPlugin` class. It is responsible for implementing the methods that are called by Godot when it needs to know how to deal with files.

```csharp
#if TOOLS
using Godot;
using Godot.Collections;

[Tool]
public partial class SillyMaterialImporter : EditorImportPlugin
{
    private enum Presets
    {
        Default
    }

    public override string _GetImporterName()
    {
        return "demos.sillymaterial";
    }

    public override string _GetVisibleName()
    {
        return "Silly Material";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return new string[] { "mtxt" };
    }

    public override string _GetSaveExtension()
    {
        return "material";
    }

    public override string _GetResourceType()
    {
        return "StandardMaterial3D";
    }

    public override int _GetPresetCount()
    {
        return System.Enum.GetValues(typeof(Presets)).Length;
    }

    public override string _GetPresetName(int presetIndex)
    {
        switch ((Presets)presetIndex)
        {
            case Presets.Default:
                return "Default";
            default:
                return "Unknown";
        }
    }

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        switch ((Presets)presetIndex)
        {
            case Presets.Default:
                return new Array<Dictionary>
                {
                    new Dictionary
                    {
                        { "name", "use_red_anyway" },
                        { "default_value", false }
                    }
                };
            default:
                return new Array<Dictionary>();
        }
    }

    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options)
    {
        return true;
    }

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
        Array<string> platformVariants, Array<string> genFiles)
    {
        using var file = FileAccess.Open(sourceFile, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return FileAccess.GetOpenError();
        }

        string line = file.GetLine();

        string[] channels = line.Split(",");
        if (channels.Length != 3)
        {
            return Error.ParseError;
        }

        Color color;
        if ((bool)options["use_red_anyway"])
        {
            color = Color.FromRgba8(255, 0, 0);
        }
        else
        {
            color = Color.FromRgba8(
                int.Parse(channels[0]),
                int.Parse(channels[1]),
                int.Parse(channels[2])
            );
        }

        var material = new StandardMaterial3D();
        material.AlbedoColor = color;

        return ResourceSaver.Save(material, $"{savePath}.{_GetSaveExtension()}");
    }
}
#endif
```

Method explanations:

- `_GetImporterName()`: This is a unique name for your plugin that is used by Godot to know which import was used in a certain file. When the file needs to be reimported, the editor will know which plugin to call.
- `_GetVisibleName()`: This method is responsible for returning the name of the type it imports and it will be shown to the user in the Import dock. You should choose this name as a continuation to "Import as", e.g. "Import as Silly Material".
- `_GetRecognizedExtensions()`: Godot's import system detects file types by their extension. This method returns an array of strings to represent each extension that this plugin can understand. If an extension is recognized by more than one plugin, the user can select which one to use when importing the files.
- `_GetSaveExtension()`: The imported files are saved in the `.import` folder at the project's root. Their extension should match the type of resource you are importing.
- `_GetResourceType()`: The imported resource has a specific type, so the editor can know which property slot it belongs to. This allows drag and drop from the FileSystem dock to a property in the Inspector.
- `_GetPresetCount()`: Returns the amount of presets that this plugin defines.
- `_GetPresetName()`: Gives names to the presets as they will be presented to the user, so be sure to use short and clear names.
- `_GetImportOptions()`: This method defines the available options. It returns an array of dictionaries, and each dictionary contains keys to customize the option as it's shown to the user.

> **Tip:** Common extensions like `.json` and `.txt` might be used by many plugins. Also, there could be files in the project that are just data for the game and should not be imported. You have to be careful when importing to validate the data. Never expect the file to be well-formed.

> **Note:** If you need to import different types from the same extension, you have to create multiple import plugins. You can abstract the import code on another file to avoid duplication in this regard.

Import options table:

| Key | Type | Description |
|-----|------|-------------|
| `name` | String | The option name; underscores become spaces, first letters capitalized |
| `default_value` | Any | Default value for this preset |
| `property_hint` | Enum value | One of the PropertyHint values as hint |
| `hint_string` | String | Hint text for the property |
| `usage` | Enum value | One of PropertyUsageFlags values |

The `name` and `default_value` keys are **mandatory**, the rest are optional.

> **Warning:** The `_GetImportOptions()` method is called even if you don't define presets (by making `_GetPresetCount()` return zero). You have to return an array even if it's empty, otherwise you can get errors.

### Platform Variants and Generated Files

The `_Import()` method receives two return arguments: `platformVariants` and `genFiles`.

The `platformVariants` argument is used if you need to import the resource differently depending on the target platform. While it's called *platform* variants, it is based on the presence of feature tags, so even the same platform can have multiple variants depending on the setup.

To import a platform variant, you need to save it with the feature tag before the extension, and then push the tag to the `platformVariants` array so the editor can know that you did:

```csharp
platformVariants.Add("mobile");
return ResourceSaver.Save(mobileMaterial, $"{savePath}.mobile.{_GetSaveExtension()}");
```

The `genFiles` argument is meant for extra files that are generated during your import process and need to be kept. The editor will look at it to understand the dependencies and make sure the extra file is not inadvertently deleted. This is also an array and should be filled with full paths of the files you save:

```csharp
var nextPass = new StandardMaterial3D();
nextPass.AlbedoColor = color.Inverted();
string nextPassPath = $"{savePath}.next_pass.{_GetSaveExtension()}";

Error err = ResourceSaver.Save(nextPass, nextPassPath);
if (err != Error.Ok)
{
    return err;
}
genFiles.Add(nextPassPath);
```

### Trying the Plugin

This has been theoretical, but now that the import plugin is done, let's test it. Make sure you created the sample file (with the contents described in the introduction section) and save it as `test.mtxt`. Then activate the plugin in the Project Settings.

If everything goes well, the import plugin is added to the editor and the file system is scanned, making the custom resource appear on the FileSystem dock. If you select it and focus the Import dock, you can see the only option to select there.

Create a `MeshInstance3D` node in the scene, and for its Mesh property set up a new `SphereMesh`. Unfold the Material section in the Inspector and then drag the file from the FileSystem dock to the material property. The object will update in the viewport with the blue color of the imported material.

Go to Import dock, enable the "Use Red Anyway" option, and click on "Reimport". This will update the imported material and should automatically update the view showing the red color instead.

---

## 1.4 Inspector Plugins

The inspector dock allows you to create custom widgets to edit properties through plugins. This can be beneficial when working with custom datatypes and resources, although you can use the feature to change the inspector widgets for built-in types. You can design custom controls for specific properties, entire objects, and even separate controls associated with particular datatypes.

This guide explains how to use the `EditorInspectorPlugin` and `EditorProperty` classes to create a custom interface for integers, replacing the default behavior with a button that generates random values between 0 and 99.

### Setting Up Your Plugin

Create a new empty plugin to get started.

Let's assume you've called your plugin folder `my_inspector_plugin`. If so, you should end up with a new `addons/my_inspector_plugin` folder that contains two files: `plugin.cfg` and `Plugin.cs`.

As before, `Plugin.cs` is a script extending `EditorPlugin` and you need to introduce new code for its `_EnterTree` and `_ExitTree` methods. To set up your inspector plugin, you must load its script, then create and add the instance by calling `AddInspectorPlugin()`. If the plugin is disabled, you should remove the instance you have added by calling `RemoveInspectorPlugin()`.

> **Note:** Here, you are loading a script and not a packed scene. Therefore you should use `new()` instead of `Instantiate()`.

```csharp
// Plugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class Plugin : EditorPlugin
{
    private MyInspectorPlugin _plugin;

    public override void _EnterTree()
    {
        _plugin = new MyInspectorPlugin();
        AddInspectorPlugin(_plugin);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_plugin);
    }
}
#endif
```

### Interacting with the Inspector

To interact with the inspector dock, your inspector plugin class must extend the `EditorInspectorPlugin` class. This class provides several virtual methods that affect how the inspector handles properties.

To have any effect at all, the script must implement the `_CanHandle()` method. This function is called for each edited `Object` and must return `true` if this plugin should handle the object or its properties.

> **Note:** This includes any `Resource` attached to the object.

You can implement four other methods to add controls to the inspector at specific positions. The `_ParseBegin()` and `_ParseEnd()` methods are called only once at the beginning and the end of parsing for each object, respectively. They can add controls at the top or bottom of the inspector layout by calling `AddCustomControl()`.

As the editor parses the object, it calls the `_ParseCategory()` and `_ParseProperty()` methods. There, in addition to `AddCustomControl()`, you can call both `AddPropertyEditor()` and `AddPropertyEditorForMultipleProperties()`. Use these last two methods to specifically add `EditorProperty`-based controls.

```csharp
// MyInspectorPlugin.cs
#if TOOLS
using Godot;

public partial class MyInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object)
    {
        // We support all objects in this example.
        return true;
    }

    public override bool _ParseProperty(GodotObject @object, Variant.Type type,
        string name, PropertyHint hintType, string hintString,
        PropertyUsageFlags usageFlags, bool wide)
    {
        // We handle properties of type integer.
        if (type == Variant.Type.Int)
        {
            // Create an instance of the custom property editor and register
            // it to a specific property path.
            AddPropertyEditor(name, new RandomIntEditor());
            // Inform the editor to remove the default property editor for
            // this property type.
            return true;
        }

        return false;
    }
}
#endif
```

### Adding an Interface to Edit Properties

The `EditorProperty` class is a special type of `Control` that can interact with the inspector dock's edited objects. It doesn't display anything but can house any other control nodes, including complex scenes.

There are three essential parts to the script extending `EditorProperty`:

1. You must define the constructor to set up the control nodes' structure.
2. You should implement the `_UpdateProperty()` method to handle changes to the data from the outside.
3. A signal must be emitted at some point to inform the inspector that the control has changed the property using `EmitChanged`.

You can display your custom widget in two ways. Use just the default `AddChild()` method to display it to the right of the property name, and use `AddChild()` followed by `SetBottomEditor()` to position it below the name.

```csharp
// RandomIntEditor.cs
#if TOOLS
using Godot;

public partial class RandomIntEditor : EditorProperty
{
    // The main control for editing the property.
    private Button _propertyControl = new Button();
    // An internal value of the property.
    private int _currentValue = 0;
    // A guard against internal changes when the property is updated.
    private bool _updating = false;

    public RandomIntEditor()
    {
        // Add the control as a direct child of EditorProperty node.
        AddChild(_propertyControl);
        // Make sure the control is able to retain the focus.
        AddFocusable(_propertyControl);
        // Setup the initial state and connect to the signal to track changes.
        RefreshControlText();
        _propertyControl.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        // Ignore the signal if the property is currently being updated.
        if (_updating)
        {
            return;
        }

        // Generate a new random integer between 0 and 99.
        _currentValue = (int)GD.Randi() % 100;
        RefreshControlText();
        EmitChanged(GetEditedProperty(), _currentValue);
    }

    public override void _UpdateProperty()
    {
        // Read the current value from the property.
        var newValue = (int)GetEditedObject().Get(GetEditedProperty());
        if (newValue == _currentValue)
        {
            return;
        }

        // Update the control with the new value.
        _updating = true;
        _currentValue = newValue;
        RefreshControlText();
        _updating = false;
    }

    private void RefreshControlText()
    {
        _propertyControl.Text = $"Value: {_currentValue}";
    }
}
#endif
```

Using the example code above you should be able to make a custom widget that replaces the default `SpinBox` control for integers with a `Button` that generates random values.

---

## 1.5 3D Gizmo Plugins

> **Note:** This article is marked as outdated in the official docs.

### Introduction

3D gizmo plugins are used by the editor and custom plugins to define the gizmos attached to any kind of Node3D node.

This tutorial shows the two main approaches to defining your own custom gizmos. The first option works well for simple gizmos and creates less clutter in your plugin structure, and the second one will let you store some per-gizmo data.

> **Note:** This tutorial assumes you already know how to make generic plugins. If in doubt, refer to the Making Plugins section above.

### The EditorNode3DGizmoPlugin

Regardless of the approach we choose, we will need to create a new `EditorNode3DGizmoPlugin`. This will allow us to set a name for the new gizmo type and define other behaviors such as whether the gizmo can be hidden or not.

This would be a basic setup:

```csharp
// MyCustomGizmoPlugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class MyCustomGizmoPlugin : EditorNode3DGizmoPlugin
{
    public override string _GetGizmoName()
    {
        return "CustomNode";
    }
}
#endif
```

```csharp
// MyCustomEditorPlugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class MyCustomEditorPlugin : EditorPlugin
{
    private MyCustomGizmoPlugin _gizmoPlugin;

    public override void _EnterTree()
    {
        _gizmoPlugin = new MyCustomGizmoPlugin();
        AddNode3DGizmoPlugin(_gizmoPlugin);
    }

    public override void _ExitTree()
    {
        RemoveNode3DGizmoPlugin(_gizmoPlugin);
    }
}
#endif
```

For simple gizmos, inheriting `EditorNode3DGizmoPlugin` is enough. If you want to store some per-gizmo data, you should go with the second approach.

### Simple Approach

The first step is to, in our custom gizmo plugin, override the `_HasGizmo()` method so that it returns `true` when the node parameter is of our target type.

```csharp
#if TOOLS
using Godot;

[Tool]
public partial class MyCustomGizmoPlugin : EditorNode3DGizmoPlugin
{
    public override bool _HasGizmo(Node3D node)
    {
        return node is MyCustomNode3D;
    }

    public MyCustomGizmoPlugin()
    {
        CreateMaterial("main", new Color(1, 0, 0));
        CreateHandleMaterial("handles");
    }

    public override void _Redraw(EditorNode3DGizmo gizmo)
    {
        gizmo.Clear();

        var node3d = gizmo.GetNode3D();

        var lines = new PackedVector3Array();

        lines.PushBack(new Vector3(0, 1, 0));
        lines.PushBack(new Vector3(0, ((MyCustomNode3D)node3d).MyCustomValue, 0));

        var handles = new PackedVector3Array();

        handles.PushBack(new Vector3(0, 1, 0));
        handles.PushBack(new Vector3(0, ((MyCustomNode3D)node3d).MyCustomValue, 0));

        gizmo.AddLines(lines, GetMaterial("main", gizmo), false);
        gizmo.AddHandles(handles, GetMaterial("handles", gizmo), new int[] { });
    }

    public override string _GetGizmoName()
    {
        return "CustomNode";
    }

    // You should implement the rest of handle-related callbacks
    // (_GetHandleName(), _GetHandleValue(), _CommitHandle(), ...).
}
#endif
```

Note that we created a material in the constructor, and retrieved it in the `_Redraw()` method using `GetMaterial()`. This method retrieves one of the material's variants depending on the state of the gizmo (selected and/or editable).

Note that we just added some handles in the `_Redraw()` method, but we still need to implement the rest of handle-related callbacks in `EditorNode3DGizmoPlugin` to get properly working handles.

### Alternative Approach

In some cases we want to provide our own implementation of `EditorNode3DGizmo`, maybe because we want to have some state stored in each gizmo or because we are porting an old gizmo plugin and we don't want to go through the rewriting process.

In these cases all we need to do is, in our new gizmo plugin, override `_CreateGizmo()`, so it returns our custom gizmo implementation for the Node3D nodes we want to target.

```csharp
// MyCustomGizmoPlugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class MyCustomGizmoPlugin : EditorNode3DGizmoPlugin
{
    public MyCustomGizmoPlugin()
    {
        CreateMaterial("main", new Color(1, 0, 0));
        CreateHandleMaterial("handles");
    }

    public override EditorNode3DGizmo _CreateGizmo(Node3D node)
    {
        if (node is MyCustomNode3D)
        {
            return new MyCustomGizmo();
        }
        else
        {
            return null;
        }
    }
}
#endif
```

This way all the gizmo logic and drawing methods can be implemented in a new class extending `EditorNode3DGizmo`, like so:

```csharp
// MyCustomGizmo.cs
#if TOOLS
using Godot;

[Tool]
public partial class MyCustomGizmo : EditorNode3DGizmo
{
    // You can store data in the gizmo itself (more useful when working with handles).
    public float GizmoSize = 3.0f;

    public override void _Redraw()
    {
        Clear();

        var node3d = GetNode3D();

        var lines = new PackedVector3Array();

        lines.PushBack(new Vector3(0, 1, 0));
        lines.PushBack(new Vector3(GizmoSize, ((MyCustomNode3D)node3d).MyCustomValue, 0));

        var handles = new PackedVector3Array();

        handles.PushBack(new Vector3(0, 1, 0));
        handles.PushBack(new Vector3(GizmoSize, ((MyCustomNode3D)node3d).MyCustomValue, 0));

        var material = GetPlugin().GetMaterial("main", this);
        AddLines(lines, material, false);

        var handlesMaterial = GetPlugin().GetMaterial("handles", this);
        AddHandles(handles, handlesMaterial, new int[] { });
    }

    // You should implement the rest of handle-related callbacks
    // (_GetHandleName(), _GetHandleValue(), _CommitHandle(), ...).
}
#endif
```

Note that we just added some handles in the `_Redraw()` method, but we still need to implement the rest of handle-related callbacks in `EditorNode3DGizmo` to get properly working handles.

---

## Part 2: INTERNATIONALIZATION (I18N)

---

## 2.1 Localization Using Gettext (PO Files)

In addition to importing translations in CSV format, Godot also supports loading translation files written in the GNU gettext format (text-based `.po` and compiled `.mo`).

> **Note:** For an introduction to gettext, check out [A Quick Gettext Tutorial](https://www.labri.fr/perso/fleury/posts/programming/a-quick-gettext-tutorial.html). It's written with C projects in mind, but much of the advice also applies to Godot (with the exception of `xgettext`). For the complete documentation, see [GNU Gettext](https://www.gnu.org/software/gettext/manual/gettext.html).

### Advantages

- gettext is a standard format, which can be edited using any text editor or GUI editors such as [Poedit](https://poedit.net/). This can be significant as it provides a lot of tools for translators, such as marking outdated strings, finding strings that haven't been translated, etc.
- gettext is supported by translation platforms such as [Transifex](https://www.transifex.com/) and [Weblate](https://weblate.org/), which makes it easier for people to collaborate to localization.
- Compared to CSV, gettext files work better with version control systems like Git, as each locale has its own messages file.
- Multiline strings are more convenient to edit in gettext PO files compared to CSV files.

### Disadvantages

- gettext PO files have a more complex format than CSV and can be harder to grasp for people new to software localization.
- People who maintain localization files will have to install gettext tools on their system. However, as Godot supports using text-based message files (`.po`), translators can test their work without having to install gettext tools.
- gettext PO files usually use English as the base language. Translators will use this base language to translate to other languages. You could still use other languages as the base language, but this is not common.

### Installing Gettext Tools

The command line gettext tools are required to perform maintenance operations, such as updating message files. Therefore, it's strongly recommended to install them.

- **Windows:** Download an installer from [this page](https://mlocati.github.io/articles/gettext-iconv-windows.html). Any architecture and binary type (shared or static) works; if in doubt, choose the 64-bit static installer.
- **macOS:** Install gettext either using [Homebrew](https://brew.sh/) with the `brew install gettext` command, or using [MacPorts](https://www.macports.org/) with the `sudo port install gettext` command.
- **Linux:** On most distributions, install the `gettext` package from your distribution's package manager.

For a GUI tool you can get Poedit from its [Official website](https://poedit.net/). The basic version is open source and available under the MIT license.

### Creating the PO Template

#### Automatic Generation Using the Editor

The editor can generate a PO template automatically from specified scene and script files. This POT generation also supports translation contexts and pluralization if used in a script, with the optional second argument of `Tr()` and the `TrN()` method.

Open **Project > Project Settings > Localization > Template Generation**, then use the **Add...** button to specify the path to your project's scenes and scripts that contain localizable strings.

After adding at least one scene or script, click **Generate** in the top-right corner, then specify the path to the output file with a `pot` file extension. This file can be placed anywhere in the project directory, but it's recommended to keep it in a subdirectory such as `locale`, as each locale will be defined in its own file.

> **Note:** Remember to regenerate the PO template after making any changes to localizable strings, or after adding new scenes or scripts. Otherwise, newly added strings will not be localizable and translators won't be able to update translations for outdated strings.

#### Manual Creation

If the automatic generation approach doesn't work out for your needs, you can create a PO template by hand in a text editor. This file can be placed anywhere in the project directory, but it's recommended to keep it in a subdirectory, as each locale will be defined in its own file.

Create a directory named `locale` in the project directory. In this directory, save a file named `messages.pot` with the following content:

```
# Don't remove the two lines below, they're required for gettext to work correctly.
msgid ""
msgstr ""

# Example of a regular string.
msgid "Hello world!"
msgstr ""

# Example of a string with pluralization.
msgid "There is %d apple."
msgid_plural "There are %d apples."
msgstr[0] ""
msgstr[1] ""

# Example of a string with a translation context.
msgctxt "Actions"
msgid "Close"
msgstr ""
```

Messages in gettext are made of `msgid` and `msgstr` pairs. `msgid` is the source string (usually in English), `msgstr` will be the translated string.

> **Warning:** The `msgstr` value in PO template files (`.pot`) should **always** be empty. Localization will be done in the generated `.po` files instead.

### Creating a Messages File from a PO Template

The `msginit` command is used to turn a PO template into a messages file. For instance, to create a French localization file, use the following command while in the `locale` directory:

```shell
msginit --no-translator --input=messages.pot --locale=fr
```

The command above will create a file named `fr.po` in the same directory as the PO template.

Alternatively, you can do that graphically using Poedit, or by uploading the POT file to your web platform of choice.

### Loading a Messages File in Godot

To register a messages file as a translation in a project, open the **Project Settings**, then go to **Localization > Translations**, click **Add...** then choose the `.po` or `.mo` file in the file dialog. The locale will be inferred from the `"Language: <code>\n"` property in the messages file.

### Updating Message Files to Follow the PO Template

After updating the PO template, you will have to update message files so that they contain new strings, while removing strings that are no longer present in the PO template. This can be done automatically using the `msgmerge` tool:

```shell
# The order matters: specify the message file *then* the PO template!
msgmerge --update --backup=none fr.po messages.pot
```

If you want to keep a backup of the original message file (which would be saved as `fr.po~` in this example), remove the `--backup=none` argument.

> **Note:** After running `msgmerge`, strings which were modified in the source language will have a "fuzzy" comment added before them in the `.po` file. This comment denotes that the translation should be updated to match the new source string, as the translation will most likely be inaccurate until it's updated. Strings with "fuzzy" comments will **not** be read by Godot until the translation is updated and the "fuzzy" comment is removed.

### Checking the Validity of a PO File or Template

It is possible to check whether a gettext file's syntax is valid. If you open with Poedit, it will display the appropriate warnings if there's some syntax errors. You can also verify by running the gettext command below:

```shell
msgfmt fr.po --check
```

If there are syntax errors or warnings, they will be displayed in the console. Otherwise, `msgfmt` won't output anything.

### Using Binary MO Files (Useful for Large Projects Only)

For large projects with several thousands of strings to translate or more, it can be worth it to use binary (compiled) MO message files instead of text-based PO files. Binary MO files are smaller and faster to read than the equivalent PO files.

You can generate an MO file with the command below:

```shell
msgfmt fr.po --no-hash -o fr.mo
```

If the PO file is valid, this command will create an `fr.mo` file besides the PO file. This MO file can then be loaded in Godot as described above.

The original PO file should be kept in version control so you can update your translation in the future. In case you lose the original PO file and wish to decompile an MO file into a text-based PO file, you can do so with:

```shell
msgunfmt fr.mo > fr.po
```

The decompiled file will not include comments or fuzzy strings, as these are never compiled in the MO file in the first place.

### Extracting Localizable Strings from Script Files

The built-in editor plugin recognizes a variety of patterns in source code to extract localizable strings from script files, including but not limited to the following:

- `Tr()`, `TrN()`, `Atr()`, and `AtrN()` calls;
- assigning properties `Text`, `PlaceholderText`, and `TooltipText`;
- `AddTab()`, `AddItem()`, `SetTabTitle()`, and other calls;
- `FileDialog` filters like `"*.png ; PNG Images"`.

> **Note:** The argument or right operand must be a constant string, otherwise the plugin will not be able to evaluate the expression and will ignore it.

If the plugin extracts unnecessary strings, you can ignore them with the `NO_TRANSLATE` comment. You can also provide additional information for translators using the `TRANSLATORS:` comment. These comments must be placed either on the same line as the recognized pattern or precede it.

```csharp
characterName.Text = "???"; // NO_TRANSLATE

// NO_TRANSLATE: Language name.
tabContainer.SetTabTitle(0, "Python");

item.Text = "Tool"; // TRANSLATORS: Up to 10 characters.

// TRANSLATORS: This is a reference to Lewis Carroll's poem "Jabberwocky",
// make sure to keep this as it is important to the plot.
Say(Tr("He took his vorpal sword in hand. The end?"));
```

### Using Context

The `context` parameter can be used to differentiate the situation where a translation is used, or to differentiate polysemic words (words with multiple meanings).

For example:

```csharp
Tr("Start", "Main Menu");
Tr("End", "Main Menu");
Tr("Shop", "Main Menu");
Tr("Shop", "In Game");
```

In a gettext PO file, a string with a context can be defined as follows:

```
# Example of a string with a translation context.
msgctxt "Main Menu"
msgid "Shop"
msgstr ""

# A different source string that is identical, but with a different context.
msgctxt "In Game"
msgid "Shop"
msgstr ""
```

### Updating PO Files

Some time or later, you'll add new content to your game, and there will be new strings that need to be translated. When this happens, you'll need to update the existing PO files to include the new strings.

First, generate a new POT file containing all the existing strings plus the newly added strings. After that, merge the existing PO files with the new POT file. There are two ways to do this:

- Use a gettext editor, and it should have an option to update a PO file from a POT file.
- Use the gettext `msgmerge` tool:

```shell
# The order matters: specify the message file *then* the PO template!
msgmerge --update --backup=none fr.po messages.pot
```

If you want to keep a backup of the original message file (which would be saved as `fr.po~` in this example), remove the `--backup=none` argument.

### POT Generation Custom Plugin

If you have any extra file format to deal with, you could write a custom plugin to parse and extract the strings from the custom file. This custom plugin will extract the strings and write into the POT file when you hit **Generate POT**. To learn more about how to create the translation parser plugin, see `EditorTranslationParserPlugin`.

---

## 2.2 Pseudolocalization

### Introduction

When creating a game, the process of localization usually starts when development has finished. This means that translations aren't available during development for testing whether the project is internationalized properly.

Godot offers pseudolocalization as a way to test how robust the project is when it comes to locale changes. Pseudolocalization simulates changes that might take place during localization. This way, any issues regarding internationalization can be recognized early on during development.

> You can see how pseudolocalization works in action using the [Pseudolocalization demo project](https://github.com/godotengine/godot-demo-projects/tree/master/gui/pseudolocalization).

### Enabling and Configuring Pseudolocalization

Enabling pseudolocalization and the configurations related to it is as simple as toggling a checkbox in the project settings. These settings can be found in **Project > Project Settings > General > Internationalization > Pseudolocalization** after enabling the **Advanced** toggle in the project settings dialog.

Pseudolocalization can also be toggled at runtime from a script.

### Pseudolocalization Configurations

Pseudolocalization in Godot can be set up according to the specific use case of the project. Here are the pseudolocalization properties that can be configured through project settings:

- **`replace_with_accents`**: Replaces all characters in the string with their accented variants. *"The quick brown fox jumped over the lazy dog"* will be converted to *"Thé qüick bröwn föx jümped över thé lazy dög"* when this setting is enabled. This can be used to spot untranslated strings that won't have accents, but is also useful to check for missing glyphs in the font(s) used by the project.
- **`double_vowels`**: Doubles all the vowels in the string. It is a good approximation to simulate expansion of text during localization. This can be used to check for text that would overflow its container (such as buttons).
- **`fake_bidi`**: Fake bidirectional text (simulates right-to-left text). This is useful to simulate right-to-left writing systems to check for potential layout issues that would occur in languages using right-to-left scripts.
- **`override`**: Replaces all the characters in the string with an asterisk (`*`). This is useful for quickly finding text that isn't being localized.
- **`expansion_ratio`**: Can be used in cases where doubling the vowels isn't a sufficient approximation. This setting pads the string with underscores (`_`) and expands it by the given ratio. An expansion ratio of `0.3` is sufficient for most practical cases; it will increase the length of the string by 30%.
- **`prefix`** and **`suffix`**: These properties can be used to specify a prefix and suffix to wrap the text in.
- **`skip_placeholders`**: Skips placeholders for string formatting like `%s` and `%f`. This is useful to identify places where more arguments are required for the formatted string to display correctly.

All of these properties can be toggled as needed according to the project's use case.

### Configuring Pseudolocalization at Runtime

Pseudolocalization can be toggled at runtime using the `TranslationServer.PseudolocalizationEnabled` property. However, if runtime configuration of pseudolocalization properties is required, they can be directly configured using `ProjectSettings.SetSetting(property, value)` and then calling `TranslationServer.ReloadPseudolocalization()` which reparses the pseudolocalization properties and reloads the pseudolocalization. The following code snippet shall turn on `replace_with_accents` and `double_vowels` properties and then call `ReloadPseudolocalization()` for the changes to get reflected:

```csharp
ProjectSettings.SetSetting("internationalization/pseudolocalization/replace_with_accents", true);
ProjectSettings.SetSetting("internationalization/pseudolocalization/double_vowels", true);
TranslationServer.Singleton.ReloadPseudolocalization();
```

---

## Part 3: MATH

---

## 3.1 Vector Math

### Introduction

This tutorial is a short and practical introduction to linear algebra as it applies to game development. Linear algebra is the study of vectors and their uses. Vectors have many applications in both 2D and 3D development and Godot uses them extensively. Developing a good understanding of vector math is essential to becoming a strong game developer.

> **Note:** This tutorial is **not** a formal textbook on linear algebra. We will only be looking at how it is applied to game development. For a broader look at the mathematics, see https://www.khanacademy.org/math/linear-algebra

### Coordinate Systems (2D)

In 2D space, coordinates are defined using a horizontal axis (`x`) and a vertical axis (`y`). A particular position in 2D space is written as a pair of values such as `(4, 3)`.

> **Note:** If you're new to computer graphics, it might seem odd that the positive `y` axis points **downwards** instead of upwards, as you probably learned in math class. However, this is common in most computer graphics applications.

Any position in the 2D plane can be identified by a pair of numbers in this way. However, we can also think of the position `(4, 3)` as an **offset** from the `(0, 0)` point, or **origin**. Draw an arrow pointing from the origin to the point.

This is a **vector**. A vector represents a lot of useful information. As well as telling us that the point is at `(4, 3)`, we can also think of it as an angle (theta) and a length (or magnitude) `m`. In this case, the arrow is a **position vector** - it denotes a position in space, relative to the origin.

A very important point to consider about vectors is that they only represent **relative** direction and magnitude. There is no concept of a vector's position. The following two vectors are identical: both vectors represent a point 4 units to the right and 3 units below some starting point. It does not matter where on the plane you draw the vector, it always represents a relative direction and magnitude.

### Vector Operations

You can use either method (x and y coordinates or angle and magnitude) to refer to a vector, but for convenience, programmers typically use the coordinate notation. For example, in Godot, the origin is the top-left corner of the screen, so to place a 2D node named `Node2D` 400 pixels to the right and 300 pixels down, use the following code:

```csharp
var node2D = GetNode<Node2D>("Node2D");
node2D.Position = new Vector2(400, 300);
```

Godot supports both `Vector2` and `Vector3` for 2D and 3D usage, respectively. The same mathematical rules discussed in this article apply to both types, and wherever we link to `Vector2` methods in the class reference, you can also check out their `Vector3` counterparts.

#### Member Access

The individual components of the vector can be accessed directly by name.

```csharp
// Create a vector with coordinates (2, 5).
var a = new Vector2(2, 5);
// Create a vector and assign x and y manually.
var b = new Vector2();
b.X = 3;
b.Y = 1;
```

#### Adding Vectors

When adding or subtracting two vectors, the corresponding components are added:

```csharp
var c = a + b;  // (2, 5) + (3, 1) = (5, 6)
```

We can also see this visually by adding the second vector at the end of the first. Note that adding `a + b` gives the same result as `b + a`.

#### Scalar Multiplication

> **Note:** Vectors represent both direction and magnitude. A value representing only magnitude is called a **scalar**. Scalars use the `float` type in Godot.

A vector can be multiplied by a **scalar**:

```csharp
var c = a * 2;  // (2, 5) * 2 = (4, 10)
var d = b / 3;  // (3, 6) / 3 = (1, 2)
var e = d * -2; // (1, 2) * -2 = (-2, -4)
```

> **Note:** Multiplying a vector by a positive scalar does not change its direction, only its magnitude. Multiplying with a negative scalar results in a vector in the opposite direction. This is how you **scale** a vector.

### Practical Applications

Let's look at two common uses for vector addition and subtraction.

#### Movement

A vector can represent **any** quantity with a magnitude and direction. Typical examples are: position, velocity, acceleration, and force. In this image, the spaceship at step 1 has a position vector of `(1, 3)` and a velocity vector of `(2, 1)`. The velocity vector represents how far the ship moves each step. We can find the position for step 2 by adding the velocity to the current position.

> **Tip:** Velocity measures the **change** in position per unit of time. The new position is found by adding the velocity multiplied by the elapsed time (here assumed to be one unit, e.g. 1 s) to the previous position. In a typical 2D game scenario, you would have a velocity in pixels per second, and multiply it by the `delta` parameter (time elapsed since the previous frame) from the `_Process()` or `_PhysicsProcess()` callbacks.

#### Pointing Toward a Target

In this scenario, you have a tank that wishes to point its turret at a robot. Subtracting the tank's position from the robot's position gives the vector pointing from the tank to the robot.

> **Tip:** To find a vector pointing from `A` to `B`, use `B - A`.

### Unit Vectors

A vector with **magnitude** of `1` is called a **unit vector**. They are also sometimes referred to as **direction vectors** or **normals**. Unit vectors are helpful when you need to keep track of a direction.

#### Normalization

**Normalizing** a vector means reducing its length to `1` while preserving its direction. This is done by dividing each of its components by its magnitude. Because this is such a common operation, Godot provides a dedicated `Normalized()` method for this:

```csharp
a = a.Normalized();
```

> **Warning:** Because normalization involves dividing by the vector's length, you cannot normalize a vector of length `0`. Attempting to do so would normally result in an error.

#### Reflection

A common use of unit vectors is to indicate **normals**. Normal vectors are unit vectors aligned perpendicularly to a surface, defining its direction. They are commonly used for lighting, collisions, and other operations involving surfaces.

For example, imagine we have a moving ball that we want to bounce off a wall or other object. The surface normal has a value of `(0, -1)` because this is a horizontal surface. When the ball collides, we take its remaining motion (the amount left over when it hits the surface) and reflect it using the normal. In Godot, there is a `Bounce()` method to handle this. Here is a code example of the above diagram using a `CharacterBody2D`:

```csharp
KinematicCollision2D collision = MoveAndCollide(_velocity * (float)delta);
if (collision != null)
{
    var reflect = collision.GetRemainder().Bounce(collision.GetNormal());
    _velocity = _velocity.Bounce(collision.GetNormal());
    MoveAndCollide(reflect);
}
```

### Dot Product

The **dot product** is one of the most important concepts in vector math, but is often misunderstood. Dot product is an operation on two vectors that returns a **scalar**. Unlike a vector, which contains both magnitude and direction, a scalar value has only magnitude.

The formula for dot product takes two common forms:

- `A . B = ||A|| * ||B|| * cos(theta)`
- `A . B = Ax * Bx + Ay * By`

However, in most cases it is easiest to use the built-in `Dot()` method. Note that the order of the two vectors does not matter:

```csharp
float c = a.Dot(b);
float d = b.Dot(a);  // These are equivalent.
```

The dot product is most useful when used with unit vectors, making the first formula reduce to just `cos(theta)`. This means we can use the dot product to tell us something about the angle between two vectors:

- When using unit vectors, the result will always be between `-1` (180 degrees) and `1` (0 degrees).

#### Facing

We can use this fact to detect whether an object is facing toward another object. In the diagram below, the player `P` is trying to avoid the zombies `A` and `B`. Assuming a zombie's field of view is **180 degrees**, can they see the player?

The green arrows `fA` and `fB` are **unit vectors** representing the zombie's facing direction and the blue semicircle represents its field of view. For zombie `A`, we find the direction vector `AP` pointing to the player using `P - A` and normalize it, however, Godot has a helper method to do this called `DirectionTo()`. If the angle between this vector and the facing vector is less than 90 degrees, then the zombie can see the player.

In code it would look like this:

```csharp
var AP = A.DirectionTo(P);
if (AP.Dot(fA) > 0)
{
    GD.Print("A sees P!");
}
```

### Cross Product

Like the dot product, the **cross product** is an operation on two vectors. However, the result of the cross product is a vector with a direction that is perpendicular to both. Its magnitude depends on their relative angle. If two vectors are parallel, the result of their cross product will be a null vector.

The cross product is calculated like this:

```csharp
var c = new Vector3();
c.X = (a.Y * b.Z) - (a.Z * b.Y);
c.Y = (a.Z * b.X) - (a.X * b.Z);
c.Z = (a.X * b.Y) - (a.Y * b.X);
```

With Godot, you can use the built-in `Vector3.Cross()` method:

```csharp
var c = a.Cross(b);
```

The cross product is not mathematically defined in 2D. The `Vector2.Cross()` method is a commonly used analog of the 3D cross product for 2D vectors.

> **Note:** In the cross product, order matters. `a.Cross(b)` does not give the same result as `b.Cross(a)`. The resulting vectors point in **opposite** directions.

#### Calculating Normals

One common use of cross products is to find the surface normal of a plane or surface in 3D space. If we have the triangle `ABC` we can use vector subtraction to find two edges `AB` and `AC`. Using the cross product, `AB x AC` produces a vector perpendicular to both: the surface normal.

Here is a function to calculate a triangle's normal:

```csharp
Vector3 GetTriangleNormal(Vector3 a, Vector3 b, Vector3 c)
{
    // Find the surface normal given 3 vertices.
    var side1 = b - a;
    var side2 = c - a;
    var normal = side1.Cross(side2);
    return normal;
}
```

#### Pointing to a Target

In the dot product section above, we saw how it could be used to find the angle between two vectors. However, in 3D, this is not enough information. We also need to know what axis to rotate around. We can find that by calculating the cross product of the current facing direction and the target direction. The resulting perpendicular vector is the axis of rotation.

### More Information

For more information on using vector math in Godot, see the following articles:

- Advanced Vector Math (below)
- Matrices and Transforms (separate documentation page)

---

## 3.2 Advanced Vector Math

### Planes

The dot product has another interesting property with unit vectors. Imagine that perpendicular to that vector (and through the origin) passes a plane. Planes divide the entire space into positive (over the plane) and negative (under the plane), and (contrary to popular belief) you can also use their math in 2D.

Unit vectors that are perpendicular to a surface (so, they describe the orientation of the surface) are called **unit normal vectors**. Though, usually they are just abbreviated as *normals*. Normals appear in planes, 3D geometry (to determine where each face or vertex is siding), etc. A **normal** *is* a **unit vector**, but it's called *normal* because of its usage. (Just like we call (0,0) the Origin!).

The plane passes by the origin and the surface of it is perpendicular to the unit vector (or *normal*). The side the vector points to is the positive half-space, while the other side is the negative half-space. In 3D this is exactly the same, except that the plane is an infinite surface (imagine an infinite, flat sheet of paper that you can orient and is pinned to the origin) instead of a line.

#### Distance to Plane

Now that it's clear what a plane is, let's go back to the dot product. The dot product between a **unit vector** and any **point in space** (yes, this time we do dot product between vector and position), returns the **distance from the point to the plane**:

```csharp
var distance = normal.Dot(point);
```

But not just the absolute distance, if the point is in the negative half space the distance will be negative, too. This allows us to tell which side of the plane a point is.

#### Away from the Origin

Remember that planes not only split space in two, but they also have *polarity*. This means that it is possible to have perfectly overlapping planes, but their negative and positive half-spaces are swapped.

With this in mind, let's describe a full plane as a **normal** *N* and a **distance from the origin** scalar *D*. Thus, our plane is represented by N and D.

For 3D math, Godot provides a `Plane` built-in type that handles this.

Basically, N and D can represent any plane in space, be it for 2D or 3D (depending on the amount of dimensions of N) and the math is the same for both. It's the same as before, but D is the distance from the origin to the plane, travelling in N direction. As an example, imagine you want to reach a point in the plane, you will just do:

```csharp
var pointInPlane = N * D;
```

This will stretch (resize) the normal vector and make it touch the plane. This math might seem confusing, but it's actually much simpler than it seems. If we want to tell, again, the distance from the point to the plane, we do the same but adjusting for distance:

```csharp
var distance = N.Dot(point) - D;
```

The same thing, using a built-in function:

```csharp
var distance = plane.DistanceTo(point);
```

This will, again, return either a positive or negative distance.

Flipping the polarity of the plane can be done by negating both N and D. This will result in a plane in the same position, but with inverted negative and positive half spaces:

```csharp
N = -N;
D = -D;
```

Godot also implements this operator in `Plane`. So, using the format below will work as expected:

```csharp
var invertedPlane = -plane;
```

So, remember, the plane's main practical use is that we can calculate the distance to it. So, when is it useful to calculate the distance from a point to a plane? Let's see some examples.

#### Constructing a Plane in 2D

Planes clearly don't come out of nowhere, so they must be built. Constructing them in 2D is easy, this can be done from either a normal (unit vector) and a point, or from two points in space.

In the case of a normal and a point, most of the work is done, as the normal is already computed, so calculate D from the dot product of the normal and the point.

```csharp
var N = normal;
var D = normal.Dot(point);
```

For two points in space, there are actually two planes that pass through them, sharing the same space but with normal pointing to the opposite directions. To compute the normal from the two points, the direction vector must be obtained first, and then it needs to be rotated 90 degrees to either side:

```csharp
// Calculate vector from `a` to `b`.
var dvec = pointA.DirectionTo(pointB);
// Rotate 90 degrees.
var normal = new Vector2(dvec.Y, -dvec.X);
// Alternatively (depending the desired side of the normal):
// var normal = new Vector2(-dvec.Y, dvec.X);
```

The rest is the same as the previous example. Either `pointA` or `pointB` will work, as they are in the same plane:

```csharp
var N = normal;
var D = normal.Dot(pointA);
// this works the same
// var D = normal.Dot(pointB);
```

Doing the same in 3D is a little more complex and is explained further down.

#### Some Examples of Planes

Here is an example of what planes are useful for. Imagine you have a convex polygon. For example, a rectangle, a trapezoid, a triangle, or just any polygon where no faces bend inwards.

For every segment of the polygon, we compute the plane that passes by that segment. Once we have the list of planes, we can do neat things, for example checking if a point is inside the polygon.

We go through all planes, if we can find a plane where the distance to the point is positive, then the point is outside the polygon. If we can't, then the point is inside.

Code should be something like this:

```csharp
var inside = true;
foreach (var p in planes)
{
    // check if distance to plane is positive
    if (p.DistanceTo(point) > 0)
    {
        inside = false;
        break; // with one that fails, it's enough
    }
}
```

Pretty cool, huh? But this gets much better! With a little more effort, similar logic will let us know when two convex polygons are overlapping too. This is called the Separating Axis Theorem (or SAT) and most physics engines use this to detect collision.

With a point, just checking if a plane returns a positive distance is enough to tell if the point is outside. With another polygon, we must find a plane where *all the other polygon points* return a positive distance to it. This check is performed with the planes of A against the points of B, and then with the planes of B against the points of A:

```csharp
var overlapping = true;

foreach (Plane plane in planesOfA)
{
    var allOut = true;
    foreach (Vector3 point in pointsOfB)
    {
        if (plane.DistanceTo(point) < 0)
        {
            allOut = false;
            break;
        }
    }

    if (allOut)
    {
        // a separating plane was found
        // do not continue testing
        overlapping = false;
        break;
    }
}

if (overlapping)
{
    // only do this check if no separating plane
    // was found in planes of A
    foreach (Plane plane in planesOfB)
    {
        var allOut = true;
        foreach (Vector3 point in pointsOfA)
        {
            if (plane.DistanceTo(point) < 0)
            {
                allOut = false;
                break;
            }
        }

        if (allOut)
        {
            overlapping = false;
            break;
        }
    }
}

if (overlapping)
{
    GD.Print("Polygons Collided!");
}
```

As you can see, planes are quite useful, and this is the tip of the iceberg. You might be wondering what happens with non convex polygons. This is usually just handled by splitting the concave polygon into smaller convex polygons, or using a technique such as BSP (which is not used much nowadays).

### Collision Detection in 3D

This is another bonus bit, a reward for being patient and keeping up with this long tutorial. Here is another piece of wisdom. This might not be something with a direct use case (Godot already does collision detection pretty well) but it's used by almost all physics engines and collision detection libraries.

Remember that converting a convex shape in 2D to an array of 2D planes was useful for collision detection? You could detect if a point was inside any convex shape, or if two 2D convex shapes were overlapping.

Well, this works in 3D too, if two 3D polyhedral shapes are colliding, you won't be able to find a separating plane. If a separating plane is found, then the shapes are definitely not colliding.

To refresh a bit: a separating plane means that all vertices of polygon A are in one side of the plane, and all vertices of polygon B are in the other side. This plane is always one of the face-planes of either polygon A or polygon B.

In 3D though, there is a problem to this approach, because it is possible that, in some cases a separating plane can't be found. To avoid it, some extra planes need to be tested as separators. These planes are the cross product between the edges of polygon A and the edges of polygon B.

So the final algorithm is something like:

```csharp
var overlapping = true;

foreach (Plane plane in planesOfA)
{
    var allOut = true;
    foreach (Vector3 point in pointsOfB)
    {
        if (plane.DistanceTo(point) < 0)
        {
            allOut = false;
            break;
        }
    }

    if (allOut)
    {
        // a separating plane was found
        // do not continue testing
        overlapping = false;
        break;
    }
}

if (overlapping)
{
    // only do this check if no separating plane
    // was found in planes of A
    foreach (Plane plane in planesOfB)
    {
        var allOut = true;
        foreach (Vector3 point in pointsOfA)
        {
            if (plane.DistanceTo(point) < 0)
            {
                allOut = false;
                break;
            }
        }

        if (allOut)
        {
            overlapping = false;
            break;
        }
    }
}

if (overlapping)
{
    foreach (Vector3 edgeA in edgesOfA)
    {
        foreach (Vector3 edgeB in edgesOfB)
        {
            var normal = edgeA.Cross(edgeB);
            if (normal.Length() == 0)
            {
                continue;
            }

            var maxA = float.MinValue; // tiny number
            var minA = float.MaxValue; // huge number

            // we are using the dot product directly
            // so we can map a maximum and minimum range
            // for each polygon, then check if they
            // overlap.

            foreach (Vector3 point in pointsOfA)
            {
                var distance = normal.Dot(point);
                maxA = Mathf.Max(maxA, distance);
                minA = Mathf.Min(minA, distance);
            }

            var maxB = float.MinValue; // tiny number
            var minB = float.MaxValue; // huge number

            foreach (Vector3 point in pointsOfB)
            {
                var distance = normal.Dot(point);
                maxB = Mathf.Max(maxB, distance);
                minB = Mathf.Min(minB, distance);
            }

            if (minA > maxB || minB > maxA)
            {
                // not overlapping!
                overlapping = false;
                break;
            }
        }

        if (!overlapping)
        {
            break;
        }
    }
}

if (overlapping)
{
    GD.Print("Polygons Collided!");
}
```

### More Information

For more information on using vector math in Godot, see the Matrices and Transforms documentation page.

If you would like additional explanation, you should check out 3Blue1Brown's excellent video series [Essence of Linear Algebra](https://www.youtube.com/watch?v=fNk_zzaMoSs&list=PLZHQObOWTQDPD3MizzM2xVFitgF8hE_ab).

---

## 3.3 Interpolation

Interpolation is a common operation in graphics programming, which is used to blend or transition between two values. Interpolation can also be used to smooth movement, rotation, etc. It's good to become familiar with it in order to expand your horizons as a game developer.

The basic idea is that you want to transition from A to B. A value `t`, represents the states in-between.

For example, if `t` is 0, then the state is A. If `t` is 1, then the state is B. Anything in-between is an *interpolation*.

Between two real (floating-point) numbers, an interpolation can be described as:

```
interpolation = A * (1 - t) + B * t
```

And often simplified to:

```
interpolation = A + (B - A) * t
```

The name of this type of interpolation, which transforms a value into another at *constant speed* is *"linear"*. So, when you hear about *Linear Interpolation*, you know they are referring to this formula.

There are other types of interpolations, which will not be covered here. A recommended read afterwards is the Beziers and Curves section below.

### Vector Interpolation

Vector types (`Vector2` and `Vector3`) can also be interpolated, they come with handy functions to do it: `Vector2.Lerp()` and `Vector3.Lerp()`.

For cubic interpolation, there are also `Vector2.CubicInterpolate()` and `Vector3.CubicInterpolate()`, which do a Bezier style interpolation.

Here is example pseudo-code for going from point A to B using interpolation:

```csharp
private float _t = 0.0f;

public override void _PhysicsProcess(double delta)
{
    _t += (float)delta * 0.4f;

    Marker2D a = GetNode<Marker2D>("A");
    Marker2D b = GetNode<Marker2D>("B");
    Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

    sprite.Position = a.Position.Lerp(b.Position, _t);
}
```

### Transform Interpolation

It is also possible to interpolate whole transforms (make sure they have either uniform scale or, at least, the same non-uniform scale). For this, the function `Transform3D.InterpolateWith()` can be used.

Here is an example of transforming a monkey from Position1 to Position2:

```csharp
private float _t = 0.0f;

public override void _PhysicsProcess(double delta)
{
    _t += (float)delta;

    Marker3D p1 = GetNode<Marker3D>("Position1");
    Marker3D p2 = GetNode<Marker3D>("Position2");
    CSGMesh3D monkey = GetNode<CSGMesh3D>("Monkey");

    monkey.Transform = p1.Transform.InterpolateWith(p2.Transform, _t);
}
```

### Smoothing Motion

Interpolation can be used to smoothly follow a moving target value, such as a position or a rotation. Each frame, `Lerp()` moves the current value towards the target value by a fixed percentage of the remaining difference between the values. The current value will smoothly move towards the target, slowing down as it gets closer. Here is an example of a circle following the mouse using interpolation smoothing:

```csharp
private const float FollowSpeed = 4.0f;

public override void _PhysicsProcess(double delta)
{
    Vector2 mousePos = GetLocalMousePosition();

    Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");

    sprite.Position = sprite.Position.Lerp(mousePos, (float)delta * FollowSpeed);
}
```

This is useful for smoothing camera movement, for allies following the player (ensuring they stay within a certain range), and for many other common game patterns.

> **Note:** Despite using `delta`, the formula used above is framerate-dependent, because the `weight` parameter of `Lerp()` represents a *percentage* of the remaining difference in values, not an *absolute amount to change*. In `_PhysicsProcess()`, this is usually fine because physics is expected to maintain a constant framerate, and therefore `delta` is expected to remain constant.
>
> For a framerate-independent version of interpolation smoothing that can also be used in `_Process()`, use the following formula instead:

```csharp
private const float FollowSpeed = 4.0f;

public override void _Process(double delta)
{
    Vector2 mousePos = GetLocalMousePosition();

    Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
    float weight = 1f - Mathf.Exp(-FollowSpeed * (float)delta);
    sprite.Position = sprite.Position.Lerp(mousePos, weight);
}
```

Deriving this formula is beyond the scope of this page. For an explanation, see [Improved Lerp Smoothing](https://www.gamedeveloper.com/programming/improved-lerp-smoothing-) or watch [Lerp smoothing is broken](https://www.youtube.com/watch?v=LSNQuFEDOyQ).

---

## 3.4 Random Number Generation

Many games rely on randomness to implement core game mechanics. This page guides you through common types of randomness and how to implement them in Godot.

After giving you a brief overview of useful functions that generate random numbers, you will learn how to get random elements from arrays, dictionaries, and how to use a noise generator. Lastly, we'll take a look at cryptographically secure random number generation and how it differs from typical random number generation.

> **Note:** Computers cannot generate "true" random numbers. Instead, they rely on [pseudorandom number generators](https://en.wikipedia.org/wiki/Pseudorandom_number_generator) (PRNGs). Godot internally uses the [PCG Family](https://www.pcg-random.org/) of pseudorandom number generators.

### Global Scope versus RandomNumberGenerator Class

Godot exposes two ways to generate random numbers: via *global scope* methods or using the `RandomNumberGenerator` class.

Global scope methods are easier to set up, but they don't offer as much control.

`RandomNumberGenerator` requires more code to use, but allows creating multiple instances, each with their own seed and state.

This tutorial uses global scope methods, except when the method only exists in the `RandomNumberGenerator` class.

### The Randomize() Method

> **Note:** Since Godot 4.0, the random seed is automatically set to a random value when the project starts. This means you don't need to call `Randomize()` in `_Ready()` anymore to ensure that results are random across project runs. However, you can still use `Randomize()` if you want to use a specific seed number, or generate it using a different method.

In global scope, you can find a `GD.Randomize()` method. **This method should be called only once when your project starts to initialize the random seed.** Calling it multiple times is unnecessary and may impact performance negatively.

Putting it in your main scene script's `_Ready()` method is a good choice:

```csharp
public override void _Ready()
{
    GD.Randomize();
}
```

You can also set a fixed random seed instead using `GD.Seed()`. Doing so will give you *deterministic* results across runs:

```csharp
public override void _Ready()
{
    GD.Seed(12345);
    // To use a string as a seed, you can hash it to a number.
    GD.Seed("Hello world".Hash());
}
```

When using the `RandomNumberGenerator` class, you should call `Randomize()` on the instance since it has its own seed:

```csharp
var random = new RandomNumberGenerator();
random.Randomize();
```

### Getting a Random Number

Let's look at some of the most commonly used functions and methods to generate random numbers in Godot.

The function `GD.Randi()` returns a random number between `0` and `2^32 - 1`. Since the maximum value is huge, you most likely want to use the modulo operator (`%`) to bound the result between 0 and the denominator:

```csharp
// Prints a random integer between 0 and 49.
GD.Print(GD.Randi() % 50);

// Prints a random integer between 10 and 60.
GD.Print(GD.Randi() % 51 + 10);
```

`GD.Randf()` returns a random floating-point number between 0 and 1. This is useful to implement a weighted random probability system, among other things.

`GD.Randfn()` returns a random floating-point number following a [normal distribution](https://en.wikipedia.org/wiki/Normal_distribution). This means the returned value is more likely to be around the mean (0.0 by default), varying by the deviation (1.0 by default):

```csharp
// Prints a random floating-point number from a normal distribution with a mean 0.0 and deviation 1.0.
GD.Print(GD.Randfn(0.0, 1.0));
```

`GD.RandRange()` takes two arguments `from` and `to`, and returns a random floating-point number between `from` and `to`:

```csharp
// Prints a random floating-point number between -4 and 6.5.
GD.Print(GD.RandRange(-4.0, 6.5));
```

`GD.RandRange()` (with integer arguments) takes two arguments `from` and `to`, and returns a random integer between `from` and `to`:

```csharp
// Prints a random integer number between -10 and 10.
GD.Print(GD.RandRange(-10, 10));
```

### Get a Random Array Element

We can use random integer generation to get a random element from an array, or use the `PickRandom()` method to do it for us:

```csharp
// Use Godot's Array type instead of a BCL type so we can use `PickRandom()` on it.
private Godot.Collections.Array<string> _fruits = ["apple", "orange", "pear", "banana"];

public override void _Ready()
{
    for (int i = 0; i < 100; i++)
    {
        // Pick 100 fruits randomly.
        GD.Print(GetFruit());
    }

    for (int i = 0; i < 100; i++)
    {
        // Pick 100 fruits randomly, this time using the `Array.PickRandom()`
        // helper method. This has the same behavior as `GetFruit()`.
        GD.Print(_fruits.PickRandom());
    }
}

public string GetFruit()
{
    string randomFruit = _fruits[GD.Randi() % _fruits.Size()];
    // Returns "apple", "orange", "pear", or "banana" every time the code runs.
    // We may get the same fruit multiple times in a row.
    return randomFruit;
}
```

To prevent the same fruit from being picked more than once in a row, we can add more logic to the above method. In this case, we can't use `PickRandom()` since it lacks a way to prevent repetition:

```csharp
private string[] _fruits = ["apple", "orange", "pear", "banana"];
private string _lastFruit = "";

public override void _Ready()
{
    for (int i = 0; i < 100; i++)
    {
        // Pick 100 fruits randomly.
        GD.Print(GetFruit());
    }
}

public string GetFruit()
{
    string randomFruit = _fruits[GD.Randi() % _fruits.Length];
    while (randomFruit == _lastFruit)
    {
        // The last fruit was picked. Try again until we get a different fruit.
        randomFruit = _fruits[GD.Randi() % _fruits.Length];
    }

    _lastFruit = randomFruit;

    // Returns "apple", "orange", "pear", or "banana" every time the code runs.
    // The function will never return the same fruit more than once in a row.
    return randomFruit;
}
```

This approach can be useful to make random number generation feel less repetitive. Still, it doesn't prevent results from "ping-ponging" between a limited set of values. To prevent this, use the shuffle bag pattern instead.

### Get a Random Dictionary Value

We can apply similar logic from arrays to dictionaries as well:

```csharp
private Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, int>> _metals = new()
{
    {"copper", new Godot.Collections.Dictionary<string, int>{{"quantity", 50}, {"price", 50}}},
    {"silver", new Godot.Collections.Dictionary<string, int>{{"quantity", 20}, {"price", 150}}},
    {"gold", new Godot.Collections.Dictionary<string, int>{{"quantity", 3}, {"price", 500}}},
};

public override void _Ready()
{
    for (int i = 0; i < 20; i++)
    {
        GD.Print(GetMetal());
    }
}

public Godot.Collections.Dictionary<string, int> GetMetal()
{
    var (_, randomMetal) = _metals.ElementAt((int)(GD.Randi() % _metals.Count));
    // Returns a random metal value dictionary every time the code runs.
    // The same metal may be selected multiple times in succession.
    return randomMetal;
}
```

### Weighted Random Probability

The `GD.Randf()` method returns a floating-point number between 0.0 and 1.0. We can use this to create a "weighted" probability where different outcomes have different likelihoods:

```csharp
public override void _Ready()
{
    for (int i = 0; i < 100; i++)
    {
        GD.Print(GetItemRarity());
    }
}

public string GetItemRarity()
{
    float randomFloat = GD.Randf();

    if (randomFloat < 0.8f)
    {
        // 80% chance of being returned.
        return "Common";
    }
    else if (randomFloat < 0.95f)
    {
        // 15% chance of being returned.
        return "Uncommon";
    }
    else
    {
        // 5% chance of being returned.
        return "Rare";
    }
}
```

You can also get a weighted random *index* using the `RandWeighted()` method on a `RandomNumberGenerator` instance. This returns a random integer between 0 and the size of the array that is passed as a parameter. Each value in the array is a floating-point number that represents the *relative* likelihood that it will be returned as an index. A higher value means the value is more likely to be returned as an index, while a value of `0` means it will never be returned as an index.

For example, if `[0.5, 1, 1, 2]` is passed as a parameter, then the method is twice as likely to return `3` (the index of the value `2`) and twice as unlikely to return `0` (the index of the value `0.5`) compared to the indices `1` and `2`.

Since the returned value matches the array's size, it can be used as an index to get a value from another array as follows:

```csharp
// Prints a random element using the weighted index that is returned by `RandWeighted()`.
// Here, "apple" will be returned twice as rarely as "orange" and "pear".
// "banana" is twice as common as "orange" and "pear", and four times as common as "apple".
string[] fruits = ["apple", "orange", "pear", "banana"];
float[] probabilities = [0.5f, 1, 1, 2];

var random = new RandomNumberGenerator();
GD.Print(fruits[random.RandWeighted(probabilities)]);
```

### "Better" Randomness Using Shuffle Bags

Taking the same example as above, we would like to pick fruits at random. However, relying on random number generation every time a fruit is selected can lead to a less *uniform* distribution. If the player is lucky (or unlucky), they could get the same fruit three or more times in a row.

You can accomplish this using the *shuffle bag* pattern. It works by removing an element from the array after choosing it. After multiple selections, the array ends up empty. When that happens, you reinitialize it to its default value:

```csharp
private Godot.Collections.Array<string> _fruits = ["apple", "orange", "pear", "banana"];
// A copy of the fruits array so we can restore the original value into `fruits`.
private Godot.Collections.Array<string> _fruitsFull;

public override void _Ready()
{
    _fruitsFull = _fruits.Duplicate();
    _fruits.Shuffle();

    for (int i = 0; i < 100; i++)
    {
        GD.Print(GetFruit());
    }
}

public string GetFruit()
{
    if(_fruits.Count == 0)
    {
        // Fill the fruits array again and shuffle it.
        _fruits = _fruitsFull.Duplicate();
        _fruits.Shuffle();
    }

    // Get a random fruit, since we shuffled the array,
    string randomFruit = _fruits[0];
    // and remove it from the `_fruits` array.
    _fruits.RemoveAt(0);
    // Returns "apple", "orange", "pear", or "banana" every time the code runs, removing it from the array.
    // When all fruit are removed, it refills the array.
    return randomFruit;
}
```

When running the above code, there is a chance to get the same fruit twice in a row. Once we picked a fruit, it will no longer be a possible return value unless the array is now empty. When the array is empty, we reset it back to its default value, making it possible to have the same fruit again, but only once.

### Random Noise

The random number generation shown above can show its limits when you need a value that *slowly* changes depending on the input. The input can be a position, time, or anything else.

To achieve this, you can use random *noise* functions. Noise functions are especially popular in procedural generation to generate realistic-looking terrain. Godot provides `FastNoiseLite` for this, which supports 1D, 2D and 3D noise. Here's an example with 1D noise:

```csharp
private FastNoiseLite _noise = new FastNoiseLite();

public override void _Ready()
{
    // Configure the FastNoiseLite instance.
    _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
    _noise.Seed = (int)GD.Randi();
    _noise.FractalOctaves = 4;
    _noise.Frequency = 1.0f / 20.0f;

    for (int i = 0; i < 100; i++)
    {
        GD.Print(_noise.GetNoise1D(i));
    }
}
```

### Cryptographically Secure Pseudorandom Number Generation

So far, the approaches mentioned above are **not** suitable for *cryptographically secure* pseudorandom number generation (CSPRNG). This is fine for games, but this is not sufficient for scenarios where encryption, authentication or signing is involved.

Godot offers a `Crypto` class for this. This class can perform asymmetric key encryption/decryption, signing/verification, while also generating cryptographically secure random bytes, RSA keys, HMAC digests, and self-signed `X509Certificate`s.

The downside of CSPRNG is that it's much slower than standard pseudorandom number generation. Its API is also less convenient to use. As a result, CSPRNG should be avoided for gameplay elements.

Example of using the Crypto class to generate 2 random integers between `0` and `2^32 - 1` (inclusive):

```csharp
var crypto = new Crypto();
// Request as many bytes as you need, but try to minimize the amount
// of separate requests to improve performance.
// Each 32-bit integer requires 4 bytes, so we request 8 bytes.
byte[] byteArray = crypto.GenerateRandomBytes(8);

// Use the DecodeU32() method from PackedByteArray to decode a 32-bit unsigned integer
// from the beginning of byteArray. This method doesn't modify byteArray.
uint randomInt1 = byteArray.DecodeU32(0);
// Do the same as above, but with an offset of 4 bytes since we've already decoded
// the first 4 bytes previously.
uint randomInt2 = byteArray.DecodeU32(4);

GD.Print($"Random integers: {randomInt1} {randomInt2}");
```

> See `PackedByteArray`'s documentation for other methods you can use to decode the generated bytes into various types of data, such as integers or floats.

---

## 3.5 Beziers, Curves and Paths

Bezier curves are a mathematical approximation of natural geometric shapes. We use them to represent a curve with as little information as possible and with a high level of flexibility.

Unlike more abstract mathematical concepts, Bezier curves were created for industrial design. They are a popular tool in the graphics software industry.

They rely on interpolation, which we saw in the previous article, combining multiple steps to create smooth curves. To better understand how Bezier curves work, let's start from its simplest form: Quadratic Bezier.

### Quadratic Bezier

Take three points, the minimum required for Quadratic Bezier to work.

To draw a curve between them, we first interpolate gradually over the two vertices of each of the two segments formed by the three points, using values ranging from 0 to 1. This gives us two points that move along the segments as we change the value of `t` from 0 to 1.

```csharp
private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
{
    Vector2 q0 = p0.Lerp(p1, t);
    Vector2 q1 = p1.Lerp(p2, t);

    Vector2 r = q0.Lerp(q1, t);
    return r;
}
```

We then interpolate `q0` and `q1` to obtain a single point `r` that moves along a curve.

This type of curve is called a *Quadratic Bezier* curve.

### Cubic Bezier

Building upon the previous example, we can get more control by interpolating between four points.

We first use a function with four parameters to take four points as an input, `p0`, `p1`, `p2` and `p3`:

We apply a linear interpolation to each couple of points to reduce them to three, then take our three points and reduce them to two, and finally to one.

Here is the full function:

```csharp
private Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
{
    Vector2 q0 = p0.Lerp(p1, t);
    Vector2 q1 = p1.Lerp(p2, t);
    Vector2 q2 = p2.Lerp(p3, t);

    Vector2 r0 = q0.Lerp(q1, t);
    Vector2 r1 = q1.Lerp(q2, t);

    Vector2 s = r0.Lerp(r1, t);
    return s;
}
```

The result will be a smooth curve interpolating between all four points.

> **Note:** Cubic Bezier interpolation works the same in 3D, just use `Vector3` instead of `Vector2`.

### Adding Control Points

Building upon Cubic Bezier, we can change the way two of the points work to control the shape of our curve freely. Instead of having `p0`, `p1`, `p2` and `p3`, we will store them as:

- `point0 = p0`: Is the first point, the source
- `control0 = p1 - p0`: Is a vector relative to the first control point
- `control1 = p3 - p2`: Is a vector relative to the second control point
- `point1 = p3`: Is the second point, the destination

This way, we have two points and two control points which are relative vectors to the respective points. If you've used graphics or animation software before, this might look familiar.

This is how graphics software presents Bezier curves to the users, and how they work and look in Godot.

### Curve2D, Curve3D, Path2D and Path3D

There are two objects that contain curves: `Curve3D` and `Curve2D` (for 3D and 2D respectively).

They can contain several points, allowing for longer paths. It is also possible to set them to nodes: `Path3D` and `Path2D` (also for 3D and 2D respectively).

Using them, however, may not be completely obvious, so following is a description of the most common use cases for Bezier curves.

### Evaluating

Only evaluating them may be an option, but in most cases it's not very useful. The big drawback with Bezier curves is that if you traverse them at constant speed, from `t = 0` to `t = 1`, the actual interpolation will *not* move at constant speed. The speed is also an interpolation between the distances between points `p0`, `p1`, `p2` and `p3` and there is not a mathematically simple way to traverse the curve at constant speed.

Let's do an example with the following pseudocode:

```csharp
private float _t = 0.0f;

public override void _Process(double delta)
{
    _t += (float)delta;
    Position = CubicBezier(p0, p1, p2, p3, _t);
}
```

As you can see, the speed (in pixels per second) of the circle varies, even though `t` is increased at constant speed. This makes beziers difficult to use for anything practical out of the box.

### Drawing

Drawing beziers (or objects based on the curve) is a very common use case, but it's also not easy. For pretty much any case, Bezier curves need to be converted to some sort of segments. This is normally difficult, however, without creating a very high amount of them.

The reason is that some sections of a curve (specifically, corners) may require considerable amounts of points, while other sections may not.

Additionally, if both control points were `0, 0` (remember they are relative vectors), the Bezier curve would just be a straight line (so drawing a high amount of points would be wasteful).

Before drawing Bezier curves, *tessellation* is required. This is often done with a recursive or divide and conquer function that splits the curve until the curvature amount becomes less than a certain threshold.

The *Curve* classes provide this via the `Curve2D.Tessellate()` function (which receives optional `stages` of recursion and angle `tolerance` arguments). This way, drawing something based on a curve is easier.

### Traversal

The last common use case for the curves is to traverse them. Because of what was mentioned before regarding constant speed, this is also difficult.

To make this easier, the curves need to be *baked* into equidistant points. This way, they can be approximated with regular interpolation (which can be improved further with a cubic option). To do this, just use the `Curve3D.SampleBaked()` method together with `Curve2D.GetBakedLength()`. The first call to either of them will bake the curve internally.

Traversal at constant speed, then, can be done with the following pseudo-code:

```csharp
private float _t = 0.0f;

public override void _Process(double delta)
{
    _t += (float)delta;
    Position = curve.SampleBaked(_t * curve.GetBakedLength(), true);
}
```

And the output will, then, move at constant speed.
