USE [mhgr_eav]
GO

-- Return patient information and phenotype.  If there are multiple results, show them all.


---- 1: Identify phenotypes that are resulted as phenotypes
--WITH phenotypes ([id], [name], [parent_id], [parent_name])
--AS
--(
--SELECT a.id, a.name, NULL AS [parent_id], CONVERT(nvarchar, N'Phenotype') AS [parent_name]
--	FROM [dbo].[attributes] a
--	INNER JOIN [dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id AND ar.attribute2_id = 7 AND ar.relationship_id = 2

--UNION ALL

--SELECT a.id, a.name, p.id AS [parent_id], CONVERT(nvarchar, p.name) AS [parent_name]
--	FROM [dbo].[attributes] a
--	INNER JOIN [dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id AND ar.relationship_id = 2
--	INNER JOIN phenotypes p ON ar.attribute2_id = p.id
--)

--SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name, p.parent_name as [phenotype], p.name as [value], ro.value_date_time AS [resulted_on]
--FROM phenotypes p
--	INNER JOIN [dbo].[result_entities] re ON re.attribute_id = p.id
--	INNER JOIN [dbo].[patients] pt on pt.id = re.patient_id
--	INNER JOIN [dbo].[result_entities] ro ON ro.attribute_id = 72 AND ro.parent_id = re.id
--	ORDER BY external_id, resulted_on DESC



---- 2: Identify phenotypes that are resulted as star variants
---- Translate the CYP2C19 stars for clopidogrel metabolism.
---- See https://www.pharmgkb.org/guideline/PA166104948
--SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name, 'Clopidogrel metabolism' AS [phenotype],
--	CASE
--		-- Ultrarapid can be *1/*17, *17/*1, *17/*17
--		WHEN pvt.value1 ='1' AND pvt.value2 = '17' THEN 'Ultrarapid metabolizer'
--		WHEN pvt.value1 = '17' AND pvt.value2 = '1' THEN 'Ultrarapid metabolizer'
--		WHEN pvt.value1 = '17' AND pvt.value2 = '17' THEN 'Ultrarapid metabolizer'
--		-- Those that are both *1
--		WHEN pvt.value1 = '1' AND pvt.value2 = '1' THEN 'Extensive metabolizer'
--		-- For intermediate metabolizer one allele as *1 or one as *17 (other allele is something non *1 and non *17), it's intermediate
--		WHEN pvt.value1 IN ('1', '17') AND pvt.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
--		WHEN pvt.value2 IN ('1', '17') AND pvt.value1 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
--		-- For poor metabolizer, they are both *2 to *8.  We need to account for all values, so we're not picking up some unknown value
--		WHEN pvt.value1 IN ('2', '3', '4', '5', '6', '7', '8') AND pvt.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Poor metabolizer'
--		ELSE 'Unknown'
--	END AS [value],
--	re.value_date_time AS [resulted_on]
--FROM 
--(
	SELECT patient_id, gene_entity_id, [1] AS [value1], [2] AS [value2]
	FROM 
	(
		SELECT gene.id AS [gene_entity_id], allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.id, allele.attribute_id ORDER BY gene.attribute_id, allele.id) AS RowNum
		FROM [mhgr_eav].[dbo].[result_entities] gene
			INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id AND allele.attribute_id = 129
		WHERE gene.attribute_id = 31 -- CYP2C19
	) a
	PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
--) pvt
--INNER JOIN [dbo].[patients] pt ON pt.id = pvt.patient_id
--INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.parent_id = gene_entity_id AND re.attribute_id = 72 -- Resulted on

--UNION ALL

---- Translate the CYP2C9 stars for warfarin metabolism
---- https://www.pharmgkb.org/guideline/PA166104949
--SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
--	'Warfarin metabolism' AS [phenotype],
--	CASE
--		-- Everything except *1/*1 is Decreased, but check to make sure we don't have unknown values
--		WHEN pvt.value1 = '1' AND pvt.value2 = '1' THEN 'Normal'
--		WHEN pvt.value1 IN ('1', '2', '3') AND pvt.value2 IN ('1', '2', '3') THEN 'Decreased'
--		ELSE 'Unknown'
--	END AS [value],
--	re.value_date_time AS [resulted_on]
--FROM 
--(
--	SELECT patient_id, gene_entity_id, [1] AS [value1], [2] AS [value2]
--	FROM 
--	(
--		SELECT gene.id AS [gene_entity_id], allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.id, allele.attribute_id ORDER BY gene.attribute_id, allele.id) AS RowNum
--		FROM [mhgr_eav].[dbo].[result_entities] gene
--			INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id AND allele.attribute_id = 129
--		WHERE gene.attribute_id = 32 -- CYP2C9
--	) a
--	PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
--) pvt
--INNER JOIN [dbo].[patients] pt ON pt.id = pvt.patient_id
--INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.parent_id = gene_entity_id AND re.attribute_id = 72 -- Resulted on
--ORDER BY pt.external_id, re.value_date_time DESC, [value]


---- Translate the CYP2C9 and VKORC1 stars for warfarin metabolism and generate dosing range recommendation
---- https://www.pharmgkb.org/guideline/PA166104949
--SELECT external_id, external_source, first_name, last_name, [phenotype], [value], MAX([resulted_on]) AS [resulted_on]
--FROM
--(
--	SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
--		'Warfarin dosing range' AS [phenotype],
--		CASE -- VKORC1 cases
--			WHEN [vkorc1_value1] = '1' AND [vkorc1_value2] = '1' THEN
--				CASE
--					WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '5-7 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '5-7 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
--					WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '3-4 mg/day'
--					WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '3-4 mg/day'
--					WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
--					ELSE 'Unknown'
--				END
--			WHEN ([vkorc1_value1] = '1' AND [vkorc1_value2] = '2') OR ([vkorc1_value1] = '2' AND [vkorc1_value2] = '1') THEN
--				CASE
--					WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '5-7 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
--					WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '3-4 mg/day'
--					WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '0.5-2 mg/day'
--					WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
--					ELSE 'Unknown'
--				END
--			WHEN [vkorc1_value1] = '2' AND [vkorc1_value2] = '2' THEN
--				CASE
--					WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '3-4 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
--					WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '0.5-2 mg/day'
--					WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '0.5-2 mg/day'
--					WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '0.5-2 mg/day'
--					WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
--					ELSE 'Unknown'
--				END
--			ELSE 'Unknown'
--		END AS [value],
--		re.value_date_time AS [resulted_on]
--	FROM 
--	(
--		SELECT patient_id, [1] AS [cyp2c9_value1], [2] AS [cyp2c9_value2], [3] AS [vkorc1_value1], [4] AS [vkorc1_value2]
--		FROM 
--		(
--			SELECT allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.patient_id ORDER BY gene.id, gene.attribute_id, allele.id) AS RowNum
--			FROM [mhgr_eav].[dbo].[result_entities] gene
--				INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id AND allele.attribute_id = 129
--			WHERE gene.attribute_id IN (32, 33) -- CYP2C9, VKORC1
--		) a
--		PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
--	) pvt
--	INNER JOIN [dbo].[patients] pt ON pt.id = pvt.patient_id
--	INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.patient_id = pvt.patient_id AND re.attribute_id = 72 -- Resulted on
--) AS a
--GROUP BY external_id, external_source, first_name, last_name, [phenotype], [value]
--ORDER BY external_id, [value]


SELECT * FROM [dbo].[