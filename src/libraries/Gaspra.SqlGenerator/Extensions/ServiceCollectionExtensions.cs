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
                .AddSingleton<IScriptFactory<IMergeScriptVariableSet>, MergeScriptFactory>()
                .AddSingleton<IScriptFactory<IDeltaScriptVariableSet>, DeltaScriptFactory>()
                .AddSingleton<IScriptVariableFactory, ScriptVariableFactory>()
                .AddSingleton<IScriptLineFactory, ScriptLineFactory>()
                .AddSingleton<IMergeScriptGenerator, MergeScriptGenerator>()
                .AddSingleton<IDatabaseToJsonGenerator, DatabaseToJsonGenerator>()
                .AddSingleton<IDeltaScriptGenerator, DeltaScriptGenerator>();

            var mergeScriptSections = Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t => t
                    .GetInterfaces()
                    .Contains(typeof(IScriptSection<IMergeScriptVariableSet>)))
                .Select(t => new ServiceDescriptor(
                    typeof(IScriptSection<IMergeScriptVariableSet>),
                    t,
                    ServiceLifetime.Singleton));

            foreach(var mergeScriptSection in mergeScriptSections)
            {
                serviceCollection.Add(mergeScriptSection);
            }

            var deltaScriptSections = Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t => t
                    .GetInterfaces()
                    .Contains(typeof(IScriptSection<IDeltaScriptVariableSet>)))
                .Select(t => new ServiceDescriptor(
                    typeof(IScriptSection<IDeltaScriptVariableSet>),
                    t,
                    ServiceLifetime.Singleton));

            foreach(var deltaScriptSection in deltaScriptSections)
            {
                serviceCollection.Add(deltaScriptSection);
            }

            return serviceCollection;
        }
    }
}
