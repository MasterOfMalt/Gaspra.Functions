using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deprecated.Gaspra.DatabaseUtility.Models.Script;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptLineFactory
    {
        Task<IReadOnlyCollection<ScriptLine>> LinesFrom(int indentation, params string[] lines);
        Task<string> StringFrom(IReadOnlyCollection<ScriptLine> scriptLines);
    }
}
