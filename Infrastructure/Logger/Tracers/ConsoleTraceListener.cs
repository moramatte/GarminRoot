using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;
using System;

namespace Infrastructure.Logger
{
	public class CustomConsoleTraceListener : CustomTraceListener
	{
		public CustomConsoleTraceListener(ILogFormatter logFormatter) : base(logFormatter) { }

		public CustomConsoleTraceListener(TraceListenerConfig config) : this(config.GetFormatter())
		{
			Name = config.Name;
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.Console
			};
		}

		public override void Write(LogEntry data, string formattedMessage)
		{
			Console.WriteLine(formattedMessage);
		}
	}

}


