using System;

namespace Gaspra.Functions.Correlation.Interfaces
{
    public interface ICorrelationContext
    {
        Guid CorrelationId { get; }
    }
}
