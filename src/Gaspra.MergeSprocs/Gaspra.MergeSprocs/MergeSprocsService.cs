using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.Interfaces;
using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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

        public MergeSprocsService(
            IDataAccess dataAccess,
            IMergeProcedureGenerator mergeProcedureGenerator,
            ILogger<MergeSprocsService> logger)
        {
            this.dataAccess = dataAccess;
            this.mergeProcedureGenerator = mergeProcedureGenerator;
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
                var columnInfo = await dataAccess.GetColumnInformation();

                var fkInfo = await dataAccess.GetFKConstraintInformation();

                var extendedProps = await dataAccess.GetExtendedProperties();

                var database = Schema.From(columnInfo, extendedProps, fkInfo);

                //var miroDraw = new MiroDraw();
                //await miroDraw.Draw(database.First());

                WriteFile(
                    "analytics.database.json",
                    JsonConvert.SerializeObject(
                        database,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));







                //var tables = DatabaseTable.From(
                //    columnInfo,
                //    fkInfo,
                //    extendedProps);
                //
                //var mergeProcedures = mergeProcedureGenerator.Generate(tables);
                //
                //WriteFile(
                //    "merge.json",
                //    JsonConvert.SerializeObject(
                //        mergeProcedures,
                //        Formatting.Indented,
                //        new JsonSerializerSettings
                //        {
                //            NullValueHandling = NullValueHandling.Ignore
                //        }));
                //
                //foreach(var mergeProcedure in mergeProcedures)
                //{
                //    WriteFile(
                //        $"Analytics.{mergeProcedure.ProcedureName}.sql",
                //        mergeProcedureGenerator.GenerateMergeProcedure(mergeProcedure));
                //}

                return;
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