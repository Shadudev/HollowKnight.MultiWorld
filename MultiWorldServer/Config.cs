using Newtonsoft.Json;
using System;
using System.IO;

namespace MultiWorldServer
{
    internal class Config
    {
        private readonly static string CONFIG_PATH = "config.json";

        public string ListeningIP { get; set; } = "0.0.0.0";
        public int ListeningPort { get; set; } = MultiWorldLib.Consts.DEFAULT_PORT;

        public string ServerName { get; set; } = "Default Server";

        public static Config Load()
        {
            Config defaultConfig = new Config();
            if (File.Exists(CONFIG_PATH))
            {
                try
                {
                    Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH));
                    if (config != null)
                        return config;
                    else
                        Server.Log($"Config file `{CONFIG_PATH}` is invalid, please revert or remove config file");
                }
                catch (Exception e)
                {
                    Server.Log($"Failed to parse config file `{CONFIG_PATH}`: " + e.Message);
                    Server.Log(e.StackTrace);
                }
            }
            else
            {
                Server.Log($"Config file `{CONFIG_PATH}` missing, using default config and saving to file");
                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
            }

            return defaultConfig;
        }
    }
}
