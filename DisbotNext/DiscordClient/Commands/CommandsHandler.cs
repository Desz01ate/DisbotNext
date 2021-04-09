using DisbotNext.Infrastructures.Sqlite;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        private readonly SqliteDbContext _sqliteDbContext;
        public CommandsHandler(SqliteDbContext dbContext)
        {
            this._sqliteDbContext = dbContext;
        }

        [Command("Test")]
        public async Task Test(CommandContext ctx, [RemainingText] string txt)
        {
            await ctx.RespondAsync(txt);
        }
    }
}
