using Gaspra.DatabaseUtility.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.DatabaseUtility.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupDatabaseUtility(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IDataAccess, DataAccess>()
                .AddSingleton<IMergeSprocsService, MergeSprocsService>();

            return serviceCollection;
        }
    }
}
