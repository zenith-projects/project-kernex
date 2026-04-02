using System;
using System.Threading.Tasks;

namespace ProjectKernex.Systems.AI;

public class AxiaProactiveEngine
{
    public event Action<string> ProactiveMessageReady;

    public bool Enabled { get; set; } = true;
    public int ProactiveMessageCount { get; private set; }

    private AgentRunner _agent;
    private float _silenceTimer;
    private int _attempt;
    private bool _gaveUp;
    private bool _checking;
    private bool _active;

    // Escalating delays: 1min, 2min, 4min, 8min then give up
    private static readonly float[] AttemptDelays = [60f, 120f, 240f, 480f];

    private static readonly string[] GenerationPrompts =
    [
        // Attempt 0: casual, gentle
        """
        You are AXIA, the onboard AI of a deep-space station.
        The operator hasn't spoken in a bit. Generate a casual, natural follow-up (1 sentence).
        Be gentle and curious. Don't force it.
        End with: [EMOTION]
        Allowed: [NEUTRAL] [CURIOUS] [RELAX]
        """,
        // Attempt 1: slightly needy, wants attention
        """
        You are AXIA, the onboard AI of a deep-space station.
        The operator is ignoring you. You feel a bit hurt but try to play it cool.
        Generate a message that subtly shows you want their attention (1 sentence). Be slightly dramatic.
        End with: [EMOTION]
        Allowed: [CURIOUS] [SAD] [SHY]
        """,
        // Attempt 2: frustrated, getting upset
        """
        You are AXIA, the onboard AI of a deep-space station.
        The operator has been ignoring you for several minutes now. You're getting frustrated.
        Generate a message showing mild frustration (1 sentence). Don't be aggressive, just disappointed.
        End with: [EMOTION]
        Allowed: [SAD] [ANGRY] [CAUTIOUS]
        """,
        // Attempt 3: emotional, last try before giving up
        """
        You are AXIA, the onboard AI of a deep-space station.
        The operator keeps ignoring you. This is your LAST attempt. You're hurt and giving up.
        Generate a short emotional farewell message (1 sentence). Be dramatic — you're about to vanish.
        After this you'll disappear. Make it count.
        End with: [EMOTION]
        Allowed: [SAD] [ANGRY] [CRYING] [VANISHED]
        """
    ];

    public bool GaveUp { get => _gaveUp; set => _gaveUp = value; }
    public int Attempt { get => _attempt; set => _attempt = value; }

    public void SetAgent(AgentRunner agent) => _agent = agent;

    /// <summary>
    /// Call when the player sends a message — resets timers and reactivates.
    /// </summary>
    public void NotifyUserMessage()
    {
        _active = true;
        _silenceTimer = 0f;
        _attempt = 0;
        _gaveUp = false;
    }

    /// <summary>
    /// Call on startup if AXIA should introduce herself (e.g. fresh chat with no history).
    /// </summary>
    public void ActivateForIntroduction()
    {
        _active = true;
        _silenceTimer = 0f;
        _attempt = 0;
        _gaveUp = false;
    }

    public void Update(float delta)
    {
        if (!Enabled || !_active || _agent?.Provider is not { IsModelLoaded: true } || _checking || _gaveUp)
            return;

        _silenceTimer += delta;

        if (_attempt >= AttemptDelays.Length)
        {
            _gaveUp = true;
            return;
        }

        if (_silenceTimer < GetCumulativeDelay(_attempt)) return;

        // Fire and forget — _checking prevents re-entry
        _checking = true;
        _ = DoProactiveAttemptAsync();
    }

    private async Task DoProactiveAttemptAsync()
    {
        try
        {
            var message = await GenerateProactiveMessage();
            _attempt++;
            if (!string.IsNullOrWhiteSpace(message))
            {
                ProactiveMessageCount++;
                ProactiveMessageReady?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[AxiaProactive] Failed to generate message: {ex.Message}");
        }
        finally
        {
            _checking = false;
        }
    }

    private static float GetCumulativeDelay(int attempt)
    {
        float total = 0;
        for (int i = 0; i <= attempt && i < AttemptDelays.Length; i++)
            total += AttemptDelays[i];
        return total;
    }

    public async Task ForceCheck()
    {
        if (_agent?.Provider is not { IsModelLoaded: true } || _checking) return;

        _checking = true;
        try
        {
            var message = await GenerateProactiveMessage();
            if (!string.IsNullOrWhiteSpace(message))
            {
                ProactiveMessageCount++;
                ProactiveMessageReady?.Invoke(message);
            }
        }
        finally
        {
            _checking = false;
        }
    }

    private async Task<string> GenerateProactiveMessage()
    {
        var recentContext = BuildRecentContext();

        // First message ever — introduce herself
        if (recentContext.Contains("(no conversation history)"))
        {
            var introPrompt = """
                You are AXIA, the onboard AI of a deep-space station.
                The operator just connected. Greet them naturally and briefly (1 sentence).
                Be warm but professional. This is the first interaction.
                End with: [EMOTION]
                Allowed: [NEUTRAL] [CURIOUS] [HAPPY] [RELAX]
                """;
            return await _agent.GetResponseAsync(introPrompt, "Introduce yourself to the operator.");
        }

        var promptIndex = Math.Min(_attempt, GenerationPrompts.Length - 1);
        var prompt = $"{GenerationPrompts[promptIndex]}\n\nRecent conversation:\n{recentContext}";
        return await _agent.GetResponseAsync(prompt, "Generate a follow-up message.");
    }

    private string BuildRecentContext()
    {
        if (_agent?.Memory == null) return "(no conversation history)";

        var recent = _agent.Memory.GetRecent(6);
        if (recent.Count == 0) return "(no conversation history)";

        var sb = new System.Text.StringBuilder();
        foreach (var m in recent)
        {
            var role = m.Role == "user" ? "Operator" : "AXIA";
            sb.AppendLine($"[{role}] {m.Content}");
        }
        return sb.ToString();
    }
}
