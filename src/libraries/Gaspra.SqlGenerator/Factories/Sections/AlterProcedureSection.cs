using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class AlterProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 4 });

        public AlterProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.TableTypeName()));
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"ALTER PROCEDURE [{variables.SchemaName}].[{variables.ProcedureName()}]",
                $"    @{variables.TableTypeVariableName()} [{variables.SchemaName}].[{variables.TableTypeName()}] READONLY",
                "AS",
                "BEGIN",
                "",
                "    SET NOCOUNT ON;",
                "    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
