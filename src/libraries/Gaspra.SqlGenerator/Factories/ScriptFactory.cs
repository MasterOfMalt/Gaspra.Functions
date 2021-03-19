using System.Threading.Tasks;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Factories
{
    public class ScriptFactory : IScriptFactory
    {
        public Task<string> ScriptFrom(IScriptVariableSet scriptVariableSet)
        {
            throw new System.NotImplementedException();
        }
    }
}
