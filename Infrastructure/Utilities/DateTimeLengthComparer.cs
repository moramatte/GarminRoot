using System;
using Unity.Injection;

namespace Infrastructure.Utilities
{
	public ref struct DateTimeLengthComparer(DateTime first, double amount)
	{
		DateTimeComparisonTypes _comparisonType = DateTimeComparisonTypes.NotSet;
		LengthTypes _lengthType = LengthTypes.NotSet;
		readonly DateTime _first = first;
		double _amount = amount;
		DateTime _second;
		bool _inverse = false;

		private enum DateTimeComparisonTypes
		{
			NotSet, OlderThan
		}

		private enum LengthTypes
		{
			NotSet, Hours, Minutes, Seconds
		}

		public DateTimeLengthComparer Not(double amount = double.NaN)
		{
			_amount = amount;
			_inverse = true;
			return this;
		}

		public DateTimeLengthComparer OlderThan(DateTime dateTime)
		{
			_comparisonType = DateTimeComparisonTypes.OlderThan;
			_second = dateTime;
			return this;
		}

		public DateTimeLengthComparer Seconds()
		{
			if (_amount == double.NaN)
				throw new NotSupportedException();
			_lengthType = LengthTypes.Seconds;
			return this; 
		}

		public DateTimeLengthComparer Minutes()
		{
			if (_amount == double.NaN)
				throw new NotSupportedException();
			_lengthType = LengthTypes.Minutes;
			return this;
		}

		public DateTimeLengthComparer Hours()
		{
			if (_amount == double.NaN)
				throw new NotSupportedException();
			_lengthType = LengthTypes.Hours;
			return this;
		}

		private readonly bool EvaluateOlderThan() => _lengthType switch
		{
			LengthTypes.Hours => Math.Abs((_first - _second).TotalHours) > _amount,
			LengthTypes.Minutes => Math.Abs((_first - _second).TotalMinutes) > _amount,
			LengthTypes.Seconds => Math.Abs((_first - _second).TotalSeconds) > _amount,
			LengthTypes.NotSet => _first < _second,
			_ => throw new NotImplementedException(),
		};

		private readonly bool Evaluate()
		{
			var result = _comparisonType switch
			{
				DateTimeComparisonTypes.OlderThan => EvaluateOlderThan(),
				DateTimeComparisonTypes.NotSet => throw new NotSupportedException("DateTime comparison without a comparison type"),
				_ => throw new NotImplementedException(),
			};
			return _inverse ? !result : result;
		}

		public static implicit operator bool(DateTimeLengthComparer dateTimeLengthComparer)
		{
			return dateTimeLengthComparer.Evaluate();
		}
	}
}
