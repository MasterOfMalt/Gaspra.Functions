using Gaspra.Database.Interfaces;
using System.Collections.Generic;

namespace Gaspra.Database.Models
{
    public class SchemaModel : CorrelatedModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<TableModel> Tables { get; set; }
    }
}
