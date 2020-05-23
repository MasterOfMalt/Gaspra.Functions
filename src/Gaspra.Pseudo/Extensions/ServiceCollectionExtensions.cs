using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.Pseudo.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupPseudo(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IWrite, Write>();

            return serviceCollection;
        }
    }
}
