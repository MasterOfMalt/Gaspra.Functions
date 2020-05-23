using Gaspra.MergeSprocs.DataAccess.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Gaspra.MergeSprocs.DataAccess.Models
{
    public class FKConstraintInformation
    {
        public string ConstraintName { get; set; }
        public string ConstraintTableSchema { get; set; }
        public string ConstraintTableName { get; set; }
        public string ConstraintTableColumn { get; set; }
        public string ReferencedTableSchema { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedTableColumn { get; set; }

        public FKConstraintInformation(
            string constraintName,
            string constraintTableSchema,
            string constraintTableName,
            string constraintTableColumn,
            string referencedTableSchema,
            string referencedTableName,
            string referencedTableColumn)
        {
            ConstraintName = constraintName;
            ConstraintTableSchema = constraintTableSchema;
            ConstraintTableName = constraintTableName;
            ConstraintTableColumn = constraintTableColumn;
            ReferencedTableSchema = referencedTableSchema;
            ReferencedTableName = referencedTableName;
            ReferencedTableColumn = referencedTableColumn;
        }

        public static async Task<IEnumerable<FKConstraintInformation>> FromDataReader(SqlDataReader dataReader)
        {
            var fkConstraints = new List<FKConstraintInformation>();

            while (await dataReader.ReadAsync())
            {
                var constraintName = dataReader[nameof(ConstraintName)].GetValue<string>();
                var constraintTableSchema = dataReader[nameof(ConstraintTableSchema)].GetValue<string>();
                var constraintTableName = dataReader[nameof(ConstraintTableName)].GetValue<string>();
                var constraintTableColumn = dataReader[nameof(ConstraintTableColumn)].GetValue<string>();
                var referencedTableSchema = dataReader[nameof(ReferencedTableSchema)].GetValue<string>();
                var referencedTableName = dataReader[nameof(ReferencedTableName)].GetValue<string>();
                var referencedColumnName = dataReader[nameof(ConstraintTableColumn)].GetValue<string>(); //todo

                fkConstraints.Add(new FKConstraintInformation(
                    constraintName,
                    constraintTableSchema,
                    constraintTableName,
                    constraintTableColumn,
                    referencedTableSchema,
                    referencedTableName,
                    referencedColumnName));
            }

            return fkConstraints;
        }
    }
}
