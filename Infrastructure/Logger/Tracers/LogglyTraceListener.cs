using Infrastructure.Logger.Enterprise;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Infrastructure.Extensions;
using log4net;
using log4net.loggly;
using System.Diagnostics;
using Awos7.WindowsService.LogConfig;
using Infrastructure.Collections;

namespace Infrastructure.Logger.Tracers
{
	public class LogglyTraceListener : CustomTraceListener
	{
		readonly LogglyAppender _appender;
		readonly ILog _log;

		public LogglyTraceListener(TraceListenerConfig config) : this(config.GetFormatter(), config.Get<string>("RootUrl"), config.Get<string>("Tag"), config.Get<string>("CustomerToken"), config.Name)
		{
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.Loggly,
				Values = new EquatableList<ConfigValue>([
					new("RootUrl", _appender.RootUrl),
					new("Tag", _appender.Tag),
					new("CustomerToken", _appender.CustomerToken),
				])
			};
		}

		public LogglyTraceListener(ILogFormatter logFormatter, string rootUrl, string tag, string customerToken, string name) : base(logFormatter)
		{
			Name = name;
			_appender = new LogglyAppender() { RootUrl = rootUrl, Tag = tag, CustomerToken = customerToken };
			var layout = new PatternLayout();
			layout.ActivateOptions();
			_appender.Layout = layout;
			_appender.ActivateOptions();
			_log = LogManager.GetLogger(Name);

			// Get the hierarchy repository and configure the root logger
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.AddAppender(_appender);
			hierarchy.Configured = true;
		}

		public LogglyAppender Appender => _appender;

		public override void Flush()
		{
			base.Flush();

			if (!_appender.Flush(2000))
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
