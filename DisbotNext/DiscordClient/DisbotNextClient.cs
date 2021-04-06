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

namespace DisbotNext.DiscordClient
{
    public class DisbotNextClient : DiscordClientAbstract
    {
        private readonly SqliteDbContext _dbContext;
        public override IReadOnlyList<DiscordChannel> Channels => base.Channels;//.Where(x => x.Name == "disbot").ToImmutableList();
        public DisbotNextClient(SqliteDbContext dbContext, DiscordConfigurations configuration) : base(configuration, typeof(CommandsHandler))
        {
            this._dbContext = dbContext;
            this.Client.MessageCreated += Client_MessageCreated;
        }

        private async System.Threading.Tasks.Task Client_MessageCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var channel = e.Channel;
            if (this.Channels.Contains(channel) && e.Author.Id != this.Client.CurrentUser.Id)
            {
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
