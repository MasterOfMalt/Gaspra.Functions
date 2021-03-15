using Gaspra.Database.Models.QueryResults;
using System.Threading.Tasks;

namespace Gaspra.Database.Interfaces
{
    public interface IDataAccess
    {
        Task<DatabaseResult> GetDatabase(string connectionString);
    }
}
