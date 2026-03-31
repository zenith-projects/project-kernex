# Example 06: Inventory UI System

## User Input
> "Create a grid-based inventory UI with item slots, drag and drop, and item tooltips"

## Expected Output

### Scene Composition

```
InventoryUI (CanvasLayer)                      ← InventoryUI.tscn
└── PanelContainer (centered popup)
    └── MarginContainer
        └── VBoxContainer
            ├── HBoxContainer (header)
            │   ├── Label "Inventory"
            │   └── Button "X" (close)
            ├── HSeparator
            ├── GridContainer                   ← populated with ItemSlot.tscn instances
            │   ├── ItemSlot.tscn x20
            │   └── ...
            ├── HSeparator
            └── PanelContainer (details)
                └── HBoxContainer
                    ├── TextureRect (item icon)
                    └── VBoxContainer
                        ├── Label (item name)
                        └── RichTextLabel (description)
```

### ItemData.cs (Custom Resource)

```csharp
// scripts/resources/ItemData.cs
using Godot;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "Item";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }
    [Export] public int MaxStackSize { get; set; } = 1;
    [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    [Export] public bool IsConsumable { get; set; }
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
```

### ItemSlot.tscn (Reusable Component)

```ini
[gd_scene load_steps=3 format=3 uid="uid://item_slot"]

[ext_resource type="Script" path="res://scripts/ui/ItemSlot.cs" id="1_script"]

[sub_resource type="StyleBoxFlat" id="SlotBg"]
bg_color = Color(0.15, 0.15, 0.2, 1)
corner_radius_top_left = 4
corner_radius_top_right = 4
corner_radius_bottom_left = 4
corner_radius_bottom_right = 4
border_width_top = 1
border_width_bottom = 1
border_width_left = 1
border_width_right = 1
border_color = Color(0.3, 0.3, 0.4, 0.5)

[node name="ItemSlot" type="PanelContainer"]
custom_minimum_size = Vector2(64, 64)
script = ExtResource("1_script")
theme_override_styles/panel = SubResource("SlotBg")

[node name="TextureRect" type="TextureRect" parent="."]
stretch_mode = 4
expand_mode = 1

[node name="StackLabel" type="Label" parent="."]
horizontal_alignment = 2
vertical_alignment = 2
text = ""
```

### ItemSlot.cs

```csharp
using Godot;

public partial class ItemSlot : PanelContainer
{
    [Signal] public delegate void SlotClickedEventHandler(int index);
    [Signal] public delegate void SlotRightClickedEventHandler(int index);
    [Signal] public delegate void ItemDroppedEventHandler(int fromIndex, int toIndex);

    public int SlotIndex { get; set; }
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }

    private TextureRect _icon;
    private Label _stackLabel;
    private StyleBoxFlat _normalStyle;
    private StyleBoxFlat _hoverStyle;
    private StyleBoxFlat _emptyStyle;

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("TextureRect");
        _stackLabel = GetNode<Label>("StackLabel");

        // Clone styles so each slot has independent appearance
        _normalStyle = GetThemeStylebox("panel") as StyleBoxFlat;
        _hoverStyle = _normalStyle.Duplicate() as StyleBoxFlat;
        _hoverStyle.BorderColor = new Color(0.6f, 0.6f, 0.8f, 1f);

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.Left)
                EmitSignal(SignalName.SlotClicked, SlotIndex);
            else if (mb.ButtonIndex == MouseButton.Right)
                EmitSignal(SignalName.SlotRightClicked, SlotIndex);
        }
    }

    public void SetItem(ItemData item, int quantity = 1)
    {
        Item = item;
        Quantity = quantity;

        if (item != null)
        {
            _icon.Texture = item.Icon;
            _icon.Modulate = Colors.White;
            _stackLabel.Text = quantity > 1 ? quantity.ToString() : "";
            UpdateRarityBorder(item.Rarity);
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        Item = null;
        Quantity = 0;
        _icon.Texture = null;
        _stackLabel.Text = "";
        if (_normalStyle != null)
            _normalStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
    }

    private void UpdateRarityBorder(ItemRarity rarity)
    {
        Color borderColor = rarity switch
        {
            ItemRarity.Common => new Color(0.5f, 0.5f, 0.5f),
            ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
            ItemRarity.Rare => new Color(0.2f, 0.4f, 1.0f),
            ItemRarity.Epic => new Color(0.7f, 0.2f, 1.0f),
            ItemRarity.Legendary => new Color(1.0f, 0.6f, 0.0f),
            _ => new Color(0.3f, 0.3f, 0.4f)
        };
        _normalStyle.BorderColor = borderColor;
    }

    private void OnMouseEntered()
    {
        AddThemeStyleboxOverride("panel", _hoverStyle);
    }

    private void OnMouseExited()
    {
        AddThemeStyleboxOverride("panel", _normalStyle);
    }
}
```

