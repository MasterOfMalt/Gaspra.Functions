using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Gaspra.MergeSprocs.Models.Merge
{
    public class MergeVariables
    {
        public string SchemaName { get; set; }
        public Table Table { get; set; }
        public IEnumerable<Column> TableTypeColumns { get; set; }
        public IEnumerable<Column> MergeIdentifierColumns { get; set; }
        public IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin { get; set; }

        public MergeVariables(
            string schemaName,
            Table table,
            IEnumerable<Column> tableTypeColumns,
            IEnumerable<Column> mergeIdentifierColumns,
            IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> tablesToJoin)
        {
            SchemaName = schemaName;
            Table = table;
            TableTypeColumns = tableTypeColumns;
            MergeIdentifierColumns = mergeIdentifierColumns;
            TablesToJoin = tablesToJoin;
        }

        public static IEnumerable<MergeVariables> From(DataStructure dataStructure)
        {
            var mergeVariables = new List<MergeVariables>();

            //var dependencyTree = TableTree.Build(schema);

            foreach(var table in dataStructure.Schema.Tables)
            {
                mergeVariables.Add(new MergeVariables(
                    dataStructure.Schema.Name,
                    table,
                    table.TableTypeColumns(dataStructure.Schema, dataStructure.DependencyTree),
                    table.MergeIdentifierColumns(dataStructure.Schema, dataStructure.DependencyTree),
                    table.TablesToJoin(dataStructure.Schema, dataStructure.DependencyTree)));
            }

            return mergeVariables;
        }
    }

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
                .Where(c => !c.ForeignKey.ConstrainedTo.Any())
                .ToList();

            var tableBranch = dependencyTree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

            var relatedBranches = dependencyTree
                .GetRelatedBranches(table);

            if(tableBranch != null)
            {
                /*
                 * lower branches
                 */
                foreach (var branch in relatedBranches.Where(b => b.Depth > tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    if(branchTable.Columns.Where(c => c.ForeignKey.ChildConstraints.Contains(table.Name)).Any())
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

            return tableTypeColumns.Distinct();
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
            if(table.ExtendedProperties != null &&
                table.ExtendedProperties.Any())
            {
                var mergeColumns = table.Columns.Where(c =>
                    table.ExtendedProperties.Any(e =>
                        e.Value.Equals(c.Name) &&
                        e.Name.Equals("MergeIdentifier"))
                    );

                identifyingColumns.AddRange(mergeColumns);
            }

            /*
             * if the table only has one column that isn't an identity
             * it's going to be the identifying column
             */
            if(table.Columns.Where(c => !c.IdentityColumn).Count().Equals(1))
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
                foreach (var branch in relatedBranches.Where(b => b.Depth < tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    var higherBranchIdentifyingColumns = branchTable.Columns.Where(c => c.IdentityColumn);

                    identifyingColumns.AddRange(higherBranchIdentifyingColumns);
                }

                /*
                 * higher branches
                 */
                foreach (var branch in relatedBranches.Where(b => b.Depth < tableBranch.Depth))
                {
                    var branchTable = schema.GetTableFrom(branch.TableGuid);

                    var higherBranchIdentifyingColumns = branchTable.Columns.Where(c => c.IdentityColumn);

                    identifyingColumns.AddRange(higherBranchIdentifyingColumns);
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

                    if(branchTable.Columns.Where(c => c.ForeignKey.ChildConstraints.Contains(table.Name)).Any())
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

            foreach(var tableInJoin in tablesInJoin)
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
