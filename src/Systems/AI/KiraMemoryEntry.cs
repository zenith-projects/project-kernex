namespace ProjectKernex.Systems.AI;

public class KiraMemoryEntry
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
    public string Emotion { get; set; } = "";
    public long Timestamp { get; set; }
    public bool Summarized { get; set; }
}
