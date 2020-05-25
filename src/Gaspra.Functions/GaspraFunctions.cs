using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions
{
    public class GaspraFunctions : IHostedService
    {
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IEnumerable<IFunction> functions;
        private readonly ICorrelationContext cxt;

        public GaspraFunctions(
            ILogger<GaspraFunctions> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IEnumerable<IFunction> functions,
            ICorrelationContext cxt)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.functions = functions;
            this.cxt = cxt;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var function = functions
                .Where(f => f.GetType().Name.Equals(cxt.Function))
                .FirstOrDefault();

            if (function == null)
            {
                logger.LogError("Function [{requestedFunction}] was not found, please choose from: [{availableFunctions}]",
                    cxt.Function,
                    functions.Select(f => f.GetType().Name));
            }
            else
            {
                logger.LogInformation("Executing [{requestedFunction}]",
                    cxt.Function);

                await function.Run();
            }

            hostApplicationLifetime
                .StopApplication();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Finished [{requestedFunction}] in [{executionTime}]",
                cxt.Function,
                DateTimeOffset.UtcNow - cxt.Timestamp);
        }
    }
}
