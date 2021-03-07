using Gaspra.DatabaseUtility.Factories;
using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Sections;
using Gaspra.DatabaseUtility.Sections.Procedure;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Gaspra.DatabaseUtility.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupDatabaseUtility(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IDataAccess, DataAccess>()
                .AddSingleton<IMergeSprocsService, MergeSprocsService>();

            serviceCollection
                .AddSingleton<IScriptLineFactory, ScriptLineFactory>()
                .AddSingleton<IScriptFactory, ScriptFactory>()
                ;

            var scriptSections = Assembly.GetExecutingAssembly().DefinedTypes.Where(t => t.GetInterfaces().Contains(typeof(IScriptSection))).Select(t => new ServiceDescriptor(typeof(IScriptSection), t, ServiceLifetime.Singleton));

            foreach(var scriptSection in scriptSections)
            {
                serviceCollection.Add(scriptSection);
            }

            return serviceCollection;
        }
    }
}