### InventoryUI.cs

```csharp
using Godot;
using System.Collections.Generic;

public partial class InventoryUI : CanvasLayer
{
    [Signal] public delegate void ItemUsedEventHandler(string itemId);

    [Export] public PackedScene SlotScene { get; set; }
    [Export] public int SlotCount { get; set; } = 20;
    [Export] public int Columns { get; set; } = 5;

    private GridContainer _grid;
    private Label _itemNameLabel;
    private RichTextLabel _itemDescLabel;
    private TextureRect _itemIcon;
    private List<ItemSlot> _slots = new();
    private int _selectedSlot = -1;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _grid = GetNode<GridContainer>("%ItemGrid");
        _itemNameLabel = GetNode<Label>("%ItemName");
        _itemDescLabel = GetNode<RichTextLabel>("%ItemDescription");
        _itemIcon = GetNode<TextureRect>("%ItemIcon");
        GetNode<Button>("%CloseButton").Pressed += Close;

        _grid.Columns = Columns;

        // Create slots (composition — each slot is an instance)
        for (int i = 0; i < SlotCount; i++)
        {
            var slot = SlotScene.Instantiate<ItemSlot>();
            slot.SlotIndex = i;
            slot.SlotClicked += OnSlotClicked;
            slot.SlotRightClicked += OnSlotRightClicked;
            _grid.AddChild(slot);
            _slots.Add(slot);
        }

        ClearDetails();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("inventory"))
        {
            Toggle();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Toggle()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
        if (Visible)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        // Try to stack first
        if (item.MaxStackSize > 1)
        {
            foreach (var slot in _slots)
            {
                if (slot.Item?.ItemId == item.ItemId && slot.Quantity < item.MaxStackSize)
                {
                    int canAdd = Mathf.Min(quantity, item.MaxStackSize - slot.Quantity);
                    slot.SetItem(item, slot.Quantity + canAdd);
                    quantity -= canAdd;
                    if (quantity <= 0) return true;
                }
            }
        }

        // Find empty slot
        foreach (var slot in _slots)
        {
            if (slot.Item == null)
            {
                slot.SetItem(item, quantity);
                return true;
            }
        }

        return false; // Inventory full
    }

    private void OnSlotClicked(int index)
    {
        _selectedSlot = index;
        var slot = _slots[index];
        if (slot.Item != null)
            ShowDetails(slot.Item, slot.Quantity);
        else
            ClearDetails();
    }

    private void OnSlotRightClicked(int index)
    {
        var slot = _slots[index];
        if (slot.Item != null && slot.Item.IsConsumable)
        {
            EmitSignal(SignalName.ItemUsed, slot.Item.ItemId);
            if (slot.Quantity > 1)
                slot.SetItem(slot.Item, slot.Quantity - 1);
            else
                slot.Clear();
            ClearDetails();
        }
    }

    private void ShowDetails(ItemData item, int quantity)
    {
        _itemIcon.Texture = item.Icon;
        _itemNameLabel.Text = item.DisplayName;
        _itemDescLabel.Text = item.Description;
        if (quantity > 1)
            _itemNameLabel.Text += $" x{quantity}";
    }

    private void ClearDetails()
    {
        _itemIcon.Texture = null;
        _itemNameLabel.Text = "";
        _itemDescLabel.Text = "Select an item";
    }
}
```

## Key Takeaways
- **ItemSlot is a reusable composed scene** — instantiated N times into the grid
- **ItemData is a Custom Resource** — data separated from behavior
- **Signals flow up**: ItemSlot.SlotClicked → InventoryUI.OnSlotClicked
- **ProcessMode.Always** lets UI work during game pause
- **Rarity colors** via StyleBox border manipulation
- **Stack support** with quantity tracking
