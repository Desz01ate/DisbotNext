using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Interfaces;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Mediators
{
    public class CovidReportMessageMediator : BaseMediator<ICovidTracker>
    {
        public CovidReportMessageMediator(ICovidTracker service) : base(service)
        {
        }

        protected override async IAsyncEnumerable<DiscordEmbed?> GenerateDiscordEmbedsAsync(string? queryString, CancellationToken cancellationToken = default)
        {
            var result = await this.Service.GetCovidTrackerDataAsync(queryString, cancellationToken);
            if (result == null)
                yield break;

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"สถานการณ์ Covid-19 ของ {result.Country} ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = result.ToString(),
                Color = new Optional<DiscordColor>(DiscordColor.Red),
            };

            yield return embed;
        }
    }
}
