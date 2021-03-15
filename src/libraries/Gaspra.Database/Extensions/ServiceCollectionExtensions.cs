using Gaspra.Database.Interfaces;
using Gaspra.Database.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.Database.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupDatabaseServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IDataAccess, DataAccess>()
                .AddSingleton<IDatabaseStructure, DatabaseStructure>();

            return serviceCollection;
        }
    }
}