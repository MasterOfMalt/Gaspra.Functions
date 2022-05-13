using System.Collections.Generic;
using Gaspra.Database.Models;

namespace Gaspra.SqlGenerator.Interfaces
{
    public interface IDeltaScriptVariableSet : IScriptVariableSet
    {
        string TableTypeName { get; set; }
        IReadOnlyCollection<ColumnModel> TableTypeColumns { get; set; }
        string TableTypeVariableName { get; set; }
        string DomainIdentifierName { get; set; }
        IReadOnlyCollection<IReadOnlyCollection<TableModel>> TablePaths { get; set; }
        string DeltaSourceTableName { get; set; }
    }
}
