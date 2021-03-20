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
    public class RetentionSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2, 3 });

        public RetentionSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(variables.RetentionPolicy != null);
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var mergeStatement = new List<string>
            {
                $"WHEN NOT MATCHED BY SOURCE AND t.{variables.RetentionPolicy.ComparisonColumn} < DATEADD(MONTH, -{variables.RetentionPolicy.RetentionMonths}, GETUTCDATE())",
                "    THEN DELETE"
            };

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
