using Godot;
using ProjectKernex.Core.Enums;

namespace ProjectKernex.Components.HUD;

public partial class KiraPanel : PanelContainer
{
    [Signal] public delegate void MessageSubmittedEventHandler(string message);

    private static readonly Color ColorGold = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorGoldBright = new(0.91f, 0.75f, 0.38f);
    private static readonly Color ColorUserText = new(0.75f, 0.75f, 0.78f);
    private static readonly Color ColorKiraText = new(0.831f, 0.659f, 0.294f);
    private static readonly Color ColorDimText = new(0.45f, 0.45f, 0.48f);
    private static readonly Color ColorBg = new(0.04f, 0.04f, 0.055f, 0.95f);

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

        LoadFont();
        ApplyStyle();
    }

    private void LoadFont()
    {
        var font = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Regular.ttf");
        var fontBold = ResourceLoader.Load<Font>("res://src/Assets/Fonts/JetBrainsMono-Bold.ttf");
        if (font == null) return;

        _chatLog.AddThemeFontOverride("normal_font", font);
        _chatLog.AddThemeFontOverride("bold_font", fontBold ?? font);
        _messageInput.AddThemeFontOverride("font", font);
        _nameLabel.AddThemeFontOverride("font", fontBold ?? font);
        _statusLabel.AddThemeFontOverride("font", font);
        _sendButton.AddThemeFontOverride("font", font);
    }

    private void ApplyStyle()
    {
        // Header
        _nameLabel.AddThemeColorOverride("font_color", ColorGold);
        _nameLabel.AddThemeFontSizeOverride("font_size", 20);
        _statusLabel.AddThemeColorOverride("font_color", ColorDimText);
        _statusLabel.AddThemeFontSizeOverride("font_size", 12);

        // Chat log
        var chatBg = new StyleBoxFlat
        {
            BgColor = new Color(0.03f, 0.03f, 0.04f, 0.6f),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            ContentMarginLeft = 14, ContentMarginTop = 10,
            ContentMarginRight = 14, ContentMarginBottom = 10,
        };
        _chatLog.AddThemeStyleboxOverride("normal", chatBg);
        _chatLog.AddThemeColorOverride("default_color", ColorUserText);
        _chatLog.AddThemeFontSizeOverride("normal_font_size", 14);

        // Thinking label
        _thinkingLabel.AddThemeColorOverride("font_color", new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.6f));
        _thinkingLabel.AddThemeFontSizeOverride("font_size", 12);

        // Input
        var inputNormal = new StyleBoxFlat
        {
            BgColor = new Color(0.06f, 0.06f, 0.075f, 0.95f),
            BorderColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.2f),
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12, ContentMarginTop = 10,
            ContentMarginRight = 12, ContentMarginBottom = 10,
            AntiAliasing = true,
        };
        var inputFocus = (StyleBoxFlat)inputNormal.Duplicate();
        inputFocus.BorderColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.5f);

        _messageInput.AddThemeStyleboxOverride("normal", inputNormal);
        _messageInput.AddThemeStyleboxOverride("focus", inputFocus);
        _messageInput.AddThemeColorOverride("font_color", ColorUserText);
        _messageInput.AddThemeColorOverride("font_placeholder_color", ColorDimText);
        _messageInput.AddThemeColorOverride("caret_color", ColorGold);
        _messageInput.AddThemeFontSizeOverride("font_size", 14);

        // Send button
        var btnNormal = new StyleBoxFlat
        {
            BgColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.15f),
            BorderColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.4f),
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
            ContentMarginLeft = 16, ContentMarginTop = 10,
            ContentMarginRight = 16, ContentMarginBottom = 10,
            AntiAliasing = true,
        };
        var btnHover = (StyleBoxFlat)btnNormal.Duplicate();
        btnHover.BgColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.25f);
        var btnPressed = (StyleBoxFlat)btnNormal.Duplicate();
        btnPressed.BgColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.35f);

        _sendButton.AddThemeStyleboxOverride("normal", btnNormal);
        _sendButton.AddThemeStyleboxOverride("hover", btnHover);
        _sendButton.AddThemeStyleboxOverride("pressed", btnPressed);
        _sendButton.AddThemeStyleboxOverride("focus", btnNormal);
        _sendButton.AddThemeColorOverride("font_color", ColorGold);
        _sendButton.AddThemeColorOverride("font_hover_color", ColorGoldBright);
        _sendButton.AddThemeFontSizeOverride("font_size", 14);

        // Separator
        var sepStyle = new StyleBoxFlat
        {
            BgColor = new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0.08f),
            ContentMarginTop = 0, ContentMarginBottom = 0,
        };
        foreach (var sep in new[] { "MainLayout/HSeparator", "MainLayout/HSeparator2" })
        {
            var node = GetNode<HSeparator>(sep);
            node.AddThemeStyleboxOverride("separator", sepStyle);
            node.AddThemeConstantOverride("separation", 1);
        }
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
