using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;
using Microsoft.Extensions.Logging;

namespace Gaspra.SqlGenerator.Services
{
    public class MergeScriptGenerator : IMergeScriptGenerator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseStructure _databaseStructure;
        private readonly IDataAccess _dataAccess;
        private readonly IScriptVariableFactory _scriptVariableFactory;
        private readonly IScriptFactory _scriptFactory;

        public MergeScriptGenerator(
            ILogger<MergeScriptGenerator> logger,
            IDataAccess dataAccess,
            IDatabaseStructure databaseStructure,
            IScriptVariableFactory scriptVariableFactory,
            IScriptFactory scriptFactory)
        {
            _logger = logger;
            _databaseStructure = databaseStructure;
            _dataAccess = dataAccess;
            _scriptFactory = scriptFactory;
            _scriptVariableFactory = scriptVariableFactory;
        }

        public async Task<IReadOnlyCollection<MergeScript>> Generate(string connectionString, IReadOnlyCollection<string> schemas)
        {
            // Get the database to generate scripts for
            DatabaseModel database;

            try
            {
                var databaseResult = await _dataAccess.GetDatabase(connectionString, schemas);

                var databaseName = connectionString.DatabaseName();

                database = await _databaseStructure.CalculateStructure(databaseName, databaseResult);

                _logger.LogInformation(
                    "Constructed [{databaseName}] database object with [{tableCount}] tables",
                    databaseName,
                    database.Schemas.SelectMany(s => s.Tables).Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to get database: [{databaseName}]",
                    connectionString.DatabaseName());

                return null;
            }

            // Build up the script variables sets
            var scriptVariableSets = new List<ScriptVariableSet>();

            try
            {
                scriptVariableSets = await _scriptVariableFactory.VariablesFrom(database);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to generate script variable sets");

                return null;
            }

            // Generate merge scripts from the variable sets
            var mergeScripts = new List<MergeScript>();

            try
            {
                foreach (var scriptVariableSet in scriptVariableSets)
                {
                    var script = await _scriptFactory.ScriptFrom(scriptVariableSet);

                    var mergeScript = new MergeScript(scriptVariableSet.ScriptName, script);

                    mergeScripts.Add(mergeScript);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to generate scripts");

                return null;
            }

            // Return scripts
            return mergeScripts;
        }
    }
}
