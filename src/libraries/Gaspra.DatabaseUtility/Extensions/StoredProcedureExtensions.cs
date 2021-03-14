namespace Gaspra.DatabaseUtility.Extensions
{
    public static class StoredProcedureExtensions
    {
        public static string GetDatabaseInformation()
        {
            return @"
/* Table Information */
DECLARE @TableInformation TABLE
(
    [TableSchema] [NVARCHAR](255),
    [Table] [NVARCHAR](255),
    [Column] [NVARCHAR](255),
    [ColumnId] [INT],
    [Nullable] [BIT],
    [Identity] [BIT],
    [DataType] [NVARCHAR](255),
    [MaxLength] [INT],
    [Precision] [INT],
    [Scale] [INT],
    [SeedValue] [SQL_VARIANT],
    [IncrementValue] [SQL_VARIANT],
    [DefaultValue] [NVARCHAR](255)
)

;WITH TableInformation AS (
    SELECT
	    ist.table_schema AS TableSchema,
	    so.name AS TableName,
	    sc.name AS ColumnName,
        sc.column_id AS ColumnId,
	    sc.is_nullable AS Nullable,
	    sc.is_identity AS IdentityColumn,
	    ct.name AS DataType,
	    CAST(CASE
                WHEN ct.name = 'text' THEN
                    NULL
	            ELSE
	                CASE
                        WHEN sc.precision=0 AND ct.name <> 'text' THEN
	                        sc.max_length
	                    ELSE
                            NULL
	                END
	         END AS SMALLINT) AS MaxLength,
	    CAST(CASE
                WHEN sc.precision>0 and ct.precision=ct.scale THEN
	                sc.precision
                ELSE
                    null
             END AS TINYINT) AS Precision,
	    CAST(CASE
                WHEN sc.precision>0 and ct.precision=ct.scale THEN
	                sc.scale
                ELSE
                    NULL
             END AS TINYINT) AS Scale,
	    CAST(CASE
                WHEN sc.is_identity=1 THEN
	                seed_value
                ELSE
                    NULL
             END AS SQL_VARIANT) AS SeedValue,
	    CAST(CASE
                WHEN sc.is_identity=1 THEN
	                increment_value
                ELSE
                    NULL
             END AS SQL_VARIANT) AS IncrementValue,
	    CAST(CASE
                WHEN sc.default_object_id>0 THEN
	                definition
                ELSE
                    NULL
             END AS NVARCHAR(4000)) AS DefaultValue
	FROM
        [information_schema].[tables] AS ist
	    INNER JOIN [sys].[objects] AS so ON ist.[table_schema] = SCHEMA_NAME(so.schema_id) AND ist.table_name = so.name
	    INNER JOIN [sys].[columns] AS sc ON so.object_id=sc.object_id
	    LEFT JOIN [sys].[identity_columns] AS ic ON so.object_id=ic.object_id
	    INNER JOIN [sys].[types] AS ct ON sc.system_type_id=ct.system_type_id AND ct.system_type_id=ct.user_type_id
	    LEFT JOIN [sys].[default_constraints] AS dc ON sc.default_object_id=dc.object_id
	WHERE
        so.type='u'
)

INSERT INTO
    @TableInformation
SELECT
    ti.TableSchema,
    ti.TableName,
    ti.ColumnName,
    ti.ColumnId,
    ti.Nullable,
    ti.IdentityColumn,
    ti.DataType,
    ti.MaxLength,
    ti.Precision,
    ti.Scale,
    ti.SeedValue,
    ti.IncrementValue,
    ti.DefaultValue
FROM
    TableInformation ti

/* Constraint Information */
DECLARE @ConstraintInformation TABLE
(
    [ConstraintSchema] [NVARCHAR](255) NOT NULL,
    [ConstraintName] [NVARCHAR](255) NOT NULL,
    [ConstraintTable] [NVARCHAR](255) NOT NULL,
    [ConstraintColumn] [NVARCHAR](255) NOT NULL,
    [ReferencedTable] [NVARCHAR](255) NOT NULL,
    [ReferencedColumn] [NVARCHAR](255) NOT NULL
)

INSERT INTO
    @ConstraintInformation
SELECT
    SCHEMA_NAME(fk.schema_id) AS ConstraintSchema,
    OBJECT_NAME(fkc.constraint_object_id) AS ConstraintName,
    OBJECT_NAME(fkc.parent_object_id) AS ConstraintTable,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ConstraintColumn,
    OBJECT_NAME(fkc.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM
    [sys].[foreign_keys] fk
    INNER JOIN [sys].[foreign_key_columns] fkc ON fk.object_id=fkc.constraint_object_id

/* Extended properties */
DECLARE @ExtendedPropertyInformation TABLE
(
    [PropertySchema] [NVARCHAR](255) NOT NULL,
    [PropertyTable] [NVARCHAR](255) NOT NULL,
    [PropertyKey] [NVARCHAR](255) NOT NULL,
    [PropertyValue] [SQL_VARIANT] NOT NULL
)

INSERT INTO
    @ExtendedPropertyInformation
SELECT
	SCHEMA_NAME(o.schema_id) AS PropertySchema,
	o.name AS PropertyTable,
	e.name AS PropertyKey,
	value AS PropertyValue
FROM
	[sys].[extended_properties] AS e
    INNER JOIN [sys].[objects] AS o ON e.major_id=o.object_id

/* Select */
SELECT
    [TableSchema],
    [Table],
    [Column],
    [ColumnId],
    [Nullable],
    [Identity],
    [DataType],
    [MaxLength],
    [Precision],
    [Scale],
    [SeedValue],
    [IncrementValue],
    [DefaultValue]
FROM
    @TableInformation

SELECT
    [ConstraintSchema],
    [ConstraintName],
    [ConstraintTable],
    [ConstraintColumn],
    [ReferencedTable],
    [ReferencedColumn]
FROM
    @ConstraintInformation

SELECT
    [PropertySchema],
    [PropertyTable],
    [PropertyKey],
    [PropertyValue]
FROM
    @ExtendedPropertyInformation
";
        }
    }
}