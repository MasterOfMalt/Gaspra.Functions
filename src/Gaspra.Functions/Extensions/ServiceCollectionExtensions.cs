using Gaspra.DatabaseUtility.Extensions;
using Gaspra.Functions.Functions;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Gaspra.Functions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterFunctions(this IServiceCollection collection)
        {
            var functionAssembly = typeof(IFunction).Assembly;

            var functionImplementations = functionAssembly
                .GetTypes()
                .Where(t => t
                    .GetInterfaces()
                    .Contains(typeof(IFunction)));

            foreach (var function in functionImplementations)
            {
                collection.Add(
                    new ServiceDescriptor(
                        typeof(IFunction),
                        function,
                        ServiceLifetime.Singleton));
            }

            collection
                .SetupDatabaseUtility()
                .AddSingleton<IHelper, Helper>();

            return collection;
        }
    }
}
