using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Delta
{
    public class EndProcedureSection : IScriptSection<IDeltaScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 2 });

        public EndProcedureSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IDeltaScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.TableTypeName));
        }

        public async Task<string> Value(IDeltaScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                "END",
                "GO"
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
