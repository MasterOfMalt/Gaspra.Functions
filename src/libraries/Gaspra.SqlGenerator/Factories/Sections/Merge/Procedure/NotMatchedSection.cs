using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections.Merge.Procedure
{
    public class NotMatchedSection : IScriptSection<IMergeScriptVariableSet>
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new(new[] { 1, 2, 1 });

        public NotMatchedSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name);

            var mergeStatement = new List<string>
            {
                $"WHEN NOT MATCHED BY TARGET",
                $"    THEN INSERT ("
            };

            var softDeleteColumn = variableSet.Table.SoftDeleteColumn();

            var insertColumns = variableSet
                .Table
                .Columns
                .Where(c => !c.IdentityColumn);
                //.Where(c => softDeleteColumn == null || !c.Equals(softDeleteColumn));

            foreach (var column in insertColumns)
            {
                var line = $"        ";

                if (column != insertColumns.First())
                {
                    line += ",";
                }
                else
                {
                    line += " ";
                }

                line += $"[{column.Name}]";

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            mergeStatement.Add("    VALUES (");

            foreach (var column in insertColumns)
            {
                var line = $"        ";

                if (column != insertColumns.First())
                {
                    line += ",";
                }
                else
                {
                    line += " ";
                }

                if (!column.Equals(softDeleteColumn))
                {
                    line += $"s.[{column.Name}]";
                }
                else
                {
                    line += "NULL";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
