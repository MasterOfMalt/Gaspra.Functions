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
    public class DeltaScriptGenerator : IDeltaScriptGenerator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseStructure _databaseStructure;
        private readonly IDataAccess _dataAccess;
        private readonly IScriptVariableFactory _scriptVariableFactory;
        private readonly IScriptFactory _scriptFactory;

        public DeltaScriptGenerator(
            ILogger<DeltaScriptGenerator> logger,
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

        public async Task<IReadOnlyCollection<SqlScript>> Generate(string connectionString, IReadOnlyCollection<string> schemas)
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

            // Generate delta scripts
            var tables = database.Schemas.First().Tables.ToList();

            var tablesWithProductHistory = tables.Where(t =>
                t.Properties != null &&
                t.Properties.Any(p =>
                    p.Key.Equals("gf.Record", StringComparison.InvariantCultureIgnoreCase) &&
                    p.Value.Equals("HistoryProduct", StringComparison.InvariantCultureIgnoreCase))).ToList();

            var testTable = tablesWithProductHistory.FirstOrDefault(t => t.Name.Equals("ProductHierarchyAttribute"));

            var pathsToRoot = new List<IReadOnlyCollection<TableModel>>();

            var unableToFigureOut = new List<TableModel>();

            foreach (var table in tablesWithProductHistory)
            {
                var pathToRoot = table.PathToRoot();

                if (pathToRoot == null)
                {
                    unableToFigureOut.Add(table);
                }
                else
                {
                    pathsToRoot.Add(pathToRoot);
                }
            }

            var script = $"DECLARE @Ignore TABLE (TableName NVARCHAR(50)){Environment.NewLine}{Environment.NewLine}" +
                         $"DECLARE @Identifier TABLE (AtomProductId INT INDEX IX_AtomProductId CLUSTERED){Environment.NewLine}{Environment.NewLine}" +
                         $"DECLARE @DeltaTime DATETIME = DATEADD(HOUR, -4, GETDATE()){Environment.NewLine}{Environment.NewLine}";

            foreach (var path in pathsToRoot)
            {
                var pathToRoot = path.ToList();

                var selectColumn = pathToRoot.Last().Properties.First(p => p.Key.Equals("MergeIdentifier", StringComparison.InvariantCultureIgnoreCase)).Value;

                var select = $"IF NOT EXISTS (SELECT 1 FROM @Ignore WHERE TableName='{pathToRoot.First().Name}'){Environment.NewLine}BEGIN{Environment.NewLine}" +
                             $"    INSERT INTO{Environment.NewLine}        @Identifier{Environment.NewLine}    SELECT{Environment.NewLine}        {path.Last().Name}.{selectColumn}{Environment.NewLine}    FROM{Environment.NewLine}";

                var joins = $"        Analytics.HistoryProduct AS HistoryProduct{Environment.NewLine}" +
                            $"        INNER JOIN Analytics.{pathToRoot.First().Name} as {pathToRoot.First().Name} ON HistoryProduct.TableName='{pathToRoot.First().Name}' AND HistoryProduct.PrimaryKeyValue={pathToRoot.First().Name}.{pathToRoot.First().IdentityColumnName()}";

                for (var j = 1; j < pathToRoot.Count; j++)
                {
                    var previousTable = pathToRoot[j - 1];

                    var previousTableConstraintColumns = previousTable.Columns.Where(c => c.Constraints != null && c.Constraints.Any());

                    var currentTable = pathToRoot[j];

                    var currentTableConstraintColumns = currentTable.Columns.Where(c => c.Constraints != null && c.Constraints.Any());

                    var joiningColumns = previousTableConstraintColumns.Where(ptc =>
                        currentTableConstraintColumns.Any(ctc => ctc.Name.Equals(ptc.Name)));

                    var joinStatement = $"        INNER JOIN Analytics.{currentTable.Name} AS {currentTable.Name} ON";

                    foreach (var joiningColumn in joiningColumns)
                    {
                        joinStatement +=
                            $" {previousTable.Name}.{joiningColumn.Name}={currentTable.Name}.{joiningColumn.Name}";
                    }

                    joins += $"{Environment.NewLine}{joinStatement}";
                }

                var where = $"{Environment.NewLine}    WHERE" +
                            $"{Environment.NewLine}        HistoryProduct.Timestamp >= @DeltaTime";

                script += select + joins + where + $"{Environment.NewLine}END{Environment.NewLine}{Environment.NewLine}";
            }

            script += $"SELECT DISTINCT AtomProductId FROM @Identifier";

            // Return scripts
            var sqlScripts = new List<SqlScript>();

            return sqlScripts;
        }
    }
}
