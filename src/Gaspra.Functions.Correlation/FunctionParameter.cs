using System.Collections.Generic;

namespace Gaspra.Functions.Correlation
{
    public interface IFunctionParameter
    {
        public string Key { get; }
        public IReadOnlyCollection<object> Values { get; }
        public bool Optional { get; }
        public string About { get; }
    }

    /*
     * todo: split this out into two objects, the one that explains what the parameter is the other
     * todo: the actual implementation and usage of the parameter, rather than combining both and
     * todo: having issues when instantiating the parameter
     */
    public class FunctionParameter : IFunctionParameter
    {
        public string Key { get; }
        public IReadOnlyCollection<object> Values { get; }
        public bool Optional { get; }
        public string About { get; }

        public FunctionParameter(
            string key,
            IReadOnlyCollection<object> values,
            bool optional = false,
            string about = "")
        {
            Key = key;
            Values = values;
            Optional = optional;
            About = about;
        }
    }
}
