using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;
using Microsoft.Extensions.Logging;

namespace Gaspra.SqlGenerator.Factories
{
    public class ScriptVariableFactory : IScriptVariableFactory
    {
        private readonly ILogger _logger;

        public ScriptVariableFactory(ILogger<ScriptVariableFactory> logger)
        {
            _logger = logger;
        }

        public Task<IReadOnlyCollection<IMergeScriptVariableSet>> MergeVariablesFrom(DatabaseModel database)
        {
            var mergeScriptVariableSets = new List<MergeScriptVariableSet>();

            foreach (var schema in database.Schemas)
            {
                foreach (var table in schema.Tables)
                {
                    try
                    {
                        var scriptName = $"Merge{table.Name}";

                        var tableTypeVariableName = $"{table.Name}Variable";

                        var tableTypeName = $"TT_{table.Name}";

                        var tableTypeColumns = table.TableTypeColumns(schema);

                        var mergeIdentifierColumns = table.MergeIdentifierColumns(schema);

                        var deleteIdentifierColumns = table.DeleteIdentifierColumns(schema);

                        var retentionPolicy = table.RetentionPolicy();

                        var tablesToJoin = table.TablesToJoin(schema);

                        var mergeScriptVariableSet = new MergeScriptVariableSet
                        {
                            Schema = schema,
                            Table = table,
                            DeleteIdentifierColumns = deleteIdentifierColumns,
                            MergeIdentifierColumns = mergeIdentifierColumns,
                            RetentionPolicy = retentionPolicy,
                            ScriptName = scriptName,
                            TablesToJoin = tablesToJoin,
                            TableTypeColumns = tableTypeColumns,
                            TableTypeName = tableTypeName,
                            TableTypeVariableName = tableTypeVariableName
                        };

                        mergeScriptVariableSets.Add(mergeScriptVariableSet);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Unable to calculate merge variable set for [{schema}].[{table}]",
                            schema.Name,
                            table.Name
                            );
                    }
                }
            }

            return Task.FromResult((IReadOnlyCollection<IMergeScriptVariableSet>)mergeScriptVariableSets);
        }
    }

    public static class TableModelExtensions
    {
        public static IReadOnlyCollection<ColumnModel> TableTypeColumns(this TableModel table, SchemaModel schema)
        {
            //todo
            return null;
        }

        public static IReadOnlyCollection<ColumnModel> MergeIdentifierColumns(this TableModel table, SchemaModel schema)
        {
            //todo
            return null;
        }

        public static IReadOnlyCollection<ColumnModel> DeleteIdentifierColumns(this TableModel table,
            SchemaModel schema)
        {
            //todo
            return null;
        }

        public static (string ComparisonColumn, string RetentionMonths) RetentionPolicy(this TableModel table)
        {
            //todo
            return (null, null);
        }

        public static
            IReadOnlyCollection<(TableModel joinTable, IReadOnlyCollection<ColumnModel> joinColumns,
                IReadOnlyCollection<ColumnModel> selectColumns)> TablesToJoin(this TableModel table, SchemaModel schema)
        {
            //todo
            return null;
        }
    }
}
