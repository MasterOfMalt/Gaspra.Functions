using System.Threading.Tasks;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptFactory
    {
        Task<string> ScriptFrom(IScriptVariables variables);
    }
}
