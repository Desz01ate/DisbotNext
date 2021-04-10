using DisbotNext.Common;
using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient.Commands;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DisbotNext.Common.Extensions;
using DisbotNext.Infrastructures.Sqlite;
using DisbotNext.Helpers;
using Laploy.ThaiSen.ML;
using System.Threading.Tasks;
using System.IO;
using System;
using DSharpPlus.CommandsNext;
using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Infrastructure.Common;

namespace DisbotNext.DiscordClient
{
    public class DisbotNextClient : DiscordClientAbstract
    {
        private readonly UnitOfWork _unitOfWork;
        public override IReadOnlyList<DiscordChannel> Channels => base.Channels;
        public DisbotNextClient(IServiceProvider service,
                                UnitOfWork unitOfWork,
                                DiscordConfigurations configuration) : base(configuration)
        {
            this._unitOfWork = unitOfWork;
            this.Client.MessageCreated += Client_MessageCreated;
            this.Client.MessageReactionAdded += Client_MessageReactionAdded;
            this.Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            this.Client.GuildMemberAdded += Client_GuildMemberAdded;
            this.Client.PresenceUpdated += Client_PresenceUpdated;

            var commands = this.Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configuration.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = service,
            });
            commands.RegisterCommands<CommandsHandler>();
            commands.CommandErrored += Commands_CommandErrored;
        }

        private Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
            return Task.CompletedTask;
        }

        private async Task Client_PresenceUpdated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            var presence = e.PresenceAfter;
            var guild = presence.Guild;
            var channels = await guild.GetChannelsAsync();
            if (!channels.Any(x => x.Name.ToLowerInvariant() == presence?.Activity?.Name?.ToLowerInvariant()))
            {
                if (presence.Activity.Name == "Custom Status")
                    return;
                var parentCategoryChannel = channels.FirstOrDefault(x => x.Name == presence.Activity.Name && x.Type == DSharpPlus.ChannelType.Category) ?? await guild.CreateChannelAsync(presence.Activity.Name, DSharpPlus.ChannelType.Category);
                var textChannel = await guild.CreateChannelAsync("text", DSharpPlus.ChannelType.Text, parentCategoryChannel);
                var voiceChannel = await guild.CreateChannelAsync("voice", DSharpPlus.ChannelType.Voice, parentCategoryChannel);
                DeleteTemporaryChannelsAsync(parentCategoryChannel, voiceChannel, textChannel);

                async Task DeleteTemporaryChannelsAsync(DiscordChannel parentChannel, DiscordChannel voiceChannel, DiscordChannel textChannel)
                {
                    // 1000 ms x 60 seconds x 60 minutes
                    var interval = 1000 * 60 * 60;
                    await Task.Delay(interval);
                    if (voiceChannel.Users.Any())
                    {
                        await DeleteTemporaryChannelsAsync(parentChannel, voiceChannel, textChannel);
                    }
                    await voiceChannel.DeleteAsync();
                    await textChannel.DeleteAsync();
                    await parentChannel.DeleteAsync();
                }
            }
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
