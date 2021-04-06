using DisbotNext.Common;
using DisbotNext.Common.Configurations;
using DisbotNext.DiscordClient.Commands;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DisbotNext.Common.Extensions;
using DisbotNext.Infrastructures.Sqlite;
using DisbotNext.Infrastructures.Sqlite.Models;
using DisbotNext.Helpers;
using Laploy.ThaiSen.ML;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace DisbotNext.DiscordClient
{
    public class DisbotNextClient : DiscordClientAbstract
    {
        private readonly SqliteDbContext _dbContext;
        public override IReadOnlyList<DiscordChannel> Channels => base.Channels;
        public DisbotNextClient(SqliteDbContext dbContext, DiscordConfigurations configuration) : base(configuration, typeof(CommandsHandler))
        {
            this._dbContext = dbContext;
            this.Client.MessageCreated += Client_MessageCreated;
            this.Client.MessageReactionAdded += Client_MessageReactionAdded;
            this.Client.MessageReactionRemoved += Client_MessageReactionRemoved;
            this.Client.GuildMemberAdded += Client_GuildMemberAdded;
        }

        private async Task Client_GuildMemberAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            var user = this._dbContext.Members.FindAsync(e.Member.Id);
            await this._dbContext.SaveChangesAsync();
        }

        private async System.Threading.Tasks.Task Client_MessageReactionRemoved(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (this.Channels.Contains(channel) && message.Author.Id != this.Client.CurrentUser.Id && message.Author.Id != e.User.Id)
            {
                var user = await this._dbContext.Members.FindAsync(message.Author.Id);
                user.ExpGained(-1);
                await this._dbContext.SaveChangesAsync();
            }
        }

        private async System.Threading.Tasks.Task Client_MessageReactionAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            var channel = e.Channel;
            var message = await channel.GetMessageAsync(e.Message.Id);
            if (this.Channels.Contains(channel) && message.Author.Id != this.Client.CurrentUser.Id && message.Author.Id != e.User.Id)
            {
                var test = await this._dbContext.Members.Include(x => x.ChatLogs).ToListAsync();
                var user = await this._dbContext.Members.FindAsync(message.Author.Id);
                user.ExpGained(1);
                await this._dbContext.SaveChangesAsync();
                await channel.SendDisposableMessageAsync($"มีคนชอบข้อความที่คุณเขียน {message.Author.Mention} และคุณได้รับ 1 EXP!");
            }
        }

        private async System.Threading.Tasks.Task Client_MessageCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var channel = e.Channel;
            if (this.Channels.Contains(channel) && e.Author.Id != this.Client.CurrentUser.Id)
            {
                (bool isRude, float _) = ThaiSen.Predict(e.Message.Content);
                if (isRude)
                {
                    await channel.SendMessageAsync($"สุภาพหน่อย!");
                    await channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "language.jpg"));
                    return;
                }
                var user = await this._dbContext.Members.FindAsync(e.Author.Id);
                var levelUp = user.ExpGained(1);
                if (levelUp)
                {
                    var avatar = AvatarHelpers.GetLevelupAvatar(e.Author.AvatarUrl, user.Level);
                    await channel.SendMessageAsync($"🎉🎉🎉 🥂{e.Author.Mention}🥂 ได้อัพเลเวลเป็น {user.Level}! 🎉🎉🎉 ");
                    await channel.SendFileAsync(avatar);
                }
                await this._dbContext.SaveChangesAsync();

            }
        }
    }
}
