using System.Collections.Generic;

namespace Deprecated.Gaspra.DatabaseUtility.Models.DataAccess
{
    public class DatabaseInformation
    {
        public IReadOnlyCollection<ColumnInformation> Columns { get; set; }
        public IReadOnlyCollection<FKConstraintInformation> ForeignKeys { get; set; }
        public IReadOnlyCollection<ExtendedPropertyInformation> ExtendedProperties { get; set; }
    }
}
