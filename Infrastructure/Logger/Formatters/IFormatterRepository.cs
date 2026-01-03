using Infrastructure.Logger.Enterprise;
using System;
using System.Collections.Generic;

namespace Infrastructure.Logger.Formatters
{
	public interface IFormatterRepository
	{
		string Serialize(ILogFormatter formatter);
		ILogFormatter Deserialize(string key);
	}
}
