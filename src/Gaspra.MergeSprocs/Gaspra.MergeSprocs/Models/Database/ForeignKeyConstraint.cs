using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class ForeignKeyConstraint
    {
        public bool IsParent { get; set; }
        public IEnumerable<string> ConstrainedTo { get; set; }

        public ForeignKeyConstraint(
            bool isParent,
            IEnumerable<string> constrainedTo)
        {
            IsParent = isParent;
            ConstrainedTo = constrainedTo;
        }
    }
}
