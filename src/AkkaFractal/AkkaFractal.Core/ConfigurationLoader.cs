using System.IO;

using Akka.Configuration;

namespace AkkaFractal.Core
{
    public class ConfigurationLoader
    {
        public static Config Load() => LoadConfig("akka.conf");

        private static Config LoadConfig(string configFile)
        {
            if (!File.Exists(configFile))
                throw new FileNotFoundException($"Cannot find akka config file {configFile}");

            var config = File.ReadAllText(configFile);
            return ConfigurationFactory.ParseString(config);
        }
    }
}