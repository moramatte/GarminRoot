using System;

namespace Infrastructure.EventAggregator
{
	public class TopLevelEventAttribute : Attribute { }

    public record Timestamp(DateTime Stamp);

    public static class Stamp
    {
        public static Timestamp Now()
        {
            return new Timestamp(DateTime.UtcNow);  
        }
    }
}
