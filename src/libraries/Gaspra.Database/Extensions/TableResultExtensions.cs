using Gaspra.Database.Models;
using Gaspra.Database.Models.QueryResults;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaspra.Database.Extensions
{
    public static class TableResultExtensions
    {
        public static Task<IReadOnlyCollection<TableModel>> CreateTables(this IReadOnlyCollection<TableResult> tableResults)
        {
            var tables = new List<TableModel>();

            foreach (var tableResult in tableResults)
            {
                var tableModel = new TableModel
                {
                    Name = tableResult.Table
                };

                tables.Add(tableModel);
            }

            return Task.FromResult((IReadOnlyCollection<TableModel>)tables);
        }

        public static Task<IReadOnlyCollection<ColumnModel>> CreateColumns(this IReadOnlyCollection<TableResult> tableResults)
        {
            var columns = new List<ColumnModel>();

            foreach (var tableResult in tableResults)
            {
                var columnModel = new ColumnModel
                {
                    Id = tableResult.ColumnId,
                    Name = tableResult.Column,
                    Nullable = tableResult.Nullable,
                    IdentityColumn = tableResult.Identity,
                    DataType = tableResult.DataType,
                    MaxLength = tableResult.MaxLength,
                    Precision = tableResult.Precision,
                    Scale = tableResult.Scale,
                    SeedValue = tableResult.SeedValue,
                    IncrementValue = tableResult.IncrementValue,
                    DefaultValue = tableResult.DefaultValue
                };

                columns.Add(columnModel);
            }

            return Task.FromResult((IReadOnlyCollection<ColumnModel>)columns);
        }
    }
}
