using DisbotNext.Infrastructures.Common.Models;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
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
            try
            {
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
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task DeleteTempChannels()
        {
            await this.semaphore.WaitAsync();
            try
            {
                foreach (var channelGroup in this._unitOfWork.TempChannelRepository.GroupBy(x => x.GroupId))
                {
                    var parent = channelGroup.Single(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Parent);
                    if (parent.ExpiredAt <= DateTime.Now)
                    {
                        try
                        {
                            var text = channelGroup.Single(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Text);
                            var voice = channelGroup.Single(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Voice);

                            var parentChannel = await this.Client.GetChannelAsync(parent.Id);
                            var textChannel = await this.Client.GetChannelAsync(text.Id);
                            var voiceChannel = await this.Client.GetChannelAsync(voice.Id);

                            // if game-group text is already contains any message
                            // it might getting some attention and should not be delete
                            // and will remove from tracking temporary channels.
                            if ((await textChannel.GetMessagesAsync(1)).Count > 0)
                            {
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(text);
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(voice);
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(parent);
                                continue;
                            }

                            // if no message in text but user(s) still in voice chat
                            // they might currently using the channel for some necessary battle-heating chat
                            // so give them some more time!
                            if (!voiceChannel.Users.Any())
                            {
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(text);
                                await textChannel.DeleteAsync("expired");
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(voice);
                                await voiceChannel.DeleteAsync("expired");
                                await this._unitOfWork.TempChannelRepository.DeleteAsync(parent);
                                await parentChannel.DeleteAsync("expired");
                            }
                        }

                        catch (Exception ex)
                        {
                            switch (ex)
                            {
                                case NotFoundException nfEx:
                                    await this._unitOfWork.TempChannelRepository.DeleteManyAsync(channelGroup.Select(x => x));
                                    break;
                                case BadRequestException _:
                                    //continue due to this is related to discord and unable to handle on our side.
                                    break;
                                default:
                                    var botIdentity = await this._unitOfWork.MemberRepository.FindOrCreateAsync(this.Client.CurrentUser.Id);
                                    await this._unitOfWork.ErrorLogRepository.InsertAsync(new ErrorLog
                                    {
                                        Method = "DeleteTempChannels",
                                        TriggeredBy = botIdentity,
                                        Log = ex.ToString(),
                                        CreatedAt = DateTime.Now
                                    });
                                    break;
                            }
                        }
                    }
                }
                await this._unitOfWork.SaveChangesAsync();
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task SendStockPriceAsync()
        {
            await this.semaphore.WaitAsync();
            try
            {
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
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
