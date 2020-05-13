using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class Schema
    {
        public string Name { get; set; }
        public IEnumerable<Table> Tables { get; set; }

        public Schema(
            string name,
            IEnumerable<Table> tables)
        {
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
                        c.TableSchema,
                        Table.From(
                            columnInformation,
                            extendedPropertyInformation,
                            foreignKeyConstraintInformation)
                        );
                });

            return schemas;
        }
    }
}
