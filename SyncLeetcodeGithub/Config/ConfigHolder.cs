using Microsoft.Extensions.Configuration;

namespace SyncLeetcodeGithub.Config
{
    public class ConfigHolder
    {
        private IConfigurationRoot configuration;
        private ConfigHolder()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
        private static ConfigHolder? instance;
        private static readonly object mutex = new object();
        public static ConfigHolder getInstance()
        {
            if (instance == null)
            {
                lock (mutex)
                {
                    if (instance == null)
                    {
                        instance = new ConfigHolder();
                    }
                }
            }
            return instance;
        }
        public static IConfigurationRoot getConfig()
        {
            return getInstance().configuration;
        }
    }
}
