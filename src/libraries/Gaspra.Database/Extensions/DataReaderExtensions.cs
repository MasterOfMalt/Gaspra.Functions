using Gaspra.Database.Models.QueryResults;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Gaspra.Database.Extensions
{
    public static class DataReaderExtensions
    {
        public static T GetValue<T>(this object data)
        {
            if (data.GetType().Equals(typeof(DBNull)))
            {
                return default;
            }

            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            if (nullableType != null)
            {
                return (T)Convert.ChangeType(data, nullableType);
            }

            return (T)data;
        }

        public static async Task<IReadOnlyCollection<TableResult>> ReadTables(this SqlDataReader dataReader)
        {
            var tables = new List<TableResult>();

            while (await dataReader.ReadAsync())
            {
                tables.Add(new TableResult
                {
                    Schema = dataReader["schema"].GetValue<string>(),
                    Table = dataReader["table"].GetValue<string>(),
                    Column = dataReader["column"].GetValue<string>(),
                    ColumnId = dataReader["columnid"].GetValue<int>(),
                    Nullable = dataReader["nullable"].GetValue<bool>(),
                    Identity = dataReader["identity"].GetValue<bool>(),
                    DataType = dataReader["datatype"].GetValue<string>(),
                    MaxLength = dataReader["maxlength"].GetValue<int?>(),
                    Precision = dataReader["precision"].GetValue<int?>(),
                    Scale = dataReader["scale"].GetValue<int?>(),
                    SeedValue = dataReader["seedvalue"].GetValue<string>(),
                    IncrementValue = dataReader["incrementvalue"].GetValue<string>(),
                    DefaultValue = dataReader["defaultvalue"].GetValue<string>()
                });
            }

            return tables;
        }

        public static async Task<IReadOnlyCollection<ConstraintResult>> ReadConstraints(this SqlDataReader dataReader)
        {
            var constraints = new List<ConstraintResult>();

            while (await dataReader.ReadAsync())
            {
                constraints.Add(new ConstraintResult
                {
                    ConstraintSchema = dataReader["constraintschema"].GetValue<string>(),
                    ConstraintName = dataReader["constraintname"].GetValue<string>(),
                    ConstraintTable = dataReader["constrainttable"].GetValue<string>(),
                    ConstraintColumn = dataReader["constraintcolumn"].GetValue<string>(),
                    ReferencedTable = dataReader["referencedtable"].GetValue<string>(),
                    ReferencedColumn = dataReader["referencedcolumn"].GetValue<string>(),
                });
            }

            return constraints;
        }

        public static async Task<IReadOnlyCollection<PropertyResult>> ReadProperties(this SqlDataReader dataReader)
        {
            var properties = new List<PropertyResult>();

            while (await dataReader.ReadAsync())
            {
                properties.Add(new PropertyResult
                {
                    Schema = dataReader["schema"].GetValue<string>(),
                    Table = dataReader["table"].GetValue<string>(),
                    Key = dataReader["key"].GetValue<string>(),
                    Value = dataReader["value"].GetValue<string>()
                });
            }

            return properties;
        }
    }
}