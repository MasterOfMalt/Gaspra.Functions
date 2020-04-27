using Gaspra.Logging.Builder;
using Gaspra.MergeSprocs.DataAccess;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.Signing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((host, config) =>
            {
                host
                    .HostingEnvironment.EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                config
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json");
            })
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
                    .RegisterSecretSigningCertificateOptionFromConfiguration(host.Configuration)
                    .RegisterSigningServices()
                    .AddSingleton<IDataAccess, AnalyticsDataAccess>()
                    .AddHostedService<MergeSprocsService>();
            });
    }
}
