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
	--SELECT patient_id, gene_entity_id, [1] AS [value1], [2] AS [value2]
	--FROM 
	--(
	--	SELECT gene.id AS [gene_entity_id], allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.id, allele.attribute_id ORDER BY gene.attribute_id, allele.id) AS RowNum
	--	FROM [mhgr_eav].[dbo].[result_entities] gene
	--		INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id AND allele.attribute_id = 129
	--	WHERE gene.attribute_id = 31 -- CYP2C19
	--) a
	--PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
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


--SELECT id, patient_id, [1] AS allele1, [2] AS allele2
--FROM
--(
--SELECT re.id, allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY re.id, allele.attribute_id ORDER BY re.attribute_id, allele.id) AS RowNum
--FROM [mhgr_eav].[dbo].[result_entities] re
--	INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = re.id AND allele.attribute_id = 128 -- SNP allele
--WHERE re.attribute_id IN (
--	SELECT attribute2_id FROM [dbo].[attribute_relationships] 
--	WHERE attribute1_id = 31 -- CYP2C19
--		AND relationship_id = 6)
--) a
--PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
--ORDER BY id, patient_id

--	SELECT pt.external_id AS [patient_id], pt.external_source,
--		CASE
--			WHEN v.value1 = v.value2 THEN
--				CASE
--					WHEN v.value1 != v.reference_bases OR v.value2 != v.reference_bases THEN 'Homozygous_Variant'
--					ELSE 'Homozygous_Normal'
--				END
--			ELSE
--				CASE
--					WHEN v.value1 != v.reference_bases OR v.value2 != v.reference_bases THEN 'Heterozygous_Variant'
--					ELSE 'Heterozygous_Normal'
--				END 
--		END AS [zygosity]
--FROM
--		(
--SELECT allele_id,
--	external_id,
--	external_source,
--	[1] AS value1,
--	[2] AS value2
--FROM
--(
--	SELECT p.external_id, p.external_source, snps.attribute_id AS [allele_id], alleles.value_short_text,
--		ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY snps.patient_id, snps.attribute_id) AS RowNum
--	FROM dbo.result_entities snps 
--	INNER JOIN dbo.attribute_relationships ar ON ar.attribute1_id = snps.attribute_id
--		AND ar.relationship_id = 5     -- Variant of gene
--		AND ar.attribute2_id = 31      -- CYP2C19
--	INNER JOIN dbo.result_entities alleles ON alleles.parent_id = snps.id
--		AND alleles.attribute_id = 128 -- SNP allele
--	INNER JOIN dbo.patients p ON p.id = snps.patient_id
--	WHERE snps.parent_id IS NULL   -- Standalone SNP result
--) a
--PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
--ORDER BY patient_id, allele_id
--) v
--INNER JOIN [dbo].[patients] pt ON pt.external_id = v.external_id AND pt.external_source = v.external_source

