using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Gaspra.DatabaseUtility.Extensions;

namespace Gaspra.DatabaseUtility.Models.DataAccess
{
    public class ExtendedPropertyInformation
    {
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public ExtendedPropertyInformation(
            string schemaName,
            string objectName,
            string propertyName,
            string value)
        {
            SchemaName = schemaName;
            ObjectName = objectName;
            PropertyName = propertyName;
            Value = value;
        }

        public static async Task<IEnumerable<ExtendedPropertyInformation>> FromDataReader(SqlDataReader dataReader)
        {
            var extendedProperties = new List<ExtendedPropertyInformation>();

            while (await dataReader.ReadAsync())
            {
                var schemaName = dataReader[nameof(SchemaName)].GetValue<string>();
                var objectName = dataReader[nameof(ObjectName)].GetValue<string>();
                var propertyName = dataReader[nameof(PropertyName)].GetValue<string>();
                var value = dataReader[nameof(Value)].GetValue<string>();

                extendedProperties.Add(new ExtendedPropertyInformation(
                    schemaName,
                    objectName,
                    propertyName,
                    value));
            }

            return extendedProperties;
        }
    }
}
