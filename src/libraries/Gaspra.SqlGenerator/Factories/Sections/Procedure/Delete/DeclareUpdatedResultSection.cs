using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure.Delete
{
    public class DeclareUpdatedResultSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 1, 0, 1 });

        public DeclareUpdatedResultSection(IScriptLineFactory scriptLineFactory)
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
                "DECLARE @UpdatedResult TABLE",
                "(",
                $"     {identityColumn.FullyQualifiedDescription(false)}"
            };

            var matchOn = variableSet
                .MergeIdentifierColumns
                .Select(c => c.Name);

            var columnLines = variableSet
                .Table
                .Columns
                .Where(c => matchOn.Any(m => m.Equals(c.Name)))
                .Where(c => c.Constraints != null);

            foreach(var columnLine in columnLines)
            {
                var line = $"    ,{columnLine.FullyQualifiedDescription(false)}";

                script.Add(line);
            }

            script.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
