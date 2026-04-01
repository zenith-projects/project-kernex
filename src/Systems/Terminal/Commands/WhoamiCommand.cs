using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Terminal.Commands;

public sealed class WhoamiCommand : ICommand
{
    public string Name => "whoami";
    public string Description => "Display operator info";
    public string Usage => "whoami  — Show current operator identity and station info";
    public string[] Aliases => [];

    public Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var lines = new string[]
        {
            "Operator:  UNKNOWN",
            "Station:   KERNEX-001",
            "Faction:   Unaligned",
            "Sector:    Q0-S0-C0",
            "Session:   Active"
        };

        return Task.FromResult(CommandResult.Info(lines));
    }
}
