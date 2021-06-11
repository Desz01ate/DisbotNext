using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace DisbotNext
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Bootstrap.PrintGraffiti();
            var host = Bootstrap.CreateHostBuilder(args).Build();
            Bootstrap.ApplyMigrations(host);
            await host.RunAsync();
        }
    }
}
