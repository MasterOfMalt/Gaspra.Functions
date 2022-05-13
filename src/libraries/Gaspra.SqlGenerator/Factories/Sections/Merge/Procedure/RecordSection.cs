using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Merge.Procedure
{
    public class RecordSection : IScriptSection<IMergeScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 7 });

        public RecordSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            var recordTable = variableSet.Table.RecordTable(variableSet.Schema);

            return Task.FromResult(recordTable != null);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var recordTable = variableSet.Table.RecordTable(variableSet.Schema);

            var mergeStatement = new List<string>
            {
                $"INSERT INTO",
                $"    [{variableSet.Schema.Name}].[{recordTable.Name}]",
                $"SELECT DISTINCT",
                $"    '{variableSet.Table.Name}',",
                $"    '{variableSet.Table.Name}Id',", //todo: use identity column
                $"    mr.{variableSet.Table.Name}Id,", //todo: use identity column
                $"    GETUTCDATE(),",
                $"    mr.MergeAction",
                $"FROM",
                $"    @MergeResult mr"
            };

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
