using Gaspra.Database.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IScriptVariableSet
    {
        string ScriptFileName { get; set; }
        string ScriptName { get; set; }
        SchemaModel Schema { get; set; }
        TableModel Table { get; set; }
    }
}
