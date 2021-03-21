using System.Collections.Generic;
using Gaspra.Database.Models.QueryResults;
using System.Threading.Tasks;

namespace Gaspra.Database.Interfaces
{
    public interface IDataAccess
    {
        /// <summary>
        /// Get database objects for a given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemas">The schemas you wish to have returned (null = all schemas)</param>
        /// <returns></returns>
        Task<DatabaseResult> GetDatabase(string connectionString, IReadOnlyCollection<string> schemas = null);
    }
}
