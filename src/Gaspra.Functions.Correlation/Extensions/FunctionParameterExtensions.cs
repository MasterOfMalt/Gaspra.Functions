using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.Functions.Correlation.Extensions
{
    public static class FunctionParameterExtensions
    {
        public static IEnumerable<FunctionParameter> ToParameters(this string[] args)
        {
            var parameters = new List<FunctionParameter>();

            var numberOfArguments = args.Count();

            var arguments = new List<(int pos, string val)>();

            for (var a = 0; a < numberOfArguments; a++)
            {
                arguments.Add((a, args[a]));
            }

            var argumentKeys = arguments
                .Where(a => a.val.StartsWith('-'))
                .Select(a => (a.pos, a.val))
                .OrderBy(a => a.pos)
                .ToArray();

            for (var k = 0; k < argumentKeys.Count(); k++)
            {
                var keyPosition = argumentKeys[k].pos;

                var nextKeyPosition = arguments.Count();

                if (k + 1 < argumentKeys.Count())
                {
                    nextKeyPosition = argumentKeys[k + 1].pos;
                }

                var argumentKey = "";

                var argumentValues = new List<string>();

                for (var kv = keyPosition; kv < nextKeyPosition; kv++)
                {
                    if (kv.Equals(keyPosition))
                    {
                        argumentKey = args[kv].TrimStart('-');
                    }
                    else
                    {
                        argumentValues.Add(args[kv]);
                    }
                }

                parameters.Add(new FunctionParameter(argumentKey, argumentValues));
            }

            return parameters;
        }
    }
}
