using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Models.Script;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptSection
    {
        ScriptOrder Order { get; }
        Task<bool> Valid(IScriptVariables variables);
        Task<string> Value(IScriptVariables variables);
    }
}
