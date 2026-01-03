using Infrastructure.Logger.Enterprise;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Infrastructure.Extensions;
using log4net;
using Awos7.WindowsService.LogConfig;
using Infrastructure.Collections;

namespace Infrastructure.Logger.Tracers
{
	public class Log4NetFileTraceListener : CustomTraceListener
	{
		private readonly ILog _log;
		private readonly FileAppender _fileAppender;

		public string FileName { get; private init; }

		public Log4NetFileTraceListener(TraceListenerConfig config) : base(config.GetFormatter())
		{
			FileName = config.Get<string>("FileName");
			Name = config.Name;
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with 
			{ 
				Type = TraceListeners.Log4NetFile, 
				Values = new EquatableList<ConfigValue>([new("FileName", FileName)])
			};
		}

		public Log4NetFileTraceListener(ILogFormatter logFormatter, string logFilePath, string name) : base(logFormatter)
		{
			FileName = logFilePath;
            var layout = new PatternLayout
            {
                ConversionPattern = "%date [%level] %message%newline"
            };
            layout.ActivateOptions();
			_fileAppender = new FileAppender
			{
				AppendToFile = true,
				File = logFilePath.HasContent() ? logFilePath : "log.txt",
				Layout = layout
			};
			_fileAppender.ActivateOptions();


			// Get the hierarchy repository and configure the root logger
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.AddAppender(_fileAppender);
			hierarchy.Configured = true;

			Name = name;
			_log = LogManager.GetLogger(Name);
		}

		public override void Flush()
		{
			base.Flush();
			if (!_fileAppender.Flush(2000))
				Log.Error(GetType(), "Flush failed.");
		}

		public override void Write(LogEntry data, string message)
		{
			switch (data.Level)
			{
				case LogLevel.Debug:
					_log.Debug(message);
					break;
				case LogLevel.Info:
					_log.Info(message);
					break;
				case LogLevel.Warning:
					_log.Warn(message);
					break;
				case LogLevel.Error:
					_log.Error(message);
					break;
				case LogLevel.Fatal:
					_log.Fatal(message);
					break;
			}
		}

	}
}
