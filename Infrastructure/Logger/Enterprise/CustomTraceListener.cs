using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Formatters;
using System.Diagnostics;

namespace Infrastructure.Logger.Enterprise
{
	/// <summary>
	/// Base class for custom trace listeners that support formatters.
	/// </summary>
	public abstract class CustomTraceListener : ITraceListener
	{
        protected CustomTraceListener(ILogFormatter formatter)
		{
			this.Formatter = formatter;
			this.Name = nameof(CustomTraceListener);
		}

        public ILogFormatter Formatter { get; set; }
		public TraceFilter Filter { get; set; }
		public string Name { get; set; }

		public void Write(TraceEventCache eventCache, string source, TraceEventType eventType, int id, LogEntry data)
		{
			if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, data.Message, null, data, null))
				return;

			if (Formatter != null)
				Write(data, Formatter.Format(data));
			else
				Write(data, data.Message);
		}

		public abstract void Write(LogEntry data, string formattedMessage);

		public virtual void Flush() { }
		public virtual void Dispose() => Flush();

		public abstract TraceListenerConfig ToConfig();
		
	}
}
