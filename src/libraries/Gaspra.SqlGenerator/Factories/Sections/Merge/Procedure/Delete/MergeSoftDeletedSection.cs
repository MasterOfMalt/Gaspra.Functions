using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Merge.Procedure.Delete
{
    public class MergeSoftDeletedSection : IScriptSection<IMergeScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 6, 4 });

        public MergeSoftDeletedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(variableSet.Table.SoftDeleteColumn() != null);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var usingType = (variableSet.TablesToJoin != null && variableSet.TablesToJoin.Any()) ? $"{variableSet.ScriptName}Variable" : $"{variableSet.TableTypeVariableName}";

            var script = new List<string>
            {
                $"/** Merge all values (anything not updated in the merge actions is effectively marked for deletion) **/",
                $"MERGE [{variableSet.Schema.Name}].[{variableSet.Table.Name}] AS t",
                $"USING @{usingType} AS s",
                $"ON",
                "("
            };

            var matchOn = variableSet
                .MergeIdentifierColumns
                .Select(c => c.Name);

            foreach (var match in matchOn)
            {
                var line = $"    ";

                if (match != matchOn.First())
                {
                    line += "AND ";
                }

                line += $"t.[{match}]=s.[{match}]";

                script.Add(line);
            }

            script.Add(")");

            script.AddRange(new List<string>
            {
                "WHEN MATCHED",
                "    THEN UPDATE SET",
                "        t.[Deleted]=NULL",
                "OUTPUT",
                $"     inserted.{variableSet.Table.Name}Id"
            });

            var outputMatchOn = variableSet
                .MergeIdentifierColumns
                .Where(c => c.Constraints != null)
                .Select(c => c.Name)
                .ToList();

            var property = variableSet
                .Table
                .Properties
                .FirstOrDefault(p => p.Key.Equals("gf.SoftDeleteIdentifier"))?
                .Value;

            if (!string.IsNullOrWhiteSpace(property))
            {
                outputMatchOn
                    .AddRange(property.Split(","));

                outputMatchOn = outputMatchOn
                    .Distinct()
                    .ToList();
            }

            var columnLines = variableSet
                .Table
                .Columns
                .Where(c => outputMatchOn.Any(m => m.Equals(c.Name)));

            foreach(var columnLine in columnLines)
            {
                var line = $"    ,inserted.{columnLine.Name}";

                script.Add(line);
            }

            script.AddRange(new List<string>
            {
                "INTO",
                "    @UpdatedResult;"
            });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
