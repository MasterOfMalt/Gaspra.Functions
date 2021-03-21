using System.Threading.Tasks;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptSection
    {
        ScriptOrder Order { get; }
        Task<bool> Valid(IMergeScriptVariableSet variableSet);
        Task<string> Value(IMergeScriptVariableSet variableSet);
    }
}
