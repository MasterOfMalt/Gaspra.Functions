using Gaspra.MergeSprocs.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.MergeSprocs.Models.Merge
{
    public class TableDependencies
    {
        public Table CurrentTable { get; set; }
        public IList<Table> ConstrainedToTables { get; set; }

        public TableDependencies(
            Table currentTable,
            IList<Table> constrainedToTables)
        {
            CurrentTable = currentTable;
            ConstrainedToTables = constrainedToTables;
        }

        public static TableDependencies From(Table table, Schema schema)
        {
            var foreignKeys = table.Columns.Select(c => c.ForeignKey).Where(f => f != null);

            var constrainedToTables = schema
                .Tables
                .Where(t => foreignKeys
                    .SelectMany(f => f.ConstrainedTo)
                    .Contains(t.Name))
                .ToList();

            return new TableDependencies(
                table,
                constrainedToTables);
        }
    }
}
