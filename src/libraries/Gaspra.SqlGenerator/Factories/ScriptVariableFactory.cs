using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Factories
{
    public class ScriptVariableFactory : IScriptVariableFactory
    {
        public Task<IReadOnlyCollection<IScriptVariableSet>> VariablesFrom(DatabaseModel database)
        {
            var scriptVariableSets = new List<ScriptVariableSet>();

            return Task.FromResult((IReadOnlyCollection<IScriptVariableSet>)scriptVariableSets);
        }
    }
}
