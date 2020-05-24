using Gaspra.Functions.Correlation.Interfaces;
using System;

namespace Gaspra.Functions.Correlation
{
    public class CorrelationContext : ICorrelationContext
    {
        public Guid CorrelationId { get; }

        public CorrelationContext()
        {
            CorrelationId = Guid.NewGuid();
        }
    }
}
