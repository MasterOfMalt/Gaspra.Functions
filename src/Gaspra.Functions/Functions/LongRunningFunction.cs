using Gaspra.Functions.Correlation;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    public class LongRunningFunction : IFunction
    {
        private readonly ILogger logger;
        public LongRunningFunction(ILogger<LongRunningFunction> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<string> FunctionAliases => new[] { "lr" };

        public string FunctionHelp => "long running";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters) => true;

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            var count = 0;

            while(!cancellationToken.IsCancellationRequested)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(500);

                    logger.LogInformation("Long running... [{count}]", ++count);
                });
            }
        }
    }
}
