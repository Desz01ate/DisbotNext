using DisbotNext.DiscordClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext
{
    public class Application
    {
        private readonly DisbotNextClient client;

        public Application(DisbotNextClient client)
        {
            this.client = client;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await this.client.ConnectAsync();
                foreach (var channel in this.client.Channels)
                {
                    await channel.SendMessageAsync("ดีจ้า");
                }
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
