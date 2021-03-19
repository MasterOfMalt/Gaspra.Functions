﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class TargetSourceSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2, 0});

        public TargetSourceSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var usingType = (variables.TablesToJoin != null && variables.TablesToJoin.Any()) ? $"{variables.ProcedureName}Variable" : $"{variables.TableTypeVariableName()}";

            var mergeStatement = new List<string>
            {
                $"MERGE [{variables.SchemaName}].[{variables.Table.Name}] AS t",
                $"USING @{usingType} AS s",
                $"ON ("
            };

            foreach (var match in matchOn)
            {
                var line = $"    t.[{match}]=s.[{match}]";

                if (match != matchOn.Last())
                {
                    line += " AND";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }


        private static string DataType(Column column)
        {
            var dataType = $"[{column.DataType}]";

            if (column.DataType.Equals("decimal") && column.Precision.HasValue && column.Scale.HasValue)
            {
                dataType += $"({column.Precision.Value},{column.Scale.Value})";
            }
            else if (column.MaxLength.HasValue)
            {
                dataType += $"({column.MaxLength.Value})";
            }

            return dataType;
        }

        private static string NullableColumn(Column column)
        {
            return column.Nullable ? "NULL" : "NOT NULL";
        }
    }
}
