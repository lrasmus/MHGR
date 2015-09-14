USE [mhgr_hybrid]
GO

-- Return patient information and phenotype.  If there are multiple results, show them all.


-- 1: Identify phenotypes that are resulted as phenotypes
SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name, p.name as [phenotype], p.value as [value], pp.resulted_on
	FROM [dbo].[patient_result_collections] prc
	INNER JOIN [dbo].[patient_result_members] prm ON prm.member_type = 1 AND prm.collection_id = prc.id
	INNER JOIN [dbo].[patients] pt ON pt.id = prc.patient_id
	INNER JOIN [dbo].[patient_phenotypes] pp ON pp.id = prm.member_id
	INNER JOIN [dbo].[phenotypes] p ON p.id = pp.phenotype_id
	ORDER BY external_id, resulted_on DESC


-- 2: Identify phenotypes that are resulted as star variants
-- Translate the CYP2C19 stars for clopidogrel metabolism.
-- See https://www.pharmgkb.org/guideline/PA166104948
SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
	'Clopidogrel metabolism' AS [phenotype],
	CASE
		-- Ultrarapid can be *1/*17, *17/*1, *17/*17
		WHEN pv.value1 ='1' AND pv.value2 = '17' THEN 'Ultrarapid metabolizer'
		WHEN pv.value1 = '17' AND pv.value2 = '1' THEN 'Ultrarapid metabolizer'
		WHEN pv.value1 = '17' AND pv.value2 = '17' THEN 'Ultrarapid metabolizer'
		-- Those that are both *1
		WHEN pv.value1 = '1' AND pv.value2 = '1' THEN 'Extensive metabolizer'
		-- For intermediate metabolizer one allele as *1 or one as *17 (other allele is something non *1 and non *17), it's intermediate
		WHEN pv.value1 IN ('1', '17') AND pv.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
		WHEN pv.value2 IN ('1', '17') AND pv.value1 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
		-- For poor metabolizer, they are both *2 to *8.  We need to account for all values, so we're not picking up some unknown value
		WHEN pv.value1 IN ('2', '3', '4', '5', '6', '7', '8') AND pv.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Poor metabolizer'
		ELSE 'Unknown'
	END AS [value],
	pv.resulted_on
	FROM [dbo].[patient_result_collections] prc
	INNER JOIN [dbo].[patient_result_members] prm ON prm.member_type = 2 AND prm.collection_id = prc.id
	INNER JOIN [dbo].[patients] pt ON pt.id = prc.patient_id
	INNER JOIN [dbo].[patient_variants] pv ON pv.variant_type = 2 AND pv.id = prm.member_id
	INNER JOIN [dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19

UNION ALL

-- Translate the CYP2C9 stars for warfarin metabolism
-- https://www.pharmgkb.org/guideline/PA166104949
SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
	'Warfarin metabolism' AS [phenotype],
	CASE
		-- Everything except *1/*1 is Decreased, but check to make sure we don't have unknown values
		WHEN pv.value1 = '1' AND pv.value2 = '1' THEN 'Normal'
		WHEN pv.value1 IN ('1', '2', '3') AND pv.value2 IN ('1', '2', '3') THEN 'Decreased'
		ELSE 'Unknown'
	END,
	pv.resulted_on
	FROM [dbo].[patient_result_collections] prc
	INNER JOIN [dbo].[patients] pt ON pt.id = prc.patient_id
	INNER JOIN [dbo].[variants] v ON v.gene_id = 2  -- CYP2C9
	INNER JOIN [dbo].[patient_result_members] prm ON prm.member_type = 2 AND prm.collection_id = prc.id
	INNER JOIN [dbo].[patient_variants] pv ON pv.variant_type = 2 AND pv.id = prm.member_id AND pv.reference_id = v.id
ORDER BY pt.external_id, pv.resulted_on DESC, [value]



-- Translate the CYP2C9 and VKORC1 stars for warfarin metabolism and generate dosing range recommendation
-- https://www.pharmgkb.org/guideline/PA166104949
SELECT pt.external_id, pt.external_source, pt.first_name, pt.last_name,
	'Warfarin dosing range' AS [phenotype],
	CASE -- VKORC1 cases
		WHEN pv2.value1 = '1' AND pv2.value2 = '1' THEN
			CASE
				WHEN pv1.value1 = '1' AND pv1.value2 = '1' THEN '5-7 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '2') OR (pv1.value1 = '2' AND pv1.value2 = '1') THEN '5-7 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '1') THEN '3-4 mg/day'
				WHEN pv1.value1 = '2' AND pv1.value2 = '2' THEN '3-4 mg/day'
				WHEN (pv1.value1 = '2' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '2') THEN '3-4 mg/day'
				WHEN pv1.value1 = '3' AND pv1.value2 = '3' THEN '0.5-2 mg/day'
				ELSE 'Unknown'
			END
		WHEN (pv2.value1 = '1' AND pv2.value2 = '2') OR (pv2.value1 = '2' AND pv2.value2 = '1') THEN
			CASE
				WHEN pv1.value1 = '1' AND pv1.value2 = '1' THEN '5-7 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '2') OR (pv1.value1 = '2' AND pv1.value2 = '1') THEN '3-4 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '1') THEN '3-4 mg/day'
				WHEN pv1.value1 = '2' AND pv1.value2 = '2' THEN '3-4 mg/day'
				WHEN (pv1.value1 = '2' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '2') THEN '0.5-2 mg/day'
				WHEN pv1.value1 = '3' AND pv1.value2 = '3' THEN '0.5-2 mg/day'
				ELSE 'Unknown'
			END
		WHEN pv2.value1 = '2' AND pv2.value2 = '2' THEN
			CASE
				WHEN pv1.value1 = '1' AND pv1.value2 = '1' THEN '3-4 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '2') OR (pv1.value1 = '2' AND pv1.value2 = '1') THEN '3-4 mg/day'
				WHEN (pv1.value1 = '1' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '1') THEN '0.5-2 mg/day'
				WHEN pv1.value1 = '2' AND pv1.value2 = '2' THEN '0.5-2 mg/day'
				WHEN (pv1.value1 = '2' AND pv1.value2 = '3') OR (pv1.value1 = '3' AND pv1.value2 = '2') THEN '0.5-2 mg/day'
				WHEN pv1.value1 = '3' AND pv1.value2 = '3' THEN '0.5-2 mg/day'
				ELSE 'Unknown'
			END
		ELSE 'Unknown'
	END AS [value],
	pv1.resulted_on
	FROM [dbo].[patient_result_collections] prc
	INNER JOIN [dbo].[patients] pt ON pt.id = prc.patient_id
	INNER JOIN [dbo].[variants] v1 ON v1.gene_id = 2  -- CYP2C9
	INNER JOIN [dbo].[patient_result_members] prm1 ON prm1.member_type = 2 AND prm1.collection_id = prc.id
	INNER JOIN [dbo].[patient_variants] pv1 ON pv1.variant_type = 2 AND pv1.id = prm1.member_id AND pv1.reference_id = v1.id
	INNER JOIN [dbo].[variants] v2 ON v2.gene_id = 3  -- VKORC1
	INNER JOIN [dbo].[patient_result_members] prm2 ON prm2.member_type = 2 AND prm2.collection_id = prm1.collection_id
	INNER JOIN [dbo].[patient_variants] pv2 ON pv2.variant_type = 2 AND pv2.id = prm2.member_id AND pv2.reference_id = v2.id
ORDER BY pt.external_id