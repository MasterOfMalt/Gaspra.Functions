using ConsoleAppFramework;
using Gaspra.Logging.Builder;
using Gaspra.Pseudo.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gaspra.Functions
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .RunConsoleAppFrameworkAsync(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging((host, logger) =>
            {
                logger
                    .SetMinimumLevel(LogLevel.Debug)
                    .ClearProviders()
                    .AddProviderConsole();
            })
            .ConfigureServices((host, services) =>
            {
                services
                    .SetupPseudo();
            });
    }
}
