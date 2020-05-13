SELECT 
	schema_name(o.schema_id) AS SchemaName,
	o.name AS ObjectName, 
	e.name AS PropertyName, 
	value
FROM
	[sys].[extended_properties] AS e 
INNER JOIN
	[sys].[objects] AS o 
		ON e.major_id=o.object_id
WHERE
	schema_name(o.schema_id) = 'Analytics'

