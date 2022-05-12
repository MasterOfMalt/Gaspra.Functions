using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IDeltaScriptGenerator
    {
        Task<IReadOnlyCollection<SqlScript>> Generate(string connectionString, IReadOnlyCollection<string> schemas);
    }
}
