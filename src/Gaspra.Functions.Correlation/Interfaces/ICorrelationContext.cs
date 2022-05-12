using System;
using System.Collections.Generic;
using System.Threading;

namespace Gaspra.Functions.Correlation.Interfaces
{
    public interface ICorrelationContext
    {
        Guid FunctionCorrelationId { get; }
        DateTimeOffset FunctionTimestamp { get; }
        CancellationTokenSource FunctionCancellationSource { get; }
        string FunctionName { get; set; }
        IReadOnlyCollection<IFunctionParameter> FunctionParameters { get; set; }
    }
}
