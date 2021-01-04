using Gaspra.Functions.Correlation;
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
            #if DEBUG
                await SetFunction(cancellationToken);
            #endif

            await ProcessFunction(cancellationToken);

            hostApplicationLifetime.StopApplication();
        }

        private Task SetFunction(CancellationToken cancellationToken)
        {
            logger.LogDebug("Type a function to use:");

            var function = Console.ReadLine();

            var inputtingParameters = true;

            var parameters = new List<FunctionParameter>();

            while (inputtingParameters)
            {
                logger.LogDebug("Type a parameter key (type # when you're done):");

                var parameterKey = Console.ReadLine();

                if (!parameterKey.Equals("#"))
                {
                    logger.LogDebug("Type [{parameterKey}] value:", parameterKey);

                    var parameterValue = Console.ReadLine();

                    parameters.Add(new FunctionParameter(parameterKey, new[] { parameterValue }));
                }
                else
                {
                    inputtingParameters = false;
                }
            }

            cxt.FunctionName = function;

            cxt.FunctionParameters = parameters;

            return Task.CompletedTask;
        }

        private async Task ProcessFunction(CancellationToken cancellationToken)
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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Finished [{requestedFunction}] in [{executionTime}]",
                cxt.FunctionName,
                DateTimeOffset.UtcNow - cxt.FunctionTimestamp);

            return Task.CompletedTask;
        }
    }
}
