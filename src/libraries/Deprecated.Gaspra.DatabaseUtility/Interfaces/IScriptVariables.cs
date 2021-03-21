using System.Collections.Generic;
using Deprecated.Gaspra.DatabaseUtility.Models.Database;
using Deprecated.Gaspra.DatabaseUtility.Models.Merge;

namespace Deprecated.Gaspra.DatabaseUtility.Interfaces
{
    public interface IScriptVariables
    {
        //todo; straight copy from MergeVariables, needs cleaning up
        string ProcedureName { get; set; }
        string SchemaName { get; set; }
        Table Table { get; set; }
        IEnumerable<Column> TableTypeColumns { get; set; }
        IEnumerable<Column> MergeIdentifierColumns { get; set; }
        IEnumerable<Column> DeleteIdentifierColumns { get; set; }
        RetentionPolicy RetentionPolicy { get; set; }
        IEnumerable<(Table joinTable, IEnumerable<Column> joinColumns, IEnumerable<Column> selectColumns)> TablesToJoin { get; set; }
    }
}
