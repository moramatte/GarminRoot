using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InfrastructureTests.Logging;

namespace InfrastructureTests
{
    /// <summary>
    /// Convenience class that enables more fluent syntax of making test assertions.
    /// </summary>
    public static class Assertions
    {
        /// <summary>
        /// Asserts that actualValue is less than the reference
        /// </summary>
        /// <param name="reference">Threshold</param>
        /// <param name="actualValue">Actual value</param>
        public static void Less(double reference, double actualValue)
        {
            Assert.IsTrue(actualValue < reference, $"{actualValue} is not less than {reference}");
        }

        /// <summary>
        /// Asserts that actualValue is greater than the reference
        /// </summary>
        /// <param name="reference">Threshold</param>
        /// <param name="actualValue">Actual value</param>
        public static void Greater(double reference, double actualValue)
        {
            Assert.IsTrue(actualValue > reference, $"{actualValue} is not greater than {reference}");
        }

        /// <summary>
        /// Calls Assert: the collection should not be empty.
        /// </summary>
        /// <param name="myCollection">Any IEnumerable/param>
        public static void IsPopulated(this IEnumerable myCollection)
        {
            var enumerator = myCollection.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext(), "The collection is empty");
        }

        /// <summary>
        /// Calls Assert: the collection should be empty.
        /// </summary>
        /// <param name="myCollection">Any collection</param>
        public static void IsEmpty(this IList myCollection)
        {
            Assert.IsTrue(myCollection.Count == 0);
        }

        /// <summary>
        /// Calls Assert: the collection should be empty.
        /// </summary>
        /// <param name="myCollection">Any collection</param>
        public static void IsEmpty(this IEnumerable myCollection)
        {
            var enumerator = myCollection.GetEnumerator();
            Assert.IsFalse(enumerator.MoveNext());
        }

        public static void AssertSequenceEqual<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            Assert.IsTrue(a.SequenceEqual(b), $"Sequence [{a.ToCommaSeparatedString()}] was not equal to sequence [{b.ToCommaSeparatedString()}]");
        }

        public static void AssertContains(this string myString, string matchString, StringComparison comparison = StringComparison.CurrentCulture)
        {
            Assert.IsTrue(myString.Contains(matchString, comparison), $"{matchString} not found in {myString}");
        }

		public static void AssertLacks(this string myString, string matchString, StringComparison comparison = StringComparison.CurrentCulture)
		{
			Assert.IsFalse(myString.Contains(matchString, comparison), $"{matchString} found in {myString}");
		}

        public static void AssertContains<T>(this IEnumerable<T> myCollection, Func<T, bool> predicate)
        {
            var matches = myCollection.Where(predicate);
            Assert.IsTrue(matches.Any(), "AssertContains: Collection did not contain predicate");
        }

		public static void AssertLacks<T>(this IEnumerable<T> myCollection, Func<T, bool> predicate)
		{
			var matches = myCollection.Where(predicate);
			Assert.AreEqual(0, matches.Count(), "AssertLacks: Collection did contain predicate");
		}

		public static async Task WaitForAssertion(Action assertion, int millisecondsToWait = 2000)
        {
            var start = DateTime.Now;
            Exception lastException = null;

            while (true)
            {
                try
                {
                    assertion();
                    return;
                }
                catch (Exception e)
                {
                    lastException = e;
                }

                var elapsed = DateTime.Now - start;
                if (elapsed.TotalMilliseconds >= millisecondsToWait)
                {
                    throw lastException;
                }

                await Task.Delay(20);
            }
        }

        public static void AssertLogContains(string partOfMessage)
        {
            InMemoryLogger logger = ServiceLocator.Resolve<ILogger>() as InMemoryLogger;
            Assert.IsTrue(logger.Contains(partOfMessage));
        }

        public static void AssertLogLacks(string partOfMessage)
        {
            InMemoryLogger logger = ServiceLocator.Resolve<ILogger>() as InMemoryLogger;
            Assert.IsFalse(logger.Contains(partOfMessage));
        }

		public static void AssertEventFired<T>()
        {
            var selectedEvents = Events.Of<T>();
            selectedEvents.IsPopulated();
        }

        public static void AssertEventFired<T>(int exactNumber)
        {
            var selectedEvents = Events.Of<T>();
            if (exactNumber != selectedEvents.Count())
            {
                throw new AssertFailedException(
                    $"Expected {exactNumber} of {typeof(T).Name} events, but we got {selectedEvents.Count()}");
            }
        }

        public static void AssertEventFired<T>(Func<T, bool> predicate)
        {
            var matchingEvents = Events.Of<T>().Where(ev => predicate(ev));
            matchingEvents.IsPopulated();
        }

        public static void AssertEventFired<T>(Func<T, bool> predicate, int exactNumber)
        {
            var matchingEvents = Events.Of<T>().Where(ev => predicate(ev));
            if (exactNumber != matchingEvents.Count())
            {
                throw new AssertFailedException(
                    $"Expected {exactNumber} of {typeof(T).Name} events, but we got {matchingEvents.Count()}");
            }
        }

        public static void AssertEventNotFired<T>()
        {
            Assert.AreEqual(0, Events.Of<T>().Count(), $"Expected no events of {typeof(T)} but got {Events.Of<T>().Count()}");
        }

        /// <summary>
        /// Asserts that an action throws an exception
        /// </summary>
        /// <param name="code">Action to assert</param>
        /// <param name="partOfMessage">Required part of the exception message</param>
        /// <remarks>
        /// Testcase attribute ExpectedException can be used (instead of this method) to assert thrown exception type.
        /// </remarks>
        public static void AssertException<T>(Action code, string partOfMessage = null) where T : Exception
        {
            T caughtException = null;

            try
            {
                code();
            }
            catch (T ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException, "Expected exception but none was thrown");

            if (partOfMessage.HasContent())
            {
                caughtException.AssertExceptionMessage(partOfMessage);
            }
        }

        public static void AssertException(Action code, string partOfMessage = null)
        {
            AssertException<Exception>(code, partOfMessage);
        }

        private static void AssertExceptionMessage(this Exception e, string partOfMessage)
        {
            while (e != null)
            {
                if (e.Message.Contains(partOfMessage))
                {
                    return; //  Ok
                }

                e = e.InnerException;
            }

            Assert.Fail($"Did not find message '{partOfMessage}' in the Exception stack");
        }

        public static void EqualsDeep(object objA, object objB)
        {
            var result = Compare.Deep(objA, objB);
            Assert.AreEqual(0, result.Count(), $"There are {result.Count()} differences");
        }

        public static void SerializeDeserialize<T>(T obj)
        {
            var json = obj.ToXml();
            var deserialized = json.FromXml<T>();
            EqualsDeep(obj, deserialized);
        }
    }
}
