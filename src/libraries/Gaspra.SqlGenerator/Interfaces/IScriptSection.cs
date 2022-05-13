using System.Threading.Tasks;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptSection<T> where T : IScriptVariableSet
    {
        ScriptOrder Order { get; }
        Task<bool> Valid(T variableSet);
        Task<string> Value(T variableSet);
    }
}
