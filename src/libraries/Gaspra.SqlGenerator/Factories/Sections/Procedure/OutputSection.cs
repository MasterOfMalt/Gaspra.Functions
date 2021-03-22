using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class OutputSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 4 });

        public OutputSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            var recordResults = variableSet.Table.RecordTable(variableSet.Schema) != null;

            return Task.FromResult(recordResults);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var mergeStatement = new List<string>
            {
                $"OUTPUT",
                $"     $action AS MergeAction",
                $"    ,inserted.{variableSet.Table.Name}Id"
            };

            mergeStatement.Add("INTO @MergeResult");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
