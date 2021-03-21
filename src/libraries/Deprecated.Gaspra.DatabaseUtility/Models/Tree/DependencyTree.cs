using Deprecated.Gaspra.DatabaseUtility.Extensions;
using Gaspra.MergeSprocs.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;

namespace Deprecated.Gaspra.DatabaseUtility.Models.Tree
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
                branches.Add(new DependencyBranch(depth, table.CorrelationId, table.Name));
            }

            return new DependencyTree(BranchOut(schema, depth, branches, factTables.Select(f => f.Name)));
        }

        private static IEnumerable<DependencyBranch> BranchOut(Schema schema, int depth, IList<DependencyBranch> branches, IEnumerable<string> completedTables)
        {
            var branchesAtCurrentDepth = branches
                .Where(b => b.Depth.Equals(depth))
                .ToList();

            var nextDepth = depth + 1;

            var currentDepthTableGuids = branchesAtCurrentDepth
                .Select(b => b.TableGuid);

            var currentDepthTables = schema
                .GetTablesFrom(currentDepthTableGuids);

            var tablesCompleted = completedTables.ToList();

            foreach (var table in currentDepthTables)
            {
                var constrainedTables = schema.GetTablesFrom(table.ConstrainedTo)
                    .Where(t => !t.Name.StartsWith("Link"));

                foreach (var constrainedTable in constrainedTables.Distinct())
                {
                    // todo: handle the composite linking table
                    if (!tablesCompleted.Any(t => t.Equals(constrainedTable.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (!branches.ContainsTable(constrainedTable))
                        {
                            branchesAtCurrentDepth.Add(new DependencyBranch(nextDepth, constrainedTable.CorrelationId, table.Name));

                            tablesCompleted.Add(constrainedTable.Name);

                            branchesAtCurrentDepth.AddRange(BranchOut(schema, nextDepth, branchesAtCurrentDepth, tablesCompleted));
                        }
                    }
                }

                //todo: clean this up, we should be able to order the tables dependencies a little nicer
                var constrainedLinkTables = schema.GetTablesFrom(table.ConstrainedTo)
                    .Where(t => t.Name.StartsWith("Link"));

                foreach (var constrainedTable in constrainedLinkTables.Distinct())
                {
                    if (!tablesCompleted.Any(t => t.Equals(constrainedTable.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (!branches.ContainsTable(constrainedTable))
                        {
                            branchesAtCurrentDepth.Add(new DependencyBranch(nextDepth, constrainedTable.CorrelationId, table.Name));

                            tablesCompleted.Add(constrainedTable.Name);

                            branchesAtCurrentDepth.AddRange(BranchOut(schema, nextDepth, branchesAtCurrentDepth, tablesCompleted));
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
