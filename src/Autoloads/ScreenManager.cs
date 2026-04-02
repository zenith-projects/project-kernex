using Godot;

namespace ProjectKernex.Autoloads;

public partial class ScreenManager : Node
{
    [Signal] public delegate void ScreenChangedEventHandler(string screenName);

    private ColorRect _fadeOverlay;
    private Node _currentScreen;

    private const float FadeDuration = 0.3f;

    public override void _Ready()
    {
        _fadeOverlay = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _fadeOverlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var canvas = new CanvasLayer { Layer = 100 };
        canvas.AddChild(_fadeOverlay);
        AddChild(canvas);
    }

    public void ChangeScreen(string scenePath)
    {
        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null)
        {
            GD.PrintErr($"[ScreenManager] Scene not found: {scenePath}");
            return;
        }
        ChangeScreen(scene);
    }

    public void ChangeScreen(PackedScene scene)
    {
        _fadeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;

        var tween = CreateTween();
        tween.TweenProperty(_fadeOverlay, "color:a", 1.0f, FadeDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            _currentScreen?.QueueFree();
            _currentScreen = scene.Instantiate();
            GetTree().Root.AddChild(_currentScreen);

            EmitSignal(SignalName.ScreenChanged, _currentScreen.Name);

            var fadeIn = CreateTween();
            fadeIn.TweenProperty(_fadeOverlay, "color:a", 0.0f, FadeDuration);
            fadeIn.TweenCallback(Callable.From(() =>
            {
                _fadeOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
            }));
        }));
    }

    public void SetInitialScreen(Node screen)
    {
        _currentScreen = screen;
    }
}
