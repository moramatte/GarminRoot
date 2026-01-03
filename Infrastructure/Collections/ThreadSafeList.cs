using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Infrastructure.Collections
{
	public class ThreadSafeList<T>
	{
		private List<T> _list = [];
		private Semaphore _lock = new(1, 1);

		public class ExclusiveAccessToken : IDisposable
		{
			readonly ThreadSafeList<T> _list;
			bool _disposed;

			~ExclusiveAccessToken() 
			{
				Dispose();
			}

			public int Count
			{
				get
				{
					if (_disposed)
						throw new ObjectDisposedException(nameof(ExclusiveAccessToken));
					return _list._list.Count;
				}
			}

			public T this[int i]
			{
				get
				{
					if (_disposed)
						throw new ObjectDisposedException(nameof(ExclusiveAccessToken));
					return _list._list[i];
				}
				set
				{
					if (_disposed)
						throw new ObjectDisposedException(nameof(ExclusiveAccessToken));
					_list._list[i] = value;
				}
			}

			public ExclusiveAccessToken(ThreadSafeList<T> list)
			{
				_list = list;
				_list._lock.WaitOne();
			}

			public void RemoveAt(int i)
			{
				if (_disposed)
					throw new ObjectDisposedException(nameof(ExclusiveAccessToken));
				_list._list.RemoveAt(i);
			}

			public void Dispose()
			{
				if (_disposed)
					return;
				_disposed = true;
				_list._lock.Release();
			}
		}

		/// <summary>
		/// Locks collection, giving exclusive access until the access-token-object is disposed.
		/// </summary>
		public ExclusiveAccessToken ExclusiveAccess() => new ExclusiveAccessToken(this);

		public int Count
		{
			get
			{
				_lock.WaitOne();
				var count = _list.Count;
				_lock.Release();
				return count;
			}
		}

		public void Add(T item)
		{
			_lock.WaitOne();
			_list.Add(item);
			_lock.Release();
		}

		public void Clear()
		{
			_lock.WaitOne();
			_list.Clear();
			_lock.Release();
		}

		public void AddRange(IEnumerable<T> item)
		{
			_lock.WaitOne();
			_list.AddRange(item);
			_lock.Release();
		}

		public List<T> Copy(Func<T, bool> filter)
		{
			_lock.WaitOne();
			var copy = _list.Where(filter).ToList();
			_lock.Release();
			return copy;
		}

		public List<T> Copy()
		{
			_lock.WaitOne();
			var copy = _list.ToList();
			_lock.Release();
			return copy;
		}

		public List<U> CopyConvert<U>(Expression<Func<T, U>> func)
		{
			_lock.WaitOne();
			var copy = _list.Select(func.Compile()).ToList();
			_lock.Release();
			return copy;
		}

		public bool Remove(T item)
		{
			_lock.WaitOne();
			var removed = _list.Remove(item);
			_lock.Release();
			return removed;
		}

	}
}
