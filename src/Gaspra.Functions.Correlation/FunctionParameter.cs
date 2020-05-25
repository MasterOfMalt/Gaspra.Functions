using System.Collections.Generic;

namespace Gaspra.Functions.Correlation
{
    public class FunctionParameter
    {
        public string Key { get; set; }
        public IEnumerable<object> Values { get; set; }

        public FunctionParameter(
            string key,
            IEnumerable<object> values)
        {
            Key = key;
            Values = values;
        }
    }
}
