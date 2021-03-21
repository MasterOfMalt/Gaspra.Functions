using System.Linq;
using System.Reflection;
using Gaspra.SqlGenerator.Factories;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.SqlGenerator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupSqlGenerator(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IScriptFactory, ScriptFactory>()
                .AddSingleton<IScriptVariableFactory, ScriptVariableFactory>()
                .AddSingleton<IScriptLineFactory, ScriptLineFactory>()
                .AddSingleton<IMergeScriptGenerator, MergeScriptGenerator>();

            var scriptSections = Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t => t
                    .GetInterfaces()
                    .Contains(typeof(IScriptSection)))
                .Select(t => new ServiceDescriptor(
                    typeof(IScriptSection),
                    t,
                    ServiceLifetime.Singleton));

            foreach(var scriptSection in scriptSections)
            {
                serviceCollection.Add(scriptSection);
            }

            return serviceCollection;
        }
    }
}
