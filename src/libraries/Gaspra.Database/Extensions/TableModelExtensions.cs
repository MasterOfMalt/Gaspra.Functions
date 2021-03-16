using Gaspra.Database.Models;
using System.Collections.Generic;

namespace Gaspra.Database.Extensions
{
    public static class TableModelExtensions
    {
        public static void AddProperty(this TableModel table, string key, string value)
        {
            var properties = table.Properties ?? new List<PropertyModel>();

            properties.Add(new PropertyModel
            {
                Key = key,
                Value = value
            });

            table.Properties = properties;
        }

        public static void AddDependantTable(this TableModel table, TableModel dependant)
        {
            var dependants = table.DependantTables ?? new List<TableModel>();

            dependants.Add(dependant);

            table.DependantTables = dependants;
        }

        public static void AddReferenceTable(this TableModel table, TableModel reference)
        {
            var references = table.ReferenceTables ?? new List<TableModel>();

            references.Add(reference);

            table.ReferenceTables = references;
        }
    }
}
