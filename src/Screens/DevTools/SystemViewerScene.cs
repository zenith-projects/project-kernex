using System;
using Godot;
using ProjectKernex.Components.UI;
using ProjectKernex.Components.World;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.World;
using ProjectKernex.Systems.Exploration;

namespace ProjectKernex.Screens.DevTools;

public partial class SystemViewerScene : Control
{
    private LineEdit _seedInput;
    private KernexButton _generateButton;
    private KernexButton _randomizeButton;
    private Control _systemLayout;
    private VBoxContainer _infoPanel;
    private Label _systemNameLabel;
    private RichTextLabel _infoContent;

    private SolarSystemData _currentSystem;
    private float _zoom = 1f;
    private bool _dragging;
    private Vector2 _dragStart;
    private PanelContainer _tooltip;
    private RichTextLabel _tooltipContent;
    private Control _tooltipTarget;
    private bool _pendingDismiss;
    private const float OrbitScale = 800f;     // pixels between orbits
    private float _starVisualRadius = 200f;    // updated per system
    private PlanetView _followTarget;          // planet the camera follows
    private bool _orbitalMode = true;          // true = orbital, false = linear
    private KernexButton _modeToggle;
    private readonly System.Collections.Generic.List<(PlanetView view, PlanetData data, float angle)> _orbitingPlanets = new();

