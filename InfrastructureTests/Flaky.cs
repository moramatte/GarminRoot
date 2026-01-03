using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
#pragma warning disable MSTESTEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
	public class FlakyAttribute : RetryBaseAttribute
	{
		public int MaxRetryAttempts { get; } = 10;

		public string Reason { get; set; } = "Flaky test - retrying";

		public int MillisecondsDelayBetweenRetries { get; set; } = 4000;

		/// <summary>
		/// Retries the test method <see cref="MaxRetryAttempts"/> times in case of failure.
		/// Note that a first run of the method was already executed and failed before this method is called.
		/// </summary>
		/// <param name="retryContext">An object to encapsulate the state needed for retry execution.</param>
		/// <returns>
		/// Returns a <see cref="RetryResult"/> object that contains the results of all attempts. Only
		/// the last added element is used to determine the test outcome.
		/// The other results are currently not used, but may be used in the future for tooling to show the
		/// state of the failed attempts.
		/// </returns>
		protected override async Task<RetryResult> ExecuteAsync(RetryContext retryContext)
		{
			var result = new RetryResult();
			int currentDelay = MillisecondsDelayBetweenRetries;
			for (int i = 0; i < MaxRetryAttempts; i++)
			{
				// The caller already executed the test once. So we need to do the delay here.
				await Task.Delay(currentDelay);

				var testResults = await retryContext.ExecuteTaskGetter();
				result.AddResult(testResults);
				if (testResults.Any(x => x.Outcome == UnitTestOutcome.Passed))
				{
					break;
				}
			}

			return result;
		}
	}
#pragma warning restore MSTESTEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
