using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class TableVariableSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 0 });

        public TableVariableSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(
                variableSet.TablesToJoin != null &&
                variableSet.TablesToJoin.Any());
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var columns = variableSet.Table.Columns.Where(c => !c.IdentityColumn);

            var tableVariableLines = new List<string>
            {
                $"DECLARE @{variableSet.ScriptName}Variable TABLE",
                "("
            };

            foreach (var column in columns)
            {
                var line = $"    [{column.Name}] {column.DataType()} {column.NullableColumn()}";

                if (column != columns.Last())
                {
                    line += ",";
                }

                tableVariableLines.Add(line);
            }

            tableVariableLines.AddRange(new List<string> {
                ")",
                $"INSERT INTO @{variableSet.ScriptName}Variable",
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
                $"    @{variableSet.TableTypeVariableName} AS tt"
            });

            foreach (var table in variableSet.TablesToJoin)
            {
                var joinColumns = string.Join(" AND ", table.selectColumns.Select(c => $"tt.[{c.Name}]=alias_{table.joinTable.Name.ToLower()}.[{c.Name}]"));

                var line = $"    INNER JOIN [{variableSet.Schema.Name}].[{table.joinTable.Name}] AS alias_{table.joinTable.Name.ToLower()} ON {joinColumns}";

                tableVariableLines.Add(line);
            }

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                tableVariableLines.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
