using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Deprecated.Gaspra.DatabaseUtility.Models.DataAccess;

namespace Deprecated.Gaspra.DatabaseUtility.Models.Database
{
    public class Schema : IEquatable<Schema>
    {
        public Guid CorrelationId { get; set; }
        public string Name { get; set; }
        public IList<Table> Tables { get; set; }

        public Schema(
            Guid correlationId,
            string name,
            IList<Table> tables)
        {
            CorrelationId = correlationId;

            Name = name;
            Tables = tables.ToList();
        }

        public static IList<Schema> From(
            IReadOnlyCollection<ColumnInformation> columnInformation,
            IReadOnlyCollection<ExtendedPropertyInformation> extendedPropertyInformation,
            IReadOnlyCollection<FKConstraintInformation> foreignKeyConstraintInformation)
        {
            var schemas = columnInformation
                .Distinct(new ColumnComparerBySchemaName())
                .Select(c =>
                {
                    return new Schema(
                        Guid.NewGuid(),
                        c.TableSchema,
                        Table.From(
                            columnInformation,
                            extendedPropertyInformation,
                            foreignKeyConstraintInformation)
                        );
                })
                .ToList();

            return schemas;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Schema);
        }

        public bool Equals([AllowNull] Schema other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(Schema left, Schema right)
        {
            return EqualityComparer<Schema>.Default.Equals(left, right);
        }

        public static bool operator !=(Schema left, Schema right)
        {
            return !(left == right);
        }
    }
}
