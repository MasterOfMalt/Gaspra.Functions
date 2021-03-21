using System;
using System.Collections.Generic;
using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models.QueryResults;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.Database.Services
{
    public class DataAccess : IDataAccess
    {
        public async Task<DatabaseResult> GetDatabase(string connectionString, IReadOnlyCollection<string> schemas)
        {
            await using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetDatabase(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            await using var dataReader = await command.ExecuteReaderAsync();

            var tables = await dataReader.ReadTables();

            await dataReader.NextResultAsync();

            var constraints = await dataReader.ReadConstraints();

            await dataReader.NextResultAsync();

            var properties = await dataReader.ReadProperties();

            if (schemas != null && schemas.Any())
            {
                tables = tables
                    .Where(t => schemas
                        .Any(s => s.Equals(t.Schema, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                constraints = constraints
                    .Where(c => schemas
                        .Any(s => s.Equals(c.ConstraintSchema, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                properties = properties
                    .Where(p => schemas
                        .Any(s => s.Equals(p.Schema, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }

            return new DatabaseResult
            {
                Tables = tables,
                Constraints = constraints,
                Properties = properties
            };
        }
    }
}
