using Infrastructure.Logger.Formatters;

namespace Infrastructure.Logger
{
    public class DefaultLogger : TraceLogger, ILogger
    {
        public DefaultLogger()
        {
            var formatter = new RawFormatter();
            _listeners.Add(new CustomConsoleTraceListener(formatter));
        }

	}
}
