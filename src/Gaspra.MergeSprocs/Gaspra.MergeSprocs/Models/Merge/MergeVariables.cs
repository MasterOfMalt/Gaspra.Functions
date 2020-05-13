using Gaspra.MergeSprocs.Models.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.Models.Merge
{
    public class MergeVariables
    {
        public Schema Schema { get; set; }
        public Table Table { get; set; }





    }

    public static class MergeVariablesExtensions
    {
        public static string ProcedureName(this MergeVariables variables)
        {
            return $"Merge{variables.Table.Name}";
        }

        public static string TableTypeVariableName(this MergeVariables variables)
        {
            return $"{variables.Table.Name}";
        }

        public static string TableTypeName(this MergeVariables variables)
        {
            return $"TT_{variables.Table.Name}";
        }

        public static IEnumerable<Column> TableTypeColumns(this MergeVariables variables)
        {

        }

        public static IEnumerable<Column> MergeIdentifierColumns(this MergeVariables variables)
        {

        }

        public static IEnumerable<Table> TablesToJoin(this MergeVariables variables)
        {

        }
    }
}
