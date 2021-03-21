using System;
using Gaspra.Database.Models;
using Gaspra.Database.Models.QueryResults;
using System.Collections.Generic;
using System.Linq;
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
                var maxLength = tableResult.MaxLength;

                if(tableResult.DataType.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase)
                   && maxLength != null)
                {
                    maxLength /= 2;
                }

                var columnModel = new ColumnModel
                {
                    Id = tableResult.ColumnId,
                    Name = tableResult.Column,
                    Nullable = tableResult.Nullable,
                    IdentityColumn = tableResult.Identity,
                    DataType = tableResult.DataType,
                    MaxLength = maxLength,
                    Precision = tableResult.Precision,
                    Scale = tableResult.Scale,
                    SeedValue = tableResult.SeedValue,
                    IncrementValue = tableResult.IncrementValue,
                    DefaultValue = tableResult.DefaultValue
                };

                columns.Add(columnModel);
            }

            columns = columns
                .OrderBy(c => c.Name)
                .ToList();

            return Task.FromResult((IReadOnlyCollection<ColumnModel>)columns);
        }
    }
}
