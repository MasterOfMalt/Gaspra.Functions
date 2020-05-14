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

            foreach(var table in schema.Tables)
            {
                mergeVariables.Add(new MergeVariables(
                    schema.Name,
                    table,
                    table.TableTypeColumns(schema),
                    table.MergeIdentifierColumns(schema),
                    table.TablesToJoin(schema)));
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

        public static IEnumerable<Column> TableTypeColumns(this Table table, Schema schema)
        {
            var requiredColumns = table.Columns.Where(c => !c.IdentityColumn).Count();

            var tableTypeColumns = table.Columns
                /*
                 * get all columns which aren't identity columns
                 */
                .Where(c => !c.IdentityColumn)
                /*
                 * get all columns which aren't foreign key columns
                 */
                .Where(c => c.ForeignKey == null)
                .ToList();

            var dependencies = TableDependencies.From(table, schema);

            /*
             * using the dependencies calculate the columns required to fill the identifier columns
             */
            foreach (var foreignKeyColumn in table.Columns.Where(c => c.ForeignKey != null && c.ForeignKey.IsParent))
            {
                var parentTable = dependencies.ParentTables.Where(t => t.Columns.Any(c => c.Name.Equals(foreignKeyColumn.Name))).FirstOrDefault();

                if(parentTable != null)
                {
                    tableTypeColumns.AddRange(parentTable.MergeIdentifierColumns(schema));
                }
            }

            /*
            foreach (var foreignKeyColumn in table.Columns.Where(c => c.ForeignKey != null && !c.ForeignKey.IsParent))
            {
                var childTable = dependencies.ChildrenTables.Where(t => t.Columns.Any(c => c.Name.Equals(foreignKeyColumn.Name))).FirstOrDefault();

                if(childTable != null)
                {
                    tableTypeColumns.AddRange(childTable.MergeIdentifierColumns(schema));
                }
            }
            */

            return tableTypeColumns;
        }

        public static IEnumerable<Column> MergeIdentifierColumns(this Table table, Schema schema)
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
            if(table.Columns.Any(c => c.ForeignKey != null && c.ForeignKey.IsParent))
            {
                identifyingColumns.AddRange(table.Columns.Where(c => c.ForeignKey != null && c.ForeignKey.IsParent));
            }

            return identifyingColumns;
        }

        public static IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin(this Table table, Schema schema)
        {
            var tablesToJoin = new List<(Table, IEnumerable<Column>, IEnumerable<Column>)>();

            var dependencies = TableDependencies.From(table, schema);

            foreach(var dependency in dependencies.ParentTables)
            {
                var mergeIdentifierColumns = dependency.MergeIdentifierColumns(schema);

                var columnsToJoinOn = dependency.Columns.Where(c => mergeIdentifierColumns.Any(m => m.Name.Equals(c.Name)));

                var tableTypeColumns = table.TableTypeColumns(schema);

                var columnsToSelect = dependency.Columns.Where(c => c.IdentityColumn);

                tablesToJoin.Add((dependency, columnsToJoinOn, columnsToSelect));
            }

            return tablesToJoin;
        }
    }
}
