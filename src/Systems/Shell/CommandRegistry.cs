using System.Collections.Generic;
using System.Linq;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Shell;

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void Register(ICommand command)
    {
        _commands[command.Name] = command;
        foreach (var alias in command.Aliases)
            _commands[alias] = command;
    }

    public ICommand GetCommand(string nameOrAlias) =>
        _commands.TryGetValue(nameOrAlias.ToLowerInvariant(), out var cmd) ? cmd : null;

    public IEnumerable<ICommand> GetAllCommands() =>
        _commands.Values.Distinct();
}
