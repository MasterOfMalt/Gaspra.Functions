using ConsoleAppFramework;
using Gaspra.Logging.Builder;
using Gaspra.MergeSprocs.DataAccess;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .RunConsoleAppFrameworkAsync<MergeSprocsService>(args);
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
                    .AddSingleton<IDataAccess, AnalyticsDataAccess>();
            });
    }
}
