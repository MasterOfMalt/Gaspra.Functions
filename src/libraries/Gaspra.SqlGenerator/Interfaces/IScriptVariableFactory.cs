using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptVariableFactory
    {
        Task<IReadOnlyCollection<IScriptVariableSet>> VariablesFrom(DatabaseModel database);
    }
}
