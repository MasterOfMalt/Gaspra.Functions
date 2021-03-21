using Gaspra.Database.Interfaces;

namespace Gaspra.Database.Models
{
    public class ConstraintModel : CorrelatedModel
    {
        public string Name { get; set; }
        public bool Parent { get; set; }
        public ColumnModel Reference { get; set; }
    }
}
