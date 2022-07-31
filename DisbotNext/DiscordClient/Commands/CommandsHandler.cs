using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Common.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.IO;
using DisbotNext.ExternalServices.OilPriceChecker;
using DisbotNext.Common.Configurations;
using DisbotNext.ExternalServices.Financial.Stock;
using DisbotNext.Infrastructures.Common;
using DisbotNext.Interfaces;
using DisbotNext.Helpers;

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        private readonly UnitOfWork unitOfWork;
        private readonly DiscordConfigurations configurations;
        private readonly IMessageMediator<ICovidTracker> covidTracker;
        private readonly IOilPriceMessageMediator oilPriceChecker;
        private readonly IMessageMediator<IStockPriceChecker> stockPriceChecker;

        public CommandsHandler(UnitOfWork unitOfWork,
                               DiscordConfigurations configurations,
                               IMessageMediator<ICovidTracker> covidTracker,
                               IOilPriceMessageMediator oilPriceChecker,
                               IMessageMediator<IStockPriceChecker> stockPriceChecker)
        {
            this.unitOfWork = unitOfWork;
            this.configurations = configurations;
            this.covidTracker = covidTracker;
            this.oilPriceChecker = oilPriceChecker;
            this.stockPriceChecker = stockPriceChecker;
        }

        [Command("level")]
        public async Task GetLevelAvatar(CommandContext ctx)
        {
            var member = await this.unitOfWork.MemberRepository.FindOrCreateAsync(ctx.Member.Id);
            var avatar = await AvatarHelpers.GetLevelUpAvatarPathAsync(ctx.Member.AvatarUrl, member.Level);
            await ctx.SendFileAsync(avatar, true);
            await ctx.RespondAsync($"Exp {member.Exp}/{member.NextExp}");
        }

        [Command("test")]
        public async Task Test(CommandContext ctx, [RemainingText] string txt)
        {
            await ctx.RespondAsync(txt);
        }

        [Command("covid")]
        public async Task GetCovidDetail(CommandContext ctx, [RemainingText] string country = "thailand")
        {
            await this.covidTracker.SendAsync(country, ctx.RespondAsync);
        }

        [Command("oilprice")]
        public async Task GetOilPriceAsync(CommandContext ctx)
        {
            await this.oilPriceChecker.SendAsync(null, ctx.RespondAsync);
        }

        [Command("stock")]
        public async Task GetStockPriceAsync(CommandContext ctx, string symbol)
        {
            await this.stockPriceChecker.SendAsync(symbol, ctx.RespondAsync);
        }

        [Command("automove")]
        public async Task SetAutoMoveAsync(CommandContext ctx, string onOff)
        {
            var member = await this.unitOfWork.MemberRepository.FindOrCreateAsync(ctx.Member.Id);

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

            await this.unitOfWork.SaveChangesAsync();
            await ctx.Channel.SendDisposableMessageAsync($"สถานะเคลื่อนย้ายอัตโนมัติของ {ctx.Member.Mention} ได้ถูกเปลี่ยนเป็น {(member.AutoMoveToChannel ? "เปิดใช้งาน" : "ปิดใช้งาน")} แล้ว");
        }

        [RequireOwner]
        [Command("dumpdb")]
        public async Task GetDatabaseDumpFile(CommandContext ctx)
        {
            File.Copy("persistent/Local.db", "Copy_of_Local.db", true);

            if (ctx.Member == null)
            {
                await ctx.Channel.SendFileAsZipAsync("Copy_of_Local.db");
            }
            else
            {
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendFileAsZipAsync("Copy_of_Local.db");
            }

            File.Delete("Copy_of_Local.db");
        }

        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        [Command("clean")]
        public async Task CleanMessagesAsync(CommandContext ctx, ulong? textChannelId = null, string type = "bot")
        {
            try
            {
                DiscordChannel channel;

                if (!textChannelId.HasValue)
                {
                    channel = ctx.Channel;
                }
                else
                {
                    channel = await ctx.Client.GetChannelAsync(textChannelId.Value);
                }

                if (channel.Type != DSharpPlus.ChannelType.Text)
                {
                    await ctx.RespondAsync($"Channel {textChannelId} is not text channel, thus it contains no message.");
                    return;
                }

                var messages = await channel.GetMessagesAsync(100);

                foreach (var message in messages)
                {
                    if ((type == "bot" && message.Author.IsBot) ||
                         type == "all" ||
                         message.Content.StartsWith(this.configurations.CommandPrefix))
                    {
                        await message.DeleteAsync();
                        await Task.Delay(10);
                    }
                }

                await ctx.SendDisposableMessageAsync("Messages deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        [Command("flush-channel")]
        public async Task FlushChannelAsync(CommandContext ctx, string? groupIdString = default)
        {

            if (!string.IsNullOrWhiteSpace(groupIdString) && Guid.TryParse(groupIdString, out var groupId))
            {
                await CommonTasks.DeleteSpecificGroupAsync(groupId, ctx.Client, unitOfWork);
            }
            else
            {
                await CommonTasks.DeleteTempChannelsAsync(ctx.Client, unitOfWork, true);
            }
        }

        [RequireOwner]
        [Command("reconnect")]
        public async Task ReconnectAsync(CommandContext ctx)
        {
            var client = ctx.Client;

            await client.ReconnectAsync(true);
        }
    }
}
