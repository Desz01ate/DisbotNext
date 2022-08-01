using System;
using System.Linq;
using System.Threading.Tasks;

namespace DisbotNext.DiscordClient
{
    public partial class DisbotNextClient
    {
        private static DateTimeOffset? lastOilPriceUpdate = null;

        public async Task SendDailyReportAsync()
        {
            await this.semaphore.WaitAsync();

            try
            {
                var channels =
                    this.Channels.Where(x => x.Name == "daily-report" && x.Type == DSharpPlus.ChannelType.Text);
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

        public async Task CheckOilPricePeriodicallyAsync()
        {
            await this.semaphore.WaitAsync();

            if (lastOilPriceUpdate?.Date == DateTimeOffset.UtcNow)
            {
                this.semaphore.Release();

                return;
            }

            try
            {
                var isPriceChanging = await this._oilPriceMessageMediator.IsPriceChangingAsync();

                if (isPriceChanging)
                {
                    var channels = this.Channels.Where(x =>
                        x.Name == "daily-report" && x.Type == DSharpPlus.ChannelType.Text);

                    foreach (var channel in channels)
                    {
                        await this._oilPriceMessageMediator.SendAsync(null, channel.SendMessageAsync);
                    }
                }

                lastOilPriceUpdate = DateTimeOffset.UtcNow;
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}