using Gaspra.DatabaseUtility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.DatabaseUtility.Factories
{
    public class ScriptFactory : IScriptFactory
    {
        private IReadOnlyCollection<IScriptSection> _scriptSections;

        public ScriptFactory(IEnumerable<IScriptSection> scriptSections)
        {
            _scriptSections = scriptSections
                .ToList()
                .AsReadOnly();
        }

        public async Task<string> ScriptFrom(IScriptVariables variables)
        {
            var deepestOrder = _scriptSections
                .Select(s => s.Order.Values.Length)
                .OrderByDescending(s => s)
                .First();

            var flattenedOrders = _scriptSections.Select(s =>
                {
                    var orderLengthAffix = new string('0', deepestOrder - s.Order.Values.Length);

                    var flatOrder = string.Join("", s.Order.Values) + orderLengthAffix;

                    var flattenedOrder = int.Parse(flatOrder);

                    return (flattenedOrder, s);
                })
                .ToList();

            var sections = flattenedOrders
                .OrderBy(f => f.flattenedOrder)
                .Select(f => f.s);

            var script = "";

            foreach (var section in sections)
            {
                if (await section.Valid(variables))
                {
                    script += $"{await section.Value(variables)}{Environment.NewLine}{Environment.NewLine}";
                }
            }

            return script;
        }
    }
}
