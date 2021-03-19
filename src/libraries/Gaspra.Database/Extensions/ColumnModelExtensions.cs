using Gaspra.Database.Models;

namespace Gaspra.Database.Extensions
{
    public static class ColumnModelExtensions
    {
        /// <summary>
        /// Returns: [columnName] [columnType] (NOT) NULL. Example: [OrderId] [INT] NOT NULL
        /// </summary>
        /// <param name="column"></param>
        /// <param name="includeNullableFlag"></param>
        /// <returns></returns>
        public static string FullyQualifiedDescription(this ColumnModel column, bool includeNullableFlag = true)
        {
            return $"[{column.Name}] {column.DataType()} {column.NullableColumn()}";
        }

        public static string DataType(this ColumnModel column)
        {
            var dataType = $"[{column.DataType}]";

            if (column.DataType.Equals("decimal") && column.Precision.HasValue && column.Scale.HasValue)
            {
                dataType += $"({column.Precision.Value},{column.Scale.Value})";
            }
            else if (column.MaxLength.HasValue)
            {
                dataType += $"({column.MaxLength.Value})";
            }

            return dataType;
        }

        public static string NullableColumn(this ColumnModel column)
        {
            return column.Nullable ? "NULL" : "NOT NULL";
        }

        public static void AddConstraint(this ColumnModel column, string constraintName, ColumnModel reference)
        {
            column.Constraint = new ConstraintModel
            {
                Name = constraintName,
                Reference = reference,
                Parent = true
            };

            if (reference != null)
            {
                reference.Constraint = new ConstraintModel
                {
                    Name = constraintName,
                    Reference = column,
                    Parent = false
                };
            }
        }
    }
}
