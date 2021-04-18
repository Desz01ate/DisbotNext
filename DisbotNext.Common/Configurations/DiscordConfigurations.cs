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
    }
}
