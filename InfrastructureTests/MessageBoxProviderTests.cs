using FakeItEasy;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class MessageBoxProviderTests
    {
        [TestMethod]
        public void AskIsPossibleWorks()
        {
            ServiceLocator.Reset();

            Assert.IsFalse(Ask.IsPossible());
            ServiceLocator.RegisterSingleton(A.Fake<IMessageBoxProvider>());
            Assert.IsTrue(Ask.IsPossible());
        }
    }
}
