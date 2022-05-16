using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Delta.Procedure
{
    public class DeltaProcedureSection : IScriptSection<IDeltaScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1 });

        public DeltaProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IDeltaScriptVariableSet variableSet)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(variableSet.Schema.Name));
        }

        public async Task<string> Value(IDeltaScriptVariableSet variableSet)
        {
            var script = new List<string>
            {
                $"DECLARE @{variableSet.DomainIdentifierName} TABLE ({variableSet.DomainIdentifierName} INT INDEX IX_{variableSet.DomainIdentifierName} CLUSTERED)",
                $""
            };

            var targetTable = variableSet.TablePaths.First().Last();

            var selectColumns = targetTable.MergeIdentifierColumns(variableSet.Schema);

            foreach (var tablePath in variableSet.TablePaths)
            {
                var rootTable = tablePath.First();

                var selectStatement = new List<string>
                {
                    $"IF NOT EXISTS (SELECT 1 FROM @{variableSet.TableTypeVariableName} WHERE Ignore='{rootTable.Name}')",
                    $"BEGIN",
                    $"    INSERT INTO",
                    $"        @{variableSet.DomainIdentifierName}",
                    $"    SELECT"
                };

                selectStatement.AddRange(from selectColumn in selectColumns
                    let lastColumn = selectColumns.Last().Equals(selectColumn) ? "" : ","
                    select $"        {targetTable.Name}.{selectColumn.Name}{lastColumn}");

                selectStatement.Add($"    FROM");

                var joinStatement = new List<string>
                {
                    $"        {variableSet.Schema.Name}.{variableSet.DeltaSourceTableName} AS {variableSet.DeltaSourceTableName}",
                    $"        INNER JOIN {variableSet.Schema.Name}.{rootTable.Name} AS {rootTable.Name} ON {variableSet.DeltaSourceTableName}.TableName='{rootTable.Name}' AND {variableSet.DeltaSourceTableName}.PrimaryKeyValue={rootTable.Name}.{rootTable.IdentityColumnName()}"
                };

                for (var j = 1; j < tablePath.Count; j++)
                {
                    var tablePathList = tablePath.ToList();

                    var previousTable = tablePathList[j - 1];

                    var previousTableConstraintColumns =
                        previousTable.Columns.Where(c => c.Constraints != null && c.Constraints.Any());

                    var currentTable = tablePathList[j];

                    var currentTableConstraintColumns =
                        currentTable.Columns.Where(c => c.Constraints != null && c.Constraints.Any());

                    var innerJoinColumns = previousTableConstraintColumns.Where(ptc =>
                        currentTableConstraintColumns.Any(ctc => ctc.Name.Equals(ptc.Name)));

                    var innerJoinStatement =
                        $"        INNER JOIN {variableSet.Schema.Name}.{currentTable.Name} AS {currentTable.Name} ON";

                    foreach (var innerJoinColumn in innerJoinColumns)
                    {
                        innerJoinStatement +=
                            $" {previousTable.Name}.{innerJoinColumn.Name}={currentTable.Name}.{innerJoinColumn.Name}";
                    }

                    joinStatement.Add(innerJoinStatement);
                }

                var whereStatement = new List<string>
                {
                    $"    WHERE",
                    $"        {variableSet.DeltaSourceTableName}.Timestamp >= @Delta",
                    $"END",
                    $""
                };

                script.AddRange(selectStatement);

                script.AddRange(joinStatement);

                script.AddRange(whereStatement);
            }

            script.AddRange(new List<string>
            {
                $"SELECT DISTINCT"
            });

            script.AddRange(from selectColumn in selectColumns
                let lastColumn = selectColumns.Last().Equals(selectColumn) ? "" : ","
                select $"    {selectColumn.Name}{lastColumn}");

            script.AddRange(new List<string>
            {
                $"FROM",
                $"    @{variableSet.DomainIdentifierName}"
            });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
            );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
