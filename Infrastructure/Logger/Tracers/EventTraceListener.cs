using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;

namespace Infrastructure.Logger.Tracers
{
	public class EventTraceListener : CustomTraceListener
	{
		public EventTraceListener(ILogFormatter logFormatter) : base(logFormatter)
		{
		}

		public EventTraceListener(TraceListenerConfig config) : base(config.GetFormatter())
		{
			Name = config.Name;
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.InternalEvent
			};
		}

		public override void Write(LogEntry data, string formattedMessage)
		{
			Fire.Event(new Logged(data.SourceObj, data.Level, formattedMessage));
		}

	}
}
