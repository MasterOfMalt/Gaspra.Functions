using Gaspra.DatabaseUtility.Models.DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IDataAccess
    {
        public Task<IList<ColumnInformation>> GetColumnInformation(string connectionString);
        public Task<IList<FKConstraintInformation>> GetFKConstraintInformation(string connectionString);
        public Task<IList<ExtendedPropertyInformation>> GetExtendedProperties(string connectionString);
    }
}
