using System.Collections.Generic;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Models
{
    public class DeltaScriptVariableSet : IDeltaScriptVariableSet
    {
        public string ScriptFileName { get; set; }
        public string ScriptName { get; set; }
        public SchemaModel Schema { get; set; }
        public TableModel Table { get; set; }
        public string TableTypeName { get; set; }
        public IReadOnlyCollection<ColumnModel> TableTypeColumns { get; set; }
        public string TableTypeVariableName { get; set; }
        public string DomainIdentifierName { get; set; }
        public IReadOnlyCollection<IReadOnlyCollection<TableModel>> TablePaths { get; set; }
        public string DeltaSourceTableName { get; set; }
    }
}
