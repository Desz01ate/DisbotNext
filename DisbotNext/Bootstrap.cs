using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.ExternalServices.Financial.Stock;
using DisbotNext.ExternalServices.OilPriceChecker;
using DisbotNext.Infrastructures.Common;
using DisbotNext.Interfaces;
using DisbotNext.Mediators;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            string connectionString, botToken, cron;
            if (args.Any())
            {
                connectionString = args[0];
                botToken = args[1];
                cron = args[2];
            }
            else
            {
                connectionString = Environment.GetEnvironmentVariable("DISBOT_CONNECTION_STRING") ?? $@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Local.db")}";
                //var commandPrefix = Environment.GetEnvironmentVariable("COMMAND_PREFIX") ?? "!";
                botToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new KeyNotFoundException("No environment variable 'DISCORD_BOT_TOKEN' found.");
                cron = Environment.GetEnvironmentVariable("DAILY_REPORT_CRON") ?? "0 0 * * *";
            }

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(config => config.AddConsole());

                    services.AddHttpClient();

                    services.AddSqliteDbContext(connectionString);

                    services.AddHangfire(config =>
                    {
                        config.UseColouredConsoleLogProvider();
                        config.UseLiteDbStorage();
                    });
                    services.AddHangfireServer();
                    services.AddSingleton<DisbotNextClient>();
                    services.AddScoped(_ => DiscordConfigurations.GetConfiguration(botToken, cron: cron));
                    services.AddScoped<UnitOfWork>();
                    services.AddTransient<ICovidTracker, CovidTracker>();
                    services.AddTransient<IOilPriceChecker, OilPriceWebScraping>();
                    services.AddTransient<IStockPriceChecker, StockPriceChecker>();
                    services.AddTransient<IMessageMediator<ICovidTracker>, CovidReportMessageMediator>();
                    services.AddTransient<IMessageMediator<IOilPriceChecker>, OilPriceMessageMediator>();
                    services.AddTransient<IMessageMediator<IStockPriceChecker>, StockReportMessageMediator>();
                    services.AddHostedService<Application>();
                })
                .UseConsoleLifetime();
            return host;
        }

        public static void ApplyMigrations(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DisbotDbContext>();
            TryApplyMigrations(dbContext);
        }

        public static bool TryApplyMigrations(DbContext dbContext)
        {
            try
            {
                dbContext.Database.Migrate();
                return true;
            }
            catch (Exception ex)
            {
                var lastColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = lastColor;
                return false;
            }
        }
    }
}
