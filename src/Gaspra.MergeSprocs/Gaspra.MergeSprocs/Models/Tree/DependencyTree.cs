using Gaspra.MergeSprocs.Extensions;
using Gaspra.MergeSprocs.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Tree
{
    public class DependencyTree
    {
        public IEnumerable<DependencyBranch> Branches { get; set; }

        public DependencyTree(IEnumerable<DependencyBranch> branches)
        {
            Branches = branches;
        }

        public static DependencyTree Calculate(Schema schema)
        {
            var branches = new List<DependencyBranch>();

            var factTables = schema.Tables.Where(t => t.ExtendedProperties.Any(e => e.Name.Equals("MergeIdentifier")));

            var depth = 1;

            foreach (var table in factTables)
            {
                branches.Add(new DependencyBranch(depth, table.CorrelationId));
            }

            return new DependencyTree(BranchOut(schema, depth, branches));
        }

        private static IEnumerable<DependencyBranch> BranchOut(Schema schema, int depth, IList<DependencyBranch> branches)
        {
            var branchesAtCurrentDepth = branches
                .Where(b => b.Depth.Equals(depth))
                .ToList();

            var nextDepth = depth + 1;

            var currentDepthTableGuids = branchesAtCurrentDepth
                .Select(b => b.TableGuid);

            var currentDepthTables = schema
                .GetTablesFrom(currentDepthTableGuids);

            foreach(var table in currentDepthTables)
            {
                var constrainedTables = schema.GetTablesFrom(table.ConstrainedTo);

                foreach(var constrainedTable in constrainedTables)
                {
                    if (!branches.ContainsTable(constrainedTable))
                    {
                        branchesAtCurrentDepth.Add(new DependencyBranch(nextDepth, constrainedTable.CorrelationId));

                        branchesAtCurrentDepth.AddRange(BranchOut(schema, nextDepth, branchesAtCurrentDepth));
                    }
                }
            }

            return branchesAtCurrentDepth
                .ToList()
                /*
                 * distinct stops cyclic dependencies
                 */
                .Distinct();
        }
    }
}
