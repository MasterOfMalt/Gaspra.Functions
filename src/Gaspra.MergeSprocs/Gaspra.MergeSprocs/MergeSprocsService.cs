using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Miro;
using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Merge;
using Gaspra.MergeSprocs.Models.Tree;
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
        private readonly IConfiguration configuration;

        public MergeSprocsService(
            IDataAccess dataAccess,
            IConfiguration configuration,
            ILogger<MergeSprocsService> logger)
        {
            this.dataAccess = dataAccess;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                logger.LogInformation($"{nameof(MergeSprocsService)} cancellation token called");
            });

            /*
             * build up database objects
             */
            var columnInfo = (await dataAccess.GetColumnInformation()).ToList();

            var fkInfo = (await dataAccess.GetFKConstraintInformation()).ToList();

            var extendedProps = (await dataAccess.GetExtendedProperties()).ToList();

            var database = Schema
                .From(columnInfo, extendedProps, fkInfo);

            database
                .First()
                .CalculateDependencies();

            /*
             * save them out to json
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

            logger.LogInformation("output: analytics.database.json");

            /*
             * calculate dependency tree and build data structure
             */
            var dependencyTree = DependencyTree
                .Calculate(database.First());

            var dataStructure = new DataStructure(database.First(), dependencyTree);

            logger.LogInformation("calculated data structure");

            /*
             * save data structure out to json
             */
            WriteFile(
                "analytics.datastructure.json",
                JsonConvert.SerializeObject(
                    dataStructure,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));

            logger.LogInformation("output: analytics.datastructure.json");

            var drawDataStructure = new DrawDataStructure();
            await drawDataStructure.DrawToMiro(dataStructure);

            /*
             * Calculate tree of tables
             */
            var tree = TableTree.Build(database.First());

            logger.LogInformation("calculated table tree");

            WriteFile(
                "analytics.database.tabletree.json",
                JsonConvert.SerializeObject(
                    tree,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));

            logger.LogInformation("output: analytics.database.tabletree.json");

            /*
             * build up merge variables
             */
            var mergeVariables = MergeVariables.From(database.First());

            logger.LogInformation("calculated merge variables");

            WriteFile(
                "analytics.database.mergevariables.json",
                JsonConvert.SerializeObject(
                    mergeVariables,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));

            logger.LogInformation("output: analytics.database.mergevariables.json");

            /*
             * Create merge statements
             */

            foreach(var mergeVariable in mergeVariables)
            {
                var mergeStatement = mergeVariable.BuildMergeSproc();

                var fileName = $"{mergeVariable.SchemaName}.{mergeVariable.ProcedureName()}.sql";

                WriteFile(fileName, mergeStatement);

                logger.LogInformation("output: {filename}", fileName);
            }

            /*
             * Draw to miro
             */
            if (bool.Parse(configuration.GetSection("miro")["draw"]))
            {
                var miroDraw = new MiroDraw();
                await miroDraw.Draw(database.First(), tree);

                logger.LogInformation("drawn miro objects");
            }

            logger.LogInformation("Done!");

            return;
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