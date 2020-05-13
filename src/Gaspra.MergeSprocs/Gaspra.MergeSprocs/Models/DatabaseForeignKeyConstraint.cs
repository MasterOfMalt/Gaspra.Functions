using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models
{
    public class DatabaseForeignKeyConstraint
    {
        public string Name { get; set; }
        public string ConstraintColumn { get; set; }
        public string ReferenceSchema { get; set; }
        public string ReferenceTable { get; set; }
        public string ReferenceColumn { get; set; }
    }
}
