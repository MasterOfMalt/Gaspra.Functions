using Gaspra.Database.Interfaces;
using System.Collections.Generic;

namespace Gaspra.Database.Models
{
    public class TableModel : CorrelatedModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<ColumnModel> Columns { get; set; }
        public IReadOnlyCollection<PropertyModel> Properties { get; set; }
    }
}
