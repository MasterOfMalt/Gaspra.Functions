using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class CreateTableTypeSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 2, 0 });

        public CreateTableTypeSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.TableTypeName));
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var createTypeLines = new List<string>
            {
                $"IF NOT EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'{variableSet.TableTypeName}' AND ss.name = N'{variableSet.Schema.Name}')",
                "BEGIN",
                $"    CREATE TYPE [{variableSet.Schema.Name}].[{variableSet.TableTypeName}] AS TABLE",
                "    ("
            };

            foreach(var column in variableSet.TableTypeColumns.OrderBy(c => c.Name))
            {
                var columnDescription = $"        ";

                if(column != variableSet.TableTypeColumns.OrderBy(c => c.Name).First())
                {
                    columnDescription += ",";
                }
                else
                {
                    columnDescription += " ";
                }

                columnDescription += $"{column.FullyQualifiedDescription()}";

                createTypeLines.Add(columnDescription);
            }

            createTypeLines.AddRange(
                new List<string>
                {
                    "    )",
                    "END",
                    "GO"
                });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                createTypeLines.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
