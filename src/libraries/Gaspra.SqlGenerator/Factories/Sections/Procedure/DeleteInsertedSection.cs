using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class DeleteInsertedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 6 });

        public DeleteInsertedSection(IScriptLineFactory scriptLineFactory)
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
                $"DELETE",
                $"    mrg_table",
                $"FROM"
            };

            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            mergeStatement.AddRange(new List<string>
            {
                $"    [{variableSet.Schema.Name}].[{variableSet.Table.Name}] mrg_table",
                $"    INNER JOIN @InsertedValues iv_inner ON mrg_table.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()} = iv_inner.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()}",
                $"    LEFT JOIN @InsertedValues iv_outer ON mrg_table.{variableSet.Table.Name}Id = iv_outer.{variableSet.Table.Name}Id",
                $"WHERE",
                $"    iv_outer.{variableSet.Table.Name}Id IS NULL"
            });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
