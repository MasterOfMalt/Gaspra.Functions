using System.Collections.Generic;

namespace Gaspra.Database.Models
{
    public class DatabaseModel
    {
        public IReadOnlyCollection<SchemaModel> Schemas { get; set; }
    }
}
