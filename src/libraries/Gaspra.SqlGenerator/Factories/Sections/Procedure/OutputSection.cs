using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class OutputSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 4 });

        public OutputSection(IScriptLineFactory scriptLineFactory)
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
            var mergeStatement = new List<string>
            {
                $"OUTPUT",
                $"    inserted.{variableSet.Table.Name}Id,"
            };

            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            var insertedColumns = variableSet.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name)));

            foreach (var column in insertedColumns)
            {
                var line = $"    inserted.{column.Name}";

                if (column != insertedColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("INTO @InsertedValues");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
