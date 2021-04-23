using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.ExternalServices.Financial.Stock;
using DisbotNext.ExternalServices.OilPriceChecker;
using DisbotNext.Infrastructure.Common;
using DisbotNext.Infrastructures.Sqlite;
using DisbotNext.Infrastructures.Sqlite.HealthChecks;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace DisbotNext
{
    public static class Bootstrap
    {
        public static void PrintGraffiti()
        {
            var graffiti = new[] {
                @"                                                                                                                     ",
                @"   _ .-') _             .-')  .-. .-')                .-') _        .-') _   ('-.  ) (`-.      .-') _                ",
                @"  ( (  OO) )           ( OO ).\  ( OO )              (  OO) )      ( OO ) )_(  OO)  ( OO ).   (  OO) )               ",
                @"   \     .'_   ,-.-') (_)---\_,-----.\  .-'),-----. /     '._ ,--./ ,--,'(,------.(_/.  \_)-./     '._      ,-.      ",
                @"   ,`'--..._)  |  |OO)/    _ | | .-.  | ( OO'  .-.  '|'--...__)|   \ |  |\ |  .---' \  `.'  / |'--...__)    | |      ",
                @"   |  |  \  '  |  |  \\  :` `. | '-' /_)/   |  | |  |'--.  .--'|    \|  | )|  |      \     /\ '--.  .--',---| |---.  ",
                @"   |  |   ' |  |  |(_/ '..`''.)| .-. `. \_) |  |\|  |   |  |   |  .     |/(|  '--.    \   \ |    |  |   '---| |---'  ",
                @"   |  |   / : ,|  |_.'.-._)   \| |  \  |  \ |  | |  |   |  |   |  |\    |  |  .--'   .'    \_)   |  |       | |      ",
                @"   |  '--'  /(_|  |   \       /| '--'  /   `'  '-'  '   |  |   |  | \   |  |  `---. /  .'.  \    |  |       `-'      ",
                @"   `-------'   `--'    `-----' `------'      `-----'    `--'   `--'  `--'  `------''--'   '--'   `--'                ",
                @"                                                                                                                     ",
            };
            //+2 for extra border chars below.
            var borderLine = string.Join("", Enumerable.Range(0, graffiti.Max(x => x.Length) + 2).Select(x => x % 2 == 0 ? "+" : "x"));
            var borderColor = ConsoleColor.Green;
            Console.ForegroundColor = borderColor;
            Console.WriteLine(borderLine);
            for (var i = 0; i < graffiti.Length; i++)
            {
                var percent = (float)i / graffiti.Length;
                var color = percent switch
                {
                    var x when x < 0.2 => ConsoleColor.DarkBlue,
                    var x when x < 0.5 => ConsoleColor.Blue,
                    var x when x < 0.7 => ConsoleColor.Blue,
                    _ => ConsoleColor.DarkBlue
                };
                var line = graffiti[i];
                var borderChar = $"{(i % 2 == 0 ? "x" : "+")}";
                Console.ForegroundColor = borderColor;
                Console.Write(borderChar);
                Console.ForegroundColor = color;
                Console.Write(line);
                Console.ForegroundColor = borderColor;
                Console.WriteLine(borderChar);
            }
            Console.WriteLine(borderLine);
            Console.WriteLine();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddDbContext<DisbotDbContext, SqliteDbContext>();
                    services.AddHealthChecks().AddCheck<DbPendingMigrationHealthCheck<SqliteDbContext>>("db-migration-check");
                    services.AddHangfire(config =>
                    {
                        config.UseColouredConsoleLogProvider();
                        config.UseLiteDbStorage();
                    });
                    services.AddHangfireServer();
                    services.AddSingleton<DisbotNextClient>();
                    services.AddScoped<UnitOfWork>();
                    services.AddTransient<ICovidTracker, CovidTracker>();
                    services.AddTransient<IOilPriceChecker, OilPriceWebScraping>();
                    services.AddTransient<IStockPriceChecker, StockPriceChecker>();
                    services.AddTransient(_ => GetConfiguration());
                    services.AddHostedService<Application>();
                })
                .UseConsoleLifetime();
            return host;
        }

        private static DiscordConfigurations GetConfiguration()
        {
            var commandPrefix = Environment.GetEnvironmentVariable("COMMAND_PREFIX") ?? "!";
            var discordBotToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new Exception("No environment variable 'DISCORD_BOT_TOKEN' found.");
            var cron = Environment.GetEnvironmentVariable("DAILY_REPORT_CRON") ?? Cron.Daily();
            return new DiscordConfigurations()
            {
                CommandPrefix = commandPrefix,
                DiscordBotToken = discordBotToken,
                DailyReportCron = cron,
            };
        }

        public static void ApplyMigrations(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DisbotDbContext>();
            ApplyMigrations(dbContext);
        }

        public static void ApplyMigrations(DisbotDbContext dbContext)
        {
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }
        }
    }
}
