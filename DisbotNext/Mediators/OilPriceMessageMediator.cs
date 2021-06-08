using DisbotNext.ExternalServices.OilPriceChecker;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Mediators
{
    public class OilPriceMessageMediator : MessageMediatorBase<IOilPriceChecker>
    {
        public OilPriceMessageMediator(IOilPriceChecker service) : base(service)
        {
        }

        protected override async IAsyncEnumerable<DiscordEmbed?> EnumerateDiscordEmbedAsync(string? queryString, CancellationToken cancellationToken = default)
        {
            var prices = await this.Service.GetOilPriceAsync(cancellationToken);
            var today = prices.SelectMany(x => x.Types)
                               .Where(x => x.PricePerLitre != null && x.RetailName != "พรุ่งนี้")
                               .GroupBy(x => x.Type)
                               .Select(x => new
                               {
                                   Type = x.Key,
                                   Info = x.OrderBy(y => y.PricePerLitre.Value).First()
                               }).ToArray();

            var tomorrow = prices.SelectMany(x => x.Types)
                               .Where(x => x.PricePerLitre != null && x.RetailName == "พรุ่งนี้")
                               .GroupBy(x => x.Type)
                               .Select(x => new
                               {
                                   Type = x.Key,
                                   Info = x.OrderBy(y => y.PricePerLitre.Value).First()
                               }).ToArray();

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"ราคาน้ำมัน ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = string.Join("\n", today.Select(x => $"{x.Type} : {x.Info.PricePerLitre} บาท/ลิตร ({x.Info.RetailName})")),
                Color = DiscordColor.Green,
            };

            yield return embed.Build();

            var list = new List<string>();
            foreach (var (todayType, tomorrowType) in today.Zip(tomorrow))
            {
                var todayPrice = todayType.Info.PricePerLitre.Value;
                var tomorrowPrice = tomorrowType.Info.PricePerLitre.Value;
                var diff = Math.Round((tomorrowPrice / todayPrice) * 100 - 100, 0);
                var displayDiff = diff == 0 ? "ไม่เปลี่ยนแปลง" : $"{(diff > 0 ? "+" : "")}{diff}%";
                list.Add($"{tomorrowType.Type} : {tomorrowPrice} บาท/ลิตร ({displayDiff})");
            }
            embed.Title = "ราคาน้ำมันวันพรุ่งนี้";
            embed.Description = string.Join("\n", list);

            yield return embed.Build();
        }
    }
}
