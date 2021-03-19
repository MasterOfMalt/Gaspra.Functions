using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class DeleteInsertedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2, 6 });

        public DeleteInsertedSection(IScriptLineFactory scriptLineFactory)
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
            var mergeStatement = new List<string>
            {
                $"DELETE",
                $"    mrg_table",
                $"FROM"
            };

            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            mergeStatement.AddRange(new List<string>
            {
                $"    [{variables.SchemaName}].[{variables.Table.Name}] mrg_table",
                $"    INNER JOIN @InsertedValues iv_inner ON mrg_table.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()} = iv_inner.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()}",
                $"    LEFT JOIN @InsertedValues iv_outer ON mrg_table.{variables.Table.Name}Id = iv_outer.{variables.Table.Name}Id",
                $"WHERE",
                $"    iv_outer.{variables.Table.Name}Id IS NULL"
            });

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
