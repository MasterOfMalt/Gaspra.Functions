using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class TargetSourceSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 0});

        public TargetSourceSection(IScriptLineFactory scriptLineFactory)
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

            var usingType = (variableSet.TablesToJoin != null && variableSet.TablesToJoin.Any()) ? $"{variableSet.ScriptName}Variable" : $"{variableSet.TableTypeVariableName}";

            var mergeStatement = new List<string>
            {
                $"MERGE [{variableSet.Schema.Name}].[{variableSet.Table.Name}] AS t",
                $"USING @{usingType} AS s",
                $"ON ("
            };

            foreach (var match in matchOn)
            {
                var line = $"    ";

                if (match != matchOn.First())
                {
                    line += "AND ";
                }

                line += $"t.[{match}]=s.[{match}]";

                mergeStatement.Add(line);
            }

            mergeStatement.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
