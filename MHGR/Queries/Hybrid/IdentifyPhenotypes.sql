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