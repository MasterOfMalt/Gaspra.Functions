using Gaspra.Functions.Correlation.Interfaces;
using System;
using System.Linq;

namespace Gaspra.Functions.Correlation
{
    public class CorrelationContext : ICorrelationContext
    {
        public Guid CorrelationId { get; }
        public DateTimeOffset Timestamp { get; }

        public string Function { get; set; }
        public string[] args { get; set; }

        public CorrelationContext(string[] args)
        {
            CorrelationId = Guid.NewGuid();
            Timestamp = DateTimeOffset.UtcNow;
            this.args = args;
            Function = args.FirstOrDefault();
        }
    }
}
