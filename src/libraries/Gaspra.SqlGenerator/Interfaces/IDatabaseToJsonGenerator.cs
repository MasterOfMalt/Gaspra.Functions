using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IDatabaseToJsonGenerator
    {
        Task<string> Generate(string connectionString, IReadOnlyCollection<string> schemas);
    }
}