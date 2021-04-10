using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Infrastructure.Common;
using DisbotNext.Infrastructures.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DisbotNext
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddDbContext<DisbotDbContext, SqliteDbContext>();
                    services.AddSingleton<DisbotNextClient>();
                    services.AddScoped<UnitOfWork>();
                    services.AddTransient<ICovidTracker, CovidTracker>();
                    services.AddTransient(_ => GetConfiguration());
                    services.AddHostedService<Application>();
                })
                .UseConsoleLifetime();
            return host;
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
