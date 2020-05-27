using Gaspra.DatabaseProcesses.Extensions;
using Gaspra.DatabaseProcesses.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Gaspra.DatabaseProcesses
{
    public interface IDatabaseProcessesService
    {
        Task<IEnumerable<RunningProcess>> GetRunningProcesses(string connectionString);
    }

    public class DatabaseProcessesService : IDatabaseProcessesService
    {
        public async Task<IEnumerable<RunningProcess>> GetRunningProcesses(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetRunningProcesses(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var runningProcesses = await RunningProcess.FromDataReader(dataReader);

            return runningProcesses;
        }
    }
}
