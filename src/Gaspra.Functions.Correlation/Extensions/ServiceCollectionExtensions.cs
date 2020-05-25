using Gaspra.Functions.Correlation.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gaspra.Functions.Correlation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupCorrelationContext(this IServiceCollection serviceCollection, string[] args)
        {
            serviceCollection
                .AddSingleton<ICorrelationContext>((service) =>
                {
                    return new CorrelationContext(args);
                });

            return serviceCollection;
        }
    }
}
