using System.Collections.Generic;

namespace ProjectKernex.Systems.Shell;

public sealed class ParsedCommand
{
    public string CommandName { get; }
    public string[] Arguments { get; }
    public Dictionary<string, string> Flags { get; }
    public string RawInput { get; }

    public ParsedCommand(string commandName, string[] arguments, Dictionary<string, string> flags, string rawInput)
    {
        CommandName = commandName;
        Arguments = arguments;
        Flags = flags;
        RawInput = rawInput;
    }

    public bool HasFlag(string name) => Flags.ContainsKey(name);

    public string GetFlag(string name, string defaultValue = "") =>
        Flags.TryGetValue(name, out var value) ? value : defaultValue;

    public string GetArgument(int index, string defaultValue = "") =>
        index >= 0 && index < Arguments.Length ? Arguments[index] : defaultValue;
}
