using System;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.EventAggregator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class AggregatedEventsTest
    {
        [TestMethod]
        public async Task AsyncEventHandlersWork()
        {
            var subject = new AsyncClass();

            Fire.Event<MyEvent>();

            await Assertions.WaitForAssertion(() => Assert.AreEqual(1, subject.Calls));
        }

        [TestMethod]
        public void DerivedEventsAreHandled() // Derived event types is an anti-pattern.
        {
            var subject = new Poco();

            Fire.Event<MyDerivedEvent>();
            Assert.AreEqual(1, subject.Calls);
        }

        [TestMethod]
        public async Task UnsubscribingWorks()
        {
            var subject = new Poco();

            Fire.Event<MyEvent>();
            Assert.AreEqual(1, subject.Calls);

            subject.UnsubscribeFromEvents();

            Fire.Event<MyEvent>();
            Assert.AreEqual(1, subject.Calls);
        }
    }

    internal record MyEvent;
    internal record MyDerivedEvent : MyEvent;

    internal class AsyncClass
    {
        public int Calls { get; private set; }

        internal AsyncClass()
        {
            this.SubscribeToEvents();
        }

        [EventReceiver]
		internal async Task AsyncMethod(MyEvent _)
        {
            Calls++;
        }
    }

    internal class Poco
    {
        public int Calls { get; private set; }
        public Poco()
        {
            this.SubscribeToEvents();
        }

        [EventReceiver]
		internal void Method(MyEvent _)
        {
            Calls++;
        }
    }
}
