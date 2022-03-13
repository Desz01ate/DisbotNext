using DisbotNext.Infrastructures.Common;
using DisbotNext.Infrastructures.Common.Models;
using DSharpPlus.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient
{
    internal static class CommonTasks
    {
        public static async Task DeleteTempChannelsAsync(DSharpPlus.DiscordClient client, UnitOfWork unitOfWork, bool flush = false)
        {
            foreach (var channelGroup in unitOfWork.TempChannelRepository.GroupBy(x => x.GroupId))
            {
                var parent = channelGroup.SingleOrDefault(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Parent);

                if (parent == null)
                {
                    continue;
                }

                if (parent.ExpiredAt <= DateTime.Now || flush)
                {
                    try
                    {
                        var text = channelGroup.SingleOrDefault(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Text);
                        var voice = channelGroup.SingleOrDefault(x => x.ChannelType == Infrastructures.Common.Enum.ChannelType.Voice);

                        // invalid data state, requires removal immediately.
                        if (text == null || voice == null)
                        {
                            await unitOfWork.TempChannelRepository.DeleteAsync(parent);

                            continue;
                        }

                        var parentChannel = await client.GetChannelAsync(parent.Id);
                        var textChannel = await client.GetChannelAsync(text.Id);
                        var voiceChannel = await client.GetChannelAsync(voice.Id);

                        // if game-group text is already contains any message
                        // it might getting some attention and should not be delete
                        // and will remove from tracking temporary channels.
                        if ((await textChannel.GetMessagesAsync(1)).Count > 0)
                        {
                            await unitOfWork.TempChannelRepository.DeleteAsync(text);
                            await unitOfWork.TempChannelRepository.DeleteAsync(voice);
                            await unitOfWork.TempChannelRepository.DeleteAsync(parent);

                            continue;
                        }

                        // if no message in text but user(s) still in voice chat
                        // they might currently using the channel for some necessary battle-heating chat
                        // so give them some more time!
                        if (!voiceChannel.Users.Any())
                        {
                            await unitOfWork.TempChannelRepository.DeleteAsync(text);
                            await textChannel.DeleteAsync("expired");

                            await unitOfWork.TempChannelRepository.DeleteAsync(voice);
                            await voiceChannel.DeleteAsync("expired");

                            await unitOfWork.TempChannelRepository.DeleteAsync(parent);
                            await parentChannel.DeleteAsync("expired");
                        }
                    }

                    catch (Exception ex)
                    {
                        switch (ex)
                        {
                            case NotFoundException nfEx:

                                await unitOfWork.TempChannelRepository.DeleteManyAsync(channelGroup.Select(x => x));

                                break;

                            case BadRequestException _:

                                //continue due to this is related to discord and unable to handle on our side.
                                break;

                            default:

                                var botIdentity = await unitOfWork.MemberRepository.FindOrCreateAsync(client.CurrentUser.Id);

                                await unitOfWork.ErrorLogRepository.InsertAsync(new ErrorLog
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

            await unitOfWork.SaveChangesAsync();
        }

        public static async Task DeleteSpecificGroupAsync(Guid groupId, DSharpPlus.DiscordClient client, UnitOfWork unitOfWork)
        {
            var channelGroup = unitOfWork.TempChannelRepository.Where(g => g.GroupId == groupId);

            foreach (var channel in channelGroup)
            {
                try
                {
                    var discordChannel = await client.GetChannelAsync(channel.Id);

                    await discordChannel.DeleteAsync();

                    await unitOfWork.TempChannelRepository.DeleteAsync(channel);
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case NotFoundException nfEx:

                            await unitOfWork.TempChannelRepository.DeleteAsync(channel);

                            break;

                        case BadRequestException _:

                            //continue due to this is related to discord and unable to handle on our side.
                            break;

                        default:

                            var botIdentity = await unitOfWork.MemberRepository.FindOrCreateAsync(client.CurrentUser.Id);

                            await unitOfWork.ErrorLogRepository.InsertAsync(new ErrorLog
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

            await unitOfWork.SaveChangesAsync();
        }
    }
}
