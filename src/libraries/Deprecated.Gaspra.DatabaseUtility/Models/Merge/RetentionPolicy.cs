using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deprecated.Gaspra.DatabaseUtility.Models.Merge
{
    public class RetentionPolicy
    {
        public string ComparisonColumn { get; set; }
        public string RetentionMonths { get; set; }
    }
}
