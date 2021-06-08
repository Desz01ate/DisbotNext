using DisbotNext.Infrastructures.Common.Models;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.ML.Runtime;
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
                foreach (var channel in channels)
                {
                    await this._covidMessageMediator.SendAsync("thailand", channel.SendMessageAsync);
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
    }
}
