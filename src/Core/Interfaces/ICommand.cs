using System.Threading.Tasks;
using ProjectKernex.Systems.Terminal;

namespace ProjectKernex.Core.Interfaces;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    string[] Aliases { get; }
    Task<CommandResult> ExecuteAsync(ParsedCommand parsed);
}