    public override void _Ready()
    {
        // Background
        var bg = new ColorRect
        {
            Color = new Color(0.06f, 0.06f, 0.1f),
        };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Main layout
        var mainLayout = new HSplitContainer
        {
            SplitOffset = -350,
        };
        mainLayout.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        mainLayout.AddThemeConstantOverride("separation", 0);
        AddChild(mainLayout);

        // Left side: free-pan viewport for system
        var viewArea = new Control
        {
            SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
            ClipContents = true,
        };
        mainLayout.AddChild(viewArea);

        _systemLayout = new Control();
        viewArea.AddChild(_systemLayout);

        // Center in viewport
        _systemLayout.Position = new Vector2(400, 400);

        // Right side: info panel
        var rightPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(350, 0),
        };
        var rightStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.04f, 0.04f, 0.06f, 0.95f),
            ContentMarginLeft = 16, ContentMarginTop = 16,
            ContentMarginRight = 16, ContentMarginBottom = 16,
        };
        rightPanel.AddThemeStyleboxOverride("panel", rightStyle);
        mainLayout.AddChild(rightPanel);

        var rightLayout = new VBoxContainer();
        rightLayout.AddThemeConstantOverride("separation", 12);
        rightPanel.AddChild(rightLayout);

        // Controls
        var titleLabel = new Label { Text = "SYSTEM VIEWER" };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.AddThemeColorOverride("font_color", Colors.White);
        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/moonhouse.ttf");
        if (font != null) titleLabel.AddThemeFontOverride("font", font);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        rightLayout.AddChild(titleLabel);

        var seedRow = new HBoxContainer();
        seedRow.AddThemeConstantOverride("separation", 8);
        rightLayout.AddChild(seedRow);

        var seedLabel = new Label { Text = "SEED:" };
        seedLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.65f, 0.75f));
        seedRow.AddChild(seedLabel);

        _seedInput = new LineEdit
        {
            SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand,
            PlaceholderText = "Enter seed...",
            Text = "42",
        };
        seedRow.AddChild(_seedInput);

        var buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 8);
        rightLayout.AddChild(buttonRow);

        _generateButton = new KernexButton { Text = "GENERATE", FontSize = 14 };
        _generateButton.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
        _generateButton.Pressed += OnGenerate;
        buttonRow.AddChild(_generateButton);

        _randomizeButton = new KernexButton { Text = "RANDOM", FontSize = 14 };
        _randomizeButton.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
        _randomizeButton.Pressed += OnRandomize;
        buttonRow.AddChild(_randomizeButton);

        // Mode toggle
        _modeToggle = new KernexButton { Text = "MODE: ORBITAL", FontSize = 12 };
        _modeToggle.SizeFlagsHorizontal = SizeFlags.Fill | SizeFlags.Expand;
        _modeToggle.Pressed += OnModeToggle;
        rightLayout.AddChild(_modeToggle);

        rightLayout.AddChild(new HSeparator());

        // System name
        _systemNameLabel = new Label
        {
            Text = "—",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _systemNameLabel.AddThemeFontSizeOverride("font_size", 18);
        _systemNameLabel.AddThemeColorOverride("font_color", Colors.White);
        if (font != null) _systemNameLabel.AddThemeFontOverride("font", font);
        rightLayout.AddChild(_systemNameLabel);

        // Info content
        _infoContent = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            SizeFlagsVertical = SizeFlags.Fill | SizeFlags.Expand,
            ScrollFollowing = false,
        };
        _infoContent.AddThemeFontSizeOverride("normal_font_size", 13);
        _infoContent.AddThemeColorOverride("default_color", new Color(0.7f, 0.75f, 0.8f));
        rightLayout.AddChild(_infoContent);

        // Floating tooltip
        _tooltip = new PanelContainer { Visible = false };
        var tooltipStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.08f, 0.92f),
            BorderColor = new Color(0.6f, 0.65f, 0.75f, 0.3f),
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            ContentMarginLeft = 12, ContentMarginTop = 8,
            ContentMarginRight = 12, ContentMarginBottom = 8,
        };
        _tooltip.AddThemeStyleboxOverride("panel", tooltipStyle);
        _tooltipContent = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            CustomMinimumSize = new Vector2(200, 0),
        };
        _tooltipContent.AddThemeFontSizeOverride("normal_font_size", 12);
        _tooltipContent.AddThemeColorOverride("default_color", new Color(0.75f, 0.78f, 0.85f));
        _tooltip.AddChild(_tooltipContent);
        AddChild(_tooltip);

        // Generate initial system
        // Defer generation to avoid lag on scene load
        CallDeferred(nameof(DeferredFirstGenerate));
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            // Zoom toward mouse position
            if (mb.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomAt(mb.GlobalPosition, 1.15f);
                GetViewport().SetInputAsHandled();
            }
            else if (mb.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomAt(mb.GlobalPosition, 0.87f);
                GetViewport().SetInputAsHandled();
            }
            // Pan with right or middle mouse
            else if (mb.ButtonIndex == MouseButton.Right || mb.ButtonIndex == MouseButton.Middle)
            {
                _dragging = mb.Pressed;
            }
            // Left click on empty area → dismiss tooltip
            else if (mb.ButtonIndex == MouseButton.Left && mb.Pressed && _tooltip.Visible)
            {
                // Defer so planet click handlers run first
                CallDeferred(MethodName.DeferredDismissTooltip);
            }
        }
        else if (@event is InputEventMouseMotion motion && _dragging)
        {
            _systemLayout.Position += motion.Relative;
            GetViewport().SetInputAsHandled();
        }
    }

    private void ZoomAt(Vector2 mousePos, float factor)
    {
        var oldZoom = _zoom;
        _zoom = Mathf.Clamp(_zoom * factor, 0.1f, 20f);

        // Adjust position so the point under the mouse stays fixed
        var layoutPos = _systemLayout.Position;
        var mouseLocal = (mousePos - layoutPos) / oldZoom;
        _systemLayout.Position = mousePos - mouseLocal * _zoom;
        _systemLayout.Scale = Vector2.One * _zoom;
    }

    private void DeferredDismissTooltip()
    {
        // If tooltip target didn't change (no planet was clicked), hide it
        if (_tooltipTarget != null && _tooltip.Visible)
        {
            // Small delay to let planet click override
            var timer = GetTree().CreateTimer(0.05);
            timer.Timeout += () =>
            {
                if (_pendingDismiss) HideTooltip();
                _pendingDismiss = false;
            };
            _pendingDismiss = true;
        }
    }

    private void DeferredFirstGenerate() => GenerateSystem(42);

    private void OnGenerate()
    {
        if (long.TryParse(_seedInput.Text, out var seed))
            GenerateSystem(seed);
    }

    private void OnRandomize()
    {
        var seed = (long)GD.Randi() << 16 | GD.Randi();
        _seedInput.Text = seed.ToString();
        GenerateSystem(seed);
    }

    public override void _Process(double delta)
    {
        for (int i = 0; i < _orbitingPlanets.Count; i++)
        {
            var (view, data, angle) = _orbitingPlanets[i];
            var orbitIndex = i + 1;
            var orbitRadius = _starVisualRadius + orbitIndex * OrbitScale;

            if (_orbitalMode)
            {
                // Orbital mode: planets orbit the star
                var omega = 0.15f / Mathf.Pow(Mathf.Max(data.OrbitalDistance, 0.1f), 1.5f);
                var newAngle = angle + (float)delta * omega;
                _orbitingPlanets[i] = (view, data, newAngle);

                var x = Mathf.Cos(newAngle) * orbitRadius;
                var y = Mathf.Sin(newAngle) * orbitRadius;
                view.Position = new Vector2(x - view.Size.X / 2, y - view.Size.Y / 2);
            }
            else
            {
                // Linear mode: planets laid out left to right, distances preserved
                var x = orbitRadius;
                var y = 0f;
                view.Position = new Vector2(x - view.Size.X / 2, y - view.Size.Y / 2);
            }
        }

        // Camera follows selected planet smoothly
        if (_followTarget != null && IsInstanceValid(_followTarget))
        {
            var targetCenter = _followTarget.Position + _followTarget.Size / 2;
            var screenCenter = GetViewportRect().Size / 2;
            var desiredPos = screenCenter - targetCenter * _zoom;
            _systemLayout.Position = _systemLayout.Position.Lerp(desiredPos, (float)delta * 3.0f);
        }
    }

    private void OnModeToggle()
    {
        _orbitalMode = !_orbitalMode;
        _modeToggle.SetText(_orbitalMode ? "MODE: ORBITAL" : "MODE: LINEAR");
    }

    private void GenerateSystem(long seed)
    {
        _currentSystem = SystemGenerator.Generate(seed);
        _systemNameLabel.Text = _currentSystem.SystemName;
        _orbitingPlanets.Clear();

        // Clear previous
        foreach (var child in _systemLayout.GetChildren())
            child.QueueFree();

        HideTooltip();

        // Add central body at center
        var bodyView = new CelestialBodyView();
        _systemLayout.AddChild(bodyView);
        bodyView.SetData(_currentSystem.CentralBody);
        bodyView.Clicked += _ => { ShowBodyTooltip(bodyView, _currentSystem.CentralBody); _followTarget = null; };
        // Center the star and calculate its visual radius for orbit spacing
        CallDeferred(nameof(CenterBody), bodyView);
        _starVisualRadius = Mathf.Clamp(
            60f + Mathf.Log(1f + _currentSystem.CentralBody.Radius * 109f) * 40f,
            80f, 400f) + 60f; // add padding

        // Add planets in orbits
        for (int i = 0; i < _currentSystem.Planets.Count; i++)
        {
            var pData = _currentSystem.Planets[i];
            var planetView = new PlanetView();
            _systemLayout.AddChild(planetView);
            planetView.SetData(pData);
            planetView.Clicked += _ => { ShowPlanetTooltip(planetView, pData); _followTarget = planetView; };

            // Random starting angle (deterministic from seed)
            // Spread planets evenly + random offset so they don't cluster
            var evenSpread = (float)i / Mathf.Max(_currentSystem.Planets.Count, 1) * Mathf.Tau;
            var randomOffset = (float)(new System.Random((int)((pData.Seed * 7919) & 0x7FFFFFFF)).NextDouble() * Mathf.Tau * 0.3);
            var startAngle = evenSpread + randomOffset;
            _orbitingPlanets.Add((planetView, pData, startAngle));
        }

        ShowSystemInfo();
    }

    private void CenterBody(CelestialBodyView body)
    {
        body.Position = -body.Size / 2;
    }

    private void ShowBodyTooltip(Control target, CelestialBodyData data)
    {
        _tooltipTarget = target;
        _tooltipContent.Clear();
        _tooltipContent.AppendText($"[b]{data.Name}[/b]\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Type:[/color] {data.Type}\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Mass:[/color] {data.Mass:F2} M☉\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Radius:[/color] {data.Radius:F2} R☉\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Temp:[/color] {data.Temperature - 273.15f:F0} °C\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Luminosity:[/color] {data.Luminosity:F3} L☉\n");
        _tooltipContent.AppendText($"[color=#aabbcc]HZ:[/color] {data.HabitableZoneStart:F2}—{data.HabitableZoneEnd:F2} AU\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Planets:[/color] {data.MaxPlanets} max\n");
        if (data.HasAccretionDisk) _tooltipContent.AppendText("[color=#ff6666]Accretion disk[/color]\n");
        if (data.PulseSpeed > 0) _tooltipContent.AppendText($"[color=#6699ff]Pulse: {data.PulseSpeed:F2} Hz[/color]\n");
        ShowTooltipAbove(target);

        // Also update side panel
        _infoContent.Clear();
        _infoContent.Text = _tooltipContent.Text;
    }

    private void ShowPlanetTooltip(Control target, PlanetData data)
    {
        _tooltipTarget = target;
        _tooltipContent.Clear();
        _tooltipContent.AppendText($"[b]{data.Name}[/b]\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Type:[/color] {data.Type}\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Mass:[/color] {data.Mass:F2} M⊕\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Radius:[/color] {data.Radius:F2} R⊕\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Distance:[/color] {data.OrbitalDistance:F2} AU\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Temp:[/color] {data.Temperature - 273.15f:F0} °C\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Atmo:[/color] {data.AtmosphereIntensity * 100:F0}%\n");
        if (data.HasRings) _tooltipContent.AppendText("[color=#ccaa88]Rings[/color]  ");
        if (data.HasMoons) _tooltipContent.AppendText($"[color=#88aacc]{data.MoonCount} moons[/color]\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Resources:[/color] {data.ResourceRichness * 100:F0}%\n");
        _tooltipContent.AppendText($"[color=#aabbcc]Threat:[/color] {data.ThreatLevel * 100:F0}%\n");
        _tooltipContent.AppendText(data.IsHabitable ? "[color=#66ff88]HABITABLE[/color]" : "[color=#666666]Not habitable[/color]");
        ShowTooltipAbove(target);

        _infoContent.Clear();
        _infoContent.Text = _tooltipContent.Text;
    }

    private void ShowTooltipAbove(Control target)
    {
        _pendingDismiss = false; // cancel any pending dismiss
        _tooltip.Visible = true;
        _tooltip.ResetSize();
        // Position above the target, centered horizontally
        CallDeferred(MethodName.PositionTooltip);
    }

    private void PositionTooltip()
    {
        if (_tooltipTarget == null) return;
        var targetGlobal = _tooltipTarget.GlobalPosition;
        var targetSize = _tooltipTarget.Size * _zoom;
        var tooltipSize = _tooltip.Size;
        var x = targetGlobal.X + (targetSize.X - tooltipSize.X) / 2;
        var y = targetGlobal.Y - tooltipSize.Y - 12;
        // Keep on screen
        x = Mathf.Clamp(x, 8, GetViewportRect().Size.X - tooltipSize.X - 8);
        y = Mathf.Max(y, 8);
        _tooltip.GlobalPosition = new Vector2(x, y);
    }

    private void HideTooltip()
    {
        _tooltip.Visible = false;
        _tooltipTarget = null;
        _followTarget = null;
    }

    private void ShowSystemInfo()
    {
        _infoContent.Clear();
        var sys = _currentSystem;
        _infoContent.AppendText($"[b]{sys.SystemName}[/b]\n");
        _infoContent.AppendText($"[color=#aabbcc]Seed:[/color] {sys.Seed}\n");
        _infoContent.AppendText($"[color=#aabbcc]Central Body:[/color] {sys.CentralBody.Type}\n");
        _infoContent.AppendText($"[color=#aabbcc]Planets:[/color] {sys.Planets.Count}\n");
        _infoContent.AppendText($"\n[color=#667788]Click a body for details.[/color]\n");
    }
}
