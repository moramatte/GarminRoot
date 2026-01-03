using Infrastructure.Threading;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Threading
{
    public interface IDeferredValue<T>
    {
        public Task<T> WaitForValue { get; }

        public T Value { get; }
        public bool HasValue { get; }
    }

    public class DeferredValue<T> : IDeferredValue<T>
    {
        TaskCompletionSource<T> _taskSource = new();
        bool _hasValue = false;
        T _value;

        public Task<T> WaitForValue => _hasValue ? Task.FromResult(_value) : _taskSource.Task;
        public T Value => _hasValue ? _value : throw new NullReferenceException("Value is not set");
        public bool HasValue => _hasValue;

        public void ResetValue()
        {
            _hasValue = false;
        }

        public void SetValue(T value)
        {
            _value = value;
            _hasValue = true;
			_taskSource.SetResult(value);
            _taskSource = new TaskCompletionSource<T>();
        }
    }
}

public static class DeferredValueExtensions
{
    public static bool Exists<T>(this DeferredValue<T> deferredValue)
    {
        if (deferredValue == null)
        {
            return false;
        }

        return deferredValue.HasValue;
    }
}
