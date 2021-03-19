using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class CreateProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 2, 1 });

        public CreateProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.ProcedureName()));
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[{variables.SchemaName}].[{variables.ProcedureName()}]') AND [type] IN (N'P'))",
                "BEGIN",
                $"    EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [{variables.SchemaName}].[{variables.ProcedureName()}] AS '",
                "END",
                "GO"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
