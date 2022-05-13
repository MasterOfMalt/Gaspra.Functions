using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Delta
{
    public class CreateProcedureSection : IScriptSection<IDeltaScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 2, 1 });

        public CreateProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IDeltaScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.ScriptName));
        }

        public async Task<string> Value(IDeltaScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[{variableSet.Schema.Name}].[{variableSet.ScriptName}]') AND [type] IN (N'P'))",
                "BEGIN",
                $"    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [{variableSet.Schema.Name}].[{variableSet.ScriptName}] AS '",
                "END",
                "GO"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
