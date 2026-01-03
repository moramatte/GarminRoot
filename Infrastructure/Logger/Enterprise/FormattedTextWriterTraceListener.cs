using System;
using System.Diagnostics;
using System.IO;

namespace Infrastructure.Logger.Enterprise
{
	/// <summary>
	/// Extends <see cref="TextWriterTraceListener"/> to add formatting capabilities.
	/// </summary>
	public class FormattedTextWriterTraceListener : TextWriterTraceListener
	{

        /// <summary>
        /// Initializes a new instance of <see cref="FormattedTextWriterTraceListener"/> with a 
        /// <see cref="ILogFormatter"/> and a file name.
        /// </summary>
        /// <param name="fileName">The file name to write to.</param>
        /// <param name="formatter">The formatter to format the messages.</param>
        public FormattedTextWriterTraceListener(string fileName, ILogFormatter formatter)
			: this(fileName)
		{
			this.Formatter = formatter;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="FormattedTextWriterTraceListener"/> with a file name.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		public FormattedTextWriterTraceListener(string fileName)
			: base(RootFileNameAndEnsureTargetFolderExists(fileName))
		{
		}

		/// <summary>
		/// Intercepts the tracing request to format the object to trace.
		/// </summary>
		/// <remarks>
		/// Formatting is only performed if the object to trace is a <see cref="LogEntry"/> and the formatter is set.
		/// </remarks>
		/// <param name="eventCache">The context information.</param>
		/// <param name="source">The trace source.</param>
		/// <param name="eventType">The severity.</param>
		/// <param name="id">The event id.</param>
		/// <param name="data">The object to trace.</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
			{
				if (data is LogEntry)
				{
					if (this.Formatter != null)
					{
						base.Write(this.Formatter.Format(data as LogEntry));
					}
					else
					{
						base.TraceData(eventCache, source, eventType, id, data);
					}
				}
				else
				{
					base.TraceData(eventCache, source, eventType, id, data);
				}
			}
		}

        /// <summary>
        /// Gets the <see cref="ILogFormatter"/> used to format the trace messages.
        /// </summary>
        public ILogFormatter Formatter { get; set; }

        private static string RootFileNameAndEnsureTargetFolderExists(string fileName)
		{
			string rootedFileName = fileName;
			if (!Path.IsPathRooted(rootedFileName))
			{
				rootedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootedFileName);
			}

			string directory = Path.GetDirectoryName(rootedFileName);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			return rootedFileName;
		}
	}

}
