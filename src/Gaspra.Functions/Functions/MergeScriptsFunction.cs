using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gaspra.Functions.Correlation;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using Gaspra.SqlGenerator.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gaspra.Functions.Functions
{
    public class MergeScriptsFunction : IFunction
    {
        private readonly ILogger _logger;
        private readonly IMergeScriptGenerator _mergeScriptGenerator;

        private string _connectionString = "";
        private IList<string> _schemas = new List<string>();

        public MergeScriptsFunction(
            ILogger<MergeScriptsFunction> logger,
            IMergeScriptGenerator mergeScriptGenerator)
        {
            _logger = logger;
            _mergeScriptGenerator = mergeScriptGenerator;
        }

        public IReadOnlyCollection<string> Aliases => new[] { "mergescripts", "ms" };

        public IReadOnlyCollection<IFunctionParameter> Parameters => new List<IFunctionParameter>
        {
            new FunctionParameter("c", null, false, "Connection string"),
            new FunctionParameter("s", null, true, "Schemas to generate merge stored procedures for, comma delimited")
        };

        public string About => "Merge stored procedure generator, will traverse the given database and schema to figure out a tree of dependencies before writing the SQL scripts";

        public bool ValidateParameters(IReadOnlyCollection<IFunctionParameter> parameters)
        {
            if(!parameters.Any())
            {
                return false;
            }

            var connectionStringParameter = parameters
                .FirstOrDefault(p => p.Key.Equals("c"));

            if(connectionStringParameter == null || !connectionStringParameter.Values.Any())
            {
                return false;
            }

            _connectionString = connectionStringParameter.Values.First().ToString();

            var schemas = parameters
                .FirstOrDefault(p => p.Key.Equals("s"));

            if (schemas != null && schemas.Values.Any())
            {
                var schemaList = schemas.Values.First().ToString();

                _schemas = schemaList?.Split(",").Select(s => s.Trim()).ToList();
            }

            return true;
        }

        public async Task Run(CancellationToken cancellationToken, IReadOnlyCollection<IFunctionParameter> parameters)
        {
            var scripts = await _mergeScriptGenerator.Generate(
                _connectionString,
                _schemas.ToList()
                );

            foreach(var script in scripts)
            {
                if(script.Script.TryWriteFile($"{script.Name}"))
                {
                    _logger.LogInformation($"File written: {script.Name}");
                }
                else
                {
                    _logger.LogError($"File failed to write: {script.Name}");
                }
            }
        }
    }
}
