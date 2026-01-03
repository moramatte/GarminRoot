using System;

namespace Infrastructure.Logger
{
	public abstract class LoggerBase : ILogger, IDisposable
	{
		public void Fatal(string message, ILogIdentifiable caller, Exception exception = null)
		{
			Write(caller, LogLevel.Fatal, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Fatal(string message, string caller, Exception exception = null)
		{
			Write(caller, LogLevel.Fatal, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Fatal(string message, Type caller, Exception exception = null)
		{
			Write(caller, LogLevel.Fatal, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Error(string message, ILogIdentifiable caller, Exception exception = null)
		{
			Write(caller, LogLevel.Error, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Error(string message, string caller, Exception exception = null)
		{
			Write(caller, LogLevel.Error, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Error(string message, Type caller, Exception exception = null)
		{
			Write(caller, LogLevel.Error, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Error(string message, Exception exception = null)
		{
			Write(null, LogLevel.Error, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Warning(string message, ILogIdentifiable caller, Exception exception = null)
		{
			Write(caller, LogLevel.Warning, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Warning(string message, string caller, Exception exception = null)
		{
			Write(caller, LogLevel.Warning, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Warning(string message, Type caller = null, Exception exception = null)
		{
			Write(caller, LogLevel.Warning, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Debug(string message, ILogIdentifiable caller, Exception exception = null)
		{
			Write(caller, LogLevel.Debug, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Debug(string message, string caller, Exception exception = null)
		{
			Write(caller, LogLevel.Debug, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Debug(string message, Type caller = null, Exception exception = null)
		{
			Write(caller, LogLevel.Debug, exception == null ? message : $"{message} : {exception?.Message}");
		}

		public void Info(string message, ILogIdentifiable caller)
		{
			Write(caller, LogLevel.Info, message);
		}

		public void Info(string message, string caller)
		{
			Write(caller, LogLevel.Info, message);
		}

		public void Info(string message, Type caller = null)
		{
			Write(caller, LogLevel.Info, message);
		}

		protected abstract void Write(object sourceObj, LogLevel level, string message);
		public abstract void Flush();

		public virtual void Dispose()
		{
			Flush();
		}
	}
}
