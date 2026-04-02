using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Systems.Shell.Commands;

public sealed class AxiaCommand : ICommand
{
    public string Name => "axia";
    public string Description => "Talk to AXIA AI";
    public string Usage => "axia <message>  — Send a message to AXIA (e.g., axia \"status report\")";
    public string[] Aliases => ["a"];

    private readonly AxiaEngine _engine;

    public AxiaCommand(AxiaEngine engine)
    {
        _engine = engine;
    }

    public async Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var message = string.Join(" ", parsed.Arguments);
        if (string.IsNullOrWhiteSpace(message))
            return CommandResult.Error("Usage: axia <message>");

        var response = await _engine.GetResponseAsync(message);
        return CommandResult.Axia($"[AXIA] {response.Text}");
    }
}
