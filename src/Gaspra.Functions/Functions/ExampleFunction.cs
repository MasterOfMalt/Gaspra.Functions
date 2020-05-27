using Gaspra.Functions.Correlation;
using Gaspra.Functions.Correlation.Interfaces;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    public class ExampleFunction : IFunction
    {
        private readonly ILogger logger;
        private readonly ICorrelationContext cxt;

        private static readonly Random random = new Random();
        private string example = "cxt";

        public ExampleFunction(
            ILogger<ExampleFunction> logger,
            ICorrelationContext cxt)
        {
            this.logger = logger;
            this.cxt = cxt;
        }

        public IEnumerable<string> FunctionAliases => new List<string> { "example" };

        public string FunctionHelp =>
@"Example function, usage is to briefly show what the Gaspra.Functions can output.
Accepts the optional parameter(s):
    -e [cxt, dyn, *]
        >cxt: will return the function call once the contextual information is displayed (default value)
        >dyn: will continously write to the console updating the previous lines dynamically
        >*: anything else supplied will cause the function to continually log

    -{anything}
        >supplying any parameters will be displayed in the contextual information at the start of the function run
";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters)
        {
            var hasParameters = parameters.Any();

            if(hasParameters && parameters.Where(p => p.Key.Equals("e")).Any())
            {
                example = parameters.Where(p => p.Key.Equals("e"))
                    .FirstOrDefault()
                    .Values
                    .FirstOrDefault()
                    .ToString();
            }

            return true;
        }

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            logger.LogInformation("Function: [{cxtFunction}] with correlation id: [{correlationId}]",
            cxt.FunctionName,
            cxt.FunctionCorrelationId);

            logger.LogInformation("Parameters: {params}",
                string.Join(", ", cxt.FunctionParameters.Select(p => $"[key: {p.Key} - values: {string.Join(", ", p.Values)}]")));

            logger.LogInformation("Started at: [{time}]",
                cxt.FunctionTimestamp);

            if(example.Equals("cxt"))
            {
                return;
            }

            var lines = new List<ConsoleLine>
            {
                new ConsoleLine("While counter [0], generating random strings:"),
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString())
            };

            var counter = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if(example.Equals("dyn"))
                {
                    foreach (var line in lines.Where(l => l.LineBuilder.ToString().StartsWith("While counter [")))
                    {
                        line.Rewrite($"While counter [{counter}], generating random strings:");
                    }

                    foreach (var line in lines.Where(l => !l.LineBuilder.ToString().StartsWith("While counter [")))
                    {
                        line.Rewrite($"[{RandomString()}]");
                    }
                }
                else
                {
                    logger.LogInformation("While counter [{counter}], generating random strings:",
                        counter);

                    logger.LogInformation("[{random}]", RandomString());
                    logger.LogInformation("[{random}]", RandomString());
                    logger.LogInformation("[{random}]", RandomString());
                }

                counter++;

                Thread.Sleep(500);
            }
        }

        private static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(0, 40))
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
