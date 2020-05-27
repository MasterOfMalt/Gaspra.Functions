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
        public async Task<IList<ColumnInformation>> GetColumnInformation(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetColumnInformation(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var columnInformationModels = await ColumnInformation.FromDataReader(dataReader);

            return columnInformationModels.ToList();
        }

        public async Task<IList<FKConstraintInformation>> GetFKConstraintInformation(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetFKConstraintInformation(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var fkConstraintModels = await FKConstraintInformation.FromDataReader(dataReader);

            return fkConstraintModels.ToList();
        }

        public async Task<IList<ExtendedPropertyInformation>> GetExtendedProperties(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedureExtensions.GetExtendedProperties(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var extendedProperties = await ExtendedPropertyInformation.FromDataReader(dataReader);

            return extendedProperties.ToList();
        }
    }
}
