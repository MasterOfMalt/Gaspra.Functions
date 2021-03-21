using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.SqlGenerator.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptLineFactory
    {
        Task<IReadOnlyCollection<ScriptLine>> LinesFrom(int indentation, params string[] lines);
        Task<string> StringFrom(IReadOnlyCollection<ScriptLine> scriptLines);
    }
}
