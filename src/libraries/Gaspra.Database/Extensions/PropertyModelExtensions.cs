using Gaspra.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.Database.Extensions
{
    public static class PropertyModelExtensions
    {
        public static bool ContainsKey(this ICollection<PropertyModel> properties, string key)
        {
            return
                properties != null &&
                properties
                    .Any(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
