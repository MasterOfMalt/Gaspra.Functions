using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class TableDependencies
    {
        public Table CurrentTable { get; set; }
        public IEnumerable<Table> ConstrainedToTables { get; set; }

        public TableDependencies(
            Table currentTable,
            IEnumerable<Table> constrainedToTables)
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
                    .Contains(t.Name));

            return new TableDependencies(
                table,
                constrainedToTables);
        }
    }
}
