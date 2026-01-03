using Infrastructure.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace InfrastructureTests
{
    [TestClass]
    public class ThreadSafeListTest
	{
        ThreadSafeList<int> _list;

		public TestContext TestContext { get; set; }

		[TestInitialize]
        public void Setup()
        {
			_list = new ThreadSafeList<int>();
        }

        [TestMethod]
        public void List_can_add()
        {
            _list.Add(1);
            _list.Add(2);
            Assert.AreEqual(2, _list.Count);
        }

		[TestMethod]
		public void List_can_add_range()
		{
			_list.AddRange([1, 2]);
			Assert.AreEqual(2, _list.Count);
		}

		[TestMethod]
		public void List_can_remove()
		{
            _list.Add(1);
            Assert.IsTrue(_list.Remove(1));
			Assert.AreEqual(0, _list.Count);
		}

		[TestMethod]
		public async Task List_can_add_concurrently()
		{
			await Parallel.ForAsync(0, 10, async (i, e) =>
			{
				_list.Add(1);
				await Task.Yield();
				_list.Add(2);
			});
			Assert.AreEqual(20, _list.Count);
		}

		[TestMethod]
		public async Task List_can_add_range_concurrently()
		{
			await Parallel.ForAsync(0, 10, async (i, e) =>
			{
				_list.AddRange([1, 2]);
				await Task.Yield();
				_list.AddRange([1, 2]);
			});
			Assert.AreEqual(40, _list.Count);
		}

		[TestMethod]
		public async Task List_can_remove_concurrently()
		{
			await Parallel.ForAsync(0, 10, async (i, e) =>
			{
				_list.Add(1);
				await Task.Yield();
				_list.Remove(1);
			});
			Assert.AreEqual(0, _list.Count);
		}

		[TestMethod]
		public void List_can_be_copied()
		{
			_list.Add(1);
			_list.Add(1);
			var copy = _list.Copy();
			using var list = _list.ExclusiveAccess();
			for (int i = 0; i < list.Count; i++)
			{
				Assert.AreEqual(list[i], copy[i]);
			}
		}

		[TestMethod]
		public async Task List_allows_exclusive_access()
		{
			_list.Add(1);
			_list.Add(1);
			_list.Add(1);

			await Parallel.ForAsync(0, 10, async (i, e) =>
			{
				using var list = _list.ExclusiveAccess();
				for (var j = 0; j < list.Count; j++)
				{
					await Task.Yield();
					list[j] = i;
				}
			});

			TestContext.WriteLine(string.Join(' ', _list));
			Assert.AreEqual(3, _list.Count);
			var copy = _list.Copy();
			var first = copy.First();
			Assert.IsTrue(copy.All(x => x == first));
		}

	}
}
