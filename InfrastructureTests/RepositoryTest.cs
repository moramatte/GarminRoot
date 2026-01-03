using System.Linq;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class RepositoryTest
    {
        [TestMethod]
        public void SimpleRefposítoryWorks()
        {
            var repo = new SimpleRepository();

            repo.Add(new ComChannel { Name = "Channel1" });
            repo.Add(new ComChannel { Name = "Channel2" });

            Assert.AreEqual(2, repo.GetAll().Count());

            repo.Remove("Channel1");
            Assert.AreEqual(1, repo.GetAll().Count());
            Assert.AreEqual("Channel2", repo.Get("Channel2").Name);
        }
    }

    internal class ComChannel : INamed
    {
        public string Name { get; set; }
    }

    internal class SimpleRepository : Repository<ComChannel> { }
}
