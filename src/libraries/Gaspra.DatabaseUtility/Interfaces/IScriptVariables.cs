using Gaspra.DatabaseUtility.Models.Database;
using Gaspra.DatabaseUtility.Models.Merge;
using System.Collections.Generic;

namespace Gaspra.DatabaseUtility.Interfaces
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
