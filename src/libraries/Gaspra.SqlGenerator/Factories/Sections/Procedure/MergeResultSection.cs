using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class MergeResultSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 1 });

        public MergeResultSection(IScriptLineFactory scriptLineFactory)
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
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var insertValues = new List<string>
            {
                "DECLARE @MergeResult TABLE",
                "(",
                $"     [MergeAction] [varchar](6)",
                $"    ,[{variableSet.Table.Name}Id] [int]",
                ")"
            };

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                insertValues.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