-- Convert CYP2C19 SNPs to phenotype
SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
	'Clopidogrel metabolism' AS [phenotype],
	CASE
		WHEN ([rs12248560] = 'Homozygous_Variant' OR [rs12248560] = 'Heterozygous_Variant')
			AND CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0
			AND CHARINDEX('Variant', [rs4986893]) = 0 AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0
			AND CHARINDEX('Variant', [rs72558186]) = 0
		THEN 'Ultrarapid metabolizer'
		WHEN ([rs12248560] = 'Heterozygous_Variant' OR [rs12248560] = 'Homozygous_Normal')
			AND (
				([rs28399504] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0 AND CHARINDEX('Variant', [rs4986893]) = 0
					AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs41291556] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0 AND CHARINDEX('Variant', [rs4986893]) = 0
					AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs4244285] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4986893]) = 0
					AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs4986893] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0
					AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs56337013] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0
					AND CHARINDEX('Variant', [rs4986893]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs72558184] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0
					AND CHARINDEX('Variant', [rs4986893]) = 0 AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558186]) = 0)
				OR ([rs72558186] = 'Heterozygous_Variant' AND 
					CHARINDEX('Variant', [rs28399504]) = 0 AND CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0
					AND CHARINDEX('Variant', [rs4986893]) = 0 AND CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0)
			)
		THEN 'Intermediate metabolizer'
		WHEN
			(CHARINDEX('Variant', [rs12248560]) = 0 AND CHARINDEX('Variant', [rs17884712]) = 0 AND CHARINDEX('Variant', [rs28399504]) = 0 AND 
			CHARINDEX('Variant', [rs41291556]) = 0 AND CHARINDEX('Variant', [rs4244285]) = 0 AND CHARINDEX('Variant', [rs4986893]) = 0 AND 
			CHARINDEX('Variant', [rs56337013]) = 0 AND CHARINDEX('Variant', [rs6413438]) = 0 AND CHARINDEX('Variant', [rs72558184]) = 0 AND 
			CHARINDEX('Variant', [rs72558186]) = 0)
		THEN 'Extensive metabolizer'
		ELSE 'Poor metabolizer'
	END AS [value],
	[resulted_on]
FROM 
(
	SELECT patient_id,
		[40] AS [rs12248560],
		[41] AS [rs28399504],
		[42] AS [rs41291556],
		[43] AS [rs72558184],
		[44] AS [rs4986893],
		[45] AS [rs4244285],
		[46] AS [rs72558186],
		[47] AS [rs56337013],
		[48] AS [rs17884712],
		[49] AS [rs6413438],
		NULL AS [resulted_on]
	FROM
	(
		SELECT patient_id,
			snp_id,
			CASE
				WHEN v.value1 = v.value2 THEN
					CASE
						WHEN v.value1 != v.reference_bases OR v.value2 != v.reference_bases THEN 'Homozygous_Variant'
						ELSE 'Homozygous_Normal'
					END
				ELSE
					CASE
						WHEN v.value1 != v.reference_bases OR v.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						ELSE 'Heterozygous_Normal'
					END 
			END AS [zygosity]
		FROM
			(
				SELECT id,
				patient_id,
				snp_id,
				[1] AS value1,
				COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				[3] AS reference_bases
			FROM
			(
				SELECT features.patient_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					3 AS RowNum
				FROM dbo.result_entities features
				INNER JOIN dbo.result_entities refs ON refs.parent_id = features.id 
					AND refs.attribute_id = 100 -- Reference bases
				INNER JOIN dbo.result_entities snps ON snps.parent_id = features.id
				INNER JOIN dbo.attribute_relationships ar ON snps.attribute_id = ar.attribute1_id
					AND ar.relationship_id = 5     -- Variant of gene
					AND ar.attribute2_id = 31      -- CYP2C19
				WHERE features.attribute_id = 96   -- GVF Feature

				UNION ALL

				-- Grab GVF features, features have children specific SNPs, which have children SNP allele attributes
				-- Limit this to just SNPs that are under CYP2C19
				SELECT features.patient_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				FROM dbo.result_entities features
				INNER JOIN dbo.result_entities snps ON snps.parent_id = features.id 
				INNER JOIN dbo.attribute_relationships ar ON ar.attribute1_id = snps.attribute_id
					AND ar.relationship_id = 5     -- Variant of gene
					AND ar.attribute2_id = 31      -- CYP2C19
				INNER JOIN dbo.result_entities alleles ON alleles.parent_id = snps.id
					AND alleles.attribute_id = 128 -- SNP allele
				WHERE features.attribute_id = 96   -- GVF Feature
			) a
			PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3]) ) AS pvt
		) v
	) snps
	PIVOT ( MAX(zygosity) FOR snp_id IN ([40], [41], [42], [43], [44], [45], [46], [47], [48], [49]) ) AS pvt
) v
INNER JOIN [dbo].[patients] pt ON pt.id = v.patient_id