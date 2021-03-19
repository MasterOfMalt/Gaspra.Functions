using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class MatchedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 2 });

        public MatchedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            return Task.FromResult(
                !matchOn.Count().Equals(variableSet.Table.Columns.Count) &&
                !variableSet.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase))));
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var updateColumns = variableSet.Table.Columns.Where(c => !matchOn.Any(m => m.Equals(c.Name, StringComparison.InvariantCultureIgnoreCase))).Where(c => !c.IdentityColumn);

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
    }
}
