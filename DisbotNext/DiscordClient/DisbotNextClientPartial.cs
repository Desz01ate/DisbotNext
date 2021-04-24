using DisbotNext.Infrastructures.Common.Models;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient
{
    public partial class DisbotNextClient
    {
        public async Task SendDailyReportAsync()
        {
            await this.semaphore.WaitAsync();
            var channels = this.Channels.Where(x => x.Name == "daily-report" && x.Type == DSharpPlus.ChannelType.Text);
            var covidReport = await this.covidTracker.GetCovidTrackerDataAsync("thailand");
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"สถานการณ์ Covid-19 ของ {covidReport.Country} ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = covidReport.ToString(),
                Color = new Optional<DiscordColor>(DiscordColor.Red)

            }.Build();
            foreach (var channel in channels)
            {
                await channel.SendMessageAsync(embed);
            }

            this.semaphore.Release();
        }

        public async Task DeleteTempChannels()
        {
            await this.semaphore.WaitAsync();
            foreach (var channel in this._unitOfWork.TempChannelRepository)
            {
                if (channel.ExpiredAt <= DateTime.Now)
                {
                    try
                    {
                        var tempChannel = await this.Client.GetChannelAsync(channel.Id);
                        if ((tempChannel.Type == DSharpPlus.ChannelType.Voice && !tempChannel.Users.Any()) || tempChannel.Type != DSharpPlus.ChannelType.Voice)
                        {
                            await tempChannel.DeleteAsync("expired");
                        }
                    }
                    catch (Exception ex)
                    {
                        var botIdentity = await this._unitOfWork.MemberRepository.FindOrCreateAsync(this.Client.CurrentUser.Id);
                        await this._unitOfWork.ErrorLogRepository.InsertAsync(new ErrorLog
                        {
                            Method = "DeleteTempChannels",
                            TriggeredBy = botIdentity,
                            Log = ex.ToString(),
                            CreatedAt = DateTime.Now
                        });
                        await this._unitOfWork.TempChannelRepository.DeleteAsync(channel);
                    }
                }
            }
            await this._unitOfWork.SaveChangesAsync();
            this.semaphore.Release();
        }

        public async Task SendStockPriceAsync()
        {
            await this.semaphore.WaitAsync();
            var subscriptions = this._unitOfWork.StockSubscriptions.Select(x => new { x.DiscordMemberId, x.Symbol }).GroupBy(x => x.Symbol);
            foreach (var group in subscriptions)
            {
                var symbol = group.Key;
                var stock = await this.stockPriceChecker.GetStockPriceAsync(symbol);
                if (stock == null)
                    continue;
                var embedBuilder = new DiscordEmbedBuilder()
                {
                    Color = new Optional<DiscordColor>(DiscordColor.White),
                    Title = $"Stock price for {stock.Symbol} at {DateTime.Now:dd/MM/yy hh:mm:ss}",
                    Description = $"Market Price : ${stock.RegularMarketPrice}\n" +
                                  $"Market Open : ${stock.RegularMarketOpen}\n" +
                                  $"Today Low : ${stock.RegularMarketDayLow}\n" +
                                  $"Today High : ${stock.RegularMarketDayHigh}"
                };
                foreach (var discordMemberId in group.Select(x => x.DiscordMemberId))
                {
                    var member = this.Members.FirstOrDefault(x => x.Id == discordMemberId);
                    if (member != null)
                    {
                        await member.SendMessageAsync(embedBuilder.Build());
                    }
                }
            }
            semaphore.Release();
        }
    }
}
