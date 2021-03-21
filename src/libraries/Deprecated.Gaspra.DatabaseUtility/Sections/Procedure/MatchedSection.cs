using Deprecated.Gaspra.DatabaseUtility.Models.Merge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Interfaces;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Script;

namespace Deprecated.Gaspra.DatabaseUtility.Sections.Procedure
{
    public class MatchedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2, 2 });

        public MatchedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            return Task.FromResult(
                !matchOn.Count().Equals(variables.Table.Columns.Count) &&
                !variables.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase))));
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var updateColumns = variables.Table.Columns.Where(c => !matchOn.Any(m => m.Equals(c.Name, StringComparison.InvariantCultureIgnoreCase))).Where(c => !c.IdentityColumn);

            var mergeStatement = new List<string>
            {
                $"WHEN MATCHED",
                $"    THEN UPDATE SET"
            };

            foreach (var column in updateColumns)
            {
                var line = $"        t.[{column.Name}]=s.[{column.Name}]";

                if (column != updateColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

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
