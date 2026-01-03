using Infrastructure.Logger.Enterprise;
using System;
using System.Collections.Generic;

namespace Infrastructure.Logger.Formatters
{
	public class FormatterRepository : IFormatterRepository
	{
		readonly Dictionary<string, Type> _formatters = [];

		public FormatterRepository() 
		{
			_formatters[nameof(RawFormatter)] = typeof(RawFormatter); 
		}

		public void RegisterFormatter<T>(string key) where T : class, ILogFormatter
		{
			_formatters[key] = typeof(T);
		}

		public string Serialize(ILogFormatter formatter)
		{
			foreach (var key in _formatters)
			{
				if (key.Value == formatter.GetType())
					return key.Key;
			}
			throw new NotSupportedException($"The LogFormatter {formatter.GetType()} is not registered in the repository");
		}

		public ILogFormatter Deserialize(string key)
		{
			if (_formatters.TryGetValue(key, out var formatter))
			{
				return Activator.CreateInstance(formatter) as ILogFormatter;
			}
			throw new NotSupportedException($"The LogFormatter {key} is not registered in the repository");
		}
	}
}
