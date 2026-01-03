using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Infrastructure.Utilities
{
    public static class SerializerNet
    {
		[DebuggerStepThrough]
		public static string ToXml(this object obj)
        {
            return ServiceLocator.Resolve<XmlSerializerRepository>().Serialize(obj);
        }

        public static void ToXmlConfigFile(this object obj, string path)
        {
            var xml = obj.ToXml();
            ServiceLocator.Resolve<IFileSystem>().File.WriteAllText(path, xml);
        }

		[DebuggerStepThrough]
		public static T FromXml<T>(this string xml)
        {
			return ServiceLocator.Resolve<XmlSerializerRepository>().Deserialize<T>(xml);
        }

		[DebuggerStepThrough]
		public static T LoadXml<T>(TextReader reader)
        {
			return ServiceLocator.Resolve<XmlSerializerRepository>().LoadXml<T>(reader);
		}

		[DebuggerStepThrough]
		public static XmlSerializer GetXmlSerializer(Type t)
        {
			return ServiceLocator.Resolve<XmlSerializerRepository>().GetXmlSerializer(t);
		}

		[DebuggerStepThrough]
		public static void SaveXml<T>(TextWriter streamWriter, T t)
		{
			ServiceLocator.Resolve<XmlSerializerRepository>().SaveXml(streamWriter, t);
		}

		[DebuggerStepThrough]
		public static T Copy<T>(T obj)
		{
			return ServiceLocator.Resolve<XmlSerializerRepository>().Copy<T>(obj);
		}

		[DebuggerStepThrough]
		public static string Pretty(string xml)
		{
			return ServiceLocator.Resolve<XmlSerializerRepository>().Pretty(xml);
		}

		[DebuggerStepThrough]
		public static XmlSerializerNamespaces GetXmlSerializerNamespaces()
        {
			return ServiceLocator.Resolve<XmlSerializerRepository>().GetXmlSerializerNamespaces();
		}

		/// <throws>'InvalidOperationException' if xml not valid</throws>
		public static T  FromXmlConfigFile<T>(this string filePath, T defaultValue = default)
        {
            var fs = ServiceLocator.Resolve<IFileSystem>();
            if (!fs.File.Exists(filePath))
            {
                defaultValue.ToXmlConfigFile(filePath);
            }
            var xml = fs.File.ReadAllText(filePath);

            var fileContents = xml.FromXml<T>();

            if (typeof(T).DerivesFrom(typeof(IVersionableConfigFile)))
            {
                var configFile = (IVersionableConfigFile)fileContents;
                if (configFile.LatestVersion > configFile.Version)
                {
					ConfigFile.Migrate(configFile);
                    Log.Info($"File {filePath} is now version {configFile.LatestVersion}. Saving file.");
                    fs.File.WriteAllText(filePath, configFile.ToXml());
                }
            }

            return fileContents;
        }

        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented, JsonSettings);
        }

        public class CustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (Attribute.IsDefined(member, typeof(XmlIgnoreAttribute), true))
                {
                    property.Ignored = true;
                }
                return property;
            }
        }

        static JsonSerializerSettings JsonSettings => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CustomContractResolver(),
        };

        public static T FromJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str, JsonSettings);
        }

        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }
	}
}