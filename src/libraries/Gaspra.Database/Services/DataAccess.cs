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
        public async Task<DatabaseResult> GetDatabase(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetDatabase(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var tables = await dataReader.ReadTables();

            await dataReader.NextResultAsync();

            var constraints = await dataReader.ReadConstraints();

            await dataReader.NextResultAsync();

            var properties = await dataReader.ReadProperties();

            return new DatabaseResult
            {
                Tables = tables,
                Constraints = constraints,
                Properties = properties
            };
        }
    }
}