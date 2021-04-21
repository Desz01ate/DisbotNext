using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace DisbotNext
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Bootstrap.PrintGraffiti();
            await Bootstrap.CreateHostBuilder(args).Build().RunAsync();
        }
    }
}
