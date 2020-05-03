using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess.Interfaces
{
    public interface IDataAccess
    {
        public Task<IEnumerable<ColumnInformation>> GetColumnInformation();
    }
}
