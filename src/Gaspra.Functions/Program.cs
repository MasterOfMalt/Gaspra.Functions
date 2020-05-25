using Gaspra.Functions.Correlation.Extensions;
using Gaspra.Functions.Extensions;
using Gaspra.Logging.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gaspra.Functions
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging((host, logger) =>
            {
                logger
                    .SetMinimumLevel(LogLevel.Debug)
                    .ClearProviders()
                    .AddProviderConsole()
                    .AddFilter("Microsoft", LogLevel.Warning);
            })
            .ConfigureServices((host, services) =>
            {
                services
                    .RegisterFunctions()
                    .SetupCorrelationContext(args)
                    .AddHostedService<GaspraFunctions>();
            });
    }
}
