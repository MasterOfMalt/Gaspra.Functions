using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class EndProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2 });

        public EndProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.TableTypeName()));
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                "END",
                "GO",
                "",
                $"ALTER AUTHORIZATION ON [{variables.SchemaName}].[{variables.ProcedureName()}] TO SCHEMA OWNER",
                "GO"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
