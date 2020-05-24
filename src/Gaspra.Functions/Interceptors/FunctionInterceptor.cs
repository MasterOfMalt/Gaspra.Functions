using ConsoleAppFramework;
using Gaspra.Functions.Correlation.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gaspra.Functions.Interceptors
{
    public class FunctionInterceptor : IConsoleAppInterceptor
    {
        private ICorrelationContext cxt;

        public async ValueTask OnEngineBeginAsync(
            IServiceProvider serviceProvider,
            ILogger<ConsoleAppEngine> logger)
        {
            cxt = (ICorrelationContext)serviceProvider.GetService(typeof(ICorrelationContext));

            await Task.Run(() =>
            {
                logger.LogInformation("task OnEngineBeginAsync {cxt.CorrelationId}",
                    cxt.CorrelationId);
            });
        }

        public async ValueTask OnEngineCompleteAsync(IServiceProvider serviceProvider, ILogger<ConsoleAppEngine> logger)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("task OnEngineCompleteAsync {cxt.CorrelationId}",
                    cxt.CorrelationId);
            });
        }

        public async ValueTask OnMethodBeginAsync(ConsoleAppContext context)
        {
            await Task.Run(() =>
            {
                context.Logger.LogInformation("task OnMethodBeginAsync {cxt.CorrelationId}",
                    cxt.CorrelationId);
            });
        }

        public async ValueTask OnMethodEndAsync(ConsoleAppContext context, string errorMessageIfFailed, Exception exceptionIfExists)
        {
            await Task.Run(() =>
            {
                context.Logger.LogInformation("task OnMethodEndAsync {cxt.CorrelationId}",
                    cxt.CorrelationId);
            });
        }
    }
}
