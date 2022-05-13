using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;
using Microsoft.Extensions.Logging;

namespace Gaspra.SqlGenerator.Services
{
    public class DatabaseToJsonGenerator : IDatabaseToJsonGenerator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseStructure _databaseStructure;
        private readonly IDataAccess _dataAccess;

        public DatabaseToJsonGenerator(
            ILogger<DatabaseToJsonGenerator> logger,
            IDataAccess dataAccess,
            IDatabaseStructure databaseStructure)
        {
            _logger = logger;
            _databaseStructure = databaseStructure;
            _dataAccess = dataAccess;
        }

        public async Task<string> Generate(string connectionString, IReadOnlyCollection<string> schemas)
        {
            // Get the database to generate scripts for
            DatabaseModel database;

            try
            {
                var databaseResult = await _dataAccess.GetDatabase(connectionString, schemas);

                var databaseName = connectionString.DatabaseName();

                database = await _databaseStructure.CalculateStructure(databaseName, databaseResult);

                _logger.LogInformation(
                    "Constructed [{databaseName}] database object with [{tableCount}] tables",
                    databaseName,
                    database.Schemas.SelectMany(s => s.Tables).Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to get database: [{databaseName}]",
                    connectionString.DatabaseName());

                return null;
            }

            var serializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                WriteIndented = true
            };

            var jsonDatabase = JsonSerializer.Serialize(database.ToJsonDatabase(), serializerOptions);

            return jsonDatabase;
        }
    }

    public class JsonColumn
    {
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public bool IdentityColumn { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string SeedValue { get; set; }
        public string IncrementValue { get; set; }
        public string DefaultValue { get; set; }
        public IReadOnlyCollection<JsonConstraint> Constraints { get; set; }
    }

    public class JsonConstraint
    {
        public string Name { get; set; }
        public bool Parent { get; set; }
        public string ReferenceName { get; set; }
    }

    public class JsonProperty
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class JsonTable
    {
        public string Name { get; set; }
        public IReadOnlyCollection<JsonColumn> Columns { get; set; }
        public IReadOnlyCollection<JsonProperty> Properties { get; set; }
    }

    public class JsonSchema
    {
        public string Name { get; set; }
        public IReadOnlyCollection<JsonTable> Tables { get; set; }
    }

    public class JsonDatabase
    {
        public string Name { get; set; }
        public IReadOnlyCollection<JsonSchema> Schemas { get; set; }
    }

    public static class JsonDatabaseExtensions
    {
        public static IReadOnlyCollection<JsonConstraint> ToJsonConstraints(this IReadOnlyCollection<ConstraintModel> constraintModels)
        {
            if (constraintModels != null && constraintModels.Any())
            {
                var jsonConstraints = constraintModels.Select(c => new JsonConstraint
                {
                    Name = c.Name,
                    Parent = c.Parent,
                    ReferenceName = c.Reference.Name
                }).ToList();

                return jsonConstraints;
            }

            return null;
        }

        public static IReadOnlyCollection<JsonProperty> ToJsonProperties(this IReadOnlyCollection<PropertyModel> propertyModels)
        {
            if (propertyModels != null && propertyModels.Any())
            {
                var jsonProperties = propertyModels.Select(p => new JsonProperty
                {
                    Key = p.Key,
                    Value = p.Value
                }).ToList();

                return jsonProperties;
            }

            return null;
        }

        public static IReadOnlyCollection<JsonColumn> ToJsonColumns(this IReadOnlyCollection<ColumnModel> columnModels)
        {
            if (columnModels != null && columnModels.Any())
            {
                var jsonColumns = columnModels.Select(c => new JsonColumn
                {
                    Name = c.Name,
                    Nullable = c.Nullable,
                    IdentityColumn = c.IdentityColumn,
                    DataType = c.DataType,
                    MaxLength = c.MaxLength,
                    Precision = c.Precision,
                    Scale = c.Scale,
                    SeedValue = c.SeedValue,
                    IncrementValue = c.IncrementValue,
                    DefaultValue = c.DefaultValue,
                    Constraints = c.Constraints.ToJsonConstraints()
                }).ToList();

                return jsonColumns;
            }

            return null;
        }

        public static IReadOnlyCollection<JsonTable> ToJsonTables(this IReadOnlyCollection<TableModel> tableModels)
        {
            if (tableModels != null && tableModels.Any())
            {
                var jsonTables = tableModels.Select(t => new JsonTable
                {
                    Name = t.Name,
                    Columns = t.Columns.ToJsonColumns(),
                    Properties = t.Properties != null ? t.Properties.ToList().ToJsonProperties() : null
                }).ToList();

                return jsonTables;
            }

            return null;
        }

        public static IReadOnlyCollection<JsonSchema> ToJsonSchemas(this IReadOnlyCollection<SchemaModel> schemaModels)
        {
            var jsonSchemas = schemaModels.Select(s => new JsonSchema
            {
                Name = s.Name,
                Tables = s.Tables.ToJsonTables()
            }).ToList();

            return jsonSchemas;
        }

        public static JsonDatabase ToJsonDatabase(this DatabaseModel databaseModel)
        {
            var jsonDatabase = new JsonDatabase
            {
                Name = databaseModel.Name,
                Schemas = databaseModel.Schemas.ToJsonSchemas()
            };

            return jsonDatabase;
        }
    }
}
