using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using Gaspra.Database.Models.QueryResults;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.Database.Services
{
    public class DatabaseStructure : IDatabaseStructure
    {
        public async Task<DatabaseModel> CalculateStructure(DatabaseResult databaseResult)
        {
            // Build up all the schemas, tables and columns
            var schemaModels = databaseResult
                .Tables
                .Select(t => t.Schema)
                .Distinct()
                .Select(s => new SchemaModel { Name = s })
                .ToList();

            var calculatedSchema = new List<SchemaModel>();

            foreach (var schema in schemaModels)
            {
                var tableResults = databaseResult
                    .Tables
                    .Where(t => t.Schema.Equals(schema.Name))
                    .Distinct(TableResultComparison.Instance)
                    .ToList();

                var tableModels = await CreateTables(tableResults);

                foreach (var table in tableModels)
                {
                    var tableColumnResults = databaseResult
                        .Tables
                        .Where(t => t.Schema.Equals(schema.Name) && t.Table.Equals(table.Name))
                        .ToList();

                    var columnModels = await CreateColumns(tableColumnResults);

                    table.Columns = columnModels;
                }

                schema.Tables = tableModels;

                calculatedSchema.Add(schema);
            }

            // Add constraints to the columns
            foreach (var constraint in databaseResult.Constraints)
            {
                var constraintTables = schemaModels
                    .Where(s => s.Name.Equals(constraint.ConstraintSchema))
                    .FirstOrDefault()
                    .Tables;

                var constraintColumns = constraintTables
                    .Where(t => t.Name.Equals(constraint.ConstraintTable))
                    .FirstOrDefault()
                    .Columns;

                var constraintColumn = constraintColumns
                    .Where(c => c.Name.Equals(constraint.ConstraintColumn))
                    .FirstOrDefault();

                var referenceTables = schemaModels
                    .Where(s => s.Name.Equals(constraint.ConstraintSchema))
                    .FirstOrDefault()
                    .Tables;

                var referenceColumns = referenceTables
                    .Where(t => t.Name.Equals(constraint.ReferencedTable))
                    .FirstOrDefault()
                    .Columns;

                var referenceColumn = referenceColumns
                    .Where(c => c.Name.Equals(constraint.ReferencedColumn))
                    .FirstOrDefault();

                constraintColumn.AddConstraint(constraint.ConstraintName, referenceColumn, true);

                referenceColumn.AddConstraint(constraint.ConstraintName, constraintColumn, false);
            }

            // Add properties to the tables
            foreach (var property in databaseResult.Properties)
            {
                var propertyTables = schemaModels
                    .Where(s => s.Name.Equals(property.Schema))
                    .FirstOrDefault()
                    .Tables;

                var propertyTable = propertyTables
                    .Where(t => t.Name.Equals(property.Table))
                    .FirstOrDefault();

                if(propertyTable != null)
                {
                    propertyTable.AddProperty(property.Key, property.Value);
                }
            }

            // return database model
            var databaseModel = new DatabaseModel
            {
                Name = "todo",
                Schemas = calculatedSchema
            };

            return databaseModel;
        }

        private Task<IReadOnlyCollection<TableModel>> CreateTables(IReadOnlyCollection<TableResult> tableResults)
        {
            var tables = new List<TableModel>();

            foreach (var tableResult in tableResults)
            {
                var tableModel = new TableModel
                {
                    Name = tableResult.Table
                };

                tables.Add(tableModel);
            }

            return Task.FromResult((IReadOnlyCollection<TableModel>)tables);
        }

        private Task<IReadOnlyCollection<ColumnModel>> CreateColumns(IReadOnlyCollection<TableResult> tableResults)
        {
            var columns = new List<ColumnModel>();

            foreach (var tableResult in tableResults)
            {
                var columnModel = new ColumnModel
                {
                    Id = tableResult.ColumnId,
                    Name = tableResult.Column,
                    Nullable = tableResult.Nullable,
                    IdentityColumn = tableResult.Identity,
                    DataType = tableResult.DataType,
                    MaxLength = tableResult.MaxLength,
                    Precision = tableResult.Precision,
                    Scale = tableResult.Scale,
                    SeedValue = tableResult.SeedValue,
                    IncrementValue = tableResult.IncrementValue,
                    DefaultValue = tableResult.DefaultValue
                };

                columns.Add(columnModel);
            }

            return Task.FromResult((IReadOnlyCollection<ColumnModel>)columns);
        }
    }
}
