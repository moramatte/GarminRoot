using FakeItEasy;
using Infrastructure;
using Infrastructure.EventAggregator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Process = System.Diagnostics.Process;

namespace InfrastructureTests
{
    [TestClass]
    public class IKillableTest
    {
        class C : IKillable { }

        [TestInitialize]
        public void SetUp()
        {
            InMyTest.OverrideSingleton<IProcessMonitor>(A.Fake<IProcessMonitor>());
        }

        [TestMethod]
        public void IKillableCanBeKilled()
        {
            var processMonitor = ServiceLocator.Resolve<IProcessMonitor>();
            new C().Kill();
            A.CallTo(() => processMonitor.Kill()).MustHaveHappenedOnceExactly();
        }
    }
}