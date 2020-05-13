using Gaspra.MergeSprocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaspra.MergeSprocs.Extensions
{
    public static class MergeSprocBuilderExtensions
    {
        public static string BuildMergeSproc(this MergeProcedureVariables variables)
        {
            var sproc = "";

            if (variables.MergeIdentifierColumns.Any())
            {
                sproc = $@"{General.Head()}
{TableType.Head(variables.TableTypeName, variables.SchemaName)}
{TableType.Body(variables.TableTypeName, variables.SchemaName, variables.TableTypeColumns)}
{TableType.Tail(variables.TableTypeName, variables.SchemaName)}
{MergeSproc.Head(variables.SchemaName, variables.ProcedureName, variables.TableTypeVariableName, variables.TableTypeName)}";

                if(variables.JoiningColumns != null && variables.JoiningColumns.Any())
                {
                    sproc += $@"{MergeSproc.TableVariable(variables.ProcedureName, variables.DatabaseTable, variables.TableTypeVariableName, variables.JoiningColumns)}
{MergeSproc.Body($"{variables.ProcedureName}Variable", variables.MergeIdentifierColumns.First().Name, variables.DatabaseTable)}";
                } else
                {
                    sproc += $"{MergeSproc.Body(variables.TableTypeVariableName, variables.MergeIdentifierColumns.First().Name, variables.DatabaseTable)}";
                }

                sproc += $@"{MergeSproc.Tail(variables.SchemaName, variables.ProcedureName)}"; //got to figure out how to use the table type columns in the merge body (getting id's for columns that don't exist)
            }
            return sproc;
        }








        public static class General
        {
            public static string Head()
            {
                return
$@"SET NOCOUNT ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
";
            }

            public static string NullableColumn(DatabaseColumn column)
            {
                return column.Nullable ? "NULL" : "NOT NULL";
            }

            public static string DataType(DatabaseColumn column)
            {
                var dataType = $"[{column.DataType}]";

                if (column.MaxLength.HasValue)
                {
                    dataType += $"({column.MaxLength.Value})";
                }

                return dataType;
            }
        }










        public static class TableType
        {
            public static string Head(string tableTypeName, string schemaName)
            {
                return
$@"IF NOT EXISTS (SELECT 1 FROM [sys].[types] st JOIN [sys].[schemas] ss ON st.schema_id = ss.schema_id WHERE st.name = N'{tableTypeName}' AND ss.name = N'{schemaName}')
BEGIN
";
            }

            public static string Body(string tableTypeName, string schemaName, IEnumerable<DatabaseColumn> columns)
            {
                var body =
$@"CREATE TYPE [{schemaName}].[{tableTypeName}] AS TABLE(
{string.Join($",{Environment.NewLine}", columns.Select(c => $"[{c.Name}] {General.DataType(c)} {General.NullableColumn(c)}"))}
)
";
                return body;
            }

            public static string Tail(string tableTypeName, string schemaName)
            {
                return
$@"END
GO

ALTER AUTHORIZATION ON TYPE::[{schemaName}].[{tableTypeName}] TO SCHEMA OWNER
GO
";
            }


        }





        public static class MergeSproc
        {
            public static string Head(string schemaName, string sprocName, string tableTypeVariable, string tableType)
            {
                return
$@"IF NOT EXISTS (SELECT 1 FROM [sys].[objects] WHERE [object_id] = OBJECT_ID(N'[{schemaName}].[{sprocName}]') AND [type] IN (N'P'))
BEGIN
	EXEC [dbo].[sp_executesql] @statement = N'CREATE PROCEDURE [{schemaName}].[{sprocName}] AS'
END
GO

ALTER PROCEDURE [{schemaName}].[{sprocName}]
    @{tableTypeVariable} {tableType} READONLY
AS
BEGIN

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
";
            }

            public static string TableVariable(string sprocName, DatabaseTable databaseTable, string tableTypeVariable, IEnumerable<(DatabaseTable table, DatabaseColumn column)> JoiningTables)
            {
                var tableVariable =
$@"DECLARE @{sprocName}Variable TABLE
(
{string.Join($",{Environment.NewLine}", databaseTable.Columns.Where(c => !c.IdentityColumn).Select(c => $"[{c.Name}] {General.DataType(c)} {General.NullableColumn(c)}"))}
)

INSERT INTO @{sprocName}Variable
SELECT
{string.Join($",{Environment.NewLine}", databaseTable.Columns.Where(c => !c.IdentityColumn).Select(c => $"{GetInsertInto(c)}"))}
FROM
    @{tableTypeVariable} AS tt
INNER JOIN {string.Join($"{Environment.NewLine}INNER JOIN ", JoiningTables.Select(t => $"[{t.table.Schema}].[{t.table.Name}] AS {t.table.Name.ToLower()} ON tt.{t.column.Name}={t.table.Name.ToLower()}.{t.column.Name}"))}

";
                return tableVariable;
            }

            private static string GetInsertInto(DatabaseColumn column)
            {//todo
                if(column.Name.Equals("OrderFactId"))
                {
                    return $"orderfact.OrderFactId";
                }

                return column.Name;
            }

            public static string Body(string tableTypeVariable, string matchOn, DatabaseTable databaseTable)
            {
                var sproc =
$@"MERGE [{databaseTable.Schema}].[{databaseTable.Name}] AS t
USING @{tableTypeVariable} AS s
    ON (t.{matchOn} = s.{matchOn})

WHEN NOT MATCHED
    THEN INSERT (
        {string.Join($",{Environment.NewLine}        ", databaseTable.Columns.Where(c => !c.IdentityColumn).Select(c => c.Name))}
    )
    VALUES (
        {string.Join($",{Environment.NewLine}        ", databaseTable.Columns.Where(c => !c.IdentityColumn).Select(c => $"s.{c.Name}"))}
    );
";
                return sproc;
            }

            public static string Tail(string schemaName, string sprocName)
            {
                return
$@"END
GO
ALTER AUTHORIZATION ON [{schemaName}].[{sprocName}] TO SCHEMA OWNER
GO";
            }

        }
    }
}
