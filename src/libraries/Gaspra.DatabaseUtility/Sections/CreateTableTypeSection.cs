using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class CreateTableTypeSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 2, 0 });

        public CreateTableTypeSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.TableTypeName()));
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var createTypeLines = new List<string>
            {
                $"IF NOT EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'{variables.TableTypeName()}' AND ss.name = N'{variables.SchemaName}')",
                "BEGIN",
                $"    CREATE TYPE [{variables.SchemaName}].[{variables.TableTypeName()}] AS TABLE("
            };

            foreach(var column in variables.TableTypeColumns.OrderBy(c => c.Name))
            {
                var columnDescription = $"        [{column.Name}] {DataType(column)} {NullableColumn(column)}";

                if(column != variables.TableTypeColumns.OrderBy(c => c.Name).Last())
                {
                    columnDescription += ",";
                }

                createTypeLines.Add(columnDescription);
            }

            createTypeLines.AddRange(
                new List<string>
                {
                    "    )",
                    "END",
                    "GO",
                    "",
                    $"ALTER AUTHORIZATION ON TYPE::[{variables.SchemaName}].[{variables.TableTypeName()}] TO SCHEMA OWNER",
                    "GO"
                });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                createTypeLines.ToArray()
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
