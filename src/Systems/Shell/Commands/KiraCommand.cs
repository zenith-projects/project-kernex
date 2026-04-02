using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;
using ProjectKernex.Systems.AI;

namespace ProjectKernex.Systems.Shell.Commands;

public sealed class KiraCommand : ICommand
{
    public string Name => "kira";
    public string Description => "Talk to KIRA AI";
    public string Usage => "kira <message>  — Send a message to KIRA (e.g., kira \"status report\")";
    public string[] Aliases => ["k"];

    private readonly KiraEngine _engine;

    public KiraCommand(KiraEngine engine)
    {
        _engine = engine;
    }

    public async Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var message = string.Join(" ", parsed.Arguments);
        if (string.IsNullOrWhiteSpace(message))
            return CommandResult.Error("Usage: kira <message>");

        var response = await _engine.GetResponseAsync(message);
        return CommandResult.Kira($"[KIRA] {response.Text}");
    }
}
