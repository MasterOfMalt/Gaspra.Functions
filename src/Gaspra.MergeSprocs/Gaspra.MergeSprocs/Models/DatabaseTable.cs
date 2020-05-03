using Gaspra.MergeSprocs.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models
{
    public static class DatabaseTableExtensions
    {
        public static IEnumerable<DatabaseTable> ConvertToDatabaseTableObjects(this IEnumerable<ColumnInformation> columns)
        {
            //todo
            var databaseTables = new List<DatabaseTable>();

            foreach (var column in columns)
            {
                var databaseTable = "todo"; // get table from databaseTables (if it doesn't exist add it)
            }


            return null;
        }
    }

    public class DatabaseTable
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IEnumerable<DatabaseColumn> Columns { get; set; }
    }

    public class DatabaseColumn
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public bool IdentityColumn { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? SeedValue { get; set; }
        public int? IncrementValue { get; set; }
        public string DefaultValue { get; set; }
    }


}
