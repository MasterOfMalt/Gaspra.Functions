using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure.Delete
{
    public class MergeUndeletedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 6, 1 });

        public MergeUndeletedSection(IScriptLineFactory scriptLineFactory)
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
                $"/** Set soft delete to NULL where rows have been un-deleted **/",
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
                "WHEN MATCHED AND t.[Deleted] IS NOT NULL",
                "    THEN UPDATE SET",
                "        t.[Deleted]=NULL"
            });

            var outputTableIdentifier = $"inserted.{variableSet.Table.IdentityColumnName()}";

            if (variableSet.RetentionPolicy.RetentionMonths != null)
            {
                outputTableIdentifier =
                    $"COALESCE(inserted.{variableSet.Table.IdentityColumnName()}, deleted.{variableSet.Table.IdentityColumnName()})";
            }

            script.AddRange(new List<string>
            {
                $"OUTPUT",
                $"     $action AS MergeAction",
                $"    ,{outputTableIdentifier}"
            });

            script.Add("INTO @MergeResult;");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
