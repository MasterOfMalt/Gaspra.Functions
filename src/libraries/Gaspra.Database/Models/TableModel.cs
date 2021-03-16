using Gaspra.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gaspra.Database.Models
{
    public class TableModel : CorrelatedModel
    {
        /// <summary>
        /// Database table name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Database table columns
        /// </summary>
        public IReadOnlyCollection<ColumnModel> Columns { get; set; }
        /// <summary>
        /// Database table extended properties
        /// </summary>
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
        /// <summary>
        /// Tables depth in the dependancy chain, where 1 is the top most
        /// dependant table, with other tables. Depth = -1 means the depth
        /// has not been calculated/ can not be calculated for this table
        /// </summary>
        public int Depth { get; set; } = -1;
    }

    public class TableModelComparison : IEqualityComparer<TableModel>
    {
        public static IEqualityComparer<TableModel> Instance { get; } = new TableModelComparison();

        private TableModelComparison() { }

        public bool Equals([AllowNull] TableModel x, [AllowNull] TableModel y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.CorrelationId.Equals(y.CorrelationId);
        }

        public int GetHashCode([DisallowNull] TableModel obj)
        {
            return HashCode.Combine(obj.CorrelationId);
        }
    }
}
