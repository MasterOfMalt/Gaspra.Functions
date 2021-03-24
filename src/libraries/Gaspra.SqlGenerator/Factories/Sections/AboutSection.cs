using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories.Sections
{
    public class AboutSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order => new ScriptOrder(new[] { 0, 3 });

        public AboutSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IMergeScriptVariableSet variableSet)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IMergeScriptVariableSet variableSet)
        {
            var matchOn = variableSet.MergeIdentifierColumns.Select(c => c.Name).ToList();

            var deleteOn = variableSet.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            var inserts = true;

            var softDeleteColumn = variableSet.Table.SoftDeleteColumn();

            if (softDeleteColumn != null)
            {
                matchOn.Add(softDeleteColumn.Name);
            }

            var updates = !matchOn.Count().Equals(variableSet.Table.Columns.Count) &&
                !variableSet.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase)));

            var deletes = softDeleteColumn != null;

            var retention = !string.IsNullOrWhiteSpace(variableSet.RetentionPolicy.ComparisonColumn);

            var retentionAmount = retention ? $"({variableSet.RetentionPolicy.RetentionMonths} months)" : "";

            var tableTypeFields = new List<string>();

            foreach (var column in variableSet.TableTypeColumns.OrderBy(c => c.Name))
            {
                var columnDescription = $" **         [{column.Name}] {column.DataType()} {column.NullableColumn()}";

                if (column != variableSet.TableTypeColumns.OrderBy(c => c.Name).Last())
                {
                    columnDescription += ",";
                }

                tableTypeFields.Add(columnDescription);
            }

            var dependantTables = new List<string>();

            foreach (var dependantTable in variableSet.TablesToJoin)
            {
                var table = $" **     [{variableSet.Schema.Name}].[{dependantTable.joinTable.Name}]";

                if(dependantTable != variableSet.TablesToJoin.Last())
                {
                    table += ",";
                }

                dependantTables.Add(table);
            }

            var aboutText = new List<string>
            {
                $" ** [{variableSet.Schema.Name}].[{variableSet.ScriptName}]",
                $" **",
                $" ** Expects table type parameter: @{variableSet.TableTypeVariableName} as",
                $" **     [{variableSet.Schema.Name}].[{variableSet.TableTypeName}] ("
            };

            aboutText.AddRange(tableTypeFields);

            aboutText.AddRange(new List<string>
            {
                $" **     )",
                $" **",
                $" ** Merges data into table:",
                $" **     [{variableSet.Schema.Name}].[{variableSet.Table.Name}]"
            });

            if(dependantTables.Any())
            {
                aboutText.Add(" **");
                aboutText.Add(" ** Merge requires data from:");

                aboutText.AddRange(dependantTables);
            }

            var assemblyValue = Assembly
                .GetEntryAssembly()?
                .CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(AssemblyInformationalVersionAttribute))?
                .ConstructorArguments
                .FirstOrDefault()
                .Value;

            var softDeleteColumnValue = "";

            if (softDeleteColumn != null)
            {
                softDeleteColumnValue = $" (column: [{softDeleteColumn.Name}])";
            }

            aboutText.AddRange(new List<string> {
                $"#pad",
                $" ** Inserts: {inserts}",
                $" ** Updates: {updates}",
                $" ** Deletes: {deletes}{softDeleteColumnValue}",
                $" ** Retention policy: {retention} {retentionAmount}",
                $"#pad",
                $" ** Gaspra.Functions v{assemblyValue}"
            });

            var affix = " **";

            var longestLine = aboutText
                .Select(t => t.Length)
                .OrderByDescending(t => t)
                .First();

            var start = "/" + new string('*', longestLine + 2);

            var end = " " + new string('*', longestLine + 2) + "/";

            var aboutLines = new List<string>
            {
                start
            };

            aboutLines
                .AddRange(aboutText.Select(t =>
                {
                    if (!t.Equals("#pad"))
                    {
                        return t + new string(' ', longestLine - t.Length) + affix;
                    }
                    else
                    {
                        return " " + new string('*', longestLine + 2 );
                    }
                }));

            aboutLines.Add(end);

            var scriptLines = await _scriptLineFactory.LinesFrom(
                0,
                aboutLines.ToArray());

            return await _scriptLineFactory.StringFrom(scriptLines);
        }
    }
}
