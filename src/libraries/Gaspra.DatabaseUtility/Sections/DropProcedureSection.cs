using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class DropProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 1, 0 });

        public DropProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.ProcedureName()));
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[{variables.SchemaName}].[{variables.ProcedureName()}]') AND [type] IN (N'P'))",
                "BEGIN",
                $"    DROP PROCEDURE [{variables.SchemaName}].[{variables.ProcedureName()}]",
                "END",
                "GO");

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
