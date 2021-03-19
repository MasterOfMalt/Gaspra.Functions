using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class InsertValuesSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 1 });

        public InsertValuesSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            return Task.FromResult(!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any());
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var insertValues = new List<string>
            {
                "DECLARE @InsertedValues TABLE (",
                $"    [{variables.Table.Name}Id] [int],"
            };

            var columnLines = variables.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name)));

            foreach(var columnLine in columnLines)
            {
                var line = $"    [{columnLine.Name}] {DataType(columnLine)}";

                if (columnLine != columnLines.Last())
                {
                    line += ",";
                }

                insertValues.Add(line);
            }

            insertValues.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                insertValues.ToArray()
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
