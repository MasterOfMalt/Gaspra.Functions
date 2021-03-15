using System.Collections.Generic;

namespace Gaspra.Database.Models.QueryResults
{
    public class DatabaseResult
    {
        public IReadOnlyCollection<ColumnModel> Columns { get; set; }
        public IReadOnlyCollection<ConstraintModel> Constraints { get; set; }
        public IReadOnlyCollection<PropertyModel> Properties { get; set; }
    }
}
