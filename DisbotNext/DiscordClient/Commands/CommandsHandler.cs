using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Common.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.IO;
using DisbotNext.ExternalServices.OilPriceChecker;
using System.Linq;
using System.Collections.Generic;
using DisbotNext.Common.Configurations;
using DisbotNext.ExternalServices.Financial.Stock;
using DisbotNext.Infrastructures.Common;
using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Interfaces;

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly DiscordConfigurations configurations;
        private readonly IMessageMediator<ICovidTracker> _covidTracker;
        private readonly IMessageMediator<IOilPriceChecker> _oilPriceChecker;
        private readonly IMessageMediator<IStockPriceChecker> _stockPriceChecker;
        public CommandsHandler(UnitOfWork unitOfWork,
                               DiscordConfigurations configurations,
                               IMessageMediator<ICovidTracker> covidTracker,
                               IMessageMediator<IOilPriceChecker> oilPriceChecker,
                               IMessageMediator<IStockPriceChecker> stockPriceChecker)
        {
            this._unitOfWork = unitOfWork;
            this.configurations = configurations;
            this._covidTracker = covidTracker;
            this._oilPriceChecker = oilPriceChecker;
            this._stockPriceChecker = stockPriceChecker;
        }

        [Command("test")]
        public async Task Test(CommandContext ctx, [RemainingText] string txt)
        {
            await ctx.RespondAsync(txt);
        }

        [Command("covid")]
        public async Task GetCovidDetail(CommandContext ctx, [RemainingText] string country = "thailand")
        {
            await this._covidTracker.SendAsync(country, ctx.RespondAsync);
        }

        [Command("oilprice")]
        public async Task GetOilPriceAsync(CommandContext ctx)
        {
            await this._oilPriceChecker.SendAsync(null, ctx.RespondAsync);
        }

        [Command("stock")]
        public async Task GetStockPriceAsync(CommandContext ctx, string symbol)
        {
            await this._stockPriceChecker.SendAsync(symbol, ctx.RespondAsync);
        }

        [Command("automove")]
        public async Task SetAutoMoveAsync(CommandContext ctx, string onOff)
        {
            var member = await this._unitOfWork.MemberRepository.FindOrCreateAsync(ctx.Member.Id);
            switch (onOff.ToLowerInvariant())
            {
                case "on":
                    member.AutoMoveToChannel = true;
                    break;
                case "off":
                    member.AutoMoveToChannel = false;
                    break;
                default:
                    await ctx.RespondAsync($"ออพชัน {onOff} ไม่รองรับในตอนนี้ (คีย์เวิร์ดที่รองรับ on,off)");
                    return;
            }
            await this._unitOfWork.SaveChangesAsync();
            await ctx.Channel.SendDisposableMessageAsync($"สถานะเคลื่อนย้ายอัตโนมัติของ {ctx.Member.Mention} ได้ถูกเปลี่ยนเป็น {(member.AutoMoveToChannel ? "เปิดใช้งาน" : "ปิดใช้งาน")} แล้ว");
        }

        [RequireOwner]
        [Command("dumpdb")]
        public async Task GetDatabaseDumpFile(CommandContext ctx)
        {
            File.Copy("Local.db", "Copy_of_Local.db", true);

            if (ctx.Member == null)
            {
                await ctx.Channel.SendFileAsync("Copy_of_Local.db");
            }
            else
            {
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendFileAsync("Copy_of_Local.db");
            }
            File.Delete("Copy_of_Local.db");
        }

        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        [Command("clean")]
        public async Task CleanMessagesAsync(CommandContext ctx, ulong textChannelId, string type = "bot")
        {
            try
            {
                var channel = await ctx.Client.GetChannelAsync(textChannelId);
                if (channel.Type != DSharpPlus.ChannelType.Text)
                {
                    await ctx.RespondAsync($"Channel {textChannelId} is not text channel, thus it contains no message.");
                    return;
                }
                var messages = await channel.GetMessagesAsync(10000);
                foreach (var message in messages)
                {
                    if ((type == "bot" && message.Author.IsBot) || type == "all" || message.Content.StartsWith(this.configurations.CommandPrefix))
                    {
                        await message.DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
