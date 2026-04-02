using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Shell.Commands;

public sealed class StatusCommand : ICommand
{
    public string Name => "status";
    public string Description => "Station overview";
    public string Usage => "status [--verbose]  — Display station status summary";
    public string[] Aliases => ["st"];

    public Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var verbose = parsed.HasFlag("verbose") || parsed.HasFlag("v");

        var lines = new List<string>
        {
            "=== Station Status ===",
            "  Hull:       100%",
            "  Power:      450 / 600 kW",
            "  Storage:    12 / 50 units",
            "  Drones:     2 active, 1 idle",
            "  Threat:     LOW",
        };

        if (verbose)
        {
            lines.Add("");
            lines.Add("=== Modules ===");
            lines.Add("  Solar Array Mk.I    — Online  (150 kW)");
            lines.Add("  Extractor Alpha     — Online  (Metallum)");
            lines.Add("  Refinery Beta       — Idle");
            lines.Add("  Storage Bay         — 24% capacity");
            lines.Add("");
            lines.Add("=== Active Drones ===");
            lines.Add("  DRN-01  Mining     AST-7A    ETA 2m");
            lines.Add("  DRN-02  Mining     AST-7B    ETA 5m");
        }

        return Task.FromResult(CommandResult.Success(lines.ToArray()));
    }
}
