using System.Collections.Generic;
using Gaspra.Functions.Correlation;

namespace Gaspra.Functions
{
    public static class Debug
    {
        public static bool DebugMode = false;

        public static string FunctionName => "";

        public static IReadOnlyCollection<FunctionParameter> FunctionParameters => new List<FunctionParameter>();
    }
}
