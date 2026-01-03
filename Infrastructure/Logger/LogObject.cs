using System;

namespace Infrastructure.Logger
{
    public class LogObject
    {
        public LogLevel LogLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public string Id { get; set; }
        public string Message { get; set; }

        public LogObject(LogLevel logLevel, DateTime timestamp, string id, string message)
        {
            LogLevel = logLevel;
            Timestamp = timestamp;
            Id = id;
            Message = message;
        }
    }

}