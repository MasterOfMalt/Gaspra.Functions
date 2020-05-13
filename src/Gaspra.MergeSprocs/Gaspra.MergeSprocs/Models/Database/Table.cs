using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class Table
    {
        public string Name { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<ExtendedProperty> ExtendedProperties { get; set; }

        public Table(
            string name,
            IEnumerable<Column> columns,
            IEnumerable<ExtendedProperty> extendedProperties)
        {
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
                                e.PropertyName, e.Value
                            ));

                    return new Table(
                            c.TableName,
                            Column.From(c.TableName, columnInformation, foreignKeyConstraintInformation),
                            extendedProperties
                        );

                });

            return tables;
        }
    }
}
