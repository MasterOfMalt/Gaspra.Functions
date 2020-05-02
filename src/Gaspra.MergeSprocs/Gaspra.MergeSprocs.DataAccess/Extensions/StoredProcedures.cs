using System;
using System.Collections.Generic;
using System.Text;

namespace Gaspra.MergeSprocs.DataAccess.Extensions
{
    public static class StoredProcedures
    {
        public static string GetTableInformation()
        {
            return
                @"
                    /*
                     *
                     * Get all the table information from a schema
                     *    
                     */
                    
                     ;WITH TableInformation AS (
                    
                        SELECT 
                    	    [InformationSchemaTables].[TABLE_SCHEMA] AS [TableSchema], 
                    	    [SysObjects].[name] AS [TableName], 
                    	    [SysColumns].[name] AS [ColumnName], 
                            [SysColumns].[column_id] AS [ColumnId],
                    	    [SysColumns].[is_nullable] AS [Nullable], 
                    	    [SysColumns].[is_identity] AS [IdentityColumn], 
                    /*	    
                            [SysColumns].[encryption_algorithm_name] AS [EncryptionAlgorithmName], 
                    	    [SysColumns].[encryption_type_desc] AS [EncryptionTypeDescription], 
                    	    CASE 
                                WHEN [ColumnEncryptionKeys].[name] IS NULL THEN 
                                    NULL 
                                ELSE 
                                    [ColumnEncryptionKeys].[name] 
                            END AS [ColumnEncryptionKeysName],
                    */
                    	    [ColumnType].[name] AS [DataType], 
                    	    CAST(CASE 
                                    WHEN [ColumnType].[name] = 'text' THEN 
                                        NULL
                    	            ELSE 
                    	                CASE 
                                            WHEN [SysColumns].[precision]=0 AND [ColumnType].[name] <> 'text' THEN
                    	                        [SysColumns].[max_length] 
                    	                    ELSE 
                                                NULL
                    	                END
                    	         END AS SMALLINT) AS [MaxLength], 
                    	    CAST(CASE 
                                    WHEN [SysColumns].[precision]>0 and [ColumnType].[precision]=[ColumnType].[scale] THEN 
                    	                [SysColumns].[precision] 
                                    ELSE 
                                        null 
                                 END AS TINYINT) AS [Precision], 
                    	    CAST(CASE 
                                    WHEN [SysColumns].[precision]>0 and [ColumnType].[precision]=[ColumnType].[scale] THEN 
                    	                [SysColumns].scale 
                                    ELSE 
                                        NULL 
                                 END AS TINYINT) AS [Scale], 
                    	    CAST(CASE 
                                    WHEN [SysColumns].[is_identity]=1 THEN 
                    	                seed_value 
                                    ELSE 
                                        NULL 
                                 END AS SQL_VARIANT) AS [SeedValue], 
                    	    CAST(CASE 
                                    WHEN [SysColumns].[is_identity]=1 THEN 
                    	                increment_value 
                                    ELSE 
                                        NULL 
                                 END AS SQL_VARIANT) [IncrementValue], 
                    	    CAST(CASE 
                                    WHEN [SysColumns].default_object_id>0 THEN 
                    	                definition 
                                    ELSE
                                        NULL 
                                 END AS NVARCHAR(4000)) [DefaultValue] 
                    	FROM 
                            INFORMATION_SCHEMA.TABLES [InformationSchemaTables] 
                    	    JOIN [sys].[objects] AS [SysObjects] 
                                ON [InformationSchemaTables].[TABLE_SCHEMA] = SCHEMA_NAME([SysObjects].[schema_id]) 
                    	            AND [InformationSchemaTables].[TABLE_NAME] = [SysObjects].[name]
                    	    JOIN [sys].[columns] AS [SysColumns] 
                                ON [SysObjects].[object_id]=[SysColumns].[object_id] 
                    /*
                    	    LEFT JOIN [sys].[column_encryption_keys] AS [ColumnEncryptionKeys] 
                    	        ON [SysColumns].[column_encryption_key_id] = [ColumnEncryptionKeys].[column_encryption_key_id]
                    */
                    	    LEFT JOIN [sys].[identity_columns] AS IdentityColumns 
                                ON [SysObjects].[object_id]=[IdentityColumns].[object_id] 
                    	    JOIN [sys].[types] AS [ColumnType] 
                                ON [SysColumns].[system_type_id]=[ColumnType].[system_type_id] 
                    	            AND [ColumnType].[system_type_id]=[ColumnType].[user_type_id] 
                    	    LEFT JOIN [sys].[default_constraints] AS DefaultConstraints 
                                ON [SysColumns].[default_object_id]=[DefaultConstraints].[object_id] 
                    	WHERE [SysObjects].[type]='u'
                    
                    ), ForeignKeyInformation AS (
                    
                        SELECT DISTINCT
                            [Constraints].[ConstraintName] AS [ConstraintName],
                            [ConstraintColumnUsage].[TABLE_SCHEMA] AS [ConstraintTableSchema], 
                            [ConstraintColumnUsage].[Table_Name] AS [ConstraintTableName],         
                            [Constraints].[ConstraintSchema] AS [ReferencedSchema],
                            [Constraints].[ReferencedTable] AS [ReferencedTable],
                            [Columns].[COLUMNNAME] AS [ColumnName], 
                            [Columns].[OrdinalPosition] AS [OrdinalPosition], 
                            [Columns].[ReferencedColumn] AS [ReferencedColumn]
                        FROM 
                            INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS [ConstraintColumnUsage]
                        JOIN 
                        (
                            SELECT 
                                [TableConstraints].[TABLE_SCHEMA] AS [TableSchema], 
                                [TableConstraints].[TABLE_NAME] AS [TableName],           
                                [TableConstraints].[CONSTRAINT_NAME] As [ConstraintName], 
                                [ReferentialConstraints].[CONSTRAINT_SCHEMA] AS [ConstraintSchema],
                                [ConstraintColumns].[ObjectId] AS [ObjectId],
                                [ConstraintColumns].[ReferencedTable] AS [ReferencedTable],
                                [ConstraintColumns].[ParentObjectId] AS [ParentObjectId],
                                [ConstraintColumns].[ConstraintObjectId] AS [ConstraintObjectId],
                                [ConstraintColumns].[ReferencedObjectId] AS [ReferencedObjectId]
                            FROM 
                                INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS [TableConstraints]
                            JOIN 
                                INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS [ReferentialConstraints]
                                    ON [TableConstraints].[CONSTRAINT_SCHEMA]=[ReferentialConstraints].[CONSTRAINT_SCHEMA]
                                        AND [TableConstraints].[CONSTRAINT_NAME]=[ReferentialConstraints].[CONSTRAINT_NAME]
                            JOIN 
                            (
                                SELECT
                                    [ForeignKeys].[object_id] AS [ObjectId],
                                    SCHEMA_NAME([ForeignKeys].[schema_id]) AS [TableSchema],
                                    OBJECT_NAME([ForeignKeyColumns].[parent_object_id]) AS [TableName],                    
                                    OBJECT_NAME([ForeignKeyColumns].[constraint_object_id]) AS [ConstraintName], 
                                    OBJECT_NAME([ForeignKeyColumns].[referenced_object_id]) AS [ReferencedTable],
                                    [ForeignKeyColumns].[parent_object_id] AS [ParentObjectId],
                                    [ForeignKeyColumns].[constraint_object_id] AS [ConstraintObjectId],
                                    [ForeignKeyColumns].[referenced_object_id] AS [ReferencedObjectId]
                                FROM sys.foreign_key_columns AS [ForeignKeyColumns] 
                                JOIN sys.foreign_keys AS [ForeignKeys] 
                                     ON [ForeignKeys].[object_id]=[ForeignKeyColumns].[constraint_object_id]
                                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS [KeyColumnUsage] 
                                    ON [KeyColumnUsage].[TABLE_NAME]=OBJECT_NAME([ForeignKeys].[parent_object_id]) 
                                        AND [KeyColumnUsage].[CONSTRAINT_NAME]=OBJECT_NAME([ForeignKeyColumns].[constraint_object_id])                
                            ) AS [ConstraintColumns]
                                ON [TableConstraints].[TABLE_SCHEMA]=[ConstraintColumns].[TableSchema]
                                    AND [TableConstraints].[TABLE_NAME]=[ConstraintColumns].[TableName]
                        ) AS [Constraints] 
                            ON [ConstraintColumnUsage].[CONSTRAINT_NAME]=[Constraints].[ConstraintName] 
                                AND [ConstraintColumnUsage].[TABLE_NAME]=[Constraints].[TableName]
                        JOIN 
                        (
                            SELECT 
                                [ConstraintColumnUsage].[ObjectId] AS [ObjectId],
                                [ConstraintColumnUsage].[TABLESCHEMA] AS [TableSchema],
                                [ConstraintColumnUsage].[TABLENAME] AS [TableName], 
                                [ConstraintColumnUsage].[ColumnName] AS [ColumnName], 
                                [ConstraintColumnUsage].[CONSTRAINTNAME] AS [ConstraintName], 
                                [ConstraintColumnUsage].[ordinalposition] AS [OrdinalPosition],
                                [ConstraintColumnUsage].[ReferencedColumn] AS [ReferencedColumn],
                                [ConstraintColumnUsage].[parent_object_id] AS [ParentObjectId],
                                [ConstraintColumnUsage].[constraint_object_id] AS [ConstraintObjectId],
                                [ConstraintColumnUsage].[referenced_object_id] AS [ReferencedObjectId]
                            FROM 
                            (
                                SELECT
                                    [ForeignKeys].[object_id] AS [ObjectId],
                                    SCHEMA_NAME([ForeignKeys].[schema_id]) AS [TableSchema],
                                    OBJECT_NAME([ForeignKeyColumns].[parent_object_id]) AS [TableName], 
                                    OBJECT_NAME([ForeignKeyColumns].[constraint_object_id]) AS [ConstraintName], 
                                    [KeyColumnUsage].[COLUMN_NAME] AS [ColumnName], 
                                    [ForeignKeyColumns].constraint_column_id AS [ORDINALPOSITION],
                                    [ForeignKeyColumns].parent_column_id AS [ReferencedColumn],
                                    [ForeignKeyColumns].parent_object_id,
                                    [ForeignKeyColumns].constraint_object_id,
                                    [ForeignKeyColumns].referenced_object_id
                                FROM 
                                    sys.foreign_key_columns AS [ForeignKeyColumns] 
                                JOIN sys.foreign_keys AS [ForeignKeys] 
                                    ON [ForeignKeys].[object_id]=[ForeignKeyColumns].[constraint_object_id]
                                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS [KeyColumnUsage] 
                                    ON [KeyColumnUsage].[TABLE_NAME]=OBJECT_NAME([ForeignKeys].[parent_object_id]) 
                                        AND [KeyColumnUsage].[CONSTRAINT_NAME]=OBJECT_NAME([ForeignKeyColumns].[constraint_object_id])
                            ) AS [ConstraintColumnUsage]
                        ) AS [Columns] 
                            ON [ConstraintColumnUsage].[TABLE_SCHEMA]=[Columns].[TABLESCHEMA]
                                AND [ConstraintColumnUsage].TABLE_NAME=[Columns].[TABLENAME]
                                AND [ConstraintColumnUsage].COLUMN_NAME=[Columns].[COLUMNNAME]
                        WHERE
                            [constraints].[ObjectId] = [columns].[ConstraintObjectId]
                    )
                    
                    SELECT 
                        * 
                    FROM 
                        TableInformation
                    WHERE 
                        TableSchema = 'Analytics'
                ";
        }
    }
}
