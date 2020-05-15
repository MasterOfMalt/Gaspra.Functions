using Gaspra.MergeSprocs.Models.Database;
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

        public static IEnumerable<MergeVariables> From(Schema schema)
        {
            var mergeVariables = new List<MergeVariables>();

            var dependencyTree = TableTree.Build(schema);

            foreach(var table in schema.Tables)
            {
                mergeVariables.Add(new MergeVariables(
                    schema.Name,
                    table,
                    table.TableTypeColumns(schema, dependencyTree),
                    table.MergeIdentifierColumns(schema, dependencyTree),
                    table.TablesToJoin(schema, dependencyTree)));
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

        public static IEnumerable<Column> TableTypeColumns(this Table table, Schema schema, TableTree dependencyTree)
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

            var (depth, dependencies) = dependencyTree.Branches.Where(b => b.dependencies.CurrentTable.Name.Equals(table.Name)).FirstOrDefault();

            /*
             * take the value from the table below it
             */
            if (table.Columns.Any(c => c.ForeignKey != null))
            {
                var lowerBranches = dependencyTree.Branches
                    .Where(b => b.depth > depth);

                var lowerRelevantBranches = lowerBranches
                    .Where(b => b.dependencies.CurrentTable.Columns.Any(c => table.Columns.Any(tc => c.Name.Equals(tc.Name))));

                tableTypeColumns.AddRange(lowerRelevantBranches.SelectMany(t => t.dependencies.CurrentTable.Columns.Where(c => !c.IdentityColumn)));
            }

            /*
             * take the id from the table above it
             */
            if (table.Columns.Any(c => c.ForeignKey != null))
            {
                var higherBranches = dependencyTree.Branches
                    .Where(b => b.depth < depth);

                var higherRelevantBranches = higherBranches
                    .Where(b => b.dependencies.CurrentTable.Columns.Any(c => table.Columns.Any(tc => c.Name.Equals(tc.Name))));

                tableTypeColumns.AddRange(higherRelevantBranches.SelectMany(t => t.dependencies.CurrentTable.Columns.Where(c => c.IdentityColumn)));
            }

            return tableTypeColumns;
        }

        public static IEnumerable<Column> MergeIdentifierColumns(this Table table, Schema schema, TableTree dependencyTree)
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
             * add child tables as part of the identifying columns
             *
             * todo: this could be incorrect
             */
            var (depth, dependencies) = dependencyTree.Branches.Where(b => b.dependencies.CurrentTable.Name.Equals(table.Name)).FirstOrDefault();

            if(table.Columns.Any(c => c.ForeignKey != null))
            {
                var higherBranches = dependencyTree.Branches
                    .Where(b => b.depth < depth);

                var higherRelevantBranches = higherBranches
                    .Where(b => b.dependencies.CurrentTable.Columns.Any(c => table.Columns.Any(tc => c.Name.Equals(tc.Name))));

                var includeContraints = table.Columns.Where(c => c.ForeignKey != null && higherRelevantBranches.Any(b => c.ForeignKey.ConstrainedTo.Contains(b.dependencies.CurrentTable.Name)));

                identifyingColumns.AddRange(includeContraints);
            }

            return identifyingColumns;
        }

        public static IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin(this Table table, Schema schema, TableTree dependencyTree)
        {
            var tablesToJoin = new List<(Table, IEnumerable<Column>, IEnumerable<Column>)>();

            var (depth, dependencies) = dependencyTree.Branches.Where(b => b.dependencies.CurrentTable.Name.Equals(table.Name)).FirstOrDefault();

            if (table.Columns.Any(c => c.ForeignKey != null))
            {
                var lowerBranches = dependencyTree.Branches
                    .Where(b => b.depth > depth);

                var lowerRelevantBranches = lowerBranches
                    .Where(b => b.dependencies.CurrentTable.Columns.Any(c => table.Columns.Any(tc => c.Name.Equals(tc.Name))));


                var tablesNeededInJoin = lowerRelevantBranches.Select(b => b.dependencies.CurrentTable);

                foreach(var tableForJoin in tablesNeededInJoin)
                {
                    /*
                     * todo:
                     * need to get teh specific columns required for the data and not every column that isn't an identity column
                     */
                    tablesToJoin.Add((tableForJoin, tableForJoin.Columns.Where(c => c.IdentityColumn), tableForJoin.Columns.Where(c => !c.IdentityColumn)));
                }
            }

            return tablesToJoin;
        }
    }
}
