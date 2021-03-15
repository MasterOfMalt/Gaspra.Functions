using System.Collections.Generic;

namespace Gaspra.Database.Models.QueryResults
{
    public class DatabaseResult
    {
        public IReadOnlyCollection<TableResult> Tables { get; set; }
        public IReadOnlyCollection<ConstraintResult> Constraints { get; set; }
        public IReadOnlyCollection<PropertyResult> Properties { get; set; }
    }
}
