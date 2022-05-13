using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Delta
{
    public class AlterProcedureSection : IScriptSection<IDeltaScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 4 });

        public AlterProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IDeltaScriptVariableSet variableSet)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(variableSet.Schema.Name));
        }

        public async Task<string> Value(IDeltaScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"ALTER PROCEDURE [{variableSet.Schema.Name}].[{variableSet.ScriptName}]",
                $"    @{variableSet.TableTypeVariableName} [{variableSet.Schema.Name}].[{variableSet.TableTypeName}] READONLY,",
                $"    @Delta DATETIME",
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
