using Gaspra.MergeSprocs.DataAccess.Extensions;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.DataAccess.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess
{
    public class AnalyticsDataAccess : IDataAccess
    {
        public async Task<IEnumerable<ColumnInformation>> GetColumnInformation(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedures.GetColumnInformation(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var columnInformationModels = await ColumnInformation.FromDataReader(dataReader);

            return columnInformationModels;
        }

        public async Task<IEnumerable<FKConstraintInformation>> GetFKConstraintInformation(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedures.GetFKConstraintInformation(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var fkConstraintModels = await FKConstraintInformation.FromDataReader(dataReader);

            return fkConstraintModels;
        }

        public async Task<IEnumerable<ExtendedPropertyInformation>> GetExtendedProperties(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);

            var command = new SqlCommand(StoredProcedures.GetExtendedProperties(), connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();

            using var dataReader = await command.ExecuteReaderAsync();

            var extendedProperties = await ExtendedPropertyInformation.FromDataReader(dataReader);

            return extendedProperties;
        }
    }
}
