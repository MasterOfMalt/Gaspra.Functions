using Gaspra.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gaspra.Database.Extensions
{
    public static class TableModelExtensions
    {
        /// <summary>
        /// Add extended property to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
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

        /// <summary>
        /// Add a dependant table to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="dependant"></param>
        public static void AddDependantTable(this TableModel table, TableModel dependant)
        {
            var dependants = table.DependantTables ?? new List<TableModel>();

            dependants.Add(dependant);

            table.DependantTables = dependants;
        }

        /// <summary>
        /// Add a reference table to a table model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reference"></param>
        public static void AddReferenceTable(this TableModel table, TableModel reference)
        {
            var references = table.ReferenceTables ?? new List<TableModel>();

            references.Add(reference);

            table.ReferenceTables = references;
        }

        /// <summary>
        /// Set all the tables in a collection to a given depth
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static Task SetTableDepths(this ICollection<TableModel> tables, int depth)
        {
            foreach (var table in tables)
            {
                table.Depth = depth;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Recursively work down all the tables increasing the
        /// depth with each recursive call
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static async Task RecurseTableDepths(this ICollection<TableModel> tables, int depth, DatabaseModel databaseModel)
        {
            if (tables != null && tables.Any())
            {
                var nextDepth = depth + 1;

                await tables.SetTableDepths(depth);

                foreach (var table in tables)
                {
                    await RecurseTableDepths(await table.GetConnectingTables(databaseModel), nextDepth, databaseModel);
                }
            }
        }

        /// <summary>
        /// Get all the tables that connect to a table via reference
        /// and dependant constraints
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Task<ICollection<TableModel>> GetConnectingTables(this TableModel table, DatabaseModel databaseModel)
        {
            var referenceTables = table
                .ReferenceTables?
                .Where(t => !t.IsLinkTable(databaseModel) && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var dependantTables = table
                .DependantTables?
                .Where(t => !t.IsLinkTable(databaseModel) && t.Depth.Equals(-1))
                .ToList() ?? new List<TableModel>();

            var tables = referenceTables;

            tables.AddRange(dependantTables);

            tables = tables
                .Distinct(TableModelComparison.Instance)
                .ToList();

            return Task.FromResult((ICollection<TableModel>)tables);
        }

        /// <summary>
        /// Returns true when table is a link table, a link table
        /// is defined by a table only being made up of constraints
        /// linking other tables together
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsLinkTable(this TableModel table, DatabaseModel databaseModel)
        {
            var isLink = false;

            if (table.Depth.Equals(-1))
            {
                var allColumnsAreConstraints = table
                    .Columns
                    .All(c => c.IdentityColumn || c.Constraint != null);

                if (allColumnsAreConstraints)
                {
                    var allReferencesHaveDepth = true;

                    foreach (var column in table.Columns.Where(c => !c.IdentityColumn))
                    {
                        var referenceTable = databaseModel.GetTableWithColumn(column.Constraint.Reference);

                        if (referenceTable.Depth.Equals(-1))
                        {
                            allReferencesHaveDepth = false;
                        }
                    }

                    isLink = allReferencesHaveDepth;
                }
            }

            return isLink;
        }
    }
}
