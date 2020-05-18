using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Extensions
{
    public static class DependencyTreeExtensions
    {
        public static Guid GetParentOf(this DependencyTree tree, Table table)
        {
            var tableBranch = tree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

            var connectingBranches = tree
                .Branches
                .Where(b => table.ConstrainedTo.Any(c => c.Equals(b.TableGuid)));

            var parentTable = connectingBranches
                .Where(c => c.Depth.Equals(tableBranch.Depth - 1))
                .Select(c => c.TableGuid)
                .FirstOrDefault();

            return parentTable;
        }

        public static IEnumerable<Guid> GetChildrenOf(this DependencyTree tree, Table table)
        {
            var tableBranch = tree
                .Branches
                .Where(b => b.TableGuid.Equals(table.CorrelationId))
                .FirstOrDefault();

            var connectingBranches = tree
                .Branches
                .Where(b => table.ConstrainedTo.Any(c => c.Equals(b.TableGuid)));

            var childrenTables = connectingBranches
                .Where(c => c.Depth.Equals(tableBranch.Depth + 1))
                .Select(c => c.TableGuid);

            return childrenTables;
        }

        public static IEnumerable<DependencyBranch> GetRelatedBranches(this DependencyTree tree, Table table)
        {
            var constrainedTableGuids = table
                .ConstrainedTo;

            var relatedBranches = tree
                .Branches
                .Where(b => constrainedTableGuids
                    .Any(c => c.Equals(b.TableGuid)));

            return relatedBranches;
        }
    }
}
