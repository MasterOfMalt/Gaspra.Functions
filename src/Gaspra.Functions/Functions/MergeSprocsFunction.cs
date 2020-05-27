using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.Functions.Correlation;
using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Bases
{
    public class MergeSprocsFunction : IFunction
    {
        private readonly ILogger logger;
        private readonly IMergeSprocsService mergeSprocsService;

        private string connectionString = "";

        public MergeSprocsFunction(
            ILogger<MergeSprocsFunction> logger,
            IMergeSprocsService mergeSprocsService)
        {
            this.logger = logger;
            this.mergeSprocsService = mergeSprocsService;
        }

        public IEnumerable<string> FunctionAliases => new[] { "mergesprocs", "ms" };

        public string FunctionHelp => "json db.";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters)
        {
            if(!parameters.Any())
            {
                return false;
            }

            var connectionStringParameter = parameters
                .Where(p => p.Key.Equals("c"))
                .FirstOrDefault();

            if(connectionStringParameter == null || !connectionStringParameter.Values.Any())
            {
                return false;
            }

            connectionString = connectionStringParameter.Values.First().ToString();

            return true;
        }

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            var mergeSprocs = await mergeSprocsService.GenerateMergeSprocs(
                connectionString,
                new[] { "Analytics" }
                );

            foreach(var mergeSproc in mergeSprocs)
            {
                if(mergeSproc.Statement.TryWriteFile($"{mergeSproc.Variables.SchemaName}.{mergeSproc.Variables.ProcedureName}.sql"))
                {
                    logger.LogInformation($"File written: {mergeSproc.Variables.SchemaName}.{mergeSproc.Variables.ProcedureName}.sql");
                }
                else
                {
                    logger.LogError($"File failed to write: {mergeSproc.Variables.SchemaName}.{mergeSproc.Variables.ProcedureName}.sql");
                }
            }
        }
    }
}
