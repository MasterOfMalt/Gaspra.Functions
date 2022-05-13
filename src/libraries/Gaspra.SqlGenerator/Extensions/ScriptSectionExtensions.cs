using System.Collections.Generic;
using System.Linq;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Extensions
{
    public static class ScriptSectionExtensions
    {
        public static IReadOnlyCollection<IScriptSection<T>> OrderSections<T>(
            this IReadOnlyCollection<IScriptSection<T>> scriptSections) where T : IScriptVariableSet
        {
            var deepestOrder = scriptSections
                .Select(s => s.Order.Values.Length)
                .OrderByDescending(s => s)
                .First();

            var flattenedOrders = scriptSections.Select(s =>
            {
                var orderLengthAffix = new string('0', deepestOrder - s.Order.Values.Length);

                var flatOrder = string.Join("", s.Order.Values) + orderLengthAffix;

                var flattenedOrder = int.Parse(flatOrder);

                return (flattenedOrder, s);
            });

            var orderedSections = flattenedOrders
                .OrderBy(f => f.flattenedOrder)
                .Select(f => f.s)
                .ToList();

            return orderedSections;
        }
    }
}
