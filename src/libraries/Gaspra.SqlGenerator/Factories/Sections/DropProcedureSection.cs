using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class DropProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 1, 0 });

        public DropProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.ScriptName));
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[{variableSet.Schema.Name}].[{variableSet.ScriptName}]') AND [type] IN (N'P'))",
                "BEGIN",
                $"    DROP PROCEDURE [{variableSet.Schema.Name}].[{variableSet.ScriptName}]",
                "END",
                "GO");

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
