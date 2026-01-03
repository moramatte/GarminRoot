using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Infrastructure.Extensions;

public static class TypeExtensions
{
	public static T GetClosestAttribute<T>(this Type type) where T : Attribute
	{
		var inheritance = new List<Type>();
		while (type != typeof(object))
		{
			inheritance.Add(type);
			type = type.BaseType;
		}

		foreach (var foundType in inheritance)
		{
			var attr = foundType.GetCustomAttribute<T>();
			if (attr != null)
				return attr;
		}
		return null;
	}

	public static string GetPrettyFullTypeName(this Type type) 
	{
		var typeString = new StringBuilder(type.ToString());
		var genericTypeCharIndex = -1;
		for (int i = 0; i < typeString.Length; i++)
		{
			if (typeString[i] == '`')
			{
				genericTypeCharIndex = i;
			}
			if (typeString[i] == ']' && (i == 0 || typeString[i - 1] != '['))
			{
				typeString[i] = '>';
			}
		}
		if (genericTypeCharIndex >= 0)
		{
			typeString.Remove(genericTypeCharIndex, 3);
			typeString.Insert(genericTypeCharIndex, '<');
		}
		return typeString.ToString();
	}

	public static string GetPrettyTypeName(this Type type)
	{
		var typeString = type.Name;
		var index = typeString.IndexOf('`');
		if (index >= 0)
		{
			typeString = typeString[..index];
		}
		return typeString;
	}
}
