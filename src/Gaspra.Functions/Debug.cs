using System.Collections.Generic;
using Gaspra.Functions.Correlation;

/*
 *
 * To disable git from tracking this file run the command:
 *
 *      git update-index --assume-unchanged [FILEPATH]/Debug.cs
 *
 */

namespace Gaspra.Functions
{
    public static class Debug
    {
        public static bool DebugMode = false;

        public static string FunctionName => "";

        public static IReadOnlyCollection<FunctionParameter> FunctionParameters => new List<FunctionParameter>();
    }
}
