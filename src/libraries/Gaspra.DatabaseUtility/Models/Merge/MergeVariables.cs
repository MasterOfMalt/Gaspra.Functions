using Gaspra.DatabaseUtility.Extensions;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Tree;
using Gaspra.MergeSprocs.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.DatabaseUtility.Models.Merge
{
    public class MergeVariables
    {
        public string ProcedureName { get; set; }
        public string SchemaName { get; set; }
        public Table Table { get; set; }
        public IEnumerable<Column> TableTypeColumns { get; set; }
        public IEnumerable<Column> MergeIdentifierColumns { get; set; }
        public IEnumerable<Column> DeleteIdentifierColumns { get; set; }
        public RetentionPolicy? RetentionPolicy { get; set; }
        public IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin { get; set; }

        public MergeVariables(
            string procedureName,
            string schemaName,
            Table table,
            IEnumerable<Column> tableTypeColumns,
            IEnumerable<Column> mergeIdentifierColumns,
            IEnumerable<Column> deleteIdentifierColumns,
            RetentionPolicy? retentionPolicy,
            IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> tablesToJoin)
        {
            ProcedureName = procedureName;
            SchemaName = schemaName;
            Table = table;
            TableTypeColumns = tableTypeColumns;
            MergeIdentifierColumns = mergeIdentifierColumns;
            DeleteIdentifierColumns = deleteIdentifierColumns;
            RetentionPolicy = retentionPolicy;
            TablesToJoin = tablesToJoin;
        }

        public static (IEnumerable<MergeVariables> mergeVariables, IEnumerable<Exception> errornousTables) From(DataStructure dataStructure)
        {
            var mergeVariables = new List<MergeVariables>();

            var errornousTables = new List<Exception>();

            foreach (var table in dataStructure.Schema.Tables)
            {
                try
                {
                    mergeVariables.Add(new MergeVariables(
                        $"Merge{table.Name}",
                        dataStructure.Schema.Name,
                        table,
                        table.TableTypeColumns(dataStructure.Schema, dataStructure.DependencyTree),
                        table.MergeIdentifierColumns(dataStructure.Schema, dataStructure.DependencyTree),
                        table.DeleteIdentifierColumns(dataStructure.Schema, dataStructure.DependencyTree),
                        table.GetRetentionPolicy(),
                        table.TablesToJoin(dataStructure.Schema, dataStructure.DependencyTree)));

                }
                catch (Exception ex)
                {
                    errornousTables.Add(ex);
                }
            }

            return (mergeVariables, errornousTables);
        }
    }

    /*
     * todo:
     * move these out of there, the tabletype/ mergeidentifier/ tablestojoin
     * should also be table extensions
     */
    public static class MergeVariablesExtensions
    {
        public static string ProcedureName(this MergeVariables variables)
        {
            return $"Merge{variables.Table.Name}";
        }

        public static string TableTypeVariableName(this MergeVariables variables)
        {
            return $"{variables.Table.Name}";
        }

        public static string TableTypeName(this MergeVariables variables)
        {
            return $"TT_{variables.Table.Name}";
        }

        /*
         * calculate all the columns needed for the table type associated with the merge sproc
         */
        public static IEnumerable<Column> TableTypeColumns(this Table table, Schema schema, DependencyTree dependencyTree)
        {
            var tableTypeColumns = table.Columns
                /*
                 * get all columns which aren't identity columns
                 */
                .Where(c => !c.IdentityColumn)
                /*
                 * get all columns which aren't foreign key columns
                 */
                .Where(c => c.ForeignKey == null || !c.ForeignKey.ConstrainedTo.Any())
                .ToList();

            var tableBranch = dependencyTree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

            var relatedBranches = dependencyTree
                .GetRelatedBranches(table);

            if (tableBranch != null)
            {
                /*
                 * lower branches
                 */
                foreach (var branch in relatedBranches.Where(b => b.Depth > tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    if (branchTable.Columns.Where(c => c.ForeignKey.ChildConstraints.Contains(table.Name)).Any())
                    {
                        var identifyingColumns = branchTable.Columns.Where(c => !c.IdentityColumn);

                        tableTypeColumns.AddRange(identifyingColumns.Distinct());
                    }
                }

                /*
                 * higher branches
                 */
                foreach (var branch in relatedBranches.Where(b => b.Depth < tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    if (table.Columns.Where(c => c.ForeignKey.ParentConstraints.Contains(branchTable.Name)).Any())
                    {
                        var identifyingColumns = branchTable.Columns.Where(c => c.IdentityColumn);

                        tableTypeColumns.AddRange(identifyingColumns.Distinct());
                    }
                }
            }

            if (!tableTypeColumns.Distinct().Any())
            {
                throw new Exception($"Couldn't calculate table type columns for [{table.Name}], unable to calculate merge variables");
            }

            return tableTypeColumns
                .Distinct()
                /*
                 * order the table type columns to ensure they always come out the same
                 */
                .OrderBy(c => c.Name);
        }

        /*
         * calculate the columns used to identify whether or not we are merging or updating
         */
        public static IEnumerable<Column> MergeIdentifierColumns(this Table table, Schema schema, DependencyTree dependencyTree)
        {
            var identifyingColumns = new List<Column>();

            /*
             * extended property defined merge identifiers
             */
            if (table.ExtendedProperties != null &&
                table.ExtendedProperties.Any(e => e.Name.Equals("MergeIdentifier")))
            {
                var mergeIdentifyingColumns = table
                    .ExtendedProperties
                    .Where(e => e.Name.Equals("MergeIdentifier"))
                    .First()
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
            if (table.Columns.Where(c => !c.IdentityColumn).Count().Equals(1))
            {
                identifyingColumns.AddRange(table.Columns.Where(c => !c.IdentityColumn));
            }


            /*
             * calculate the higher branches identifiers
             * (relating a detail back to a fact)
             */
            var tableBranch = dependencyTree
            .Branches
            .Where(b => b.TableGuid.Equals(table.CorrelationId))
            .FirstOrDefault();

            var relatedBranches = dependencyTree
                .GetRelatedBranches(table);

            if (tableBranch != null)
            {
                /*
                 * lower branches
                 */
                //foreach (var branch in relatedBranches.Where(b => b.Depth > tableBranch.Depth))
                //{
                //    var branchTable = schema.GetTableFrom(branch.TableGuid);
                //
                //    var higherBranchIdentifyingColumns = branchTable.Columns.Where(c => c.IdentityColumn);
                //
                //    identifyingColumns.AddRange(higherBranchIdentifyingColumns);
                //}

                /*
                 * higher branches
                 */
                foreach (var branch in relatedBranches.Where(b => b.Depth < tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    var higherBranchIdentifyingColumns = branchTable
                        .Columns
                        .Where(c => c.IdentityColumn);

                    var foreignKeyColumnsToHigherBranches = table
                        .Columns
                        .Where(c => c.ForeignKey.ParentConstraints.Any(p => p.Equals(branchTable.Name)));

                    identifyingColumns.AddRange(foreignKeyColumnsToHigherBranches);
                }
            }

            if (!identifyingColumns.Distinct().Any())
            {
                throw new Exception($"Couldn't calculate identifying columns for [{table.Name}], unable to calculate merge variables");
            }

            return identifyingColumns
                .Distinct();
        }

        public static RetentionPolicy? GetRetentionPolicy(this Table table)
        {
            var retentionPolicies = new List<RetentionPolicy>();

            if (table.ExtendedProperties != null)
            {
                if(table.ExtendedProperties.Any(p => p.Name.Equals("RetentionMonths")) && table.ExtendedProperties.Any(p => p.Name.Equals("RetentionComparisonColumn")))
                {
                    return new RetentionPolicy
                    {
                        ComparisonColumn = table.ExtendedProperties.Where(p => p.Name.Equals("RetentionComparisonColumn")).Select(p => p.Value).First(),
                        RetentionMonths = table.ExtendedProperties.Where(p => p.Name.Equals("RetentionMonths")).Select(p => p.Value).First()
                    };
                }
            }

            return null;
        }

        public static IEnumerable<Column> DeleteIdentifierColumns(this Table table, Schema schema, DependencyTree dependencyTree)
        {
            var identifyingColumns = new List<Column>();

            if (table.Name.Equals("ProductTag"))
            {
                /*
                 * extended property defined merge identifiers
                 */
                //if (table.ExtendedProperties != null &&
                //    table.ExtendedProperties.Any(e => e.Name.Equals("MergeIdentifier")))
                //{
                //    var mergeIdentifyingColumns = table
                //        .ExtendedProperties
                //        .Where(e => e.Name.Equals("MergeIdentifier"))
                //        .First()
                //        .Value
                //        .Split(",");
                //
                //    var mergeColumns = table.Columns.Where(c =>
                //        mergeIdentifyingColumns.Any(m => m.Equals(c.Name))
                //        );
                //
                //    identifyingColumns.AddRange(mergeColumns);
                //}

                /*
                 * if the table only has one column that isn't an identity
                 * it's going to be the identifying column
                 */
                if (table.Columns.Where(c => !c.IdentityColumn).Count().Equals(1))
                {
                    identifyingColumns.AddRange(table.Columns.Where(c => !c.IdentityColumn));
                }


                /*
                 * calculate the higher branches identifiers
                 * (relating a detail back to a fact)
                 */
                var tableBranch = dependencyTree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

                var relatedBranches = dependencyTree
                    .GetRelatedBranches(table);

                if (tableBranch != null)
                {
                    /*
                     * lower branches
                     */
                    //foreach (var branch in relatedBranches.Where(b => b.Depth > tableBranch.Depth))
                    //{
                    //    var branchTable = schema.GetTableFrom(branch.TableGuid);
                    //
                    //    var higherBranchIdentifyingColumns = branchTable.Columns.Where(c => c.IdentityColumn);
                    //
                    //    identifyingColumns.AddRange(higherBranchIdentifyingColumns);
                    //}

                    /*
                     * higher branches
                     */
                    foreach (var branch in relatedBranches.Where(b => b.Depth < tableBranch.Depth))
                    {
                        var branchTable = schema.GetTableFrom(branch.TableGuid);

                        var higherBranchIdentifyingColumns = branchTable
                            .Columns
                            .Where(c => c.IdentityColumn);

                        var foreignKeyColumnsToHigherBranches = table
                            .Columns
                            .Where(c => c.ForeignKey.ParentConstraints.Any(p => p.Equals(branchTable.Name)));

                        identifyingColumns.AddRange(foreignKeyColumnsToHigherBranches);
                    }
                }

                if (!identifyingColumns.Distinct().Any())
                {
                    throw new Exception($"Couldn't calculate identifying columns for [{table.Name}], unable to calculate merge variables");
                }
            }

            return identifyingColumns
                .Distinct();
        }

        /*
         * calculate all the tables we need to join to, and the columns we should join on/ select from
         */
        public static IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin(this Table table, Schema schema, DependencyTree dependencyTree)
        {
            var tablesToJoin = new List<(Table, IEnumerable<Column>, IEnumerable<Column>)>();

            /**/
            var tableBranch = dependencyTree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

            var relatedBranches = dependencyTree
                .GetRelatedBranches(table);

            var tablesInJoin = new List<Table>();

            if (tableBranch != null)
            {
                foreach (var branch in relatedBranches.Where(b => b.Depth > tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    if (branchTable.Columns.Where(c => c.ForeignKey.ChildConstraints.Contains(table.Name)).Any())
                    {
                        if (!tablesInJoin.Contains(branchTable))
                        {
                            if (!branchTable.Name.EndsWith("Link"))
                            {
                                tablesInJoin.Add(branchTable);
                            }
                        }
                    }
                }
            }

            foreach (var tableInJoin in tablesInJoin)
            {
                /*
                 * todo:
                 * need to get teh specific columns required for the data and not every column that isn't an identity column
                 */
                tablesToJoin.Add((
                        tableInJoin,
                        tableInJoin.Columns.Where(c => c.IdentityColumn),//MergeIdentifierColumns(schema, dependencyTree),
                        tableInJoin.Columns.Where(c => !c.IdentityColumn)
                        ));
            }

            return tablesToJoin;
        }
    }
}
