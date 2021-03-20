using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Deprecated.Gaspra.DatabaseUtility.Extensions;

namespace Deprecated.Gaspra.DatabaseUtility.Models.DataAccess
{
    public class ExtendedPropertyInformation
    {
        public string PropertySchema { get; set; }
        public string PropertyTable { get; set; }
        public string PropertyKey { get; set; }
        public string PropertyValue { get; set; }

        public ExtendedPropertyInformation(
            string propertySchema,
            string propertyTable,
            string propertyKey,
            string propertyValue)
        {
            PropertySchema = propertySchema;
            PropertyTable = propertyTable;
            PropertyKey = propertyKey;
            PropertyValue = propertyValue;
        }

        public static async Task<IEnumerable<ExtendedPropertyInformation>> FromDataReader(SqlDataReader dataReader)
        {
            var extendedProperties = new List<ExtendedPropertyInformation>();

            while (await dataReader.ReadAsync())
            {
                var propertySchema = dataReader[nameof(PropertySchema)].GetValue<string>();
                var propertyTable = dataReader[nameof(PropertyTable)].GetValue<string>();
                var propertyKey = dataReader[nameof(PropertyKey)].GetValue<string>();
                var propertyValue = dataReader[nameof(PropertyValue)].GetValue<string>();

                extendedProperties.Add(new ExtendedPropertyInformation(
                    propertySchema,
                    propertyTable,
                    propertyKey,
                    propertyValue));
            }

            return extendedProperties;
        }
    }
}
