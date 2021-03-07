using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptFactory
    {
        Task<string> ScriptFrom(IScriptVariables variables);
    }
}
