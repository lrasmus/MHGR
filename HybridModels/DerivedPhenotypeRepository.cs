using MHGR.Models;
using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.HybridModels
{
    public class DerivedPhenotypeRepository : IDerivedPhenotypeRepository
    {
        private HybridEntities entities = new HybridEntities();

        public List<string> GetResultFileDetailsForPhenotype(string source, int fileId, string phenotype)
        {
            List<string> details = new List<string>();

            var file = entities.result_files.Where(x => x.id == fileId).FirstOrDefault();
            if (file == null)
            {
                details.Add("The results file that created this result could not be found");
                return details;
            }

            switch (source)
            {
                case "Phenotype":
                case "Star":
                case "SNP":
                case "VCF":
                case "GVF":
                    details.AddRange(GetPhenotypeDetails(file, phenotype));
                    break;
                default:
                    details.Add(string.Format("We're unable to handle '{0}' results", source));
                    break;
            }

            details.Add("<div class='result-footer'>");
            details.Add(string.Format("<div><small>Received in the file: {0}</small></div>", file.name));
            details.Add(string.Format("<div><small>Received on: {0}</small></div>", file.received_on.ToShortDateString()));
            details.Add("</div>");

            return details;
        }

        private int[] GetGeneFilterForPhenotype(string phenotype)
        {
            switch (phenotype)
            {
                case "Clopidogrel metabolism":
                    return entities.genes.Where(x => x.name == "CYP2C19").Select(x => x.id).ToArray();
                case "Familial Thrombophilia":
                    return entities.genes.Where(x => x.name == "CYP2C9" || x.name == "VKORC1").Select(x => x.id).ToArray();
                case "Hypertrophic Cardiomyopathy":
                    return entities.genes.Where(x => x.name == "F2" || x.name == "F5").Select(x => x.id).ToArray();
                case "Warfarin metabolism":
                    return entities.genes.Where(x => x.name == "MYH7" || x.name == "TNNT2" || x.name == "TPM1" || x.name == "MYBPC3").Select(x => x.id).ToArray();
                default:
                    return null;
            }
        }

        private List<string> GetPhenotypeDetails(result_files file, string phenotype)
        {
            List<string> details = new List<string>();
            var geneIds = GetGeneFilterForPhenotype(phenotype);
            if (geneIds == null)
            {
                return details;
            }

            foreach (var collection in file.patient_result_collections)
            {
                foreach (var member in collection.patient_result_members)
                {
                    switch (member.member_type)
                    {
                        case Enums.ResultMemberType.Phenotype:
                            var patientPhenotype = entities.patient_phenotypes.Where(x => x.id == member.member_id).FirstOrDefault();
                            details.Add(string.Format("<div>The phenotype {0} {1} was resulted on {2} <small>{3} {4}</small></div>",
                                patientPhenotype.phenotype.name, patientPhenotype.phenotype.value, patientPhenotype.resulted_on.Value.ToShortDateString(), patientPhenotype.phenotype.external_source, patientPhenotype.phenotype.external_id));
                            break;
                        case Enums.ResultMemberType.Variant:
                            StringBuilder builder = new StringBuilder();
                            var patientVariant = entities.patient_variants.Where(x => x.id == member.member_id).FirstOrDefault();
                            switch (patientVariant.variant_type)
                            {
                                case Enums.PatientVariantType.SNP:
                                case Enums.PatientVariantType.StarVariant:
                                    var variant = entities.variants.Where(x =>
                                        x.id == patientVariant.reference_id && x.gene_id.HasValue && geneIds.Contains(x.gene_id.Value)).FirstOrDefault();
                                    if (variant != null)
                                    {
                                        builder.AppendFormat("<div class='variant'>{0}: ", variant.external_id);
                                        builder.AppendFormat("<span class='{0}'>{1}</span>",
                                            variant.reference_bases == patientVariant.value1 ? "alternate" : "normal",
                                            patientVariant.value1);
                                        builder.AppendFormat("<span class='{0}'>{1}</span>",
                                            variant.reference_bases == patientVariant.value2 ? "alternate" : "normal",
                                            patientVariant.value2);
                                        builder.AppendFormat("<small>On chr{0}, Reference is {1} from {2}</small></div>", variant.chromosome, variant.reference_bases, variant.reference_genome);
                                    }
                                    break;
                                case Enums.PatientVariantType.Collection:
                                    break;
                                default:
                                    details.Add(string.Format("The variant type {0} for variant {1} could not be displayed", patientVariant.variant_type, patientVariant.reference_id));
                                    break;
                            }
                            details.Add(builder.ToString());
                            //details.Add(string.Format("<div>{0} = {1}{2} ({3})</div>", formattedVariant, patientVariant.value1, patientVariant.value2, patientVariant.resulted_on.Value.ToShortDateString()));
                            break;
                    }
                }
            }

            return details;
        }

        public List<DerivedPhenotype> GetPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT prc.result_file_id AS [ResultFileId], p.name as [Phenotype], p.value as [Value], CONVERT(VARCHAR, pp.resulted_on, 101) AS [ResultedOn], 'Phenotype' AS [Source]
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 1 AND prm.collection_id = prc.id
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_phenotypes] pp ON pp.id = prm.member_id
	            INNER JOIN [mhgr_hybrid].[dbo].[phenotypes] p ON p.id = pp.phenotype_id
                WHERE prc.patient_id=@p0", id);
            return data.ToList();
        }

        public List<DerivedPhenotype> GetDosing(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT prc.result_file_id AS [ResultFileId],
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
	            CONVERT(VARCHAR, pv1.resulted_on, 101) AS [ResultedOn], 'Star' AS [Source]
	        FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
	            INNER JOIN [mhgr_hybrid].[dbo].[variants] v1 ON v1.gene_id = 2  -- CYP2C9
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm1 ON prm1.member_type = 2 AND prm1.collection_id = prc.id
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv1 ON pv1.variant_type = 2 AND pv1.id = prm1.member_id AND pv1.reference_id = v1.id
	            INNER JOIN [mhgr_hybrid].[dbo].[variants] v2 ON v2.gene_id = 3  -- VKORC1
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm2 ON prm2.member_type = 2 AND prm2.collection_id = prm1.collection_id
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv2 ON pv2.variant_type = 2 AND pv2.id = prm2.member_id AND pv2.reference_id = v2.id
            WHERE prc.patient_id=@p0", id);
            return data.ToList();
        }

        public List<DerivedPhenotype> GetSNPPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT result_file_id AS [ResultFileId],
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
	            [resulted_on] AS [ResultedOn], 'SNP' AS [Source]
            FROM 
            (
	            SELECT patient_id,
                result_file_id,
	            [1] AS [rs12248560],	-- *17
	            [2] AS [rs17884712],	-- *9
	            [3] AS [rs28399504],	-- *4
	            [4] AS [rs41291556],	-- *8
	            [5] AS [rs4244285],		-- *2
	            [6] AS [rs4986893],		-- *3
	            [7] AS [rs56337013],	-- *5
	            [8] AS [rs6413438],		-- *10
	            [9] AS [rs72558184],	-- *6
	            [10] AS [rs72558186],	-- *7
	            [11] AS [resulted_on]
	            FROM
	            (
	            SELECT prc.patient_id,
                    prc.result_file_id,
		            CASE
			            WHEN pv.value1 = pv.value2 THEN
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
					            ELSE 'Homozygous_Normal'
				            END
			            ELSE
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
					            ELSE 'Heterozygous_Normal'
				            END 
		            END AS [zygosity],
		            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19
                WHERE prc.patient_id=@p0

	            UNION ALL

	            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 11 AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19
                WHERE prc.patient_id=@p0
                GROUP BY prc.patient_id, prc.result_file_id
	            ) a
	            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Warfarin metabolism' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs1057910]) = 0 AND CHARINDEX('Variant', [rs1799853]) = 0 THEN 'Normal'
		            ELSE 'Decreased'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'SNP' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS rs1057910,
		            [2] AS rs1799853,
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
	                WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Familial Thrombophilia' AS [phenotype],
	            CASE
		            WHEN [rs6025] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Homozygous Factor V Leiden mutation'
		            WHEN [rs6025] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Heterozygous Factor V Leiden mutation'
		            WHEN [rs1799963] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Homozygous prothrombin G20210A mutation'
		            WHEN [rs1799963] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Heterozygous prothrombin G20210A mutation'
		            WHEN CHARINDEX('Variant', [rs6025]) > 0 AND CHARINDEX('Variant', [rs1799963]) > 0 THEN 'Double heterozygous for prothrombin G20210A mutation and Factor V Leiden mutation'
		            WHEN [rs6025] = 'Homozygous_Normal' AND [rs1799963] = 'Homozygous_Normal' THEN 'No genetic risk for thrombophilia, due to factor V Leiden or prothrombin'
		            ELSE 'Unknown'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'SNP' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
                    [1] AS [rs6025],	-- F5
		            [2] AS [rs1799963],	-- F2
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
			            prc.result_file_id,
                        CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
                    WHERE prc.patient_id=@p0
		            
                    UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Hypertrophic Cardiomyopathy' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs121913626]) > 0 OR CHARINDEX('Variant', [rs3218713]) > 0 OR CHARINDEX('Variant', [rs3218714]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 1' 
		            WHEN CHARINDEX('Variant', [rs121964855]) > 0 OR CHARINDEX('Variant', [rs121964856]) > 0 OR CHARINDEX('Variant', [rs121964857]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 2' 
		            WHEN CHARINDEX('Variant', [rs28934269]) > 0 OR CHARINDEX('Variant', [rs104894504]) > 0 OR CHARINDEX('Variant', [rs28934270]) > 0 OR CHARINDEX('Variant', [rs727504290]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 3' 
		            WHEN CHARINDEX('Variant', [rs375882485]) > 0 OR CHARINDEX('Variant', [rs397515937]) > 0 OR CHARINDEX('Variant', [rs397515963]) > 0 OR CHARINDEX('Variant', [rs397516074]) > 0 OR CHARINDEX('Variant', [rs397516083]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 4' 
		            ELSE 'No genetic risk found'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'SNP' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
                    [1] AS [rs121913626],	-- MYH7
		            [2] AS [rs3218713],
		            [3] AS [rs3218714],
		            [4] AS [rs121964855],	-- TNNT2
		            [5] AS [rs121964856],
		            [6] AS [rs121964857],
		            [7] AS [rs28934269],	-- TPM1
		            [8] AS [rs104894504],
		            [9] AS [rs28934270],
		            [10] AS [rs727504290],
		            [11] AS [rs375882485],	-- MYBPC3
		            [12] AS [rs397515937],
		            [13] AS [rs397515963],
		            [14] AS [rs397516074],
		            [15] AS [rs397516083],
		            [16] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
                        CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 16 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id NOT IN (4, 5)
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12], [13], [14], [15], [16]) ) AS pvt
            ) v
            ORDER BY phenotype", id);
            return data.ToList();
        }

        public List<DerivedPhenotype> GetStarPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT prc.result_file_id,
	            'Clopidogrel metabolism' AS [Phenotype],
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
	            END AS [Value],
	            CONVERT(VARCHAR, pv.resulted_on, 101) AS [ResultedOn], 'Star' AS [Source]
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 AND prm.collection_id = prc.id
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 2 AND pv.id = prm.member_id
	            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19
                WHERE prc.patient_id=@p0

            UNION ALL

            SELECT prc.result_file_id,
	            'Warfarin metabolism' AS [Phenotype],
	            CASE
		            -- Everything except *1/*1 is Decreased, but check to make sure we don't have unknown values
		            WHEN pv.value1 = '1' AND pv.value2 = '1' THEN 'Normal'
		            WHEN pv.value1 IN ('1', '2', '3') AND pv.value2 IN ('1', '2', '3') THEN 'Decreased'
		            ELSE 'Unknown'
	            END AS [Value],
	            CONVERT(VARCHAR, pv.resulted_on, 101) AS [ResultedOn], 'Star' AS [Source]
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
	            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.gene_id = 2  -- CYP2C9
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 AND prm.collection_id = prc.id
	            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 2 AND pv.id = prm.member_id AND pv.reference_id = v.id
                WHERE prc.patient_id=@p0
            ORDER BY CONVERT(VARCHAR, pv.resulted_on, 101) DESC, [value]", id);
            return data.ToList();
        }

        public List<DerivedPhenotype> GetVCFPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT result_file_id,
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
	            [resulted_on] AS [ResultedOn], 'VCF' AS [Source]
            FROM 
            (
	            SELECT patient_id,
                result_file_id,
	            [1] AS [rs12248560],	-- *17
	            [2] AS [rs17884712],	-- *9
	            [3] AS [rs28399504],	-- *4
	            [4] AS [rs41291556],	-- *8
	            [5] AS [rs4244285],		-- *2
	            [6] AS [rs4986893],		-- *3
	            [7] AS [rs56337013],	-- *5
	            [8] AS [rs6413438],		-- *10
	            [9] AS [rs72558184],	-- *6
	            [10] AS [rs72558186],	-- *7
	            [11] AS [resulted_on]
	            FROM
	            (
	            SELECT prc.patient_id,
                    result_file_id,
		            CASE
			            WHEN pv.value1 = pv.value2 THEN
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
					            ELSE 'Homozygous_Normal'
				            END
			            ELSE
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
					            ELSE 'Heterozygous_Normal'
				            END 
		            END AS [zygosity],
		            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19
                WHERE prc.patient_id=@p0

	            UNION ALL

	            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 11 AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1 -- CYP2C19
                WHERE prc.patient_id=@p0
                GROUP BY prc.patient_id, prc.result_file_id
	            ) a
	            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Warfarin metabolism' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs1057910]) = 0 AND CHARINDEX('Variant', [rs1799853]) = 0 THEN 'Normal'
		            ELSE 'Decreased'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'VCF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs1057910],
		            [2] AS [rs1799853],
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Familial Thrombophilia' AS [phenotype],
	            CASE
		            WHEN [rs6025] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Homozygous Factor V Leiden mutation'
		            WHEN [rs6025] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Heterozygous Factor V Leiden mutation'
		            WHEN [rs1799963] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Homozygous prothrombin G20210A mutation'
		            WHEN [rs1799963] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Heterozygous prothrombin G20210A mutation'
		            WHEN CHARINDEX('Variant', [rs6025]) > 0 AND CHARINDEX('Variant', [rs1799963]) > 0 THEN 'Double heterozygous for prothrombin G20210A mutation and Factor V Leiden mutation'
		            WHEN [rs6025] = 'Homozygous_Normal' AND [rs1799963] = 'Homozygous_Normal' THEN 'No genetic risk for thrombophilia, due to factor V Leiden or prothrombin'
		            ELSE 'Unknown'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'VCF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs6025],	-- F5
		            [2] AS [rs1799963],	-- F2
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Hypertrophic Cardiomyopathy' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs121913626]) > 0 OR CHARINDEX('Variant', [rs3218713]) > 0 OR CHARINDEX('Variant', [rs3218714]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 1' 
		            WHEN CHARINDEX('Variant', [rs121964855]) > 0 OR CHARINDEX('Variant', [rs121964856]) > 0 OR CHARINDEX('Variant', [rs121964857]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 2' 
		            WHEN CHARINDEX('Variant', [rs28934269]) > 0 OR CHARINDEX('Variant', [rs104894504]) > 0 OR CHARINDEX('Variant', [rs28934270]) > 0 OR CHARINDEX('Variant', [rs727504290]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 3' 
		            WHEN CHARINDEX('Variant', [rs375882485]) > 0 OR CHARINDEX('Variant', [rs397515937]) > 0 OR CHARINDEX('Variant', [rs397515963]) > 0 OR CHARINDEX('Variant', [rs397516074]) > 0 OR CHARINDEX('Variant', [rs397516083]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 4' 
		            ELSE 'No genetic risk found'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'VCF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs121913626],	-- MYH7
		            [2] AS [rs3218713],
		            [3] AS [rs3218714],
		            [4] AS [rs121964855],	-- TNNT2
		            [5] AS [rs121964856],
		            [6] AS [rs121964857],
		            [7] AS [rs28934269],	-- TPM1
		            [8] AS [rs104894504],
		            [9] AS [rs28934270],
		            [10] AS [rs727504290],
		            [11] AS [rs375882485],	-- MYBPC3
		            [12] AS [rs397515937],
		            [13] AS [rs397515963],
		            [14] AS [rs397516074],
		            [15] AS [rs397516083],
		            [16] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 16 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 5
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12], [13], [14], [15], [16]) ) AS pvt
            ) v
            ORDER BY result_file_id, phenotype", id);
            return data.ToList();
        }

        public List<DerivedPhenotype> GetGVFPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT result_file_id,
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
	            [resulted_on] AS [ResultedOn], 'GVF' AS [Source]
            FROM 
            (
	            SELECT patient_id,
                result_file_id,
	            [1] AS [rs12248560],	-- *17
	            [2] AS [rs17884712],	-- *9
	            [3] AS [rs28399504],	-- *4
	            [4] AS [rs41291556],	-- *8
	            [5] AS [rs4244285],		-- *2
	            [6] AS [rs4986893],		-- *3
	            [7] AS [rs56337013],	-- *5
	            [8] AS [rs6413438],		-- *10
	            [9] AS [rs72558184],	-- *6
	            [10] AS [rs72558186],	-- *7
	            [11] As [resulted_on]
	            FROM
	            (
	            SELECT prc.patient_id, prc.result_file_id,
		            CASE
			            WHEN pv.value1 = pv.value2 THEN
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
					            ELSE 'Homozygous_Normal'
				            END
			            ELSE
				            CASE
					            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
					            ELSE 'Heterozygous_Normal'
				            END 
		            END AS [zygosity],
		            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1  -- CYP2C19
                WHERE prc.patient_id=@p0

	            UNION ALL

	            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 11 AS RowNum
	            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
		            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
			            AND prm.collection_id = prc.id
		            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
			            AND pv.id = prm.member_id
		            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 1 -- CYP2C19
                WHERE prc.patient_id=@p0
                GROUP BY prc.patient_id, prc.result_file_id
	            ) a
	            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Warfarin metabolism' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs1057910]) = 0 AND CHARINDEX('Variant', [rs1799853]) = 0 THEN 'Normal'
		            ELSE 'Decreased'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'GVF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs1057910],
		            [2] AS [rs1799853],
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id, prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id = 2  -- CYP2C9
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Familial Thrombophilia' AS [phenotype],
	            CASE
		            WHEN [rs6025] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Homozygous Factor V Leiden mutation'
		            WHEN [rs6025] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs1799963]) = 0 THEN 'Heterozygous Factor V Leiden mutation'
		            WHEN [rs1799963] = 'Homozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Homozygous prothrombin G20210A mutation'
		            WHEN [rs1799963] = 'Heterozygous_Variant' AND CHARINDEX('Variant', [rs6025]) = 0 THEN 'Heterozygous prothrombin G20210A mutation'
		            WHEN CHARINDEX('Variant', [rs6025]) > 0 AND CHARINDEX('Variant', [rs1799963]) > 0 THEN 'Double heterozygous for prothrombin G20210A mutation and Factor V Leiden mutation'
		            WHEN [rs6025] = 'Homozygous_Normal' AND [rs1799963] = 'Homozygous_Normal' THEN 'No genetic risk for thrombophilia, due to factor V Leiden or prothrombin'
		            ELSE 'Unknown'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'GVF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs6025],	-- F5
		            [2] AS [rs1799963],	-- F2
		            [3] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id, prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 3 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (4, 5)  -- F5 and F2
	                WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id,
	            'Hypertrophic Cardiomyopathy' AS [phenotype],
	            CASE
		            WHEN CHARINDEX('Variant', [rs121913626]) > 0 OR CHARINDEX('Variant', [rs3218713]) > 0 OR CHARINDEX('Variant', [rs3218714]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 1' 
		            WHEN CHARINDEX('Variant', [rs121964855]) > 0 OR CHARINDEX('Variant', [rs121964856]) > 0 OR CHARINDEX('Variant', [rs121964857]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 2' 
		            WHEN CHARINDEX('Variant', [rs28934269]) > 0 OR CHARINDEX('Variant', [rs104894504]) > 0 OR CHARINDEX('Variant', [rs28934270]) > 0 OR CHARINDEX('Variant', [rs727504290]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 3' 
		            WHEN CHARINDEX('Variant', [rs375882485]) > 0 OR CHARINDEX('Variant', [rs397515937]) > 0 OR CHARINDEX('Variant', [rs397515963]) > 0 OR CHARINDEX('Variant', [rs397516074]) > 0 OR CHARINDEX('Variant', [rs397516083]) > 0 THEN 'Cardiomyopathy, Familial Hypertrophic, 4' 
		            ELSE 'No genetic risk found'
	            END AS [value],
	            [resulted_on] AS [ResultedOn], 'GVF' AS [Source]
            FROM
            (
	            SELECT patient_id,
                    result_file_id,
		            [1] AS [rs121913626],	-- MYH7
		            [2] AS [rs3218713],
		            [3] AS [rs3218714],
		            [4] AS [rs121964855],	-- TNNT2
		            [5] AS [rs121964856],
		            [6] AS [rs121964857],
		            [7] AS [rs28934269],	-- TPM1
		            [8] AS [rs104894504],
		            [9] AS [rs28934270],
		            [10] AS [rs727504290],
		            [11] AS [rs375882485],	-- MYBPC3
		            [12] AS [rs397515937],
		            [13] AS [rs397515963],
		            [14] AS [rs397516074],
		            [15] AS [rs397516083],
		            [16] AS [resulted_on]
	            FROM
	            (
		            SELECT prc.patient_id,
                        prc.result_file_id,
			            CASE
				            WHEN pv.value1 = pv.value2 THEN
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Homozygous_Variant'
						            ELSE 'Homozygous_Normal'
					            END
				            ELSE
					            CASE
						            WHEN pv.value1 != v.reference_bases OR pv.value2 != v.reference_bases THEN 'Heterozygous_Variant'
						            ELSE 'Heterozygous_Normal'
					            END 
			            END AS [zygosity],
			            ROW_NUMBER() OVER (PARTITION BY prc.patient_id ORDER BY v.gene_id, v.external_id) AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0

		            UNION ALL

		            SELECT prc.patient_id, prc.result_file_id, CONVERT(VARCHAR, MAX(pv.resulted_on), 101), 16 AS RowNum
		            FROM [mhgr_hybrid].[dbo].[patient_result_collections] prc
			            INNER JOIN [mhgr_hybrid].[dbo].[result_files] rf ON rf.id = prc.result_file_id AND rf.result_source_id = 4
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_result_members] prm ON prm.member_type = 2 -- Variant type
				            AND prm.collection_id = prc.id
			            INNER JOIN [mhgr_hybrid].[dbo].[patient_variants] pv ON pv.variant_type = 1  -- SNP variant type
				            AND pv.id = prm.member_id
			            INNER JOIN [mhgr_hybrid].[dbo].[variants] v ON v.id = pv.reference_id AND v.gene_id IN (6, 7, 8, 9)  -- MYH7, TNNT2, TPM1, MYBPC3
                    WHERE prc.patient_id=@p0
                    GROUP BY prc.patient_id, prc.result_file_id
		            ) a
		            PIVOT ( MAX(zygosity) FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12], [13], [14], [15], [16]) ) AS pvt
            ) v
            ORDER BY result_file_id, phenotype", id);
            return data.ToList();
        }
    }
}
