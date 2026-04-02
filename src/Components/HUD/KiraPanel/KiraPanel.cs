using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Components.HUD;

public partial class KiraPanel : PanelContainer
{
    [Signal] public delegate void MessageSubmittedEventHandler(string message);

    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorUserText = new(0.75f, 0.75f, 0.78f);
    private static readonly Color ColorKiraText = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorDimText = new(0.45f, 0.45f, 0.48f);

    private KiraAvatar _avatar;
    private Label _nameLabel;
    private Label _statusLabel;
    private RichTextLabel _chatLog;
    private ScrollContainer _chatScroll;
    private Label _thinkingLabel;
    private LineEdit _messageInput;
    private Button _sendButton;

    public override void _Ready()
    {
        _avatar = GetNode<KiraAvatar>("MainLayout/HeaderBar/HeaderMargin/HeaderContent/KiraAvatar");
        _nameLabel = GetNode<Label>("MainLayout/HeaderBar/HeaderMargin/HeaderContent/HeaderInfo/NameLabel");
        _statusLabel = GetNode<Label>("MainLayout/HeaderBar/HeaderMargin/HeaderContent/HeaderInfo/StatusLabel");
        _chatScroll = GetNode<ScrollContainer>("MainLayout/ChatScroll");
        _chatLog = GetNode<RichTextLabel>("MainLayout/ChatScroll/ChatMargin/ChatLog");
        _thinkingLabel = GetNode<Label>("MainLayout/ThinkingLabel");
        _messageInput = GetNode<LineEdit>("MainLayout/InputMargin/InputBar/MessageInput");
        _sendButton = GetNode<Button>("MainLayout/InputMargin/InputBar/SendButton");

        _chatLog.BbcodeEnabled = true;
        _thinkingLabel.Visible = false;

        _sendButton.Pressed += OnSendPressed;
        _messageInput.TextSubmitted += OnTextSubmitted;
    }

    public void SetLevel(KiraLevel level)
    {
        _avatar.SetLevel(level);
        _statusLabel.Text = $"Level {(int)level} — {level}";
    }

    public void SetThinking(bool thinking)
    {
        _thinkingLabel.Visible = thinking;
        _messageInput.Editable = !thinking;
        _sendButton.Disabled = thinking;
        if (thinking)
            _thinkingLabel.Text = "KIRA is thinking...";
    }

    public void AppendUserMessage(string message)
    {
        var hex = ColorDimText.ToHtml(false);
        var textHex = ColorUserText.ToHtml(false);
        _chatLog.AppendText($"\n[color=#{hex}]You[/color]\n");
        _chatLog.AppendText($"[color=#{textHex}]{EscapeBbcode(message)}[/color]\n");
        ScrollToBottom();
    }

    public void AppendKiraMessage(string message)
    {
        var labelHex = ColorGold.ToHtml(false);
        var textHex = ColorKiraText.ToHtml(false);
        _chatLog.AppendText($"\n[color=#{labelHex}]KIRA[/color]\n");
        _chatLog.AppendText($"[color=#{textHex}]{EscapeBbcode(message)}[/color]\n");
        ScrollToBottom();
    }

    public void ClearChat() => _chatLog.Clear();

    public void FocusInput() => _messageInput.GrabFocus();

    private void OnSendPressed() => SubmitMessage();
    private void OnTextSubmitted(string text) => SubmitMessage();

    private void SubmitMessage()
    {
        var text = _messageInput.Text.Trim();
        _messageInput.Clear();
        if (string.IsNullOrEmpty(text)) return;
        EmitSignal(SignalName.MessageSubmitted, text);
    }

    private void ScrollToBottom() => CallDeferred(MethodName.DeferredScrollToBottom);

    private void DeferredScrollToBottom() =>
        _chatScroll.ScrollVertical = (int)_chatScroll.GetVScrollBar().MaxValue;

    private static string EscapeBbcode(string text) => text.Replace("[", "[lb]");
}
