using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using Gaspra.Database.Models.QueryResults;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.Database.Services
{
    public class DatabaseStructure : IDatabaseStructure
    {
        public async Task<DatabaseModel> CalculateStructure(string databaseName, DatabaseResult databaseResult)
        {
            // Create database structure
            var schemaModels = databaseResult
                .Tables
                .Select(t => t.Schema)
                .Distinct()
                .Select(s => new SchemaModel { Name = s })
                .ToList();

            foreach (var schema in schemaModels)
            {
                var tableResults = databaseResult
                    .Tables
                    .Where(t => t.Schema.Equals(schema.Name))
                    .Distinct(TableResultComparison.Instance)
                    .ToList();

                var tableModels = await tableResults.CreateTables();

                foreach (var table in tableModels)
                {
                    var tableColumnResults = databaseResult
                        .Tables
                        .Where(t => t.Schema.Equals(schema.Name) && t.Table.Equals(table.Name))
                        .ToList();

                    var columnModels = await tableColumnResults.CreateColumns();

                    table.Columns = columnModels;
                }

                schema.Tables = tableModels;
            }

            // Add constraints to columns and tables
            foreach (var constraint in databaseResult.Constraints)
            {
                var constraintTable = schemaModels
                    .FirstOrDefault(s => s.Name.Equals(constraint.ConstraintSchema))?
                    .Tables
                    .FirstOrDefault(t => t.Name.Equals(constraint.ConstraintTable));

                var constraintColumn = constraintTable?
                    .Columns
                    .FirstOrDefault(c => c.Name.Equals(constraint.ConstraintColumn));

                var referenceTable = schemaModels
                    .FirstOrDefault(s => s.Name.Equals(constraint.ConstraintSchema))?
                    .Tables
                    .FirstOrDefault(t => t.Name.Equals(constraint.ReferencedTable));

                var referenceColumn = referenceTable?
                    .Columns
                    .FirstOrDefault(c => c.Name.Equals(constraint.ReferencedColumn));

                constraintColumn.AddConstraint(constraint.ConstraintName, referenceColumn);

                constraintTable.AddDependantTable(referenceTable);

                referenceTable.AddReferenceTable(constraintTable);
            }

            // Add properties to the tables
            foreach (var property in databaseResult.Properties)
            {
                var propertyTables = schemaModels
                    .FirstOrDefault(s => s.Name.Equals(property.Schema))?
                    .Tables;

                var propertyTable = propertyTables?
                    .FirstOrDefault(t => t.Name.Equals(property.Table));

                if (propertyTable != null)
                {
                    propertyTable.AddProperty(property.Key, property.Value);
                }
            }

            // Create database model
            var databaseModel = new DatabaseModel
            {
                Name = databaseName,
                Schemas = schemaModels
            };

            // Map table depths
            await databaseModel.CalculateTableDepth();

            // Return model
            return databaseModel;
        }
    }
}
