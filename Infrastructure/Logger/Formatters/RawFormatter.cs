using Infrastructure.Logger.Enterprise;

namespace Infrastructure.Logger.Formatters
{
	public class RawFormatter : ILogFormatter
	{
		public string Format(LogEntry log)
		{
			return log.Message;
		}
	}
}
