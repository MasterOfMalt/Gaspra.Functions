using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure
{
    public class RecordSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 7 });

        public RecordSection(IScriptLineFactory scriptLineFactory)
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

            var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            var insertedColumns = variableSet.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name))).Where(c => !c.IdentityColumn).Where(c => c.Constraints != null);

            var valuesToConcat = "";

            foreach (var column in insertedColumns)
            {
                valuesToConcat += $",' {column.Name}=',mr.{column.Name}";
            }

            var mergeStatement = new List<string>
            {
                $"INSERT INTO",
                $"    [Analytics].[ProofOfConceptHistory]",
                $"SELECT",
                $"    mr.MergeAction,",
                $"    GETUTCDATE(),",
                $"    CONCAT('{variableSet.Table.Name}Id=',mr.{variableSet.Table.Name}Id{valuesToConcat})",
                $"FROM",
                $"    @MergeResult mr"
            };

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
