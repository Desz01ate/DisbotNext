using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var web = CreateHostBuilder(args).Build();
            using var scope = web.Services.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<IHost>();
            await web.RunAsync();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static Task StartHostServiceAsync(IConfiguration config)
        {
            var discordSection = config.GetSection("Discord");
            var connectionString = config.GetConnectionString("Default");
            var botToken = discordSection["BotToken"];
            var cron = discordSection["ReportCron"];

            var host = DisbotNext.Program.Main(new[] { connectionString, botToken, cron });
            return host;
        }
    }
}
