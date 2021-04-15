using DisbotNext.Common;
using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient.Commands;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DisbotNext.Common.Extensions;
using DisbotNext.Helpers;
using Laploy.ThaiSen.ML;
using System.Threading.Tasks;
using System.IO;
using System;
using DSharpPlus.CommandsNext;
using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Infrastructure.Common;
using Hangfire;
using System.Threading;
using DisbotNext.Infrastructure.Common.Models;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.ExternalServices.OildPriceChecker;

namespace DisbotNext.DiscordClient
{
    public class DisbotNextClient : DiscordClientAbstract
    {
        private readonly SemaphoreSlim semaphore;
        private readonly ICovidTracker covidTracker;
        private readonly IOilPriceChecker oilPriceChecker;
        private readonly UnitOfWork _unitOfWork;

        public override IReadOnlyList<DiscordChannel> Channels => base.Channels;

        public DisbotNextClient(IServiceProvider service,
                                ICovidTracker covidTracker,
                                IOilPriceChecker oilPriceChecker,
                                UnitOfWork unitOfWork,
                                DiscordConfigurations configuration) : base(configuration)
        {
            this.semaphore = new SemaphoreSlim(1, 1);
            this.covidTracker = covidTracker;
            this.oilPriceChecker = oilPriceChecker;
            this._unitOfWork = unitOfWork;
            this.Client.MessageCreated += Client_MessageCreated;
            this.Client.MessageReactionAdded += Client_MessageReactionAdded;
            this.Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            this.Client.GuildMemberAdded += Client_GuildMemberAdded;
            this.Client.PresenceUpdated += Client_PresenceUpdated;
            this.Client.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            var commands = this.Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configuration.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = service,
            });
            commands.RegisterCommands<CommandsHandler>();
            commands.CommandErrored += Commands_CommandErrored;

