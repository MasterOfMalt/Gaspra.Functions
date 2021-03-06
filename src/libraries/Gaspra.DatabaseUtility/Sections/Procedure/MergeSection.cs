using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Merge;
using Gaspra.DatabaseUtility.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Sections.Procedure
{
    public class MergeSection : IScriptSection
    {
        private readonly IScriptLineFactory _scriptLineFactory;

        public ScriptOrder Order { get; } = new ScriptOrder(new[] { 1, 2 });

        public MergeSection(IScriptLineFactory scriptLineFactory)
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

            var mergeStatement = new List<string>
            {
                $"MERGE [{variables.SchemaName}].[{variables.Table.Name}] AS t",
                $"USING @{variables.TableTypeVariableName()} AS s",
                $"    ON ("
            };

            foreach (var match in matchOn)
            {
                var line = $"        t.[{match}]=s[{match}]";

                if (match != matchOn.Last())
                {
                    line += " AND";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            mergeStatement.Add("WHEN NOT MATCHED BY TARGET");

            mergeStatement.Add("    THEN INSERT (");

            var insertColumns = variables.Table.Columns.Where(c => !c.IdentityColumn);

            foreach (var column in insertColumns)
            {
                var line = $"        [{column.Name}]";

                if (column != insertColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            mergeStatement.Add("    VALUES (");

            foreach (var column in insertColumns)
            {
                var line = $"        s.[{column.Name}]";

                if (column != insertColumns.Last())
                {
                    line += ",";
                }

                mergeStatement.Add(line);
            }

            mergeStatement.Add("    )");

            if(!matchOn.Count().Equals(variables.Table.Columns.Count) &&
                !variables.Table.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name).All(n => matchOn.Any(m => m.Equals(n, StringComparison.InvariantCultureIgnoreCase))))
            {
                mergeStatement.AddRange(new List<string>
                {
                    "WHEN MATCHED",
                    "    THEN UPDATE SET"
                });

                var updateColumns = variables.Table.Columns.Where(c => !matchOn.Any(m => m.Equals(c.Name, StringComparison.InvariantCultureIgnoreCase)));

                foreach (var column in updateColumns)
                {
                    var line = $"        t.[{column.Name}]=s.[{column.Name}]";

                    mergeStatement.Add(line);
                }
            }

            if (variables.RetentionPolicy != null)
            {
                mergeStatement.AddRange(new List<string>
                {
                    $"WHEN NOT MATCHED BY SOURCE AND t.{variables.RetentionPolicy.ComparisonColumn} < DATEADD(MONTH, -{variables.RetentionPolicy.RetentionMonths}, GETUTCDATE())",
                    "    THEN DELETE"
                });
            }

            var deleteOn = variables.DeleteIdentifierColumns.Select(c => c.Name);

            var deleteOnFactId = matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any())
            {
                mergeStatement.AddRange(new List<string>
                {
                    "OUTPUT",
                    $"    inserted.{variables.Table.Name}Id,"
                });

                var insertedColumns = variables.Table.Columns.Where(c => matchOn.Any(m => m.Equals(c.Name)));

                foreach (var column in insertedColumns)
                {
                    var line = $"    inserted.{column.Name}";

                    if (column != insertedColumns.Last())
                    {
                        line += ",";
                    }

                    mergeStatement.Add(line);
                }

                mergeStatement.Add("INTO @InsertedValues;");
            }

            if (!string.IsNullOrWhiteSpace(deleteOnFactId) && deleteOn.Any())
            {
                mergeStatement.AddRange(new List<string>
                {
                    "DELETE",
                    "    mrg_table",
                    "FROM",
                    $"    [{variables.SchemaName}].[{variables.Table.Name}] mrg_table",
                    $"    INNER JOIN @InsertedValues iv_inner ON mrg_table.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()} = iv_inner.{matchOn.Where(m => !deleteOn.Any(d => d.Equals(m))).FirstOrDefault()}",
                    $"    LEFT JOIN @InsertedValues iv_outer ON mrg_table.{variables.Table.Name}Id = iv_outer.{variables.Table.Name}Id",
                    "WHERE",
                    $"    iv_outer.{variables.Table.Name}Id IS NULL"
                });
            }

            var scriptLines = await _scriptLineFactory.LinesFrom(
                1,
                mergeStatement.ToArray()
                );

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
