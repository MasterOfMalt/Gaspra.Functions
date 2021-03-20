using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Deprecated.Gaspra.DatabaseUtility.Extensions;

namespace Deprecated.Gaspra.DatabaseUtility.Models.DataAccess
{
    public class ColumnInformation
    {
        public string TableSchema { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public int ColumnId { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? SeedValue { get; set; }
        public int? IncrementValue { get; set; }
        public string DefaultValue { get; set; }

        public ColumnInformation(
            string tableSchema,
            string table,
            string column,
            int columnId,
            bool nullable,
            bool identity,
            string dataType,
            int? maxLength,
            int? precision,
            int? scale,
            int? seedValue,
            int? incrementValue,
            string defaultValue)
        {
            TableSchema = tableSchema;
            Table = table;
            Column = column;
            ColumnId = columnId;
            Nullable = nullable;
            Identity = identity;
            DataType = dataType;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            SeedValue = seedValue;
            IncrementValue = incrementValue;
            DefaultValue = defaultValue;
        }

        public static async Task<IEnumerable<ColumnInformation>> FromDataReader(SqlDataReader dataReader)
        {
            var columns = new List<ColumnInformation>();

            while (await dataReader.ReadAsync())
            {
                var tableSchema = dataReader[nameof(TableSchema)].GetValue<string>();
                var table = dataReader[nameof(Table)].GetValue<string>();
                var column = dataReader[nameof(Column)].GetValue<string>();
                var columnId = dataReader[nameof(ColumnId)].GetValue<int>();
                var nullable = dataReader[nameof(Nullable)].GetValue<bool>();
                var identity = dataReader[nameof(Identity)].GetValue<bool>();
                var dataType = dataReader[nameof(DataType)].GetValue<string>();
                var maxLength = dataReader[nameof(MaxLength)].GetValue<int?>();
                var precision = dataReader[nameof(Precision)].GetValue<int?>();
                var scale = dataReader[nameof(Scale)].GetValue<int?>();
                var seedValue = dataReader[nameof(SeedValue)].GetValue<int?>();
                var incrementValue = dataReader[nameof(IncrementValue)].GetValue<int?>();
                var defaultValue = dataReader[nameof(DefaultValue)].GetValue<string>();

                columns.Add(new ColumnInformation(
                    tableSchema,
                    table,
                    column,
                    columnId,
                    nullable,
                    identity,
                    dataType,
                    maxLength,
                    precision,
                    scale,
                    seedValue,
                    incrementValue,
                    defaultValue));
            }

            return columns;
        }
    }

    public class ColumnComparerByTableName : IEqualityComparer<ColumnInformation>
    {
        public bool Equals(ColumnInformation x, ColumnInformation y)
        {
            return x.Table.Equals(y.Table);
        }

        public int GetHashCode(ColumnInformation obj)
        {
            return obj.Table.GetHashCode();
        }
    }

    public class ColumnComparerBySchemaName : IEqualityComparer<ColumnInformation>
    {
        public bool Equals(ColumnInformation x, ColumnInformation y)
        {
            return x.TableSchema.Equals(y.TableSchema);
        }

        public int GetHashCode(ColumnInformation obj)
        {
            return obj.TableSchema.GetHashCode();
        }
    }
}
