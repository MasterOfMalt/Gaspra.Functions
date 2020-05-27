using Gaspra.DatabaseUtility.Models.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gaspra.DatabaseUtility.Models.Database
{
    public class Table : IEquatable<Table>
    {
        public Guid CorrelationId { get; set; }
        public string Name { get; set; }
        public IList<Column> Columns { get; set; }
        public IList<ExtendedProperty> ExtendedProperties { get; set; }
        public IList<Guid> ConstrainedTo { get; set; }

        public Table(
            Guid correlationId,
            string name,
            IList<Column> columns,
            IList<ExtendedProperty> extendedProperties)
        {
            CorrelationId = correlationId;

            Name = name;
            Columns = columns;
            ExtendedProperties = extendedProperties;
        }

        public static IList<Table> From(
            IList<ColumnInformation> columnInformation,
            IList<ExtendedPropertyInformation> extendedPropertyInformation,
            IList<FKConstraintInformation> foreignKeyConstraintInformation)
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
                            ))
                        .ToList();

                    return new Table(
                            Guid.NewGuid(),
                            c.TableName,
                            Column.From(c.TableName, columnInformation, foreignKeyConstraintInformation),
                            extendedProperties
                        );
                })
                .ToList();

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
