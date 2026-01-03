using System.Threading.Tasks;
using Infrastructure.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Threading
{
	[TestClass]
	public class DeferredValueTest
	{
		[TestMethod]
		public void ValueCanBeSet()
        {
            var myVal = new DeferredValue<Poco>();
			Assert.IsFalse(myVal.HasValue);

			myVal.SetValue(new Poco());
            Assert.IsTrue(myVal.HasValue);
		}

        [TestMethod]
        public async Task ValueCanBeAwaited()
        {
            var myVal = new DeferredValue<Poco>();
            Assert.IsFalse(myVal.HasValue);

            bool gotValue = false;

			Task.Run(() =>
            {
                myVal.WaitForValue.Wait();
                Assert.IsTrue(myVal.HasValue);
                gotValue = true;
			});

            myVal.SetValue(new Poco());
			await Task.Delay(100);

			Assert.IsTrue(gotValue);
		}
	}
}
