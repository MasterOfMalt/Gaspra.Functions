using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;
using Gaspra.SqlGenerator.Extensions;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class InsertValuesSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 1 });

        public InsertValuesSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            return Task.FromResult(!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any());
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var insertValues = new List<string>
            {
                "DECLARE @InsertedValues TABLE (",
                $"    [{variableSet.Table.Name}Id] [int],"
            };

            var columnLines = variableSet.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name)));

            foreach(var columnLine in columnLines)
            {
                var line = $"    [{columnLine.Name}] {columnLine.DataType()}";

                if (columnLine != columnLines.Last())
                {
                    line += ",";
                }

                insertValues.Add(line);
            }

            insertValues.Add(")");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                insertValues.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
