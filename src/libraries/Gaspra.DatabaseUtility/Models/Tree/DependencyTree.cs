using Gaspra.DatabaseUtility.Extensions;
using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.MergeSprocs.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gaspra.DatabaseUtility.Models.Tree
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

            var factTables = schema.Tables.Where(t => t.ExtendedProperties.Any(e => e.Name.Equals("FactTable")));

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

            foreach (var table in currentDepthTables)
            {
                var constrainedTables = schema.GetTablesFrom(table.ConstrainedTo);

                foreach (var constrainedTable in constrainedTables.Distinct())
                {
                    // todo: handle the composite linking table
                    if (!constrainedTable.Name.Contains("composite", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!branches.ContainsTable(constrainedTable))
                        {

                            branchesAtCurrentDepth.Add(new DependencyBranch(nextDepth, constrainedTable.CorrelationId));

                            branchesAtCurrentDepth.AddRange(BranchOut(schema, nextDepth, branchesAtCurrentDepth));

                        }
                    }
                }
            }

            return branchesAtCurrentDepth
                .ToList()
                .Distinct(new DependencyBranchComparison());
        }
    }

    public class DependencyBranchComparison : IEqualityComparer<DependencyBranch>
    {
        public bool Equals([AllowNull] DependencyBranch x, [AllowNull] DependencyBranch y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.TableGuid.Equals(y.TableGuid);
        }

        public int GetHashCode([DisallowNull] DependencyBranch obj)
        {
            return HashCode.Combine(obj.TableGuid);
        }
    }

    public class TableComparison : IEqualityComparer<Table>
    {
        public bool Equals([AllowNull] Table x, [AllowNull] Table y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.CorrelationId.Equals(y.CorrelationId);
        }

        public int GetHashCode([DisallowNull] Table obj)
        {
            return HashCode.Combine(obj.CorrelationId);
        }
    }
}
