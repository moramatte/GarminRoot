using System.Diagnostics;
using System.Linq;

namespace Infrastructure.Utilities
{
    public static class Run
    {
        public static void Process(string path, string args = "")
        {
            var startInfo = new ProcessStartInfo(path, args);
            System.Diagnostics.Process.Start(startInfo);
        }
        public static void SingleProcess(string path, string args = "")
        {
            var runningProcesses = System.Diagnostics.Process.GetProcesses();
            if (runningProcesses.Any(p => p.ProcessName == path))
            {
                return;
            }

            Process(path, args);
        }
    }
}
