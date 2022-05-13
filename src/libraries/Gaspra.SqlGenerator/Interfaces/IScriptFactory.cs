using System.Threading.Tasks;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptFactory<T> where T : IScriptVariableSet
    {
        Task<string> ScriptFrom(T scriptVariableSet);
    }
}
