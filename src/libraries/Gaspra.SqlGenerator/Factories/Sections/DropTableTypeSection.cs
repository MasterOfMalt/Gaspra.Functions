using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class DropTableTypeSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 0, 1, 1 });

        public DropTableTypeSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(
                !string.IsNullOrWhiteSpace(variableSet.Schema.Name) &&
                !string.IsNullOrWhiteSpace(variableSet.ScriptName) &&
                !string.IsNullOrWhiteSpace(variableSet.TableTypeName));
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                $"IF EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'{variableSet.TableTypeName}' AND ss.name = N'{variableSet.Schema.Name}')",
                "BEGIN",
                $"    DROP TYPE [{variableSet.Schema.Name}].[{variableSet.TableTypeName}]",
                "END",
                "GO");

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
