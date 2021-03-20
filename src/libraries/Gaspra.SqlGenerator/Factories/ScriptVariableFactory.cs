using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
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
                        var scriptName = $"{schema}.Merge{table.Name}";

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
}
