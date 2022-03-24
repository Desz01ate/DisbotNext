using DisbotNext.Infrastructures.Common;
using DisbotNext.Infrastructures.Common.Models;
using DSharpPlus.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient
{
    public partial class DisbotNextClient
    {
        public async Task SendDailyReportAsync()
        {
            await this.semaphore.WaitAsync();

            try
            {
                var channels = this.Channels.Where(x => x.Name == "daily-report" && x.Type == DSharpPlus.ChannelType.Text);
                foreach (var channel in channels)
                {
                    await this._covidMessageMediator.SendAsync("thailand", channel.SendMessageAsync);
                }
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task DeleteTempChannels()
        {
            await this.semaphore.WaitAsync();

            try
            {
                await CommonTasks.DeleteTempChannelsAsync(this.Client, this._unitOfWork);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task ReconnectAsync()
        {
            await this.semaphore.WaitAsync();

            try
            {
                await this.Client.ReconnectAsync(true);
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}