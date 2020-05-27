using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IJsonDatabaseService
    {
        Task<string> SerializeDatabaseToJson(string connectionString, IEnumerable<string> schemaNames);
    }
}
