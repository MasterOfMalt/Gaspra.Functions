using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Interfaces
{
    public interface IFunction
    {
        IEnumerable<string> FunctionAliases { get; }
        string FunctionHelp { get; }
        bool ValidateParameters();
        Task Run(CancellationToken cancellationToken);
    }
}
