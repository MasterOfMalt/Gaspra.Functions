using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gaspra.Database.Models.QueryResults
{
    public class TableResult
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public int ColumnId { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string SeedValue { get; set; }
        public string IncrementValue { get; set; }
        public string DefaultValue { get; set; }
    }

    public class TableResultComparison : IEqualityComparer<TableResult>
    {
        public static IEqualityComparer<TableResult> Instance { get; } = new TableResultComparison();

        private TableResultComparison() { }

        public bool Equals([AllowNull] TableResult x, [AllowNull] TableResult y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.Schema.Equals(y.Schema) && x.Table.Equals(y.Table);
        }

        public int GetHashCode([DisallowNull] TableResult obj)
        {
            return HashCode.Combine(obj.Schema, obj.Table);
        }
    }
}
