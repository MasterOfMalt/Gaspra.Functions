using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class TableTree
    {
        public IEnumerable<(int depth, TableDependencies dependencies)> Branches { get; set; }

        public TableTree(IEnumerable<(int depth, TableDependencies dependencies)> branches)
        {
            Branches = branches;
        }

        public static TableTree Build(Schema schema)
        {
            var branches = new List<(int depth, TableDependencies dependencies)>();

            var factTables = schema.Tables.Where(t => t.ExtendedProperties.Any(e => e.Name.Equals("MergeIdentifier")));

            var depth = 1;

            foreach (var table in factTables)
            {
                branches.Add((depth, TableDependencies.From(table, schema)));
            }

            return new TableTree(BranchOut(depth, schema, branches));
        }

        /*
         * recurses down a tree of related tables, calculating a heirarchy of tables based on
         * knowing the starting point. (This gives us context of whether we have a foreignkey value
         * at a specific point in the merges or not)
         */
        private static IEnumerable<(int depth, TableDependencies dependencies)> BranchOut(int depth, Schema schema, IEnumerable<(int depth, TableDependencies dependencies)> branches)
        {
            var branchedDependencies = branches.ToList();

            var branchesAtCurrentDepth = branchedDependencies.Where(b => b.depth.Equals(depth));

            var nextDepth = depth + 1;

            var tablesToIterateThrough = branchesAtCurrentDepth
                .Select(b => b.dependencies.CurrentTable)
                .Select(t => TableDependencies.From(t, schema))
                .SelectMany(d => d.ConstrainedToTables)
                .Where(t => !branchedDependencies.Select(d => d.dependencies.CurrentTable).Any(d => d.Name.Equals(t.Name)));

            if (tablesToIterateThrough.Any())
            {
                var toIterate = tablesToIterateThrough.ToList();

                branchedDependencies.AddRange(toIterate.Select(t => (nextDepth, TableDependencies.From(t, schema))));

                branchedDependencies.AddRange(BranchOut(nextDepth, schema, branchedDependencies));
            }

            return branchedDependencies.ToList().Distinct();
        }
    }
}
