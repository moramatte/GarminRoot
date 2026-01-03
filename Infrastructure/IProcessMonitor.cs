using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Infrastructure
{
    public interface IProcessMonitor
    {
        void Kill()
        {
            Process.GetCurrentProcess().Kill();
        }
    }

    public class ProcessMonitor : IProcessMonitor {}
}