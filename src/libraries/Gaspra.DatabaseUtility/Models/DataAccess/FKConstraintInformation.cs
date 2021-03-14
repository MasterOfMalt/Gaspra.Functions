using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Gaspra.DatabaseUtility.Extensions;

namespace Gaspra.DatabaseUtility.Models.DataAccess
{
    public class FKConstraintInformation
    {
        public string ConstraintSchema { get; set; }
        public string ConstraintName { get; set; }
        public string ConstraintTable { get; set; }
        public string ConstraintColumn { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }

        public FKConstraintInformation(
            string constraintSchema,
            string constraintName,
            string constraintTable,
            string constraintColumn,
            string referencedTable,
            string referencedColumn)
        {
            ConstraintSchema = constraintSchema;
            ConstraintName = constraintName;
            ConstraintTable = constraintTable;
            ConstraintColumn = constraintColumn;
            ReferencedTable = referencedTable;
            ReferencedColumn = referencedColumn;
        }

        public static async Task<IEnumerable<FKConstraintInformation>> FromDataReader(SqlDataReader dataReader)
        {
            var fkConstraints = new List<FKConstraintInformation>();

            while (await dataReader.ReadAsync())
            {
                var constraintSchema = dataReader[nameof(ConstraintSchema)].GetValue<string>();
                var constraintName = dataReader[nameof(ConstraintName)].GetValue<string>();
                var constraintTable = dataReader[nameof(ConstraintTable)].GetValue<string>();
                var constraintColumn = dataReader[nameof(ConstraintColumn)].GetValue<string>();
                var referencedTable = dataReader[nameof(ReferencedTable)].GetValue<string>();
                var referencedColumn = dataReader[nameof(ReferencedColumn)].GetValue<string>();

                fkConstraints.Add(new FKConstraintInformation(
                    constraintSchema,
                    constraintName,
                    constraintTable,
                    constraintColumn,
                    referencedTable,
                    referencedColumn));
            }

            return fkConstraints;
        }
    }
}
