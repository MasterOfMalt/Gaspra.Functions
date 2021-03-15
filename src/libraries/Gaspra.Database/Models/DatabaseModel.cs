using Gaspra.Database.Interfaces;
using System.Collections.Generic;

namespace Gaspra.Database.Models
{
    public class DatabaseModel : CorrelatedModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<SchemaModel> Schemas { get; set; }
    }
}
