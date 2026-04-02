using System.Threading.Tasks;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.Shell.Commands;

public sealed class UptimeCommand : ICommand
{
    public string Name => "uptime";
    public string Description => "Station uptime";
    public string Usage => "uptime  — Display how long the station has been running";
    public string[] Aliases => [];

    private readonly ulong _startTimeMsec;

    public UptimeCommand()
    {
        _startTimeMsec = Godot.Time.GetTicksMsec();
    }

    public Task<CommandResult> ExecuteAsync(ParsedCommand parsed)
    {
        var elapsed = Godot.Time.GetTicksMsec() - _startTimeMsec;
        var seconds = elapsed / 1000;
        var minutes = seconds / 60;
        var hours = minutes / 60;

        var uptime = hours > 0
            ? $"{hours}h {minutes % 60}m {seconds % 60}s"
            : minutes > 0
                ? $"{minutes}m {seconds % 60}s"
                : $"{seconds}s";

        return Task.FromResult(CommandResult.Info($"Station uptime: {uptime}"));
    }
}
