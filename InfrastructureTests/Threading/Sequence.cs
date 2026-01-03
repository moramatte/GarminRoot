using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InfrastructureTests.Threading
{
	public class Sequence<T>()
	{
		List<T> _queue = [];
		int _current;

		public void Add(T val) => _queue.Add(val);

		public async Task<T> GetNext(int timeoutMs = 10000)
		{
			var success = await Async.Condition(HasNewItem, timeoutMs);
			if (!success)
				throw new TimeoutException("Nothing arrived.");

			var value = _queue[_current];
			_current++;
			return value;
		}

		private bool HasNewItem() => _queue.Count > _current;

		public async Task AssertNext(T val, int timeoutMs = 10000)
		{
			try
			{
				var inQueue = await GetNext(timeoutMs);
				Assert.AreEqual(val, inQueue);
			}
			catch (TimeoutException)
			{
				Assert.Fail($"Expected <{val}>. But nothing arrived");
			}
		}

		public async Task AssertNextWithOptional(T val, T optional, int timeoutMs = 10000)
		{
			try
			{
				var inQueue = await GetNext(timeoutMs);
				if (inQueue.Equals(optional))
					inQueue = await GetNext(timeoutMs);
				Assert.AreEqual(val, inQueue);
			}
			catch (TimeoutException)
			{
				Assert.Fail($"Expected <{val}>. But nothing arrived");
			}
		}

		public void SkipToFront()
		{
			_current = _queue.Count;
		}
	}
}
