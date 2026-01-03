using System.Diagnostics;

namespace Infrastructure;

public interface IKillable
{
    void Kill()
    {
        var monitor = ServiceLocator.Resolve<IProcessMonitor>();
        monitor.Kill();
    }
}

public static class IKillableExtensions
{
    public static void Kill(this IKillable killable)
    {
        killable.Kill();
    }
}