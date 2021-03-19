using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class EndProcedureSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 2 });

        public EndProcedureSection(IScriptLineFactory scriptLineFactory)
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
                "END",
                "GO",
                "",
                $"ALTER AUTHORIZATION ON [{variableSet.Schema.Name}].[{variableSet.ScriptName}] TO SCHEMA OWNER",
                "GO"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
