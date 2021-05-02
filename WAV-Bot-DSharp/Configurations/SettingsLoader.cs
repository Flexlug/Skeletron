using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace WAV_Bot_DSharp.Configurations
{
    /// <summary>
    /// Singleton class that loads settings from a file on the first usage.
    /// Settings are stored in the Cfg-Property.
    /// </summary>
    public sealed class SettingsLoader
    {
        public string DefaultConfigFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bot.cfg");

        /// <summary>
        /// Serializes Settings object to JSON and writes it to a file
        /// </summary>
        /// <param name="configFile">Configurations file<param>
        /// <param name="settings">Settings object to save</param>
        public void SaveToFile(string configFile, Settings settings)
        {
            File.WriteAllText(configFile, JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// Serializes Settings object to JSON and writes it to default config file
        /// </summary>
        /// <param name="settings"></param>
        public void SaveToFile(Settings settings)
        {
            SaveToFile(DefaultConfigFile, settings);
        }

        /// <summary>
        /// Deserializes a file to a Settings object
        /// </summary>
        /// <param name="configFile">File to deserialize</param>
        /// <returns>deserialized Settings object</returns>
        public Settings LoadFromFile(string configFile)
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configFile));
        }

        /// <summary>
        /// Deserializes default config file to a Settings object
        /// </summary>
        /// <returns>deserialized Settings object</returns>
        public Settings LoadFromFile()
        {
            return LoadFromFile(DefaultConfigFile);
        }
    }
}
