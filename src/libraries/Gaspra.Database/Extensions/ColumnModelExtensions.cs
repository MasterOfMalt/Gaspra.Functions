using Gaspra.Database.Models;

namespace Gaspra.Database.Extensions
{
    public static class ColumnModelExtensions
    {
        public static void AddConstraint(this ColumnModel column, string constraintName, ColumnModel reference)
        {
            column.Constraint = new ConstraintModel
            {
                Name = constraintName,
                Reference = reference,
                Parent = true
            };

            reference.Constraint = new ConstraintModel
            {
                Name = constraintName,
                Reference = column,
                Parent = false
            };
        }
    }
}
