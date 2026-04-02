using System.Collections.Generic;
using Godot;

namespace ProjectKernex.Components.HUD;

public partial class ShellInput : HBoxContainer
{
    [Signal] public delegate void CommandSubmittedEventHandler(string command);

    [Export] public string Prompt { get; set; } = "vsh> ";
    [Export] public int MaxHistory { get; set; } = 50;

    private Label _promptLabel;
    private LineEdit _inputField;
    private readonly List<string> _history = new();
    private int _historyIndex = -1;

    public override void _Ready()
    {
        _promptLabel = GetNode<Label>("PromptLabel");
        _inputField = GetNode<LineEdit>("InputField");

        _promptLabel.Text = Prompt;
        _inputField.PlaceholderText = "type a command...";
        _inputField.TextSubmitted += OnTextSubmitted;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_inputField.HasFocus()) return;

        if (@event is InputEventKey { Pressed: true } key)
        {
            if (key.Keycode == Key.Up)
            {
                NavigateHistory(-1);
                GetViewport().SetInputAsHandled();
            }
            else if (key.Keycode == Key.Down)
            {
                NavigateHistory(1);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public void FocusInput() => _inputField.GrabFocus();

    public void SetPrompt(string prompt)
    {
        Prompt = prompt;
        if (_promptLabel != null)
            _promptLabel.Text = prompt;
    }

    private void OnTextSubmitted(string text)
    {
        var trimmed = text.Trim();
        _inputField.Clear();

        if (string.IsNullOrEmpty(trimmed)) return;

        if (_history.Count == 0 || _history[^1] != trimmed)
        {
            _history.Add(trimmed);
            if (_history.Count > MaxHistory)
                _history.RemoveAt(0);
        }
        _historyIndex = -1;

        EmitSignal(SignalName.CommandSubmitted, trimmed);
    }

    private void NavigateHistory(int direction)
    {
        if (_history.Count == 0) return;

        if (_historyIndex == -1)
        {
            if (direction == -1)
                _historyIndex = _history.Count - 1;
            else
                return;
        }
        else
        {
            _historyIndex += direction;
        }

        _historyIndex = Mathf.Clamp(_historyIndex, 0, _history.Count - 1);
        _inputField.Text = _history[_historyIndex];
        _inputField.CaretColumn = _inputField.Text.Length;
    }
}
