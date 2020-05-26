using Gaspra.Functions.Correlation;
using Gaspra.Functions.Extensions;
using Gaspra.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gaspra.Functions.Functions
{
    public class DynamicFunction : IFunction
    {
        public IEnumerable<string> FunctionAliases => new[] { "dynamic" };

        public string FunctionHelp => "";

        public bool ValidateParameters(IEnumerable<IFunctionParameter> parameters) => true;

        public async Task Run(CancellationToken cancellationToken, IEnumerable<IFunctionParameter> parameters)
        {
            var lines = new List<ConsoleLine>
            {
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString()),
                new ConsoleLine(RandomString())
            };

            while(!cancellationToken.IsCancellationRequested)
            {
                foreach(var line in lines)
                {
                    line.Rewrite(RandomString());
                }

                Thread.Sleep(500);
            }
        }

        private static Random random = new Random();
        public static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(0, 40))
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
