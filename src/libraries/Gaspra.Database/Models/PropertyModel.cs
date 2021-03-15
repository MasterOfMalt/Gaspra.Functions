using Gaspra.Database.Interfaces;

namespace Gaspra.Database.Models
{
    public class PropertyModel : CorrelatedModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
