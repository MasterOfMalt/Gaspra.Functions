using Gaspra.Database.Extensions;
using Gaspra.Database.Interfaces;
using Gaspra.Database.Models.QueryResults;
using System.Data;
using System.Data.SqlClient;
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

            var columns = dataReader.ReadColums();

            await dataReader.NextResultAsync();

            var constraints = dataReader.ReadConstraints();

            await dataReader.NextResultAsync();

            var properties = dataReader.ReadProperties();

            return new DatabaseResult
            {
                Columns = columns,
                Constraints = constraints,
                Properties = properties
            };
        }
    }
}