using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections
{
    public class AlterProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1 });

        public AlterProcedureSection(IScriptLineFactory scriptLineFactory)
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
