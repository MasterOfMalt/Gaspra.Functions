using Gaspra.MergeSprocs.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess.Interfaces
{
    public interface IDataAccess
    {
        public Task<IEnumerable<ColumnInformation>> GetColumnInformation();
        public Task<IEnumerable<FKConstraintInformation>> GetFKConstraintInformation();
        public Task<IEnumerable<ExtendedPropertyInformation>> GetExtendedProperties();
    }
}
