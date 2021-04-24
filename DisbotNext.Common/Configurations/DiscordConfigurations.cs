using System;
using System.Collections.Generic;
using System.Text;

namespace DisbotNext.Common.Configurations
{
    public class DiscordConfigurations
    {
        public string DiscordBotToken { get; set; }
        public string CommandPrefix { get; set; }
        public string DailyReportCron { get; set; }

        public static DiscordConfigurations GetConfiguration()
        {
            var commandPrefix = Environment.GetEnvironmentVariable("COMMAND_PREFIX") ?? "!";
            var discordBotToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? throw new Exception("No environment variable 'DISCORD_BOT_TOKEN' found.");
            var cron = Environment.GetEnvironmentVariable("DAILY_REPORT_CRON") ?? "0 0 * * *";
            return new DiscordConfigurations()
            {
                CommandPrefix = commandPrefix,
                DiscordBotToken = discordBotToken,
                DailyReportCron = cron,
            };
        }
    }
}
