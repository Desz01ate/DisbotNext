using DisbotNext.DiscordClient;
using DisbotNext.Infrastructure.Common;
using Hangfire;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext
{
    public class Application : IHostedService
    {
        private readonly DisbotNextClient _client;

        private readonly UnitOfWork _unitOfWork;

        public Application(DisbotNextClient client,
                           UnitOfWork unitOfWork)
        {
            this._client = client;
            this._unitOfWork = unitOfWork;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this._client.ConnectAsync();
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this._client.DisposeAsync();
        }
    }
}
