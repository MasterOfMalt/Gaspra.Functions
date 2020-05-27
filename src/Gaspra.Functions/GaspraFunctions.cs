using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEnumerable<string> HelpParameters = new[] { "h", "help" };

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
                .Where(f => f.FunctionAliases.Any(f => f.Equals(cxt.FunctionName, StringComparison.InvariantCultureIgnoreCase)))
                .FirstOrDefault();

            if (function == null)
            {
                logger.LogError("Function [{requestedFunction}] was not found, please choose from: {availableFunctions}",
                    cxt.FunctionName,
                    functions.Select(f => $"[{string.Join(", ", f.FunctionAliases)}]")
                    );
            }
            else
            {
                if (cxt
                    .FunctionParameters
                    .Any(p => HelpParameters
                        .Any(h => h.Equals(p.Key, StringComparison.InvariantCultureIgnoreCase))))
                {
                    logger.LogInformation(function.FunctionHelp);
                }
                else
                {
                    if (function.ValidateParameters(cxt.FunctionParameters))
                    {
                        logger.LogInformation("Executing [{requestedFunction}] (ctrl+c to exit)",
                            cxt.FunctionName);

                        Console.CancelKeyPress += (sender, e) =>
                        {
                            cxt.FunctionCancellationSource
                                .Cancel();
                        };

                        await function.Run(cxt.FunctionCancellationSource.Token, cxt.FunctionParameters);
                    }
                    else
                    {
                        logger.LogWarning("Function [{requestedFunction}] parameters are invalid so it will not run.",
                            cxt.FunctionName);
                    }
                }
            }


            hostApplicationLifetime
                .StopApplication();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Finished [{requestedFunction}] in [{executionTime}]",
                cxt.FunctionName,
                DateTimeOffset.UtcNow - cxt.FunctionTimestamp);
        }
    }
}
