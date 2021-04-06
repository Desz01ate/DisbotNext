using Autofac;
using DisbotNext.DependencyInjection;
using DisbotNext.DiscordClient;
using System;
using System.Threading.Tasks;

namespace DisbotNext
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var scope = ConfigurationContainer.Configure();
            var app = scope.Resolve<Application>();
            await app.RunAsync();
        }
    }
}
