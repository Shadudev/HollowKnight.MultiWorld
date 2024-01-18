using Newtonsoft.Json;
using System;
using System.IO;

namespace MultiWorldServer
{
    [Serializable]
    public class Config
    {
        private readonly static string CONFIG_PATH = "config.json";

        public string ListeningIP { get; set; } = "0.0.0.0";
        public int ListeningPort { get; set; } = MultiWorldLib.Consts.DEFAULT_PORT;

        public string ServerName { get; set; } = "Default Server";

        public uint MaxGeneralLogSize { get; set; } = 16 * 1024 * 1024; // 16 MB

        public bool CreateLogPerGameSession { get; internal set; } = false;

        public uint MaxGameLogSize { get; set; } = 1024 * 1024;
        public string PagedGamesDirectory { get; internal set; } = "Paged Games";
        public TimeSpan InactiveGameSessionPagingTime { get; internal set; } = TimeSpan.FromMinutes(5); // Page game after 5 minutes of inactivity

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
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to parse config file `{CONFIG_PATH}`: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    throw e;
                }

                string msg = $"Config file `{CONFIG_PATH}` is invalid, please revert or remove config file";
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
            else
            {
                Console.WriteLine($"Config file `{CONFIG_PATH}` missing, using default config and saving to file");
                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
            }

            return defaultConfig;
        }
    }
}
