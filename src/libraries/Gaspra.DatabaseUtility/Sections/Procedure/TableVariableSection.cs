using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections.Procedure
{
    public class TableVariableSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 0 });

        public TableVariableSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(
                variables.TablesToJoin != null &&
                variables.TablesToJoin.Any());
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var columns = variables.Table.Columns.Where(c => !c.IdentityColumn);

            var tableVariableLines = new List<string>
            {
                $"DECLARE @{variables.ProcedureName}Variable TABLE",
                "("
            };

            foreach (var column in columns)
            {
                var line = $"    [{column.Name}] {DataType(column)} {NullableColumn(column)}";

                if (column != columns.Last())
                {
                    line += ",";
                }

                tableVariableLines.Add(line);
            }

            tableVariableLines.AddRange(new List<string> {
                ")",
                $"INSERT INTO @{variables.ProcedureName}Variable",
                "SELECT"
            });

            foreach (var column in columns)
            {
                var line = $"    [{column.Name}]";

                if (column != columns.Last())
                {
                    line += ",";
                }

                tableVariableLines.Add(line);
            }

            tableVariableLines.AddRange(new List<string> {
                "FROM",
                $"    @{variables.TableTypeVariableName()} AS tt"
            });

            foreach (var table in variables.TablesToJoin)
            {
                var joinColumns = string.Join(" AND ", table.selectColumns.Select(c => $"tt.[{c.Name}]=alias_{table.joinTable.Name.ToLower()}.[{c.Name}]"));

                var line = $"    INNER JOIN [{variables.SchemaName}].[{table.joinTable.Name}] AS alias_{table.joinTable.Name.ToLower()} ON {joinColumns}";

                tableVariableLines.Add(line);
            }

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                tableVariableLines.ToArray()
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
