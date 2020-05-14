using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.Interfaces;
using Gaspra.MergeSprocs.Miro;
using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Merge;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs
{
    public class MergeSprocsService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IDataAccess dataAccess;
        private readonly IMergeProcedureGenerator mergeProcedureGenerator;
        private readonly IConfiguration configuration;

        public MergeSprocsService(
            IDataAccess dataAccess,
            IMergeProcedureGenerator mergeProcedureGenerator,
            IConfiguration configuration,
            ILogger<MergeSprocsService> logger)
        {
            this.dataAccess = dataAccess;
            this.mergeProcedureGenerator = mergeProcedureGenerator;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                logger.LogInformation($"{nameof(MergeSprocsService)} cancellation token called");
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                /*
                 * build up database objects
                 */
                var columnInfo = await dataAccess.GetColumnInformation();

                var fkInfo = await dataAccess.GetFKConstraintInformation();

                var extendedProps = await dataAccess.GetExtendedProperties();

                var database = Schema.From(columnInfo, extendedProps, fkInfo);

                /*
                 * save them out to json/ miro
                 */
                WriteFile(
                    "analytics.database.json",
                    JsonConvert.SerializeObject(
                        database,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));

                if (bool.Parse(configuration.GetSection("miro")["draw"]))
                {
                    var miroDraw = new MiroDraw();
                    await miroDraw.Draw(database.First());
                }

                /*
                 * build up merge variables
                 */
                var mergeVariables = MergeVariables.From(database.First());

                WriteFile(
                    "analytics.database.mergevariables.json",
                    JsonConvert.SerializeObject(
                        mergeVariables,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));

                break;
            }
        }

        private void WriteFile(string fileName, string fileContents)
        {
            var outputDirectory = $"{Directory.GetCurrentDirectory()}/.output/";

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            File.WriteAllText($"{outputDirectory}/{fileName}", fileContents);
        }
    }
}