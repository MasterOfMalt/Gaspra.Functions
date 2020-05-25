//using ConsoleAppFramework;
//using Gaspra.DatabaseUtility.Interfaces;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading.Tasks;

//namespace Gaspra.Functions.Bases
//{
//    public class DatabaseUtilityBase : ConsoleAppBase
//    {
//        private readonly ILogger logger;
//        private readonly IJsonDatabaseService jsonDatabaseService;

//        public DatabaseUtilityBase(
//            ILogger<DatabaseUtilityBase> logger,
//            IJsonDatabaseService jsonDatabaseService)
//        {
//            this.logger = logger;
//            this.jsonDatabaseService = jsonDatabaseService;
//        }

//        [Command(new[] { "DatabaseToJson", "dtj" }, "Serialize a database to json")]
//        public async Task Run(
//            [Option("c", "connection string")]string connectionString = "",
//            [Option("s", "schemas to serialize")]string schemas = ""
//            )
//        {
//            var json = await jsonDatabaseService.SerializeDatabaseToJson(connectionString, new[] { schemas });

//            logger.LogInformation(json);
//        }
//    }
//}
