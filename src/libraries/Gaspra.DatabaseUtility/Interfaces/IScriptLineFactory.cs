using Gaspra.DatabaseUtility.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptLineFactory
    {
        Task<IReadOnlyCollection<ScriptLine>> LinesFrom(int indentation, params string[] lines);
        Task<string> StringFrom(IReadOnlyCollection<ScriptLine> scriptLines);
    }
}
