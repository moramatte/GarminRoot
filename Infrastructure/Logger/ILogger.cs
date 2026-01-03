using System;

namespace Infrastructure.Logger
{
	public interface ILogger
    {
        void Fatal(string message, ILogIdentifiable caller, Exception exception = null);
        void Fatal(string message, string caller, Exception exception = null);
        void Fatal(string message, Type caller, Exception exception = null);

        void Error(string message, ILogIdentifiable caller, Exception exception = null);
        void Error(string message, string caller, Exception exception = null);
        void Error(string message, Type caller, Exception exception = null);
        void Error(string message, Exception exception = null);

        void Warning(string message, ILogIdentifiable caller, Exception exception = null);
        void Warning(string message, string caller, Exception exception = null);
        void Warning(string message, Type caller = null, Exception exception = null);
        

        void Debug(string message, ILogIdentifiable caller, Exception exception = null);
        void Debug(string message, string caller, Exception exception = null);
        void Debug(string message, Type caller = null, Exception exception = null);

        void Info(string message, ILogIdentifiable caller);
        void Info(string message, string caller);
        void Info(string message, Type caller = null);

        void Flush();
    }
}
