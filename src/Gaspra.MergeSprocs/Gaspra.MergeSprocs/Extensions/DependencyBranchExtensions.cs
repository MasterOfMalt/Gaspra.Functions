using Gaspra.MergeSprocs.Models.Database;
using Gaspra.MergeSprocs.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Extensions
{
    public static class DependencyBranchExtensions
    {
        public static bool ContainsTable(this IList<DependencyBranch> branches, Table table)
        {
            return branches.Select(b => b.TableGuid).Contains(table.CorrelationId);
        }
    }
}
