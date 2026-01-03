using System;

namespace Infrastructure.Utilities
{
	public static class Clone
	{
		public static object Object(object o)
		{
			object clone = null;
			if (o != null)
			{
				var type = o.GetType();
				clone = Activator.CreateInstance(type);

				var propertyInfos = type.GetProperties();
				foreach (var propertyInfo in propertyInfos)
				{
					var propertyType = propertyInfo.PropertyType;
					if ((propertyType == typeof(string) || propertyType.IsValueType) && propertyInfo.GetSetMethod() != null && propertyInfo.GetGetMethod() != null)
					{
						try
						{
							var value = propertyInfo.GetValue(o, null);
							propertyInfo.SetValue(clone, value, null);
						}
						catch (Exception)
						{
							//in case of indexed property
						}
					}
				}
			}
			return clone;
		}
	}
}
