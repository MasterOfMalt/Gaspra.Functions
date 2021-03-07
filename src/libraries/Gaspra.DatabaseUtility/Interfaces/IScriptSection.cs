using Gaspra.DatabaseUtility.Models.Script;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptSection
    {
        ScriptOrder Order { get; }
        Task<bool> Valid(IScriptVariables variables);
        Task<string> Value(IScriptVariables variables);
    }
}
