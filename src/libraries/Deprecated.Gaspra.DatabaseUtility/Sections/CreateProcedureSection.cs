using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Merge;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Interfaces;
using Deprecated.Gaspra.DatabaseUtility.Models.Script;

namespace Deprecated.Gaspra.DatabaseUtility.Sections
{
    public class CreateProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 2, 1 });

        public CreateProcedureSection(IScriptLineFactory scriptLineFactory)
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
