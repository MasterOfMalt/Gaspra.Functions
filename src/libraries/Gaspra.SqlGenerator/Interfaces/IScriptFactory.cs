using System.Threading.Tasks;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptFactory
    {
        Task<string> ScriptFrom(IMergeScriptVariableSet scriptVariableSet);
    }
}
