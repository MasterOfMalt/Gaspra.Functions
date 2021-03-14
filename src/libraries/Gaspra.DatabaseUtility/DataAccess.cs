using Gaspra.DatabaseUtility.Extensions;
using Gaspra.DatabaseUtility.Interfaces;
using Gaspra.DatabaseUtility.Models.DataAccess;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility
{
    public class DataAccess : IDataAccess
    {
        public async Task<DatabaseInformation> GetDatabaseInformation(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetDatabaseInformation(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var columnInformation = await ColumnInformation.FromDataReader(dataReader);

            await dataReader.NextResultAsync();

            var foreignKeyInformation = await FKConstraintInformation.FromDataReader(dataReader);

            await dataReader.NextResultAsync();

            var extendedPropertyInformation = await ExtendedPropertyInformation.FromDataReader(dataReader);

            return new DatabaseInformation
            {
                Columns = columnInformation.ToList(),
                ForeignKeys = foreignKeyInformation.ToList(),
                ExtendedProperties = extendedPropertyInformation.ToList()
            };
        }
    }
}
