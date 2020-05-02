using Gaspra.Connection;
using Gaspra.MergeSprocs.DataAccess.Extensions;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.MergeSprocs.DataAccess.Models;
using Gaspra.Signing.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess
{
    public class AnalyticsDataAccess : IDataAccess
    {
        private readonly ConnectionDetails connectionDetails;

        public AnalyticsDataAccess(IConfiguration configuration)
        {
            connectionDetails = ConnectionDetailsExtensions
                .ToConnectionDetails(configuration.GetSection("ConnectionDetails"));
        }

        public async Task<IEnumerable<SqlTableModel>> GetTableInformation()
        {
            using (var connection = new SqlConnection(connectionDetails.ToConnectionString()))
            {
                var command = new SqlCommand(StoredProcedures.GetTableInformation(), connection)
                {
                    CommandType = CommandType.Text
                };

                connection.Open();

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var sqlTableModels = await SqlTableModel.FromDataReader(dataReader);

                    return sqlTableModels;
                }
            }
        }
    }
}
