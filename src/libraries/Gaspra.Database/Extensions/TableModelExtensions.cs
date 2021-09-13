using System;
using Gaspra.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace Gaspra.Database.Extensions
{
    public static class TableModelExtensions
    {
        /// <summary>
        /// Add extended property to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddProperty(this TableModel table, string key, string value)
        {
            var properties = table.Properties ?? new List<PropertyModel>();

            properties.Add(new PropertyModel
            {
                Key = key,
                Value = value
            });

            table.Properties = properties;
        }

        /// <summary>
        /// Add a dependant table to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="dependant"></param>
        public static void AddDependantTable(this TableModel table, TableModel dependant)
        {
            if (table == null) return;

            var dependants = table.DependantTables ?? new List<TableModel>();

            dependants.Add(dependant);

            table.DependantTables = dependants;
        }

        /// <summary>
        /// Add a reference table to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reference"></param>
        public static void AddReferenceTable(this TableModel table, TableModel reference)
        {
            if (table == null) return;

            var references = table.ReferenceTables ?? new List<TableModel>();

            references.Add(reference);

            table.ReferenceTables = references;
        }

        /// <summary>
        /// Set all the tables in a collection to a given depth
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static Task SetTableDepths(this ICollection<TableModel> tables, int depth)
        {
            foreach (var table in tables)
            {
                table.Depth = depth;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Recursively work down all the tables increasing the
        /// depth with each recursive call
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="depth"></param>
        /// <param name="databaseModel"></param>
        /// <returns></returns>
        public static async Task RecurseTableDepths(this ICollection<TableModel> tables, int depth)
        {
            if (tables != null && tables.Any())
            {
                var nextDepth = depth + 1;

                await tables.SetTableDepths(depth);

                foreach (var table in tables)
                {
                    await RecurseTableDepths(await table.GetConnectingTables(), nextDepth);
                }
            }
        }

        /// <summary>
        /// Get all the tables that connect to a table via reference
        /// and dependant constraints
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Task<ICollection<TableModel>> GetConnectingTables(this TableModel table)
        {
            var referenceTables = table
                .ReferenceTables?
                .Where(t => t != null && !t.IsLinkTable() && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var dependantTables = table
                .DependantTables?
                .Where(t => t != null && !t.IsLinkTable() && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var tables = referenceTables;

            tables.AddRange(dependantTables);

            tables = tables
                .Distinct(TableModelComparison.Instance)
                .ToList();

            return Task.FromResult((ICollection<TableModel>)tables);
        }

        /// <summary>
        /// Returns true when table is a link table, a link table
        /// is defined by a table only being made up of constraints
        /// linking other tables together
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsLinkTable(this TableModel table)
        {
            var isLink = false;

            if (table != null)// && table.Depth.Equals(-1))
            {
                var allColumnsAreConstraints = table
                    .Columns
                    .All(c => c.IdentityColumn || c.Constraints != null);

                // all columns must be constraints or identity columns
                if (allColumnsAreConstraints)
                {
                    // must be dependant on tables
                    if (table.DependantTables != null)
                    {
                        var deadEnd = false;

                        foreach (var dependant in table.DependantTables)
                        {
                            // if any of the dependant tables can't reach
                            // the root this is not a link table
                            if (!dependant.CanReachRoot(new[] { table }))
                            {
                                deadEnd = true;
                            }
                        }

                        isLink = !deadEnd;
                    }
                }
            }

            return isLink;
        }

        /// <summary>
        /// todo; summary
        /// todo; refactor (yey recursion)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="avoidingTables"></param>
        /// <returns></returns>
        public static bool CanReachRoot(this TableModel table, IReadOnlyCollection<TableModel> avoidingTables)
        {
            var linkedTables = new List<TableModel>();

            if (table.DependantTables != null)
            {
                linkedTables.AddRange(table.DependantTables);
            }

            if (table.ReferenceTables != null)
            {
                linkedTables.AddRange(table.ReferenceTables);
            }

            if (linkedTables.Any(l => l.Depth.Equals(1)))
            {
                return true;
            }

            var recurseTables = new List<TableModel>();

            foreach (var link in linkedTables)
            {
                if (!avoidingTables.Any(a => a.Equals(link)))
                {
                    recurseTables.Add(link);
                }
            }

            if (!recurseTables.Any())
            {
                return false;
            }

            foreach (var recurse in recurseTables)
            {
                var avoid = avoidingTables.ToList();

                avoid.AddRange(recurseTables);

                if (recurse.CanReachRoot(avoid))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// todo; write summary
        /// todo; refactor extension method
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IReadOnlyCollection<ColumnModel> TableTypeColumns(this TableModel table, SchemaModel schema)
        {
            var tableTypeColumns = new List<ColumnModel>();

            if (table.IsLinkTable())
            {
                tableTypeColumns = table
                    .Columns
                    .Where(c => !c.IdentityColumn)
                    .ToList();
            }
            else
            {
                var softDeleteColumn = table.SoftDeleteColumn();

                tableTypeColumns = table.Columns
                    // get all columns which aren't identity columns
                    .Where(c => !c.IdentityColumn)
                    //get all columns which aren't foreign key columns
                    .Where(c => c.Constraints == null)
                    .Where(c => softDeleteColumn == null || !c.Equals(softDeleteColumn))
                    .ToList();

                //
                if (table.DependantTables != null)
                {
                    foreach (var dependantTable in table.DependantTables.Where(t => t.Depth >= table.Depth))
                    {
                        var dependantSoftDeleteColumn = dependantTable.SoftDeleteColumn();

                        var identifyingColumns = new List<ColumnModel>();

                        // if the dependant table has merge identifiers just use them to get the table,
                        // otherwise fall back on comparing the whole table (without the identity column/ soft delete column)
                        if (dependantTable.Properties != null && dependantTable.Properties.Any(p => p.Key.Equals("MergeIdentifier")))
                        {
                            var mergeIdentifierColumnNames = dependantTable.Properties
                                .First(p => p.Key.Equals("MergeIdentifier")).Value.Split(",");

                            identifyingColumns.AddRange(dependantTable.Columns.Where(c => mergeIdentifierColumnNames.Any(i => i.Trim().Equals(c.Name, StringComparison.InvariantCultureIgnoreCase))));
                        }
                        else
                        {
                            identifyingColumns.AddRange(dependantTable
                                .Columns
                                .Where(c => !c.IdentityColumn)
                                .Where(c => dependantSoftDeleteColumn == null || !c.Equals(dependantSoftDeleteColumn)));
                        }

                        tableTypeColumns.AddRange(identifyingColumns.Distinct());
                    }

                    foreach (var dependantTable in table.DependantTables.Where(t => t.Depth < table.Depth))
                    {
                        var identifyingColumns = dependantTable.Columns.Where(c => c.IdentityColumn);

                        tableTypeColumns.AddRange(identifyingColumns.Distinct());
                    }
                }
            }

            if (!tableTypeColumns.Any())
            {
                throw new Exception($"Couldn't calculate table type columns for [{table.Name}]");
            }

            return tableTypeColumns
                .Distinct()
                .OrderBy(c => c.Name)
                .ToList();
        }

        /// <summary>
        /// todo; write summary
        /// todo; refactor extension method
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IReadOnlyCollection<ColumnModel> MergeIdentifierColumns(this TableModel table, SchemaModel schema)
        {
            var identifyingColumns = new List<ColumnModel>();

            /*
             * extended property defined merge identifiers
             */
            if (table.Properties != null &&
                table.Properties.Any(p => p.Key.Equals("MergeIdentifier")))
            {
                var mergeIdentifyingColumns = table
                    .Properties
                    .First(p => p.Key.Equals("MergeIdentifier"))
                    .Value
                    .Split(",");

                var mergeColumns = table.Columns.Where(c =>
                    mergeIdentifyingColumns.Any(m => m.Equals(c.Name))
                    );

                identifyingColumns.AddRange(mergeColumns);
            }

            /*
             * if the table only has one column that isn't an identity
             * it's going to be the identifying column
             */
            if (table.Columns.Count(c => !c.IdentityColumn).Equals(1))
            {
                identifyingColumns.AddRange(table.Columns.Where(c => !c.IdentityColumn));
            }

            /*
             * calculate the higher branches identifiers
             * (relating a detail back to a fact)
             */
            var linkedTables = new List<TableModel>();

            if (table.DependantTables != null)
            {
                linkedTables.AddRange(table.DependantTables);
            }

            if (table.ReferenceTables != null)
            {
                linkedTables.AddRange(table.ReferenceTables);
            }

            foreach (var linkedTable in linkedTables.Distinct().Where(lt => lt.Depth < table.Depth))
            {

                    var higherForeignKeyColumns = table
                        .Columns
                        .Where(c => c.Constraints != null && c.Constraints.Any(x => linkedTable.Columns.Contains(x.Reference) && x.Parent))
                        .ToList();

                    identifyingColumns.AddRange(higherForeignKeyColumns);
            }

            if (!identifyingColumns.Any())
            {
                throw new Exception($"Couldn't calculate identifying columns for [{table.Name}]");
            }

            return identifyingColumns
                .Distinct()
                .OrderBy(c => c.Name)
                .ToList();
        }

        /// <summary>
        /// todo; write summary
        /// todo; refactor extension method
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IReadOnlyCollection<ColumnModel> DeleteIdentifierColumns(this TableModel table,
            SchemaModel schema)
        {
            var identifyingColumns = new List<ColumnModel>();

            if (table.Properties != null && table.Properties.Any(p => p.Key.Equals("MergeIdentifier")))
            {
                var mergeIdentifyingColumns = table
                    .Properties
                    .First(e => e.Key.Equals("MergeIdentifier"))
                    .Value
                    .Split(",");

                var mergeColumns = table.Columns.Where(c =>
                    mergeIdentifyingColumns.Any(m => m.Equals(c.Name))
                );

                identifyingColumns.AddRange(mergeColumns);
            }

            return identifyingColumns
                .Distinct()
                .OrderBy(c => c.Name)
                .ToList();
        }

        /// <summary>
        /// todo; write summary
        /// todo; refactor extension method
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static (string ComparisonColumn, string RetentionMonths) RetentionPolicy(this TableModel table)
        {
            if (table.Properties != null &&
                table.Properties.Any(p => p.Key.Equals("RetentionMonths"))
                && table.Properties.Any(p => p.Key.Equals("RetentionComparisonColumn")))
            {
                return (
                    table.Properties.FirstOrDefault(p => p.Key.Equals("RetentionComparisonColumn"))?.Value,
                    table.Properties.FirstOrDefault(p => p.Key.Equals("RetentionMonths"))?.Value
                );
            }

            return (null, null);
        }

        /// <summary>
        /// todo; write summary
        /// todo; refactor extension method
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static
            IReadOnlyCollection<(TableModel joinTable, IReadOnlyCollection<ColumnModel> joinColumns,
                IReadOnlyCollection<ColumnModel> selectColumns)> TablesToJoin(this TableModel table, SchemaModel schema)
        {
            var tablesToJoin = new List<(TableModel, IReadOnlyCollection<ColumnModel>, IReadOnlyCollection<ColumnModel>)>();

            var tablesInJoin = new List<TableModel>();

            var linkedTables = new List<TableModel>();

            if (table.DependantTables != null)
            {
                linkedTables.AddRange(table.DependantTables);
            }

            //if (table.ReferenceTables != null)
            //{
            //    linkedTables.AddRange(table.ReferenceTables);
            //}

            foreach (var branchTable in linkedTables.Where(b => b.Depth >= table.Depth))
            {
                if (!tablesInJoin.Contains(branchTable))
                {
                    tablesInJoin.Add(branchTable);
                }
            }

            foreach (var tableInJoin in tablesInJoin)
            {
                /*
                 * todo:
                 * need to get the specific columns required for the data and not every column that isn't an identity column
                 */
                tablesToJoin.Add((
                    tableInJoin,
                    tableInJoin.Columns.Where(c => c.IdentityColumn).ToList(),//MergeIdentifierColumns(schema, dependencyTree),
                    tableInJoin.Columns.Where(c => !c.IdentityColumn).ToList()
                ));
            }

            return tablesToJoin
                .OrderBy(t => t.Item1.Name)
                .ToList();
        }

        /// <summary>
        /// todo; write summary
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ColumnModel SoftDeleteColumn(this TableModel table)
        {
            if (table.Properties != null && table.Properties.Any(p => p.Key.Equals("gf.SoftDelete", StringComparison.InvariantCultureIgnoreCase)))
            {
                var columnName = table
                    .Properties
                    .FirstOrDefault(p => p.Key.Equals("gf.SoftDelete", StringComparison.InvariantCultureIgnoreCase))?
                    .Value;

                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    return table
                        .Columns
                        .FirstOrDefault(c => c.Name.Equals(columnName));
                }
            }

            return null;
        }

        /// <summary>
        /// todo; write summary
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool ShouldSkipTable(this TableModel table)
        {
            if (table.Properties != null && table.Properties.Any(p => p.Key.Equals("gf.Skip", StringComparison.InvariantCultureIgnoreCase)))
            {
                var shouldSkip = table
                    .Properties
                    .FirstOrDefault(p => p.Key.Equals("gf.Skip", StringComparison.InvariantCultureIgnoreCase))?
                    .Value;

                return !string.IsNullOrWhiteSpace(shouldSkip);
            }

            return false;
        }

        /// <summary>
        /// todo; write summary
        /// </summary>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static TableModel RecordTable(this TableModel table, SchemaModel schema)
        {
            if (table.Properties != null && table.Properties.Any(p => p.Key.Equals("gf.Record", StringComparison.InvariantCultureIgnoreCase)))
            {
                var tableName = table
                    .Properties
                    .FirstOrDefault(p => p.Key.Equals("gf.Record", StringComparison.InvariantCultureIgnoreCase))?
                    .Value;

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    return schema
                        .Tables
                        .FirstOrDefault(c => c.Name.Equals(tableName));
                }
            }

            return null;
        }

        /// <summary>
        /// Return the identity column name for a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string IdentityColumnName(this TableModel table)
        {
            var identityColumn = table
                .Columns
                .FirstOrDefault(c => c.IdentityColumn);

            return identityColumn?.Name;
        }
    }
}
