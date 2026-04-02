using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Shell.Commands;

public sealed class HelpCommand : ICommand
{
    public string Name => "help";
    public string Description => "List all available commands";
    public string Usage => "help [command]  — Show help for a specific command, or list all commands";
    public string[] Aliases => ["h"];

    private readonly CommandRegistry _registry;

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry;
    }

    public Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var target = parsed.GetArgument(0);

        if (!string.IsNullOrEmpty(target))
        {
            var cmd = _registry.GetCommand(target);
            if (cmd == null)
                return Task.FromResult(CommandResult.Error($"Unknown command: {target}"));

            var lines = new List<string>
            {
                $"  {cmd.Name} — {cmd.Description}",
                $"  Usage: {cmd.Usage}"
            };
            if (cmd.Aliases.Length > 0)
                lines.Add($"  Aliases: {string.Join(", ", cmd.Aliases)}");

            return Task.FromResult(CommandResult.Info(lines.ToArray()));
        }

        var all = _registry.GetAllCommands().OrderBy(c => c.Name).ToList();
        var output = new List<string> { "Available commands:" };
        foreach (var cmd in all)
            output.Add($"  {cmd.Name,-12} {cmd.Description}");
        output.Add("");
        output.Add("Type 'help <command>' for detailed usage.");

        return Task.FromResult(CommandResult.Info(output.ToArray()));
    }
}
