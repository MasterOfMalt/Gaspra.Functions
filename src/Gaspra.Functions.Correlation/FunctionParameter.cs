using System.Collections.Generic;

namespace Gaspra.Functions.Correlation
{
    public interface IFunctionParameter
    {
        public string Key { get; }
        public IEnumerable<object> Values { get; }
    }

    public class FunctionParameter : IFunctionParameter
    {
        public string Key { get; }
        public IEnumerable<object> Values { get; }

        public FunctionParameter(
            string key,
            IEnumerable<object> values)
        {
            Key = key;
            Values = values;
        }
    }
}
