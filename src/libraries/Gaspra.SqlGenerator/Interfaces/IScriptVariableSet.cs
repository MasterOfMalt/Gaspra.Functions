using Gaspra.Database.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptVariableSet
    {
        string ScriptName { get; set; }
        SchemaModel Schema { get; set; }
        TableModel Table { get; set; }
    }
}
