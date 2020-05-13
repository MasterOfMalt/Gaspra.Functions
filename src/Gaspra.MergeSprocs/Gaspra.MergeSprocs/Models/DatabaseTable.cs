using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models
{
    public class DatabaseTable
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IEnumerable<DatabaseColumn> Columns { get; set; }
        public IEnumerable<DatabaseForeignKeyConstraint> ForeignKeys { get; set; }
        public IEnumerable<ExtendedPropertyInformation> ExtendedProperties { get; set; }

        public DatabaseTable(
            string schema,
            string name,
            IEnumerable<DatabaseColumn> columns,
            IEnumerable<DatabaseForeignKeyConstraint> foreignKeys,
            IEnumerable<ExtendedPropertyInformation> extendedProperties)
        {
            Schema = schema;
            Name = name;
            Columns = columns;
            ForeignKeys = foreignKeys;
            ExtendedProperties = extendedProperties;
        }

        public static IEnumerable<DatabaseTable> From(
            IEnumerable<ColumnInformation> columnInformation,
            IEnumerable<FKConstraintInformation> fkConstraintInformation,
            IEnumerable<ExtendedPropertyInformation> extendedProperties)
        {
            var databaseTables = new List<DatabaseTable>();

            foreach (var columnGroup in columnInformation.GroupBy(g => new { g.TableName, g.TableSchema }))
            {
                var schema = columnGroup
                    .First()
                    .TableSchema;

                var name = columnGroup
                    .First()
                    .TableName;

                var columns = new List<DatabaseColumn>();

                foreach (var column in columnGroup)
                {
                    var databaseColumn = new DatabaseColumn
                    {
                        Id = column.ColumnId,
                        Name = column.ColumnName,
                        Nullable = column.Nullable,
                        IdentityColumn = column.IdentityColumn,
                        DataType = column.DataType,
                        MaxLength = column.MaxLength,
                        Precision = column.Precision,
                        Scale = column.Scale,
                        SeedValue = column.SeedValue,
                        IncrementValue = column.IncrementValue,
                        DefaultValue = column.DefaultValue
                    };

                    columns
                        .Add(databaseColumn);
                }

                var foreignKeys = new List<DatabaseForeignKeyConstraint>();

                foreach(var fkConstraint in fkConstraintInformation.Where(fk => fk.ConstraintTableSchema.Equals(schema) && fk.ConstraintTableName.Equals(name)))
                {
                    var foreignKey = new DatabaseForeignKeyConstraint
                    {
                        Name = fkConstraint.ConstraintName,
                        ConstraintColumn = fkConstraint.ConstraintTableColumn,
                        ReferenceSchema = fkConstraint.ReferencedTableSchema,
                        ReferenceTable = fkConstraint.ReferencedTableName,
                        ReferenceColumn = fkConstraint.ReferencedTableColumn
                    };

                    foreignKeys
                        .Add(foreignKey);
                }

                var extendedProps = extendedProperties.Where(e => e.ObjectName.Equals(name));

                databaseTables
                    .Add(new DatabaseTable(
                        schema,
                        name,
                        columns,
                        foreignKeys,
                        extendedProps));
            }

            return databaseTables;
        }
    }
}
