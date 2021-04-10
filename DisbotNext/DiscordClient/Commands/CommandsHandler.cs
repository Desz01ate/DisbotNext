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

namespace DisbotNext.DiscordClient.Commands
{
    public class CommandsHandler : BaseCommandModule
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ICovidTracker _covidTracker;
        public CommandsHandler(UnitOfWork unitOfWork,
                               ICovidTracker covidTracker)
        {
            this._unitOfWork = unitOfWork;
            this._covidTracker = covidTracker;
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
            }
            var sb = new StringBuilder();
            sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบวันนี้ {result.TodayCases}");
            sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบทั้งสิ้น {result.Cases}");
            sb.AppendLine($"💀 จำนวนผู้เสียชีวิตวันนี้ {result.TodayDeaths}");
            sb.AppendLine($"💀 จำนวนผู้เสียชีวิตทั้งสิ้น {result.Deaths}");
            sb.AppendLine($"🏨 จำนวนผู้ป่วยอาการวิกฤตทั้งสิ้น {result.Critical}");
            sb.AppendLine($"🏨 จำนวนผู้ป่วยที่กำลังรักษาตัว {result.Active}");
            sb.AppendLine($"👌 จำนวนผู้ป่วยที่รักษาหายแล้วทั้งสิ้น {result.Recovered}");
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"สถานการณ์ของ {result.Country} ณ วันที่ {DateTime.Now.ToString("dd/MM/yyyy")}",
                Description = sb.ToString(),
                Color = new Optional<DiscordColor>(DiscordColor.Red),
            };
            await ctx.RespondAsync(embed);
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
    }
}
