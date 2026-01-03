using System;

namespace Infrastructure.Utilities;

public class TimedCache<T>
{
	private T _value;
	private DateTime _startTime;
	private TimeSpan _lifeSpan;

	public bool HasValue => _startTime.Add(_lifeSpan) > DateTime.UtcNow;

	public TimedCache(TimeSpan timeSpan)
	{
		_startTime = DateTime.MinValue;
		_lifeSpan = timeSpan;
	}

	public T Value => _value;

	public void SetValue(T value)
	{
		_value = value;
		_startTime = DateTime.UtcNow;
	}
}