            RecurringJob.AddOrUpdate(() => DeleteTempChannels(), Cron.Minutely());
            RecurringJob.AddOrUpdate(() => SendDailyReportAsync(), Cron.Daily());
        }

        private async Task Client_GuildDownloadCompleted(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildDownloadCompletedEventArgs e)
        {
            await SendDailyReportAsync();
            foreach (var channel in this.Channels.Where(x => x.Name == "bot-status" && x.Type == DSharpPlus.ChannelType.Text))
            {
                await channel.SendMessageAsync($"[{DateTime.Now}] ขณะนี้บอทพร้อมใช้งานแล้ว");
            }
        }

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

        private async Task Client_PresenceUpdated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            var presence = e.PresenceAfter;
            var guild = presence.Guild;
            var channels = await guild.GetChannelsAsync();
            DiscordChannel? parentCategoryChannel, textChannel, voiceChannel;
            if (!channels.Any(x => x.Name.ToLowerInvariant() == presence?.Activity?.Name?.ToLowerInvariant()))
            {
                if (presence.Activity.Name == "Custom Status")
                    return;
                parentCategoryChannel = channels.FirstOrDefault(x => x.Name == presence.Activity.Name && x.Type == DSharpPlus.ChannelType.Category) ?? await guild.CreateChannelAsync(presence.Activity.Name, DSharpPlus.ChannelType.Category);
                textChannel = await guild.CreateChannelAsync("text", DSharpPlus.ChannelType.Text, parentCategoryChannel);
                voiceChannel = await guild.CreateChannelAsync("voice", DSharpPlus.ChannelType.Voice, parentCategoryChannel);

                var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.User.Id);
                if (user.AutoMoveToChannel)
                {
                    var member = await guild.GetMemberAsync(user.Id);
                    var voiceState = member.VoiceState;
                    // we can only move people when they are already in any voice channel.
                    if (voiceState != null)
                    {
                        await voiceChannel.PlaceMemberAsync(member);
                    }
                }

                var createdAt = DateTime.Now;

                await QueueDeleteTempChannelAsync(parentCategoryChannel, createdAt);
                await QueueDeleteTempChannelAsync(textChannel, createdAt);
                await QueueDeleteTempChannelAsync(voiceChannel, createdAt);

                await this._unitOfWork.SaveChangesAsync();
            }
            else
            {
                parentCategoryChannel = channels.Single(x => x.Name.ToLowerInvariant() == presence?.Activity?.Name?.ToLowerInvariant());
                voiceChannel = parentCategoryChannel.Children.SingleOrDefault(x => x.Name.ToLowerInvariant() == "voice");
            }

            var memberInRepo = await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.User.Id);
            if (memberInRepo.AutoMoveToChannel)
            {
                var guildMember = await guild.GetMemberAsync(memberInRepo.Id);
                var voiceState = guildMember.VoiceState;
                // we can only move people when they are already in any voice channel.
                if (voiceState != null && voiceChannel != null && voiceState.Channel.Id != voiceChannel.Id)
                {
                    await voiceChannel.PlaceMemberAsync(guildMember);
                }
            }

            async Task QueueDeleteTempChannelAsync(DiscordChannel channel, DateTime createdAt)
            {
                await this._unitOfWork.TempChannelRepository.InsertAsync(new Infrastructure.Common.Models.TempChannel
                {
                    Id = channel.Id,
                    ChannelName = channel.Name,
                    CreatedAt = createdAt,
                    ExpiredAt = createdAt.AddHours(1),
                });
            }
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var triggeredMember = await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.Context.User.Id);
            var log = new ErrorLog()
            {
                Method = e.Command.Name,
                Log = e.Exception.ToString(),
                CreatedAt = DateTime.Now,
                TriggeredBy = triggeredMember,
            };
            await this._unitOfWork.ErrorLogRepository.InsertAsync(log);
        }


        private async Task Client_GuildMemberAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.Member.Id);
            await this._unitOfWork.SaveChangesAsync();
        }

        private async System.Threading.Tasks.Task Client_MessageReactionRemoved(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (this.Channels.Contains(channel) && !message.Author.IsBot && message.Author.Id != e.User.Id)
            {
                var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(message.Author.Id);
                user.ExpGained(-1);
                await this._unitOfWork.SaveChangesAsync();
            }
        }

        private async System.Threading.Tasks.Task Client_MessageReactionAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (this.Channels.Contains(channel) && !message.Author.IsBot && message.Author.Id != e.User.Id)
            {
                var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(message.Author.Id);
                user.ExpGained(1);
                await this._unitOfWork.SaveChangesAsync();
                await channel.SendDisposableMessageAsync($"มีคนชอบข้อความที่คุณเขียน {message.Author.Mention} และคุณได้รับ 1 EXP!");
            }
        }

        private async System.Threading.Tasks.Task Client_MessageCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var channel = e.Channel;
            if (this.Channels.Contains(channel) && !e.Author.IsBot)
            {
                (bool isRude, float _) = ThaiSen.Predict(e.Message.Content);
                if (isRude)
                {
                    await channel.SendMessageAsync($"สุภาพหน่อย!");
                    await channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "language.jpg"));
                    return;
                }
                var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.Author.Id);
                var levelUp = user.ExpGained(1);
                if (levelUp)
                {
                    var avatar = AvatarHelpers.GetLevelupAvatar(e.Author.AvatarUrl, user.Level);
                    await channel.SendMessageAsync($"🎉🎉🎉 🥂{e.Author.Mention}🥂 ได้อัพเลเวลเป็น {user.Level}! 🎉🎉🎉 ");
                    await channel.SendFileAsync(avatar);
                }
                await this._unitOfWork.ChatLogRepository.InsertAsync(new ChatLog
                {
                    Author = user,
                    Content = e.Message.Content,
                    CreateAt = DateTime.Now
                });
                await this._unitOfWork.SaveChangesAsync();

            }
        }
    }
}
