using Gaspra.Functions.Correlation;
using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    public class SampleFunction : IFunction
    {
        private readonly ILogger logger;
        private readonly ICorrelationContext cxt;

        public SampleFunction(
            ILogger<SampleFunction> logger,
            ICorrelationContext cxt)
        {
            this.logger = logger;
            this.cxt = cxt;
        }

        public IEnumerable<string> FunctionAliases => new List<string> { nameof(SampleFunction), "sf" };

        public string FunctionHelp => "Sample function, requires any parameter be supplied.";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters)
        {
            var hasParameters = parameters.Any();

            if(!hasParameters)
            {
                logger.LogWarning("There are no parameters");
            }

            return hasParameters;
        }

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            logger.LogInformation("Function: [{cxtFunction}] with correlation id: [{correlationId}]",
                cxt.FunctionName,
                cxt.FunctionCorrelationId);

            logger.LogInformation("Parameters: {params}",
                string.Join(", ", cxt.FunctionParameters.Select(p => $"[key: {p.Key} - values: {string.Join(", ", p.Values)}]")));

            logger.LogInformation("Started at: [{time}]",
                cxt.FunctionTimestamp);
        }
    }
}
