using System.Collections.Generic;
using Gaspra.Database.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IMergeScriptVariableSet : IScriptVariableSet
    {
        //todo; refactor
        IReadOnlyCollection<ColumnModel> TableTypeColumns { get; set; }
        string TableTypeVariableName { get; set; }
        string TableTypeName { get; set; }
        IReadOnlyCollection<ColumnModel> MergeIdentifierColumns { get; set; }
        IReadOnlyCollection<ColumnModel> DeleteIdentifierColumns { get; set; }
        IReadOnlyCollection<(TableModel joinTable, IReadOnlyCollection<ColumnModel> joinColumns, IReadOnlyCollection<ColumnModel> selectColumns)> TablesToJoin { get; set; }
        (string ComparisonColumn, string RetentionMonths) RetentionPolicy { get; set; }
    }
}
