using Gaspra.MergeSprocs.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess.Interfaces
{
    public interface IDataAccess
    {
        public Task<IEnumerable<ColumnInformation>> GetColumnInformation(string connectionString);
        public Task<IEnumerable<FKConstraintInformation>> GetFKConstraintInformation(string connectionString);
        public Task<IEnumerable<ExtendedPropertyInformation>> GetExtendedProperties(string connectionString);
    }
}
