using Gaspra.DatabaseUtility.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.DatabaseUtility.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupDatabaseUtility(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IDataAccess, DataAccess>()
                .AddSingleton<IJsonDatabaseService, JsonDatabaseService>();

            return serviceCollection;
        }
    }
}
