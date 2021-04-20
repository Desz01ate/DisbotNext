using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.ExternalServices.OilPriceChecker;
using DisbotNext.Infrastructure.Common;
using DisbotNext.Infrastructures.Sqlite;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext
{
    class Program
    {
        static async Task Main(string[] args)
        {
            PrintGraffiti();
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static void PrintGraffiti()
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

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.AddDbContext<DisbotDbContext, SqliteDbContext>();
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
                    CommandPrefix = "!",
                    DailyReportCron = Hangfire.Cron.Daily(),
                };
                File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig));
                return defaultConfig;
            }
            return JsonConvert.DeserializeObject<DiscordConfigurations>(File.ReadAllText(configPath));
        }
    }
}
