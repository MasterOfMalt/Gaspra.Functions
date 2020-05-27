using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.DatabaseProcesses.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupDatabaseProcesses(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IDatabaseProcessesService, DatabaseProcessesService>();

            return serviceCollection;
        }
    }
}
