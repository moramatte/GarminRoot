using System.Diagnostics;

namespace Infrastructure.Logger
{
	public class LogLevelFilter : TraceFilter
	{
		private TraceEventType _eventType;
		public TraceEventType EventType => _eventType;

		public LogLevelFilter(TraceEventType eventType) 
		{ 
			_eventType = eventType;
		}

		public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
		{
			return ShouldTrace(eventType);
		}

		public bool ShouldTrace(TraceEventType eventType)
		{
			return eventType <= _eventType;
		}
	}
}
