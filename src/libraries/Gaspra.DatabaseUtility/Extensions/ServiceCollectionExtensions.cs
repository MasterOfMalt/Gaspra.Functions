using Gaspra.DatabaseUtility.Factories;
using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Sections;
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

            serviceCollection
                .AddSingleton<IScriptLineFactory, ScriptLineFactory>()
                .AddSingleton<IScriptFactory, ScriptFactory>()
                .AddSingleton<IScriptSection, SettingsSection>()
                .AddSingleton<IScriptSection, AboutSection>()
                .AddSingleton<IScriptSection, DropProcedureSection>()
                .AddSingleton<IScriptSection, DropTableTypeSection>()
                .AddSingleton<IScriptSection, CreateTableTypeSection>()
                .AddSingleton<IScriptSection, CreateProcedureSection>()
                .AddSingleton<IScriptSection, AlterProcedureSection>()
                .AddSingleton<IScriptSection, ProcedureSection>()
                .AddSingleton<IScriptSection, EndProcedureSection>()
                ;


            //todo; register all instances of IScriptSection automagically
            //serviceCollection.Add(ServiceDescriptor item)

            return serviceCollection;
        }
    }
}
