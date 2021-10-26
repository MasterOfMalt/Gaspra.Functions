using System.Collections.Generic;
using System.Linq;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Tree;

namespace Deprecated.Gaspra.DatabaseUtility.Extensions
{
    public static class DependencyBranchExtensions
    {
        public static bool ContainsTable(this IList<DependencyBranch> branches, Table table)
        {
            return branches.Select(b => b.TableGuid).Contains(table.CorrelationId);
        }
    }
}
