using Gaspra.Functions.Correlation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Interfaces
{
    public interface IFunction
    {
        IReadOnlyCollection<string> Aliases { get; }

        IReadOnlyCollection<IFunctionParameter> Parameters { get; }

        string About { get; }

        bool ValidateParameters(IReadOnlyCollection<IFunctionParameter> parameters);

        Task Run(CancellationToken cancellationToken, IReadOnlyCollection<IFunctionParameter> parameters);
    }
}
