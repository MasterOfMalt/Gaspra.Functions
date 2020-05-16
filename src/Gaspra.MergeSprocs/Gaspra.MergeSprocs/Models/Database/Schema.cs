using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class Schema : IEquatable<Schema>
    {
        public Guid CorrelationId { get; set; }
        public string Name { get; set; }
        public IEnumerable<Table> Tables { get; set; }

        public Schema(
            Guid correlationId,
            string name,
            IEnumerable<Table> tables)
        {
            CorrelationId = correlationId;

            Name = name;
            Tables = tables;
        }

        public static IEnumerable<Schema> From(
            IEnumerable<ColumnInformation> columnInformation,
            IEnumerable<ExtendedPropertyInformation> extendedPropertyInformation,
            IEnumerable<FKConstraintInformation> foreignKeyConstraintInformation)
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
                });

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
