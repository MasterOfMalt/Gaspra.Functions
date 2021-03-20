using Gaspra.Functions.Correlation;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.Functions.Bases
{
    public class MergeSprocsFunction : IFunction
    {
        private readonly ILogger _logger;
        private readonly IMergeScriptGenerator _mergeScriptGenerator;

        private string connectionString = "";

        public MergeSprocsFunction(
            ILogger<MergeSprocsFunction> logger,
            IMergeScriptGenerator mergeScriptGenerator)
        {
            _logger = logger;
            _mergeScriptGenerator = mergeScriptGenerator;
        }

        public IEnumerable<string> FunctionAliases => new[] { "mergesprocs", "ms" };

        public string FunctionHelp => "Merge sprocs";

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
            var scripts = await _mergeScriptGenerator.Generate(
                connectionString,
                new[] { "Analytics" }
                );

            foreach(var script in scripts)
            {
                if(script.Script.TryWriteFile($"{script.Name}.sql"))
                {
                    _logger.LogInformation($"File written: {script.Name}.sql");
                }
                else
                {
                    _logger.LogError($"File failed to write: {script.Name}.sql");
                }
            }
        }
    }
}
