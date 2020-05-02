using Gaspra.MergeSprocs.DataAccess.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess.Models
{
    public class SqlTableModel
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int ColumnId { get; set; }
        public bool Nullable { get; set; }
        public bool IdentityColumn { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? SeedValue { get; set; }
        public int? IncrementValue { get; set; }
        public string DefaultValue { get; set; }

        public SqlTableModel(
            string tableSchema,
            string tableName,
            string columnName,
            int columnId,
            bool nullable,
            bool identityColumn,
            string dataType,
            int? maxLength,
            int? precision,
            int? scale,
            int? seedValue,
            int? incrementValue,
            string defaultValue)
        {
            TableSchema = tableSchema;
            TableName = tableName;
            ColumnName = columnName;
            ColumnId = columnId;
            Nullable = nullable;
            IdentityColumn = identityColumn;
            DataType = dataType;
            MaxLength = maxLength;
            Precision = precision;
            Scale = scale;
            SeedValue = seedValue;
            IncrementValue = incrementValue;
            DefaultValue = defaultValue;
        }

        public static async Task<IEnumerable<SqlTableModel>> FromDataReader(SqlDataReader dataReader)
        {
            var sqlTableModels = new List<SqlTableModel>();

            while (await dataReader.ReadAsync())
            {
                var tableSchema = dataReader[nameof(TableSchema)].GetValue<string>();
                var tableName = dataReader[nameof(TableName)].GetValue<string>();
                var columnName = dataReader[nameof(ColumnName)].GetValue<string>();
                var columnId = dataReader[nameof(ColumnId)].GetValue<int>();
                var nullable = dataReader[nameof(Nullable)].GetValue<bool>();
                var identityColumn = dataReader[nameof(IdentityColumn)].GetValue<bool>();
                var dataType = dataReader[nameof(DataType)].GetValue<string>();
                var maxLength = dataReader[nameof(MaxLength)].GetValue<int?>();
                var precision = dataReader[nameof(Precision)].GetValue<int?>();
                var scale = dataReader[nameof(Scale)].GetValue<int?>();
                var seedValue = dataReader[nameof(SeedValue)].GetValue<int?>();
                var incrementValue = dataReader[nameof(IncrementValue)].GetValue<int?>();
                var defaultValue = dataReader[nameof(DefaultValue)].GetValue<string>();

                sqlTableModels.Add(new SqlTableModel(
                    tableSchema,
                    tableName,
                    columnName,
                    columnId,
                    nullable,
                    identityColumn,
                    dataType,
                    maxLength,
                    precision,
                    scale,
                    seedValue,
                    incrementValue,
                    defaultValue));
            }

            return sqlTableModels;
        }
    }
}
