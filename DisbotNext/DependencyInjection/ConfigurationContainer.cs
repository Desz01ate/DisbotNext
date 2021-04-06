using Autofac;
using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient;
using DisbotNext.Infrastructures.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.DependencyInjection
{
    public static class ConfigurationContainer
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SqliteDbContext>();
            builder.Register(x => GetConfiguration());
            builder.RegisterType<DisbotNextClient>();
            builder.RegisterType<Application>();
            return builder.Build();
        }

        private static DiscordConfigurations GetConfiguration()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            if (!File.Exists(configPath))
            {
                var defaultConfig = new DiscordConfigurations()
                {
                    DiscordBotToken = "",
                    CommandPrefix = "!"
                };
                File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig));
                return defaultConfig;
            }
            return JsonConvert.DeserializeObject<DiscordConfigurations>(File.ReadAllText(configPath));
        }
    }
}
