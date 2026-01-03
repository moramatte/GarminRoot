using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace InfrastructureTests
{
	public static class TestHelper
	{
		public static void AssertEqualCollectionContent(IEnumerable expected, IEnumerable actual)
		{
			int expectedLength = expected.Cast<object>().Count();
			int actualLength = actual.Cast<object>().Count();

			Assert.AreEqual(expectedLength, actualLength, "Length");

			for (int index = 0; index < expectedLength; index++)
			{
				object expectedElement = expected.Cast<object>().ElementAt(index);
				object actualElement = actual.Cast<object>().ElementAt(index);

				AssertPropertiesAreEqual(expectedElement, actualElement);
			}
		}

		public static void AssertPropertiesAreEqual(object expectedObject, object actualObject, params string[] propertyNames)
		{
			if (propertyNames.Length == 0 && expectedObject.GetType() != actualObject.GetType())
				throw new InvalidOperationException("Compared objects must be of same type if no property names are provided.");

			var expectedType = expectedObject.GetType();
			var actualType = actualObject.GetType();

			var propertyInfosNames = propertyNames.Length == 0 ? expectedType.GetProperties().Select(p => p.Name).ToArray()
														  : propertyNames;

			foreach (var propertyName in propertyInfosNames)
			{
				try
				{
					var expectedValue = expectedType.GetProperty(propertyName).GetValue(expectedObject, null);
					var actualValue = actualType.GetProperty(propertyName).GetValue(actualObject, null);

					Assert.AreEqual(expectedValue, actualValue, "Property: " + propertyName);
				}
				catch (Exception)
				{
					Console.WriteLine("Property: " + propertyName);
					throw;
				}
			}
		}

		public static void AutoSetStringProperties(this object thisObject)
		{
			var stringProperties = thisObject.GetType()
											 .GetProperties()
											 .Where(p => p.PropertyType == typeof(string));

			foreach (var stringProperty in stringProperties)
			{
				stringProperty.SetValue(thisObject, stringProperty.Name, null);

			}
		}

		public static byte[] HexToBytes(this string thisString)
		{
			var tokens = thisString.Split(',');

			var bytes = tokens.Select(t => byte.Parse(t, NumberStyles.HexNumber)).ToArray();

			return bytes;
		}

	}

	public class ActionVerifier<T>
	{
		private Queue<object> _queue = new Queue<object>();
		private Action<T> _logAction;

		public bool IsCompleted { get { return _queue.Count < 1; } }
		public bool IsFailed { get; set; }
		public AutoResetEvent Waiter { get; private set; }

		public ActionVerifier()
		{
			Waiter = new AutoResetEvent(false);
		}

		public void SetLogAction(Action<T> action)
		{
			_logAction = action;
		}

		public void Verify(T obj)
		{
			try
			{
				_logAction?.Invoke(obj);
                var actionOrFunc = _queue.Dequeue();
                if (actionOrFunc is Action<T>)
                {
                    (actionOrFunc as Action<T>).Invoke(obj);
                }
				else if (!(actionOrFunc as Func<T, bool>).Invoke(obj))
                {
                    IsFailed = true;
                }
				Waiter.Set();
			}
			catch (Exception)
			{
				IsFailed = true;
				Waiter.Set();
				throw;
			}
		}

		//TODO: should use the func method instead since actions containing asserts can throw but test is still okay
		public void Add(Action<T> action)
		{
			_queue.Enqueue(action);
		}

        public void Add(Func<T, bool> func)
        {
			_queue.Enqueue(func);
        }

		public void AssertWait(int? time = null)
		{
			if (!Wait(time))
				Assert.Fail();
		}

		public bool Wait(int? time = null)
		{
			if (IsFailed)
				return false;

			if (IsCompleted)
				return true;

			var waitOne = Waiter.WaitOne(time == null ? 30000 : time.Value);
			if (!waitOne)
			{
				Log.Debug(GetType(), "Waiter time is up. Queue is not empty");
			}

			if (IsFailed)
				return false;
			return waitOne;
		}
	}

	public class StateAction
	{
		private List<WhenAction> _whenActions = [];
		private List<AfterAction> _afterActions = [];
		private List<AfterActionTask> _afterActionTasks = [];

		private class AfterActionTask(TaskCompletionSource source, object state, int times = 1)
		{
			public TaskCompletionSource Source { get; init; } = source;
			public object State { get; init; } = state;
			public int Times { get; set; } = times;
		}

		public class AfterAction
		{
			public AfterAction(object state, int times)
			{
				State = state;
				Times = times;
			}

			internal object State { get; }
			internal int Times { get; set; }
			internal WhenAction WhenAction { get; private set; }
			internal Action DoAction { get; private set; }
			public void ForOnce(object customState, Action action)
			{
				WhenAction = GetForOnce(customState, action);
			}

			public void ForTimes(object customState, int times, Action action)
			{
				WhenAction = GetForTimes(customState, times, action);
			}

            public void Do(Action action)
            {
                DoAction = action;
            }
		}

		public class WhenAction
		{
			internal object State { get; }
			internal int Times { get; set; }
			internal Action Action { get; }

			public WhenAction(object state, int times, Action action)
			{
				State = state;
				Times = times;
				Action = action;
			}
		}

		public AfterAction AfterOnce(object state)
		{
			var afterAction = new AfterAction(state, 1);
			_afterActions.Add(afterAction);
			return afterAction;
		}

		public Task AfterOnceAsync(object state, int timeoutMs = 3000)
		{
			var source = new TaskCompletionSource();
			var cts = Cancel.After(timeoutMs);
			cts.Register(() => source.TrySetCanceled(cts));
			_afterActionTasks.Add(new AfterActionTask(source, state));
			return source.Task;
		}

		public AfterAction AfterTimes(object state, int times)
		{
			var afterAction = new AfterAction(state, times);
			_afterActions.Add(afterAction);
			return afterAction;
		}

		public void ForOnce(object customState, Action action)
		{
			_whenActions.Add(GetForOnce(customState, action));
		}

		private static WhenAction GetForOnce(object customState, Action action)
		{
			return new(customState, 1, action);
		}

		public void ForTimes(object customState, int times, Action action)
		{
			_whenActions.Add(GetForTimes(customState, times, action));
		}

		private static WhenAction GetForTimes(object customState, int times, Action action)
		{
			return new(customState, times, action);
		}

		public void CheckState(object state)
		{
			foreach (var whenAction in _whenActions)
			{
				if (state.Equals(whenAction.State) && (whenAction.Times > 0 || whenAction.Times < 0))
				{
					whenAction.Times--;
					whenAction.Action.Invoke();
				}
			}

			foreach (var afterAction in _afterActions)
			{
				if (state.Equals(afterAction.State) && afterAction.Times > 0)
				{
					afterAction.Times--;
                    if (afterAction.Times == 0)
                    {
						if(afterAction.WhenAction != null)
                            _whenActions.Add(afterAction.WhenAction);
						afterAction.DoAction?.Invoke();
                    }
				}
			}

			for (int i = 0; i < _afterActionTasks.Count; i++)
			{
				AfterActionTask afterAction = _afterActionTasks[i];
				if (state.Equals(afterAction.State) && afterAction.Times > 0)
				{
					afterAction.Times--;
					if (afterAction.Times == 0)
					{
						afterAction.Source.TrySetResult();
					}
				}
			}
		}
	}

	public class QueueVerifier
	{
		private Queue<object> _states;
		private Func<object> _func;
		private Queue<Tuple<object, Action>> _customVerifiers;

		private object[] _orgStates;
		public AutoResetEvent Waiter { get; private set; }
		public bool IsCompleted { get { return _states.Count < 1 && (_customVerifiers == null || _customVerifiers.Count < 1); } }
		public bool IsFailed { get; set; }

		public QueueVerifier()
		{
			Waiter = new AutoResetEvent(false);
		}

		public void Add(Func<object> func, params object[] states)
		{
			_func = func;
			_states = new Queue<object>(states);

			_orgStates = states.ToArray();
		}

		public void VerifyOn(object customState, Action action)
		{
			if (_customVerifiers == null)
				_customVerifiers = new Queue<Tuple<object, Action>>();
			_customVerifiers.Enqueue(new Tuple<object, Action>(customState, action));
		}

		public void AssertWait(int timeMs = 30000)
		{
			if (!Wait(timeMs))
				Assert.Fail(_failedMsg);
		}

		private string _failedMsg = string.Empty;
		public bool Wait(int timeMs = 30000)
		{
			if (IsFailed)
				return false;

			if (IsCompleted)
				return true;

			var waitOne = Waiter.WaitOne(timeMs);
			if (!waitOne)
			{
				var rest = new StringBuilder("Waiter time is up. Never entered: ");
				if (_states != null && _states.Count > 0)
				{
					rest.Append($"{_states.Peek()}");
				}
				if (_customVerifiers != null && _customVerifiers.Count > 0)
				{
					if (rest.Length > 0)
						rest.Append(' ');
					rest.Append($"{_customVerifiers.Peek().Item1}");
				}

				_failedMsg = rest.ToString();
				Log.Debug(GetType(), _failedMsg);
			}

			if (IsFailed)
				return false;
			return waitOne;
		}

		public void VerifyInitialState()
		{
			Verify(null);
		}

		public void VerifyOnMatch(object o)
		{
			var state = _func.Invoke();
			if (o != null)
				state = o;

			if (_states.Count > 0)
			{
				var expected = _states.Peek();
				if (expected.Equals(state))
				{
					_states.Dequeue();

					if (IsCompleted)
						Waiter.Set();
				}
			}
		}

		public void Verify(object o)
		{
			var state = _func.Invoke();
			if (o != null)
				state = o;

			if (_states.Count > 0)
			{
				var expected = _states.Dequeue();
				Log.Debug(GetType(), $"Asserting {expected} == {state}");
				if (!expected.Equals(state))
				{
					Log.Debug(GetType(), $"Asserting {expected} == {state} failed");
					_failedMsg = $"Expected {expected} was {state} at state {_orgStates.Length - _states.Count}";
					IsFailed = true;
				}
			}

			if (_customVerifiers != null && _customVerifiers.Any() && (int)_customVerifiers.Peek().Item1 == (int)state)
			{
				var tuple = _customVerifiers.Dequeue();
				Log.Debug(GetType(), $"Custom verifier for {tuple.Item1}");
				try
				{
					tuple.Item2.Invoke();
				}
				catch (Exception exc)
				{
					Log.Debug(GetType(), $"Failed custom verifier for {tuple.Item1}, error = {exc}");
					IsFailed = true;
					_failedMsg = $"Custom verifier for {tuple.Item1}, error = {exc}";
					Waiter.Set();
					throw;
				}
			}

			if (_states.Count < 1 && (_customVerifiers == null || _customVerifiers.Count < 1))
				Waiter.Set();
		}

		public void Fail(string message = null)
		{
			Assert.Fail("States sequence (reached " + string.Join(", ", _orgStates.Take(_orgStates.Length - _states.Count)) + " ) stopped at: " + string.Join(", ", _states) + " " + message);
		}
	}
}
