using Gaspra.Database.Models;

namespace Gaspra.Database.Extensions
{
    public static class ColumnModelExtensions
    {
        public static void AddConstraint(this ColumnModel column, string constraintName, ColumnModel reference, bool isParent)
        {
            column.Constraint = new ConstraintModel
            {
                Name = constraintName,
                Reference = reference,
                Parent = isParent
            };
        }
    }
}
