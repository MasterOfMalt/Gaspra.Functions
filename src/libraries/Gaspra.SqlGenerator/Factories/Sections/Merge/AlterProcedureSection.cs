using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Merge
{
    public class AlterProcedureSection : IScriptSection<IMergeScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 4 });

        public AlterProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.TableTypeName));
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"ALTER PROCEDURE [{variableSet.Schema.Name}].[{variableSet.ScriptName}]",
                $"    @{variableSet.TableTypeVariableName} [{variableSet.Schema.Name}].[{variableSet.TableTypeName}] READONLY",
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
