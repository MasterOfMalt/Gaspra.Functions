using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class MergeResultSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 1 });

        public MergeResultSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            //var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);
            //
            //var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);
            //
            //var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();
            //
            //return Task.FromResult(!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any());

            return Task.FromResult(true);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var insertValues = new List<string>
            {
                "DECLARE @MergeResult TABLE",
                "(",
                $"     [MergeAction] [varchar](6)",
                $"    ,[{variableSet.Table.Name}Id] [int]",
                ")"
            };

            // var columnLines = variableSet.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name))).Where(c => c.Constraints != null);
            //
            // foreach(var columnLine in columnLines)
            // {
            //     var line = $"    ,{columnLine.FullyQualifiedDescription(false)}";
            //
            //     insertValues.Add(line);
            // }
            //
            // insertValues.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                insertValues.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
