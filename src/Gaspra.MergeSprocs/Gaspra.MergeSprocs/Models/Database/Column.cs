using Gaspra.MergeSprocs.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public bool IdentityColumn { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? SeedValue { get; set; }
        public int? IncrementValue { get; set; }
        public string DefaultValue { get; set; }
        public ForeignKeyConstraint ForeignKey { get; set; }

        public Column(
            int id,
            string name,
            bool nullable,
            bool identityColumn,
            string dataType,
            int? maxLength,
            int? precision,
            int? scale,
            int? seedValue,
            int? incrementValue,
            string defaultValue,
            ForeignKeyConstraint foreignKey)
        {
            Id = id;
            Name = name;
            Nullable = nullable;
            IdentityColumn = identityColumn;
            DataType = dataType;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            SeedValue = seedValue;
            IncrementValue = incrementValue;
            DefaultValue = defaultValue;
            ForeignKey = foreignKey;
        }

        public static IEnumerable<Column> From(
            string tableName,
            IEnumerable<ColumnInformation> columnInformation,
            IEnumerable<FKConstraintInformation> foreignKeyInformation)
        {
            var columns = columnInformation
                .Where(c => c.TableName.Equals(tableName))
                .Select(c =>
                {
                    var columnsForeignKey = foreignKeyInformation
                        .Where(f => (f.ConstraintTableColumn.Equals(c.ColumnName) && f.ConstraintTableName.Equals(tableName)) ||
                                    (f.ReferencedTableColumn.Equals(c.ColumnName) && f.ReferencedTableName.Equals(tableName)))
                        .FirstOrDefault();

                    ForeignKeyConstraint foreignKey = null;

                    if(columnsForeignKey != null)
                    {
                        var isParent = columnsForeignKey.ConstraintTableName.Equals(tableName);

                        var constrainedTo =
                            isParent ?
                                foreignKeyInformation
                                    .Where(f => f.ConstraintTableName.Equals(tableName) &&
                                                f.ConstraintTableColumn.Equals(c.ColumnName))
                                    .Select(f => f.ReferencedTableName) :
                                foreignKeyInformation
                                    .Where(f => f.ReferencedTableName.Equals(tableName) &&
                                                f.ReferencedTableColumn.Equals(c.ColumnName))
                                    .Select(f => f.ConstraintTableName);

                        foreignKey = new ForeignKeyConstraint(isParent, constrainedTo);
                    }

                    return new Column(
                        c.ColumnId,
                        c.ColumnName,
                        c.Nullable,
                        c.IdentityColumn,
                        c.DataType,
                        c.MaxLength,
                        c.Precision,
                        c.Scale,
                        c.SeedValue,
                        c.IncrementValue,
                        c.DefaultValue,
                        foreignKey
                        );
                });

            return columns;
        }
    }
}
