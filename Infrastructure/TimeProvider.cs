using System;

namespace Infrastructure
{
    public class TimeProvider : ITimeProvider
    {
        public DateTime Now { get { return DateTime.Now; } }
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
        public DateTime Today { get { return DateTime.Today; } }
    }
}
