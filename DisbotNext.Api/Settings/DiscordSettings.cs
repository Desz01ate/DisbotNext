using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.Api.Settings
{
    public record DiscordSettings
    {
        public string BotToken { get; init; }
        public string ReportCron { get; init; }
    }
}
