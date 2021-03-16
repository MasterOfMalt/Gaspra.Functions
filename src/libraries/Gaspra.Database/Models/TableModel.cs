using Gaspra.Database.Interfaces;
using System.Collections.Generic;

namespace Gaspra.Database.Models
{
    public class TableModel : CorrelatedModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<ColumnModel> Columns { get; set; }
        public ICollection<PropertyModel> Properties { get; set; }
        /// <summary>
        /// Tables this table is dependant on for data to exist. A row must
        /// exist in the dependant table in order for a row to exist in this
        /// table, due to constraints.
        /// </summary>
        public ICollection<TableModel> DependantTables { get; set; }
        /// <summary>
        /// Tables this table is referenced in and rely upon for data to
        /// exist. A row must exist in this table for the reference tables
        /// to have a row, due to constraints.
        /// </summary>
        public ICollection<TableModel> ReferenceTables { get; set; }
    }
}
