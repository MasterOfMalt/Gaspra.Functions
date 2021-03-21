using Gaspra.Database.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.Database.Extensions
{
    public static class DatabaseModelExtensions
    {
        /// <summary>
        /// Work out all the depth values for all the tables in all the schemas
        /// </summary>
        /// <param name="databaseModel"></param>
        /// <returns></returns>
        public static async Task CalculateTableDepth(this DatabaseModel database)
        {
            var depth = 1;

            foreach (var schema in database.Schemas)
            {
                var factTables = schema
                    .Tables
                    .Where(t => t.Properties.ContainsKey("FactTable"))
                    .ToList();

                if (factTables.Any())
                {
                    await factTables.RecurseTableDepths(depth, database);
                }

                var parentConstraintTables = schema
                    .Tables
                    .Where(t =>
                        !t.IsLinkTable(database) &&
                        t.Depth.Equals(-1) &&
                        (t.DependantTables == null || !t.DependantTables.Any()))
                    .ToList();

                if (parentConstraintTables.Any())
                {
                    await parentConstraintTables.RecurseTableDepths(depth, database);
                }

                var linkTables = schema
                    .Tables
                    .Where(t => t.IsLinkTable(database))
                    .ToList();

                foreach (var linkTable in linkTables)
                {
                    if (linkTable.DependantTables != null)
                    {
                        var linkDepth = linkTable
                            .DependantTables
                            .Select(t => t.Depth)
                            .OrderByDescending(d => d)
                            .First();

                        linkTable.Depth = linkDepth + 1;
                    }
                }

                // if (linkTables.Any())
                // {
                //     await linkTables.RecurseTableDepths(0, database);
                // }
            }
        }

        /// <summary>
        /// Get the table model that contains the column model
        /// </summary>
        /// <param name="database"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static TableModel GetTableWithColumn(this DatabaseModel database, ColumnModel column)
        {
            foreach (var schema in database.Schemas)
            {
                foreach (var table in schema.Tables)
                {
                    if(table.Columns.Any(t => t.CorrelationId.Equals(column.CorrelationId)))
                    {
                        return table;
                    }
                }
            }

            return null;
        }
    }
}
