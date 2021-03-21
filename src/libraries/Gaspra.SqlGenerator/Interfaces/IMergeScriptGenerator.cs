using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IMergeScriptGenerator
    {
        Task<IReadOnlyCollection<MergeScript>> Generate(string connectionString, IReadOnlyCollection<string> schemas);
    }
}
