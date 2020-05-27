using Gaspra.DatabaseUtility.Models.Merge;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IMergeSprocsService
    {
        Task<IEnumerable<MergeStatement>> GenerateMergeSprocs(string connectionString, IEnumerable<string> schemaNames);
    }
}
