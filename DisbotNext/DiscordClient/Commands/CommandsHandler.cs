using DisbotNext.ExternalServices.CovidTracker;
using DisbotNext.Infrastructure.Common;
using DisbotNext.Common.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DisbotNext.ExternalServices.OilPriceChecker;
using System.Linq;
using System.Collections.Generic;
using DisbotNext.Common.Configurations;

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly DiscordConfigurations configurations;
        private readonly ICovidTracker _covidTracker;
        private readonly IOilPriceChecker _oilPriceChecker;
        public CommandsHandler(UnitOfWork unitOfWork,
                               DiscordConfigurations configurations,
                               ICovidTracker covidTracker,
                               IOilPriceChecker oilPriceChecker)
        {
            this._unitOfWork = unitOfWork;
            this.configurations = configurations;
            this._covidTracker = covidTracker;
            this._oilPriceChecker = oilPriceChecker;
        }

        [Command("test")]
        public async Task Test(CommandContext ctx, [RemainingText] string txt)
        {
            await ctx.RespondAsync(txt);
        }

        [Command("covid")]
        public async Task GetCovidDetail(CommandContext ctx, [RemainingText] string country = "thailand")
        {
            var result = await this._covidTracker.GetCovidTrackerDataAsync(country);
            if (result == null)
            {
                await ctx.RespondAsync($"ไม่พบประเทศ '{country}' ในระบบ");
                return;
            }
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"สถานการณ์ Covid-19 ของ {result.Country} ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = result.ToString(),
                Color = new Optional<DiscordColor>(DiscordColor.Red),
            };
            await ctx.RespondAsync(embed);
        }

        [Command("oilprice")]
        public async Task GetOilPriceAsync(CommandContext ctx)
        {
            var prices = await this._oilPriceChecker.GetOilPriceAsync();
            var today = prices.SelectMany(x => x.Types)
                               .Where(x => x.PricePerLitre != null && x.RetailName != "พรุ่งนี้")
                               .GroupBy(x => x.Type)
                               .Select(x => new
                               {
                                   Type = x.Key,
                                   Info = x.OrderBy(y => y.PricePerLitre.Value).First()
                               }).ToArray();

            var tomorrow = prices.SelectMany(x => x.Types)
                               .Where(x => x.PricePerLitre != null && x.RetailName == "พรุ่งนี้")
                               .GroupBy(x => x.Type)
                               .Select(x => new
                               {
                                   Type = x.Key,
                                   Info = x.OrderBy(y => y.PricePerLitre.Value).First()
                               }).ToArray();
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"ราคาน้ำมัน ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = string.Join("\n", today.Select(x => $"{x.Type} : {x.Info.PricePerLitre} บาท/ลิตร ({x.Info.RetailName})")),
                Color = DiscordColor.Green,
            };
            await ctx.RespondAsync(embed.Build());

            var list = new List<string>();
            foreach (var (todayType, tomorrowType) in today.Zip(tomorrow))
            {
                var todayPrice = todayType.Info.PricePerLitre.Value;
                var tomorrowPrice = tomorrowType.Info.PricePerLitre.Value;
                var diff = Math.Round((tomorrowPrice / todayPrice) * 100 - 100, 0);
                var displayDiff = diff == 0 ? "ไม่เปลี่ยนแปลง" : $"{(diff > 0 ? "+" : "")}{diff}%";
                list.Add($"{tomorrowType.Type} : {tomorrowPrice} บาท/ลิตร ({displayDiff})");
            }
            embed.Title = "ราคาน้ำมันวันพรุ่งนี้";
            embed.Description = string.Join("\n", list);
            await ctx.RespondAsync(embed.Build());
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
