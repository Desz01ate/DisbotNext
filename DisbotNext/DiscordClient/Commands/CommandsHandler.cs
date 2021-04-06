using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        public CommandsHandler()
        {
        }

        [Command("Test")]
        public async Task Test(CommandContext ctx, [RemainingText] string txt)
        {
            await ctx.RespondAsync(txt);
        }
    }
}
