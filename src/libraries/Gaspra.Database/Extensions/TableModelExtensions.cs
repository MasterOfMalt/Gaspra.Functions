using Gaspra.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.Database.Extensions
{
    public static class TableModelExtensions
    {
        public static void AddProperty(this TableModel table, string key, string value)
        {
            var properties = new List<PropertyModel>();

            if(table.Properties != null && table.Properties.Any())
            {
                properties = (List<PropertyModel>)table.Properties;
            }

            properties.Add(new PropertyModel
            {
                Key = key,
                Value = value
            });

            table.Properties = properties;
        }
    }
}
