using System;

namespace Gaspra.MergeSprocs.Models.Tree
{
    public class DependencyBranch
    {
        public int Depth { get; set; }
        public Guid TableGuid { get; set; }

        public DependencyBranch(
            int depth,
            Guid tableGuid)
        {
            Depth = depth;
            TableGuid = tableGuid;
        }
    }
}
