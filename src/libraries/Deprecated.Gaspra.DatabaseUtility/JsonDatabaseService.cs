using Deprecated.Gaspra.DatabaseUtility.Extensions;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Interfaces;
using Newtonsoft.Json;

namespace Deprecated.Gaspra.DatabaseUtility
{
    public class JsonDatabaseService : IJsonDatabaseService
    {
        private readonly ILogger logger;
        private readonly IDataAccess dataAccess;

        public JsonDatabaseService(
            ILogger<JsonDatabaseService> logger,
            IDataAccess dataAccess)
        {
            this.logger = logger;
            this.dataAccess = dataAccess;
        }

        public async Task<string> SerializeDatabaseToJson(string connectionString, IEnumerable<string> schemaNames)
        {
            return "";
            //var columnInformation = await dataAccess.GetColumnInformation(connectionString);
            //
            //var foreignKeyInformation = await dataAccess.GetFKConstraintInformation(connectionString);
            //
            //var extendedPropertyInformation = await dataAccess.GetExtendedProperties(connectionString);
            //
            //var databaseSchemas = Schema
            //    .From(columnInformation, extendedPropertyInformation, foreignKeyInformation)
            //    .Where(s => schemaNames.Contains(s.Name));
            //
            //foreach(var schema in databaseSchemas)
            //{
            //    schema.CalculateDependencies();
            //}
            //
            //var json = JsonConvert.SerializeObject(
            //    databaseSchemas,
            //    Formatting.Indented,
            //    new JsonSerializerSettings
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });
            //
            //return json;
        }
    }
}
