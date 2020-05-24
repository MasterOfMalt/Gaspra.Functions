using ConsoleAppFramework;
using Gaspra.DatabaseUtility.Extensions;
using Gaspra.Functions.Correlation.Extensions;
using Gaspra.Functions.Interceptors;
using Gaspra.Logging.Builder;
using Gaspra.Pseudo.Extensions;
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
                .RunConsoleAppFrameworkAsync(
                    args,
                    new CompositeConsoleAppInterceptor(new [] {
                        new FunctionInterceptor()
                    }));
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
                    .SetupCorrelationContext()
                    .SetupPseudo()
                    .SetupDatabaseUtility();
            });
    }
}
