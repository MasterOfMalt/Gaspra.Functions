using System.Collections.Generic;

namespace Gaspra.MergeSprocs.Models
{
    public class MergeProcedureVariables
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ProcedureName { get; set; }
        public string TableTypeVariableName { get; set; }
        public string TableTypeName { get; set; }

        public DatabaseTable DatabaseTable { get; set; }
        public IEnumerable<DatabaseTableDependencyTree> Dependencies { get; set; }

        public IEnumerable<DatabaseColumn> MergeIdentifierColumns { get; set; }
        public IEnumerable<DatabaseColumn> TableTypeColumns { get; set; }
        public IEnumerable<(DatabaseTable, DatabaseColumn)> JoiningColumns { get; set; }

    }
}
