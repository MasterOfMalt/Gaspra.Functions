using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class NotMatchedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 1 });

        public NotMatchedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var mergeStatement = new List<string>
            {
                $"WHEN NOT MATCHED BY TARGET",
                $"    THEN INSERT ("
            };

            var insertColumns = variableSet.Table.Columns.Where(c => !c.IdentityColumn);

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
    }
}
