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
    public class DatabaseToJsonFunction : IFunction
    {
        private readonly ILogger _logger;
        private readonly IDatabaseToJsonGenerator _databaseToJsonGenerator;

        private string connectionString = "";

        public DatabaseToJsonFunction(
            ILogger<DatabaseToJsonFunction> logger,
            IDatabaseToJsonGenerator databaseToJsonGenerator)
        {
            _logger = logger;
            _databaseToJsonGenerator = databaseToJsonGenerator;
        }

        public IEnumerable<string> FunctionAliases => new[] { "databasetojson", "dtj" };

        public string FunctionHelp => "Database to JSON";

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
            var jsonDatabase = await _databaseToJsonGenerator.Generate(
                connectionString,
                new[] { "Analytics" }
                );

            if (jsonDatabase.TryWriteFile("database.json"))
            {
                _logger.LogInformation($"File written: database");
            }
            else
            {
                _logger.LogError($"File failed to write: database");
            }
        }
    }
}
