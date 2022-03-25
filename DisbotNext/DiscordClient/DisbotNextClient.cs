using DisbotNext.Common;
using DisbotNext.Common.Configurations;
using DisbotNext.Common.Extensions;
using DisbotNext.DiscordClient.Commands;
using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Helpers;
using DisbotNext.Infrastructures.Common;
using DisbotNext.Infrastructures.Common.Enum;
using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Interfaces;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient
{
    public partial class DisbotNextClient : DiscordClientAbstract
    {
        private readonly SemaphoreSlim semaphore;
        private readonly UnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMessageMediator<ICovidTracker> _covidMessageMediator;

        public DisbotNextClient(ILogger<DisbotNextClient> logger,
                                IServiceProvider service,
                                IMessageMediator<ICovidTracker> covidMessageMediator,
                                UnitOfWork unitOfWork,
                                DiscordConfigurations configuration) : base(configuration)
        {
            var c = System.IO.Directory.GetCurrentDirectory();
            this.semaphore = new SemaphoreSlim(1, 1);
            this._logger = logger;
            this._covidMessageMediator = covidMessageMediator ?? throw new ArgumentNullException(nameof(covidMessageMediator));
            this._unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            this.Client.MessageCreated += Client_MessageCreated;
            this.Client.MessageReactionAdded += Client_MessageReactionAdded;
            this.Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            this.Client.GuildMemberAdded += Client_GuildMemberAdded;
            this.Client.PresenceUpdated += Client_PresenceUpdated;
            this.Client.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            this.Client.Heartbeated += Client_Heartbeated;
            this.Client.ChannelDeleted += Client_ChannelDeleted;

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
            RecurringJob.AddOrUpdate(() => SendDailyReportAsync(), configuration.DailyReportCron);
        }

        private async Task Client_ChannelDeleted(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.ChannelDeleteEventArgs e)
        {
            this._logger.LogInformation("Channel deleted triggered.");
            var deletedChannel = e.Channel;
            await this._unitOfWork.TempChannelRepository.DeleteAsync(x => x.Id == deletedChannel.Id);
            await this._unitOfWork.SaveChangesAsync();
        }

        private async Task Client_Heartbeated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.HeartbeatEventArgs e)
        {
            this._logger.LogInformation("Heartbeated triggered.");

            var activeChannels = this._unitOfWork.TempChannelRepository.GroupBy(x => x.GroupId).Count();

            this._logger.LogTrace($"{activeChannels} channels tracking.");

            await sender.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = $"{activeChannels} games channels."
            });
        }

        private async Task Client_GuildDownloadCompleted(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildDownloadCompletedEventArgs e)
        {
            this._logger.LogTrace("Guild download completed.");

            foreach (var channel in this.Channels.Where(x => x.Name == "bot-status" && x.Type == DSharpPlus.ChannelType.Text))
            {
                await channel.SendMessageAsync($"[{DateTime.Now}] ขณะนี้บอทพร้อมใช้งานแล้ว");
            }
        }

        private async Task Client_PresenceUpdated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            var presence = e.PresenceAfter;
            if (presence.User.IsBot)
                return;

            this._logger.LogInformation($"{e.User.Username} trigger presence updated.");
            var guild = presence.Guild;
            var channels = await guild.GetChannelsAsync();
            DiscordChannel? parentCategoryChannel, textChannel, voiceChannel;
            if (!channels.Any(x => x.Name.ToLowerInvariant() == presence?.Activity?.Name?.ToLowerInvariant()))
            {
                if (presence.Activity.Name == "Custom Status")
                    return;
                parentCategoryChannel = channels.FirstOrDefault(x => x.Name == presence.Activity.Name && x.Type == DSharpPlus.ChannelType.Category)
                                        ?? await guild.CreateChannelAsync(presence.Activity.Name, DSharpPlus.ChannelType.Category);
                textChannel = await guild.CreateChannelAsync("text", DSharpPlus.ChannelType.Text, parentCategoryChannel);
                voiceChannel = await guild.CreateChannelAsync("voice", DSharpPlus.ChannelType.Voice, parentCategoryChannel);

                var createdAt = DateTime.Now;
                var groupId = Guid.NewGuid();

                await QueueDeleteTempChannelAsync(parentCategoryChannel, createdAt, groupId, ChannelType.Parent);
                await QueueDeleteTempChannelAsync(textChannel, createdAt, groupId, ChannelType.Text);
                await QueueDeleteTempChannelAsync(voiceChannel, createdAt, groupId, ChannelType.Voice);

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

            async Task QueueDeleteTempChannelAsync(DiscordChannel channel, DateTime createdAt, Guid groupId, ChannelType channelType)
            {
                await this._unitOfWork.TempChannelRepository.InsertAsync(new TempChannel
                {
                    Id = channel.Id,
                    ChannelName = channel.Name,
                    CreatedAt = createdAt,
                    ExpiredAt = createdAt.AddHours(1),
                    ChannelType = channelType,
                    GroupId = groupId,
                });
            }
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            this._logger.LogError(e.Exception, $"Unexpected error occurred by [{e.Command.Name}]");
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
            this._logger.LogTrace($"{e.Member.Username} has been added to guild.");
            await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.Member.Id);
            await this._unitOfWork.SaveChangesAsync();
        }

        private async Task Client_MessageReactionRemoved(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            this._logger.LogTrace($"Message reaction removed event triggered.");
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (message.Author.IsBot ||
                message.Author.Id == e.User.Id ||
                !this.Channels.Contains(channel))
                return;

            var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(message.Author.Id);
            user.ExpGained(-1);
            await this._unitOfWork.SaveChangesAsync();
        }

        private async Task Client_MessageReactionAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            this._logger.LogTrace($"Message reaction added event triggered.");
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (message.Author.IsBot ||
                message.Author.Id == e.User.Id ||
                !this.Channels.Contains(channel))
                return;

            var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(message.Author.Id);
            user.ExpGained(1);
            await this._unitOfWork.SaveChangesAsync();
            await channel.SendDisposableMessageAsync($"มีคนชอบข้อความที่คุณเขียน {message.Author.Mention} และคุณได้รับ 1 EXP!");
        }

        private async Task Client_MessageCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            this._logger.LogInformation("Message created event triggered.");
            var channel = e.Channel;

            if (!this.Channels.Contains(channel) ||
                e.Author.IsBot)
                return;


            var user = await this._unitOfWork.MemberRepository.FindOrCreateAsync(e.Author.Id);
            var levelUp = user.ExpGained(1);
            if (levelUp)
            {
                var avatar = AvatarHelpers.GetLevelUpAvatarPath(e.Author.AvatarUrl, user.Level);
                await channel.SendMessageAsync($"🎉🎉🎉 🥂{e.Author.Mention}🥂 ได้อัพเลเวลเป็น {user.Level}! 🎉🎉🎉 ");
                await channel.SendFileAsync(avatar, true);
            }
            string content;
            if (!string.IsNullOrWhiteSpace(e.Message.Content))
            {
                content = e.Message.Content;
            }
            else
            {
                content = string.Join(',', e.Message.Attachments.Select(x => x.Url));
            }

            await this._unitOfWork.ChatLogRepository.InsertAsync(new ChatLog
            {
                Author = user,
                Content = content,
                CreateAt = DateTime.Now
            });
            await this._unitOfWork.SaveChangesAsync();
        }
    }
}
