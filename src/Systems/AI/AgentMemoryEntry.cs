namespace ProjectKernex.Systems.AI;

public class AgentMemoryEntry
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
    public string Tag { get; set; } = "";
    public long Timestamp { get; set; }
    public bool Summarized { get; set; }
}
