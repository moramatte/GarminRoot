using Infrastructure.Logger.Enterprise;
using System;

namespace InfrastructureTests.Logging
{
    public class MockDateTimeProvider : DateTimeProvider
    {
        public MockDateTimeProvider() { Current = DateTime.UtcNow; }
        public DateTime Current { get; set; }
        public override DateTime CurrentUtcDateTime => Current;
    }
}
