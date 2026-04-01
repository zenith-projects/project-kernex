using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Terminal.Commands;

public sealed class ClearCommand : ICommand
{
    public string Name => "clear";
    public string Description => "Clear the terminal screen";
    public string Usage => "clear  — Clear all output from the terminal";
    public string[] Aliases => ["cls"];

    public Task<CommandResult> ExecuteAsync(ParsedCommand parsed) =>
        Task.FromResult(CommandResult.ClearScreen());
}
