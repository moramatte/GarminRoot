using System;
using Infrastructure.Logger;

namespace Infrastructure.Utilities
{
    public static class Time
    {
        public static void This(string heading, Action code)
        {
            Log.Info(typeof(Time), $"Starting timed operation {heading}");
            var start = DateTime.UtcNow;

            code();

            var elapsed = DateTime.UtcNow - start;
            Log.Info(typeof(Time), $"Finished timed operation {heading} in {elapsed}");
        }
    }
}
