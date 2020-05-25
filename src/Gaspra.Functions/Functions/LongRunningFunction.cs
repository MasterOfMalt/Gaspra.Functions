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

        public bool ValidateParameters()
        {
            return true;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var count = 0;

            while(!cancellationToken.IsCancellationRequested)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(2000);

                    logger.LogInformation("Long running... [{count}]", ++count);
                });
            }
        }
    }
}
