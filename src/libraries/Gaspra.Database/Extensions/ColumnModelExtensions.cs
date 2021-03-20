using System.Collections.Generic;
using System.Linq;
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
            var columnConstraints = new List<ConstraintModel>();

            if (column.Constraints != null)
            {
                columnConstraints.AddRange(column.Constraints);
            }

            columnConstraints.Add(new ConstraintModel
            {
                Name = constraintName,
                Reference = reference,
                Parent = true
            });

            column.Constraints = columnConstraints;

            if (reference != null)
            {
                var referenceConstraints = new List<ConstraintModel>();

                if (reference.Constraints != null)
                {
                    referenceConstraints.AddRange(reference.Constraints);
                }

                referenceConstraints.Add(new ConstraintModel
                {
                    Name = constraintName,
                    Reference = column,
                    Parent = false
                });

                reference.Constraints = referenceConstraints;
            }
        }
    }
}
