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
                    .Where(s => s.Name.Equals(constraint.ConstraintSchema))
                    .FirstOrDefault()
                    .Tables
                    .Where(t => t.Name.Equals(constraint.ConstraintTable))
                    .FirstOrDefault();

                var constraintColumn = constraintTable
                    .Columns
                    .Where(c => c.Name.Equals(constraint.ConstraintColumn))
                    .FirstOrDefault();

                var referenceTable = schemaModels
                    .Where(s => s.Name.Equals(constraint.ConstraintSchema))
                    .FirstOrDefault()
                    .Tables
                    .Where(t => t.Name.Equals(constraint.ReferencedTable))
                    .FirstOrDefault();

                var referenceColumn = referenceTable
                    .Columns
                    .Where(c => c.Name.Equals(constraint.ReferencedColumn))
                    .FirstOrDefault();

                constraintColumn.AddConstraint(constraint.ConstraintName, referenceColumn);

                constraintTable.AddDependantTable(referenceTable);

                referenceTable.AddReferenceTable(constraintTable);
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
            await CalculateTableDepth(databaseModel);

            /*
                --debug
                var depths = databaseModel.Schemas.Where(s => s.Name.Equals("Analytics")).SelectMany(s => s.Tables).Select(t => new
                {
                    Name = t.Name,
                    Depth = t.Depth
                })
                .OrderBy(o => o.Depth)
                .ToList();
            */

            // Return model
            return databaseModel;
        }

        private async Task CalculateTableDepth(DatabaseModel databaseModel)
        {
            var depth = 1;

            foreach (var schema in databaseModel.Schemas)
            {
                var topTables = schema
                    .Tables
                    .Where(t => t.Properties.ContainsKey("FactTable"))
                    .ToList();

                if(topTables != null && topTables.Any())
                {
                    await RecurseTableDepths(topTables, depth);
                }
            }
        }

        private async Task RecurseTableDepths(ICollection<TableModel> tables, int depth)
        {
            if (tables != null && tables.Any())
            {
                var nextDepth = depth + 1;

                await SetTableDepths(tables, depth);

                foreach (var table in tables)
                {
                    await RecurseTableDepths(await GetConnectingTables(table), nextDepth);
                }
            }
        }

        private Task SetTableDepths(ICollection<TableModel> tables, int depth)
        {
            foreach (var table in tables)
            {
                table.Depth = depth;
            }

            return Task.CompletedTask;
        }

        private Task<ICollection<TableModel>> GetConnectingTables(TableModel table)
        {
            var referenceTables = table
                .ReferenceTables?
                .Where(t => !t.Name.StartsWith("Link") && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var dependantTables = table
                .DependantTables?
                .Where(t => !t.Name.StartsWith("Link") && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var tables = referenceTables;

            tables.AddRange(dependantTables);

            tables = tables
                .Distinct(TableModelComparison.Instance)
                .ToList();

            return Task.FromResult((ICollection<TableModel>)tables);
        }
    }
}
