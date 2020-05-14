using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class TableDependencies
    {
        public IEnumerable<Table> ParentTables { get; set; }
        public IEnumerable<Table> ChildrenTables { get; set; }

        public TableDependencies(
            IEnumerable<Table> parentTables,
            IEnumerable<Table> childrenTables)
        {
            ParentTables = parentTables;
            ChildrenTables = childrenTables;
        }

        public static TableDependencies From(Table table, Schema schema)
        {
            var childForeignKeys = table.Columns.Select(c => c.ForeignKey).Where(f => f != null && !f.IsParent);

            var childTables = schema
                .Tables
                .Where(t => childForeignKeys
                    .SelectMany(f => f.ConstrainedTo)
                    .Contains(t.Name))
                //todo
                .Where(t => !t.Name.EndsWith("Link"));


            var parentForeignKeys = table.Columns.Select(c => c.ForeignKey).Where(f => f != null && f.IsParent);

            var parentTables = schema
                .Tables
                .Where(t => parentForeignKeys
                    .SelectMany(f => f.ConstrainedTo)
                    .Contains(t.Name))
                //todo
                .Where(t => !t.Name.EndsWith("Link"));

            return new TableDependencies(
                parentTables,
                childTables);
        }
    }
}
