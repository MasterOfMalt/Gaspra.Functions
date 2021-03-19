﻿using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class DropTableTypeSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 0, 1, 1 });

        public DropTableTypeSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariableSet variables)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variables.SchemaName) &&
                !string.IsNullOrWhiteSpace(variables.ProcedureName()) &&
                !string.IsNullOrWhiteSpace(variables.TableTypeName()));
        }

        public async Task<string> Value(IScriptVariableSet variables)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'{variables.TableTypeName()}' AND ss.name = N'{variables.SchemaName}')",
                "BEGIN",
                $"    DROP TYPE [{variables.SchemaName}].[{variables.TableTypeName()}]",
                "END",
                "GO");

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
