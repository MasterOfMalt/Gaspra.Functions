using Deprecated.Gaspra.DatabaseUtility.Models.Merge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Interfaces;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Script;

namespace Deprecated.Gaspra.DatabaseUtility.Sections
{
    public class AboutSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order => new ScriptOrder(new[] { 0, 3 });

        public AboutSection(IScriptLineFactory scriptLineFactory)
        {
            _scriptLineFactory = scriptLineFactory;
        }

        public Task<bool> Valid(IScriptVariables variables)
        {
            return Task.FromResult(true);
        }

        public async Task<string> Value(IScriptVariables variables)
        {
            var matchOn = variables.MergeIdentifierColumns.Select(c => c.Name);

            var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            var inserts = true;

            var updates = !matchOn.Count().Equals(variables.Table.Columns.Count) &&
                !variables.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase)));

            var deletes = !string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any();

            var retention = variables.RetentionPolicy != null;

            var retentionAmount = retention ? $"({variables.RetentionPolicy.RetentionMonths} months)" : "";

            var tableTypeFields = new List<string>();

            foreach (var column in variables.TableTypeColumns.OrderBy(c => c.Name))
            {
                var columnDescription = $" **         [{column.Name}] {DataType(column)} {NullableColumn(column)}";

                if (column != variables.TableTypeColumns.OrderBy(c => c.Name).Last())
                {
                    columnDescription += ",";
                }

                tableTypeFields.Add(columnDescription);
            }

            var dependantTables = new List<string>();

            foreach (var dependantTable in variables.TablesToJoin)
            {
                var table = $" **     [{variables.SchemaName}].[{dependantTable.joinTable.Name}]";

                if(dependantTable != variables.TablesToJoin.Last())
                {
                    table += ",";
                }

                dependantTables.Add(table);
            }

            var aboutText = new List<string>
            {
                $" ** [{variables.SchemaName}].[{variables.ProcedureName()}]",
                $" **",
                $" ** Expects table type parameter: @{variables.TableTypeVariableName()} as",
                $" **     [{variables.SchemaName}].[{variables.TableTypeName()}] ("
            };

            aboutText.AddRange(tableTypeFields);

            aboutText.AddRange(new List<string>
            {
                $" **     )",
                $" **",
                $" ** Merges data into table:",
                $" **     [{variables.SchemaName}].[{variables.Table.Name}]"
            });

            if(dependantTables.Any())
            {
                aboutText.Add(" **");
                aboutText.Add(" ** Merge requires data from:");

                aboutText.AddRange(dependantTables);
            }

            aboutText.AddRange(new List<string> {
                $"#pad",
                $" ** Inserts: {inserts}",
                $" ** Updates: {updates}",
                $" ** Deletes: {deletes}",
                $" ** Retention policy: {retention} {retentionAmount}",
                $"#pad",
                $" ** Gaspra.Functions v{Assembly.GetEntryAssembly().GetName().Version}"
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

        private static string DataType(Column column)
        {
            var dataType = $"[{column.DataType}]";

            if (column.DataType.Equals("decimal") && column.Precision.HasValue && column.Scale.HasValue)
            {
                dataType += $"({column.Precision.Value},{column.Scale.Value})";
            }
            else if (column.MaxLength.HasValue)
            {
                dataType += $"({column.MaxLength.Value})";
            }

            return dataType;
        }

        private static string NullableColumn(Column column)
        {
            return column.Nullable ? "NULL" : "NOT NULL";
        }
    }
}
