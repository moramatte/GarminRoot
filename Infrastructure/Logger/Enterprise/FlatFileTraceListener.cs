using System.Diagnostics;

namespace Infrastructure.Logger.Enterprise
{
	public class FlatFileTraceListener : FormattedTextWriterTraceListener
	{
		private string header = string.Empty;
		private string footer = string.Empty;

		/// <summary>
		/// Initializes a new instance of <see cref="FlatFileTraceListener"/> with a file name, a header, a footer 
		/// </summary>
		/// <param name="fileName">The file stream.</param>
		/// <param name="header">The header.</param>
		/// <param name="footer">The footer.</param>
		public FlatFileTraceListener(string fileName, string header = null, string footer = null, ILogFormatter logFormatter = null)
			: base(fileName, logFormatter)
		{
			this.header = header ?? string.Empty;
			this.footer = footer ?? string.Empty;
		}

		/// <summary>
		/// Delivers the trace data to the underlying file.
		/// </summary>
		/// <param name="eventCache">The context information provided by <see cref="System.Diagnostics"/>.</param>
		/// <param name="source">The name of the trace source that delivered the trace data.</param>
		/// <param name="eventType">The type of event.</param>
		/// <param name="id">The id of the event.</param>
		/// <param name="data">The data to trace.</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
			{
				if (header.Length > 0)
					WriteLine(header);

				if (data is LogEntry)
				{
					if (this.Formatter != null)
					{
						base.WriteLine(this.Formatter.Format(data as LogEntry));
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

				if (footer.Length > 0)
					WriteLine(footer);
			}
		}

	}
}
