using System;

namespace Gaspra.Functions.Correlation.Interfaces
{
    public interface ICorrelationContext
    {
        Guid CorrelationId { get; }
        DateTimeOffset Timestamp { get; }

        string Function { get; set; }
    }
}
