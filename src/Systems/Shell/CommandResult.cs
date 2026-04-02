using ProjectKernex.Core.Enums;

namespace ProjectKernex.Systems.Shell;

public sealed class CommandResult
{
    public CommandResultType Type { get; }
    public string[] Lines { get; }

    private CommandResult(CommandResultType type, string[] lines)
    {
        Type = type;
        Lines = lines;
    }

    public static CommandResult Success(string text) =>
        new(CommandResultType.Success, [text]);

    public static CommandResult Success(string[] lines) =>
        new(CommandResultType.Success, lines);

    public static CommandResult Error(string text) =>
        new(CommandResultType.Error, [text]);

    public static CommandResult Info(string text) =>
        new(CommandResultType.Info, [text]);

    public static CommandResult Info(string[] lines) =>
        new(CommandResultType.Info, lines);

    public static CommandResult Warning(string text) =>
        new(CommandResultType.Warning, [text]);

    public static CommandResult Axia(string text) =>
        new(CommandResultType.Axia, [text]);

    public static CommandResult Axia(string[] lines) =>
        new(CommandResultType.Axia, lines);

    public static CommandResult SystemMsg(string text) =>
        new(CommandResultType.System, [text]);

    public static CommandResult ClearScreen() =>
        new(CommandResultType.Clear, []);
}
