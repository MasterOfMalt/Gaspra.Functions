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
    public class NotMatchedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2, 1 });

        public NotMatchedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var mergeStatement = new List<string>
            {
                $"WHEN NOT MATCHED BY TARGET",
                $"    THEN INSERT ("
            };

            var insertColumns = variables.Table.Columns.Where(c => !c.IdentityColumn);

            foreach (var column in insertColumns)
            {
                var line = $"        [{column.Name}]";

                if (column != insertColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            mergeStatement.Add("    VALUES (");

            foreach (var column in insertColumns)
            {
                var line = $"        s.[{column.Name}]";

                if (column != insertColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

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
