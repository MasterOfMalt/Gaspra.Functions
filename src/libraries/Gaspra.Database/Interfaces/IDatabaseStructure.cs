using Gaspra.Database.Models;
using Gaspra.Database.Models.QueryResults;
using System.Threading.Tasks;

namespace Gaspra.Database.Interfaces
{
    public interface IDatabaseStructure
    {
        Task<DatabaseModel> CalculateStructure(DatabaseResult databaseResult);
    }
}
