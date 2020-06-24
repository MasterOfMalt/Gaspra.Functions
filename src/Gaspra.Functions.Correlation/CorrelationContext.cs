using Gaspra.Functions.Correlation.Extensions;
using Gaspra.Functions.Correlation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gaspra.Functions.Correlation
{
    public class CorrelationContext : ICorrelationContext
    {
        public Guid FunctionCorrelationId { get; }
        public DateTimeOffset FunctionTimestamp { get; }
        public CancellationTokenSource FunctionCancellationSource { get; }
        public string FunctionName { get; set; }
        public IEnumerable<IFunctionParameter> FunctionParameters { get; set; }

        public CorrelationContext(string[] args)
        {
            FunctionCorrelationId = Guid.NewGuid();

            FunctionTimestamp = DateTimeOffset.UtcNow;

            FunctionCancellationSource = new CancellationTokenSource();

            var functionName = args
                .FirstOrDefault();

            if(!string.IsNullOrWhiteSpace(functionName))
            {
                FunctionName = functionName.TrimStart('-');
            }
            else
            {
                FunctionName = "help";
            }

            FunctionParameters = args.ToParameters();
        }
    }
}
