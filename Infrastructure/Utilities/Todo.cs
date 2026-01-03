using System.Diagnostics;

namespace Infrastructure.Utilities
{
	public static class Todo
	{
		[Conditional("ConditionThatIsNeverTrue")]
		public static void MissingInAspNetCore(string extraReason = null)
		{

		}
	}
}
