USE [mhgr_eav]
GO

-- Return patient information and phenotype.  If there are multiple results, show them all.


-- 1: Identify phenotypes that are resulted as phenotypes
WITH phenotypes ([id], [name], [parent_id], [parent_name])
AS
(
SELECT a.id, a.name, NULL AS [parent_id], CONVERT(nvarchar, N'Phenotype') AS [parent_name]
	FROM [dbo].[attributes] a
	INNER JOIN [dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id AND ar.attribute2_id = 7 AND ar.relationship_id = 2

UNION ALL

SELECT a.id, a.name, p.id AS [parent_id], CONVERT(nvarchar, p.name) AS [parent_name]
	FROM [dbo].[attributes] a
	INNER JOIN [dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id AND ar.relationship_id = 2
	INNER JOIN phenotypes p ON ar.attribute2_id = p.id
)

SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name, p.parent_name as [phenotype], p.name as [value], ro.value_date_time AS [resulted_on]
FROM phenotypes p
	INNER JOIN [dbo].[result_entities] re ON re.attribute_id = p.id
	INNER JOIN [dbo].[patients] pt on pt.id = re.patient_id
	INNER JOIN [dbo].[result_entities] ro ON ro.attribute_id = 72 AND ro.parent_id = re.id
	ORDER BY external_id, resulted_on DESC