using Awos7.WindowsService.LogConfig;
using Infrastructure.Collections;
using Infrastructure.Logger.Formatters;
using System.Diagnostics;

namespace Infrastructure.Logger.Enterprise
{
	public partial class RollingFlatFileTraceListener : FlatFileTraceListener, ITraceListener
	{
		/// <summary>
		/// Defines the behavior when the roll file is created.
		/// </summary>
		public enum RollFileExistsBehavior
		{
			/// <summary>
			/// Overwrites the file if it already exists.
			/// </summary>
			Overwrite,
			/// <summary>
			/// Use a secuence number at the end of the generated file if it already exists. If it fails again then increment the secuence until a non existent filename is found.
			/// </summary>
			Increment
		};

		private readonly StreamWriterRollingHelper rollingHelper;

		private readonly RollFileExistsBehavior rollFileExistsBehavior;
		private readonly RollInterval rollInterval;
		private readonly int rollSizeInBytes;
		private readonly string timeStampPattern;
		private readonly int maxArchivedFiles;
		private readonly int minDaysToArchive;

		private bool disposed;

		/// <summary>
		/// Represents the default separator used for headers and footers.
		/// </summary>
		public const string DefaultSeparator = "----------------------------------------";

        public string FileName { get; private init; }
        public int RollSizeKB { get; private init; }
		public string TimeStampPattern => timeStampPattern;
		public int MaxArchivedFiles => maxArchivedFiles;

		/// <summary>
		/// Initializes a new instance of the <see cref="RollingFlatFileTraceListener"/> class.
		/// </summary>
		/// <param name="fileName">The filename where the entries will be logged.</param>
		/// <param name="header">The header to add before logging an entry.</param>
		/// <param name="footer">The footer to add after logging an entry.</param>
		/// <param name="formatter">The formatter.</param>
		/// <param name="rollSizeKB">The maxium file size (KB) before rolling.</param>
		/// <param name="timeStampPattern">The date format that will be appended to the new roll file.</param>
		/// <param name="rollFileExistsBehavior">Expected behavior that will be used when the roll file has to be created.</param>
		/// <param name="rollInterval">The time interval that makes the file rolles.</param>
		/// <param name="maxArchivedFiles">The maximum number of archived files to keep.</param>
		/// <param name="minimumDays">Regardless of maximum number of files, logs will be kept for at least this amount of days</param>
		public RollingFlatFileTraceListener(string fileName,
											ILogFormatter formatter = null,
											int rollSizeKB = 0,
											string timeStampPattern = "yyyy-MM-dd",
											RollFileExistsBehavior rollFileExistsBehavior = RollFileExistsBehavior.Overwrite,
											RollInterval rollInterval = RollInterval.None,
											int maxArchivedFiles = 0, 
											int minimumDays = 45)
			: base(fileName, string.Empty, string.Empty, formatter)
		{
            FileName = fileName;
			RollSizeKB = rollSizeKB;
            this.rollSizeInBytes = rollSizeKB * 1024;
			this.timeStampPattern = timeStampPattern;
			this.rollFileExistsBehavior = rollFileExistsBehavior;
			this.rollInterval = rollInterval;
			this.maxArchivedFiles = maxArchivedFiles;
			this.minDaysToArchive = minimumDays;
			this.rollingHelper = new StreamWriterRollingHelper(this);
		}

		public RollingFlatFileTraceListener(TraceListenerConfig config) : this(config.Get<string>("FileName"), config.GetFormatter(), config.Get<int>("RollSizeKB"), config.Get<string>("TimeStampPattern"), RollFileExistsBehavior.Increment, RollInterval.Midnight, config.Get<int>("MaxArchivedFiles"))
		{
			Filter = config.GetFilter();
			Name = config.Name;
		}

		public TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.RollingFile,
				Values = new EquatableList<ConfigValue>(
				[
					new("FileName", FileName), 
					new("RollSizeKB", RollSizeKB),
					new("TimeStampPattern", TimeStampPattern), 
					new("MaxArchivedFiles", maxArchivedFiles)
				])
			};
		}

		/// <summary>
		/// Gets the <see cref="StreamWriterRollingHelper"/> for the flat file.
		/// </summary>
		/// <value>
		/// The <see cref="StreamWriterRollingHelper"/> for the flat file.
		/// </value>
		public StreamWriterRollingHelper RollingHelper
		{
			get { return rollingHelper; }
		}

		/// <summary>
		/// Writes trace information, a data object and event information to the file, performing a roll if necessary.
		/// </summary>
		/// <param name="eventCache">A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
		/// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
		/// <param name="eventType">One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.</param>
		/// <param name="id">A numeric identifier for the event.</param>
		/// <param name="data">The trace data to emit.</param>
		public override void TraceData(TraceEventCache eventCache,
									   string source,
									   TraceEventType eventType,
									   int id,
									   object data)
		{
			rollingHelper.RollIfNecessary();
			
			base.TraceData(eventCache, source, eventType, id, data);
		}

		public void Write(TraceEventCache eventCache, string source, TraceEventType eventType, int id, LogEntry data)
		{
			TraceData(eventCache, source, eventType, id, data);
		}

		/// <summary>
		/// Releases managed resources.
		/// </summary>
		/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; 
		/// <see langword="false"/> to release only unmanaged resources. </param>
		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					base.Dispose(disposing);
				}

				this.disposed = true;
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RollingFlatFileTraceListener"/> class.
		/// </summary>
		~RollingFlatFileTraceListener()
		{
			this.Dispose(false);
		}

	}
}
