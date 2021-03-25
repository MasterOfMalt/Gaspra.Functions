using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Procedure.Delete
{
    public class InsertSoftDeletedSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 6, 4 });

        public InsertSoftDeletedSection(IScriptLineFactory scriptLineFactory)
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

            var usingType = (variableSet.TablesToJoin != null && variableSet.TablesToJoin.Any()) ? $"{variableSet.ScriptName}Variable" : $"{variableSet.TableTypeVariableName}";

            var matchOn = variableSet
                .MergeIdentifierColumns
                .Select(c => c.Name);

            var deleteOn = variableSet
                .DeleteIdentifierColumns
                .Select(c => c.Name);

            var innerResultColumns = matchOn
                .Where(m => !deleteOn.Any(d => d.Equals(m)))
                .ToList();

            var property = variableSet
                .Table
                .Properties
                .FirstOrDefault(p => p.Key.Equals("gf.SoftDeleteIdentifier"))?
                .Value;

            if (!string.IsNullOrWhiteSpace(property))
            {
                innerResultColumns
                    .AddRange(property.Split(","));

                innerResultColumns = innerResultColumns
                    .Distinct()
                    .ToList();
            }

            var innerResultJoin = "";

            foreach (var innerResultColumn in innerResultColumns)
            {
                innerResultJoin += $"{variableSet.Table.Name}.{innerResultColumn}=innerResult.{innerResultColumn}";

                if (innerResultColumn != innerResultColumns.Last())
                {
                    innerResultJoin += " AND ";
                }
            }

            var script = new List<string>
            {
                "IF EXISTS",
                "(",
                $"    SELECT {variableSet.Table.Name}.{identityColumn.Name} FROM [{variableSet.Schema.Name}].[{variableSet.Table.Name}] {variableSet.Table.Name} INNER JOIN @{usingType} innerResult ON {innerResultJoin} WHERE {variableSet.Table.Name}.Deleted IS NULL",
                "    EXCEPT",
                $"    SELECT UpdatedResult.{identityColumn.Name} FROM @UpdatedResult UpdatedResult",
                ")",
                $"BEGIN",
                $"    INSERT INTO",
                $"        @SoftDelete",
                $"    SELECT",
                $"        {variableSet.Table.Name}.{identityColumn.Name}"
            };

            script.AddRange(new List<string>
            {
                $"    FROM",
                $"        [{variableSet.Schema.Name}].[{variableSet.Table.Name}] {variableSet.Table.Name}",
                $"        INNER JOIN @UpdatedResult innerResult ON {innerResultJoin}",
                $"        LEFT JOIN @UpdatedResult outerResult ON {variableSet.Table.Name}.{identityColumn.Name} = outerResult.{identityColumn.Name}",
                $"    WHERE",
                $"        outerResult.{identityColumn.Name} IS NULL"
            });

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                script.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
