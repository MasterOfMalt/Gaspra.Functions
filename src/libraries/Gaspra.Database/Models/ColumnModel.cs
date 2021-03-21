using System.Collections.Generic;
using Gaspra.Database.Interfaces;

namespace Gaspra.Database.Models
{
    public class ColumnModel : CorrelatedModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public bool IdentityColumn { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string SeedValue { get; set; }
        public string IncrementValue { get; set; }
        public string DefaultValue { get; set; }
        public IReadOnlyCollection<ConstraintModel> Constraints { get; set; }
    }
}
