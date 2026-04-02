using System.Text.RegularExpressions;

namespace ProjectKernex.Systems.AI;

public readonly struct AxiaResponse
{
    public static readonly string[] ValidEmotions =
    [
        "NEUTRAL", "CURIOUS", "HAPPY", "SAD", "ANGRY",
        "CRAZY", "CRYING", "CAUTIOUS", "RELAX", "SHY", "VANISHED"
    ];

    private static readonly Regex EmotionTagRegex = new(@"\[([A-Z_]+)\]", RegexOptions.Compiled);

    public string Text { get; }
    public string Emotion { get; }

    public AxiaResponse(string text, string emotion)
    {
        Text = text;
        Emotion = emotion;
    }

    public static AxiaResponse Parse(string rawResponse)
    {
        if (string.IsNullOrEmpty(rawResponse))
            return new AxiaResponse("", "NEUTRAL");

        var lastEmotion = "NEUTRAL";
        var matches = EmotionTagRegex.Matches(rawResponse);

        foreach (Match match in matches)
        {
            var tag = match.Groups[1].Value;
            foreach (var valid in ValidEmotions)
            {
                if (tag == valid)
                {
                    lastEmotion = tag;
                    break;
                }
            }
        }

        // Strip all emotion tags from the display text
        var cleanText = EmotionTagRegex.Replace(rawResponse, "");
        cleanText = Regex.Replace(cleanText, @"\s{2,}", " ").Trim();

        return new AxiaResponse(cleanText, lastEmotion);
    }
}
