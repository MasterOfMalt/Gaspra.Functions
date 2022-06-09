using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;
using Microsoft.Extensions.Logging;

namespace Gaspra.SqlGenerator.Services
{
    public class DeltaScriptGenerator : IDeltaScriptGenerator
    {
        private readonly ILogger _logger;
        private readonly IDatabaseStructure _databaseStructure;
        private readonly IDataAccess _dataAccess;
        private readonly IScriptVariableFactory _scriptVariableFactory;
        private readonly IScriptFactory<IDeltaScriptVariableSet> _scriptFactory;

        public DeltaScriptGenerator(
            ILogger<DeltaScriptGenerator> logger,
            IDataAccess dataAccess,
            IDatabaseStructure databaseStructure,
            IScriptVariableFactory scriptVariableFactory,
            IScriptFactory<IDeltaScriptVariableSet> scriptFactory)
        {
            _logger = logger;
            _databaseStructure = databaseStructure;
            _dataAccess = dataAccess;
            _scriptFactory = scriptFactory;
            _scriptVariableFactory = scriptVariableFactory;
        }

        public async Task<IReadOnlyCollection<SqlScript>> Generate(string connectionString, IReadOnlyCollection<string> schemas)
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

            // Generate delta script variable sets
            var deltaScriptVariableSets = new List<IDeltaScriptVariableSet>();

            try
            {
                var schema = database.Schemas.First();

                var tables = schema.Tables.ToList();

                var tableHistoryInformationList = tables
                    .Where(t => t.Properties != null &&
                                t.Properties.Any(p => p.Key.Equals("gf.Record")))
                    .Select(table => new TableHistoryInformation
                    {
                        Table = table,
                        HistoryTable = table.Properties.First(p => p.Key.Equals("gf.Record")).Value,
                        RootPath = table.PathToRoot()
                    })
                    .GroupBy(t => "Delta" + t.RootPath?.Last()?.Name + "From" + t.HistoryTable)
                    .ToList();

                foreach (var tableGroup in tableHistoryInformationList)
                {
                    var pathsToRoot = tableGroup.Select(t => t.RootPath).ToList();

                    var domainIdentifierColumn = pathsToRoot.First().Last().MergeIdentifierColumns(schema).First();

                    var deltaScriptVariableSet = new DeltaScriptVariableSet
                    {
                        ScriptFileName = $"{schema.Name}.{tableGroup.Key}.sql",
                        ScriptName = $"{tableGroup.Key}",
                        Schema = schema,
                        Table = null,
                        TableTypeName = $"TT_{tableGroup.Key}Include",
                        TableTypeColumns = new List<ColumnModel>
                        {
                            new ColumnModel()
                            {
                                Name = "Include",
                                DataType = "NVARCHAR",
                                MaxLength = 50
                            }
                        },
                        TableTypeVariableName = $"{tableGroup.Key}Include",
                        DomainIdentifierName = domainIdentifierColumn.Name,
                        TablePaths = pathsToRoot,
                        DeltaSourceTableName = tableGroup.First().HistoryTable
                    };

                    deltaScriptVariableSets.Add(deltaScriptVariableSet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to generate script variable sets");

                return null;
            }

            var deltaScripts = new List<SqlScript>();

            try
            {
                foreach (var deltaScriptVariableSet in deltaScriptVariableSets)
                {
                    var script = await _scriptFactory.ScriptFrom(deltaScriptVariableSet);

                    var sqlScript = new SqlScript(deltaScriptVariableSet.ScriptFileName, script);

                    deltaScripts.Add(sqlScript);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "unable to generate scripts");

                return null;
            }

            return deltaScripts;

        }
    }

    //todo: refactor
    public class TableHistoryInformation
    {
        public TableModel Table { get; set; }

        public string HistoryTable { get; set; }

        public IReadOnlyCollection<TableModel> RootPath { get; set; }
    }
}
