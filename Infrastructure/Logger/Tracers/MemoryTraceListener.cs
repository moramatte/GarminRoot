using Infrastructure.Logger.Enterprise;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Logger.Tracers
{
	[Obsolete($"Old class that broke single responsibility principle. Replaced by {nameof(LogObjectTraceListener)} and {nameof(InMemoryTraceListener)}")]
	public class MemoryTraceListener
	{

	}
}
