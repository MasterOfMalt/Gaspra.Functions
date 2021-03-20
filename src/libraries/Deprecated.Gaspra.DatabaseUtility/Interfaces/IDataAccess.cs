using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Models.DataAccess;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IDataAccess
    {
        public Task<DatabaseInformation> GetDatabaseInformation(string connectionString);
    }
}
