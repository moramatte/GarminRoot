using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Infrastructure.Logger.Enterprise
{
	/// <summary>
	/// Represents a log message.  Contains the common properties that are required for all log messages.
	/// </summary>
	[XmlRoot("logEntry")]
	[Serializable]
	public class LogEntry : ICloneable
	{
		private int _threadId;
		private int _eventId = 0;
		private LogLevel _logLevel;
		private string _message = string.Empty;
		private object _sourceObj;
		private string _source;

        public string Source
        {
            get => _source;
            set => _source = value;
        }

        public DateTime Timestamp { get; set; }

        public string Message
		{ 
			get => _message; 
			set => _message = value;
		}

		/// <summary>
		/// Unique Id for each log message
		/// </summary>
		public int EventId
		{ 
			get => _eventId; 
			set => _eventId = value;
		}

		public int ThreadId
		{
			get => _threadId;
			set => _threadId = value;
		}

		/// <summary>
		/// The object which created the log message
		/// </summary>
		public object SourceObj
		{
			get => _sourceObj;
			set => _sourceObj = value;
		}

		public TraceEventType Severity
		{ 
			get => LogLevelToTraceEventType(_logLevel); 
		}

		private static TraceEventType LogLevelToTraceEventType(LogLevel logLevel)
		{
			return logLevel switch
			{
				LogLevel.Debug => TraceEventType.Verbose,
				LogLevel.Error => TraceEventType.Error,
				LogLevel.Fatal => TraceEventType.Critical,
				LogLevel.Warning => TraceEventType.Warning,
				_ => TraceEventType.Information,
			};
		}

		public LogLevel Level
		{
			get => _logLevel;
			set => _logLevel = value;
		}

		public object Clone()
		{
			return new LogEntry { EventId = _eventId, Message = _message, SourceObj = _sourceObj, Level = _logLevel, ThreadId = _threadId };
		}
	}
}
