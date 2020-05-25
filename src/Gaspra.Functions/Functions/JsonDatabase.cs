using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Gaspra.Functions.Bases
{
    public class JsonDatabase : IFunction
    {
        private readonly ILogger logger;
        private readonly ICorrelationContext cxt;
        private readonly IJsonDatabaseService jsonDatabaseService;

        public JsonDatabase(
            ILogger<JsonDatabase> logger,
            ICorrelationContext cxt,
            IJsonDatabaseService jsonDatabaseService)
        {
            this.cxt = cxt;
            this.logger = logger;
            this.jsonDatabaseService = jsonDatabaseService;
        }

        public async Task Run()
        {
            //use cxt to get parameters

            var json = await jsonDatabaseService.SerializeDatabaseToJson(
                "",
                new[] { "Analytics" });

            logger.LogInformation(json);
        }
    }
}
