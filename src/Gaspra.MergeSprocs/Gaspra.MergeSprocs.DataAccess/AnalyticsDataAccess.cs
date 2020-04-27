using Gaspra.Connection;
using Gaspra.MergeSprocs.DataAccess.Interfaces;
using Gaspra.Signing.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.DataAccess
{
    public class AnalyticsDataAccess : IDataAccess
    {
        private readonly ConnectionDetails connectionDetails;

        public AnalyticsDataAccess(SigningService signingService, IConfiguration configuration)
        {
            connectionDetails = ConnectionDetailsExtensions
                .EncryptedToConnectionDetails(configuration.GetSection("ConnectionDetails"), signingService);
        }
    }
}
