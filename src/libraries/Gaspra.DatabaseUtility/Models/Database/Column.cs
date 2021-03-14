using Gaspra.DatabaseUtility.Models.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gaspra.DatabaseUtility.Models.Database
{
    public class Column : IEquatable<Column>
    {
        public Guid CorrelationId { get; set; }
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
            Guid correlationId,
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
            CorrelationId = correlationId;
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

        public static IList<Column> From(
            string tableName,
            IReadOnlyCollection<ColumnInformation> columnInformation,
            IReadOnlyCollection<FKConstraintInformation> foreignKeyInformation)
        {
            var columns = columnInformation
                .Where(c => c.Table.Equals(tableName))
                .Select(c =>
                {
                    var columnsForeignKey = foreignKeyInformation
                        .Where(f => (f.ConstraintColumn.Equals(c.Column) && f.ConstraintTable.Equals(tableName)) ||
                                    (f.ReferencedColumn.Equals(c.Column) && f.ReferencedTable.Equals(tableName)))
                        .FirstOrDefault();

                    ForeignKeyConstraint foreignKey = null;

                    if(foreignKeyInformation.Any(f => f.ConstraintTable.Equals(tableName) || f.ReferencedTable.Equals(tableName)))
                    {
                        var parentOf = foreignKeyInformation
                            .Where(f => f.ConstraintTable.Equals(tableName) && f.ConstraintColumn.Equals(c.Column))
                            .Select(f => f.ReferencedTable);

                        var childOf = foreignKeyInformation
                            .Where(f => f.ReferencedTable.Equals(tableName) && f.ReferencedColumn.Equals(c.Column))
                            .Select(f => f.ConstraintTable);

                        foreignKey = new ForeignKeyConstraint(Guid.NewGuid(), childOf, parentOf);
                    }

                    int? maxLength = null;

                    maxLength = c.MaxLength;

                    if(c.DataType.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase)
                        && maxLength != null)
                    {
                        maxLength /= 2;
                    }

                    return new Column(
                        Guid.NewGuid(),
                        c.ColumnId,
                        c.Column,
                        c.Nullable,
                        c.Identity,
                        c.DataType,
                        maxLength,
                        c.Precision,
                        c.Scale,
                        c.SeedValue,
                        c.IncrementValue,
                        c.DefaultValue,
                        foreignKey
                        );
                })
                .ToList();

            return columns;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Column);
        }

        public bool Equals([AllowNull] Column other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(Column left, Column right)
        {
            return EqualityComparer<Column>.Default.Equals(left, right);
        }

        public static bool operator !=(Column left, Column right)
        {
            return !(left == right);
        }
    }
}
