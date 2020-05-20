using ConsoleAppFramework;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Models;
using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Merge;
using Gaspra.MergeSprocs.Models.Tree;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs
{
    public class MergeSprocsService : ConsoleAppBase
    {
        private readonly ILogger logger;
        private readonly IDataAccess dataAccess;

        public MergeSprocsService(
            IDataAccess dataAccess,
            ILogger<MergeSprocsService> logger)
        {
            this.dataAccess = dataAccess;
            this.logger = logger;
        }

        public async Task Run(
                [Option("c", "database connection string")] string connectionString,
                [Option("s", "schema name")] string schemaName,
                [Option("o", "output path")] string outputPath = @"*\.output",
                [Option("ij", "include the json files (default false)")] bool includeJson = false
            )
        {
            Schema databaseSchema = null;

            /*
             * build up database objects
             */
            try
            {
                var columnInfo = (await dataAccess.GetColumnInformation(connectionString)).ToList();

                var fkInfo = (await dataAccess.GetFKConstraintInformation(connectionString)).ToList();

                var extendedProps = (await dataAccess.GetExtendedProperties(connectionString)).ToList();

                databaseSchema = Schema
                    .From(columnInfo, extendedProps, fkInfo)
                    .Where(s => s.Name.Equals(schemaName))
                    .FirstOrDefault();

                databaseSchema
                    .CalculateDependencies();

                logger.LogInformation("Read schema information for [{schemaName}] with [{tableCount}] tables",
                    schemaName,
                    databaseSchema.Tables.Count());

                if(includeJson)
                {
                    WriteFile(
                        "analytics.database.json",
                        JsonConvert.SerializeObject(
                            databaseSchema,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }),
                        outputPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "Unable to calculate schema: [{schemaName}] due to: {ex}",
                    schemaName,
                    ex.Message);
            }

            /*
             * calculate dependency tree and build data structure
             */
            var dependencyTree = DependencyTree
                .Calculate(databaseSchema);

            var dataStructure = new DataStructure(databaseSchema, dependencyTree);

            if (includeJson)
            {
                WriteFile(
                    "analytics.datastructure.json",
                    JsonConvert.SerializeObject(
                        dataStructure,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }),
                    outputPath);
            }

            logger.LogInformation("Calculated dependency tree with [{branchCount}] branches",
                dependencyTree.Branches.Count());

            /*
             * build up merge variables
             */
            var (mergeVariables, errornousTables) = MergeVariables.From(dataStructure);

            if(errornousTables.Any())
            {
                logger.LogError("Tables that won't generate merge sprocs: [{tables}], due to exceptions: [{exceptions}]",
                    dataStructure.Schema.Tables.Select(t => t.Name).Except(mergeVariables.Select(m => m.Table.Name)),
                    errornousTables);
            }

            if (includeJson)
            {
                WriteFile(
                    "analytics.database.mergevariables.json",
                    JsonConvert.SerializeObject(
                        mergeVariables,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }),
                    outputPath);
            }

            logger.LogInformation("Calculated merge variables");

            /*
             * Create merge statements
             */
            foreach (var mergeVariable in mergeVariables)
            {
                var mergeStatement = mergeVariable.BuildMergeSproc();

                var fileName = $"{mergeVariable.SchemaName}.{mergeVariable.ProcedureName()}.sql";

                WriteFile(fileName, mergeStatement, outputPath);
            }
        }

        private void WriteFile(string fileName, string fileContents, string output)
        {
            var outputDirectory = output.Replace("*", $"{Directory.GetCurrentDirectory()}");

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (fileContents.Length == 0)
            {
                logger.LogError("Zero length file: {0}",fileName);
            }
            else
            {
                File.WriteAllText($@"{outputDirectory}\{fileName}", fileContents);

                logger.LogInformation($@"Saved: {outputDirectory}\{fileName}");
            }
        }
    }
}