using DisbotNext.ExternalServices.Financial.Stock;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Mediators
{
    public class StockReportMessageMediator : BaseMediator<IStockPriceChecker>
    {
        public StockReportMessageMediator(IStockPriceChecker stockPriceChecker) : base(stockPriceChecker)
        {
        }

        protected override async IAsyncEnumerable<DiscordEmbed?> GenerateDiscordEmbedsAsync(string? queryString, CancellationToken cancellationToken = default)
        {
            var stock = await this.Service.GetStockPriceAsync(queryString, cancellationToken);
            if (stock == null)
                yield break;


            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = new Optional<DiscordColor>(DiscordColor.White),
                Title = $"Stock report for {stock.Symbol} at {DateTime.Now:dd/MM/yy hh:mm:ss}",
                Description = $"Market Price : ${stock.RegularMarketPrice}\n" +
                                  $"Market Open : ${stock.RegularMarketOpen}\n" +
                                  $"Today Low : ${stock.RegularMarketDayLow}\n" +
                                  $"Today High : ${stock.RegularMarketDayHigh}"
            };

            yield return embedBuilder.Build();
        }
    }
}
