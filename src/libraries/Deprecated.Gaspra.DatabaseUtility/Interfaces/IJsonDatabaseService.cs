using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IJsonDatabaseService
    {
        Task<string> SerializeDatabaseToJson(string connectionString, IEnumerable<string> schemaNames);
    }
}
