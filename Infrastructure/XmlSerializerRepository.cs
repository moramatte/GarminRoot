using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Infrastructure
{
	public class XmlSerializerRepository
	{
		private readonly Dictionary<Type, XmlSerializer> _xmlSerializers = [];
		private readonly XmlSerializerNamespaces _xmlnsEmpty;
		private readonly object _getSerializerLock = new();

		public XmlSerializerRepository() 
		{
			_xmlnsEmpty = new XmlSerializerNamespaces();
			_xmlnsEmpty.Add("", "");
		}

		public T LoadXml<T>(TextReader reader)
		{
			var serializer = GetXmlSerializer(typeof(T));
			T userSettings = (T)serializer.Deserialize(reader);
			return userSettings;
		}

		public void SaveXml<T>(TextWriter streamWriter, T userSettings)
		{
			var serializer = GetXmlSerializer(typeof(T));
			serializer.Serialize(streamWriter, userSettings, GetXmlSerializerNamespaces());
		}

		public string Pretty(string xml)
		{
			var document = new XmlDocument();
			document.Load(new StringReader(xml));

			var builder = new StringBuilder();
			using var stringWriter = new StringWriter(builder);
			using var writer = new XmlTextWriter(stringWriter);
			writer.Formatting = Formatting.Indented;
			document.Save(writer);

			return builder.ToString();
		}

		public XmlSerializerNamespaces GetXmlSerializerNamespaces()
		{
			return _xmlnsEmpty;
		}

		public XmlSerializer GetXmlSerializer(Type t)
		{
			lock (_getSerializerLock)
			{
				if (!_xmlSerializers.ContainsKey(t))
				{
					_xmlSerializers.Add(t, new XmlSerializer(t));
				}
			}

			return _xmlSerializers[t];
		}

		public string Serialize(object o)
		{
			var sb = new StringBuilder();
			using var writer = new StringWriter(sb);

			var xmlSerializer = GetXmlSerializer(o.GetType());
			xmlSerializer.Serialize(writer, o);
			string content = sb.ToString();
			return content;
		}

		public string Serialize<T>(object o)
		{
			var sb = new StringBuilder();
			using var writer = new StringWriter(sb);

			var xmlSerializer = GetXmlSerializer(typeof(T));
			xmlSerializer.Serialize(writer, o);
			string content = sb.ToString();
			return content;
		}

		public T Deserialize<T>(string content)
		{
			var xmlSerializer = GetXmlSerializer(typeof(T));
			if (content != null)
				content = content.Trim();
			using var reader = new StringReader(content);
			T result = (T)xmlSerializer.Deserialize(reader);
			return result;
		}

		public object Deserialize(string content, Type t)
		{
			var xmlSerializer = GetXmlSerializer(t);
			if (content != null)
				content = content.Trim();
			using var reader = new StringReader(content);
			return xmlSerializer.Deserialize(reader);
		}

		public T Copy<T>(T obj)
		{
			T result;
			using var stream = new MemoryStream();
			var xmlSerializer = GetXmlSerializer(typeof(T));
			xmlSerializer.Serialize(stream, obj);
			stream.Position = 0;
			result = (T)xmlSerializer.Deserialize(stream);
			return result;
		}
	}
}
