using System;
using System.ComponentModel;
using System.IO;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using Infrastructure.Utilities;

namespace Infrastructure
{
	public interface IPopulateDefaultValues
	{
		void PopulateDefaultValues();
	}

	public class ConfigFile
	{
		public static string Folder =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Combitech");

		private static string F(string fileName) => Path.Combine(Folder, fileName);

        public static T Get<T>() where T : class, new()
        {
            var fileName = $"{typeof(T).Name}.json";
            var fullPath = F(fileName);
            if (IO.FileExists(fullPath))
            {
                var existingFileContents = IO.ReadAllText(fullPath);
                var deserializedConfig = existingFileContents.FromJson<T>();

                if (deserializedConfig is IVersionableConfigFile migratable)
                {
                    Migrate(migratable);
                    Save(migratable);
                }

				return deserializedConfig;
			}

			var newConfig = new T();
			PopulateDefaults(newConfig);
			Save(newConfig);
			return newConfig;
		}

		public static void Migrate(IVersionableConfigFile config)
		{
			if (config.Version < config.LatestVersion)
			{
				Log.Info($"Config {config.GetType().Name} is ConfigFile version {config.Version}. Migrating to {config.LatestVersion}");
				config.Migrate();
				config.Version = config.LatestVersion;
			}
			else
			{
				Log.Info($"Config {config.GetType().Name} is ConfigFile version {config.Version}.Up to date.");
			}
		}

		public static void Save<T>(T config)
		{
			var fileName = $"{config.GetType().Name}.json";
			var json = config.ToJson();
			IO.WriteAllText(F(fileName), json);
		}

		private static void PopulateDefaults<T>(T newConfig)
		{
			var properties = newConfig.GetType().GetProperties();
			foreach (var propertyInfo in properties)
			{
				var defaultValue = propertyInfo.GetAttribute<DefaultValueAttribute>();
				if (defaultValue != null)
				{
					propertyInfo.SetValue(newConfig, defaultValue.Value);
				}
			}

			if (newConfig is IPopulateDefaultValues defaulter)
			{
				defaulter.PopulateDefaultValues();
			}
		}
	}
}
