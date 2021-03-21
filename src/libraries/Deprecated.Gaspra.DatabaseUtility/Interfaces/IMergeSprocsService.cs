using System.Collections.Generic;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Models.Merge;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IMergeSprocsService
    {
        Task<IEnumerable<MergeStatement>> GenerateMergeSprocs(string connectionString, IEnumerable<string> schemaNames);
    }
}
