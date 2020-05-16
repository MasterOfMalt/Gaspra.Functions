using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class Table : IEquatable<Table>
    {
        public Guid CorrelationId { get; set; }
        public string Name { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<ExtendedProperty> ExtendedProperties { get; set; }
        public IEnumerable<Guid> ConstrainedTo { get; set; }

        public Table(
            Guid correlationId,
            string name,
            IEnumerable<Column> columns,
            IEnumerable<ExtendedProperty> extendedProperties)
        {
            CorrelationId = correlationId;

            Name = name;
            Columns = columns;
            ExtendedProperties = extendedProperties;
        }

        public static IEnumerable<Table> From(
            IEnumerable<ColumnInformation> columnInformation,
            IEnumerable<ExtendedPropertyInformation> extendedPropertyInformation,
            IEnumerable<FKConstraintInformation> foreignKeyConstraintInformation)
        {
            var distinctTables = columnInformation.Distinct(new ColumnComparerByTableName());

            var tables = columnInformation
                .Distinct(new ColumnComparerByTableName())
                .Select(c =>
                {
                    var extendedProperties = extendedPropertyInformation
                        .Where(e => e.ObjectName.Equals(c.TableName))
                        .Select(e => new ExtendedProperty(
                                Guid.NewGuid(), e.PropertyName, e.Value
                            ));

                    return new Table(
                            Guid.NewGuid(),
                            c.TableName,
                            Column.From(c.TableName, columnInformation, foreignKeyConstraintInformation),
                            extendedProperties
                        );
                });

            return tables;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Table);
        }

        public bool Equals([AllowNull] Table other)
        {
            return other != null &&
                   CorrelationId.Equals(other.CorrelationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CorrelationId);
        }

        public static bool operator ==(Table left, Table right)
        {
            return EqualityComparer<Table>.Default.Equals(left, right);
        }

        public static bool operator !=(Table left, Table right)
        {
            return !(left == right);
        }
    }
}
