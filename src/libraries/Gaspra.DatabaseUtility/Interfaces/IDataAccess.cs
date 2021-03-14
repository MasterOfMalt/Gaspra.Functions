using Gaspra.DatabaseUtility.Models.DataAccess;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IDataAccess
    {
        public Task<DatabaseInformation> GetDatabaseInformation(string connectionString);
    }
}
