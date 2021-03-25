using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure.Delete
{
    public class DeclareSoftDeleteSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 6, 2 });

        public DeclareSoftDeleteSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(variableSet.Table.SoftDeleteColumn() != null);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var identityColumn = variableSet
                .Table
                .Columns
                .FirstOrDefault(c => c.IdentityColumn);

            var script = new List<string>
            {
                "DECLARE @SoftDelete TABLE",
                "(",
                $"     {identityColumn.FullyQualifiedDescription(false)}",
                ")"
            };

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
