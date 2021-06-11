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

        public static DiscordConfigurations GetConfiguration(string discordBotToken, string commandPrefix = "!", string cron = "0 0 * * *")
        {
            return new DiscordConfigurations()
            {
                CommandPrefix = commandPrefix,
                DiscordBotToken = discordBotToken,
                DailyReportCron = cron,
            };
        }
    }
}
