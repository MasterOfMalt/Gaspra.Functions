using System.Collections.Generic;
using Gaspra.Database.Models;
using Gaspra.SqlGenerator.Interfaces;

namespace Gaspra.SqlGenerator.Models
{
    public class MergeScriptVariableSet : IMergeScriptVariableSet
    {
        public string ScriptFileName { get; set; }
        public string ScriptName { get; set; }
        public SchemaModel Schema { get; set; }
        public TableModel Table { get; set; }
        public IReadOnlyCollection<ColumnModel> TableTypeColumns { get; set; }
        public string TableTypeVariableName { get; set; }
        public string TableTypeName { get; set; }
        public IReadOnlyCollection<ColumnModel> MergeIdentifierColumns { get; set; }
        public IReadOnlyCollection<ColumnModel> DeleteIdentifierColumns { get; set; }
        public IReadOnlyCollection<(TableModel joinTable, IReadOnlyCollection<ColumnModel> joinColumns, IReadOnlyCollection<ColumnModel> selectColumns)> TablesToJoin { get; set; }
        public (string ComparisonColumn, string RetentionMonths) RetentionPolicy { get; set; }
    }
}
