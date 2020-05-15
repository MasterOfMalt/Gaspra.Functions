using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Database
{
    public class ForeignKeyConstraint
    {
        public IEnumerable<string> ConstrainedTo { get; set; }

        public ForeignKeyConstraint(
            IEnumerable<string> constrainedTo)
        {
            ConstrainedTo = constrainedTo;
        }
    }
}
