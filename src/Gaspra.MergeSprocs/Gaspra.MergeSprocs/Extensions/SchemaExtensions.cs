using Gaspra.MergeSprocs.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaspra.MergeSprocs.Extensions
{
    public static class SchemaExtensions
    {
        public static void CalculateDependencies(this Schema schema)
        {
            foreach(var table in schema.Tables)
            {
                var foreignKeys = table
                    .Columns
                    .Select(c => c.ForeignKey)
                    .Where(f => f != null && f.ConstrainedTo.Any());

                var constrainedToTables = schema
                    .Tables
                    .Where(t => foreignKeys
                        .SelectMany(f => f.ConstrainedTo)
                        .Contains(t.Name))
                    .Select(t => t.CorrelationId)
                    .ToList();

                table.ConstrainedTo = constrainedToTables;
            }
        }

        public static IEnumerable<Table> GetTablesFrom(this Schema schema, IEnumerable<Guid> guids)
        {
            var tables = schema.Tables.Where(t => guids.Contains(t.CorrelationId));

            return tables;
        }
    }
}
