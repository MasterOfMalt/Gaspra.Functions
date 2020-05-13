;WITH ForeignKeyInformation AS (

    SELECT DISTINCT
        [Constraints].[ConstraintName] AS [ConstraintName],
        [ConstraintColumnUsage].[TABLE_SCHEMA] AS [ConstraintTableSchema],
        [ConstraintColumnUsage].[Table_Name] AS [ConstraintTableName],
        [Columns].[COLUMNNAME] AS [ConstraintTableColumn],
        [Constraints].[ConstraintSchema] AS [ReferencedTableSchema],
        [Constraints].[ReferencedTable] AS [ReferencedTableName]--,
        --[Columns].[ReferencedColumn] AS [ReferencedTableColumn]
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

SELECT * FROM
	ForeignKeyInformation
WHERE
    ConstraintTableSchema = 'Analytics'