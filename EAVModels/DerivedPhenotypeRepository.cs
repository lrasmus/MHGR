using MHGR.Models;
using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class DerivedPhenotypeRepository : BaseDerivedPhenotypeRepository
    {
        private EAVEntities entities = new EAVEntities();

        public override List<string> GetResultFileDetailsForPhenotype(string source, int fileId, string phenotype)
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
                    details.AddRange(GetPhenotypeDetails(file, phenotype));
                    break;
                case "Star":
                    details.AddRange(GetPhenotypeDetailsForStar(file, phenotype));
                    break;
                case "SNP":
                case "VCF":
                case "GVF":
                    details.AddRange(GetPhenotypeDetailsForSNP(file, phenotype));
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

        private class DetailResult
        {
            public string AttributeName { get; set; }
            public result_entities Result { get; set; }
        }
        
        private List<string> GetPhenotypeDetails(result_files file, string phenotype)
        {
            List<string> details = new List<string>();
            var geneIds = GetGeneFilterForPhenotype(phenotype);
            if (geneIds == null)
            {
                return details;
            }

            var results = (from re in entities.result_entities
                           join attr in entities.attributes on re.attribute_id equals attr.id
                           join phenRel in entities.attribute_relationships on attr.id equals phenRel.attribute1_id
                           join phenAttr in entities.attributes on phenRel.attribute2_id equals phenAttr.id
                           join geneRel in entities.attribute_relationships on phenRel.attribute2_id equals geneRel.attribute1_id
                           where phenRel.relationship_id == 2 && geneIds.Contains(geneRel.attribute2_id) && geneRel.relationship_id == 3
                            && re.result_file_id == file.id
                           select new List<string>() { phenAttr.name, attr.name }).ToArray();

            List<string> uniqueValues = new List<string>();
            foreach (var result in results) 
            {
                string value = string.Format("<div>{0}: {1}</div>", result[0], result[1]);
                if (!uniqueValues.Contains(value))
                {
                    details.Add(value);
                    uniqueValues.Add(value);
                }
                
            }

            return details;
        }

        private List<string> GetPhenotypeDetailsForStar(result_files file, string phenotype)
        {
            List<string> details = new List<string>();
            var geneIds = GetGeneFilterForPhenotype(phenotype);
            if (geneIds == null)
            {
                return details;
            }

            var results = (from re in entities.result_entities
                           join parentEnt in entities.result_entities on re.parent_id equals parentEnt.id
                           join attr in entities.attributes on parentEnt.attribute_id equals attr.id
                           where geneIds.Contains(attr.id)
                            && re.result_file_id == file.id && re.attribute_id == 129
                           select new DetailResult() { AttributeName = attr.name, Result = re }).ToArray();

            string currentGene = "";
            foreach (var result in results)
            {
                if (string.IsNullOrEmpty(currentGene))
                {
                    details.Add(string.Format("<div>{0}: *{1}", result.AttributeName, FormatResultEntityValue(result.Result)));
                }
                else if (currentGene != result.AttributeName)
                {
                    details.Add(string.Format("</div><div>{0}: {1}", result.AttributeName, FormatResultEntityValue(result.Result)));
                }
                else
                {
                    details.Add(string.Format("/*{0}", FormatResultEntityValue(result.Result)));
                }

                currentGene = result.AttributeName;
            }
            details.Add("</div>");

            return details;
        }

        private List<string> GetPhenotypeDetailsForSNP(result_files file, string phenotype)
        {
            List<string> details = new List<string>();
            var geneIds = GetGeneFilterForPhenotype(phenotype);
            if (geneIds == null)
            {
                return details;
            }

            var results = (from valRes in entities.result_entities
                           join varRes in entities.result_entities on valRes.parent_id equals varRes.id
                           join attr in entities.attributes on varRes.attribute_id equals attr.id
                           join rel in entities.attribute_relationships on attr.id equals rel.attribute1_id
                           where rel.relationship_id == 5 && geneIds.Contains(rel.attribute2_id) && varRes.result_file_id == file.id
                             && (valRes.attribute_id == 128 || valRes.attribute_id == 129)
                           orderby attr.name
                           select new DetailResult() { AttributeName = attr.name, Result = valRes }).ToArray();

            string currentVariant = "";
            foreach (var result in results)
            {
                if (string.IsNullOrEmpty(currentVariant))
                {
                    details.Add(string.Format("<div>{0}: {1}", result.AttributeName, FormatResultEntityValue(result.Result)));
                }
                else if (currentVariant != result.AttributeName)
                {
                    details.Add(string.Format("</div><div>{0}: {1}", result.AttributeName, FormatResultEntityValue(result.Result)));
                }
                else
                {
                    details.Add(FormatResultEntityValue(result.Result));
                }

                currentVariant = result.AttributeName;
            }
            details.Add("</div>");

            return details;
        }

        private string FormatResultEntityValue(result_entities entity)
        {
            if (entity == null)
            {
                return string.Empty;
            }

            if (entity.value_date_time.HasValue)
            {
                return entity.value_date_time.Value.ToShortDateString();
            }
            else if (entity.value_float.HasValue)
            {
                return entity.value_float.ToString();
            }
            else if (entity.value_int.HasValue)
            {
                return entity.value_int.ToString();
            }
            else if (!string.IsNullOrEmpty(entity.value_short_text))
            {
                return entity.value_short_text;
            }
            else if (!string.IsNullOrEmpty(entity.value_text))
            {
                return entity.value_text;
            }

            return string.Empty;
        }

        public override int[] GetGeneIdListForGeneNames(string[] geneNames)
        {
            return (from attr in entities.attributes
                    join rel in entities.attribute_relationships on attr.id equals rel.attribute1_id
                    where rel.attribute2_id == 30 && rel.relationship_id == 2 && geneNames.Contains(attr.name)
                    select attr.id).ToArray();
            //return entities.attribute_relationships.Where(x => x.attribute2_id == 30 && x.relationship_id == 2).Select(x => x.attribute1_id).ToArray();
        }

        private List<attribute> GetVariantsForGenes(int[] genes)
        {
            var variants = from attr in entities.attributes
                           join rel in entities.attribute_relationships on attr.id equals rel.attribute1_id
                           where rel.relationship_id == 5 && genes.Contains(rel.attribute2_id)
                           select attr;
            return variants.ToList();
            //return entities.attribute_relationships.Where(x => genes.Contains(x.attribute2_id) && x.relationship_id == 5).Select(x => x.attribute1_id).ToArray();
        }

        public override List<DerivedPhenotype> GetPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"WITH phenotypes ([id], [name], [parent_id], [parent_name])
            AS
            (
            SELECT a.id, a.name, NULL AS [parent_id], CONVERT(nvarchar, N'Phenotype') AS [parent_name]
	            FROM [mhgr_eav].[dbo].[attributes] a
	            INNER JOIN [mhgr_eav].[dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id
		            AND ar.attribute2_id = 7	-- Phenotype
		            AND ar.relationship_id = 2	-- Is a

            UNION ALL

            SELECT a.id, a.name, p.id AS [parent_id], CONVERT(nvarchar, p.name) AS [parent_name]
	            FROM [mhgr_eav].[dbo].[attributes] a
	            INNER JOIN [mhgr_eav].[dbo].[attribute_relationships] ar ON a.id = ar.attribute1_id
		            AND ar.relationship_id = 2	-- Is a
	            INNER JOIN phenotypes p ON ar.attribute2_id = p.id
            )

            SELECT re.result_file_id AS [ResultFileId], p.parent_name as [phenotype], p.name as [value], CONVERT(VARCHAR, ro.value_date_time, 101) AS [ResultedOn], 'Phenotype' AS [Source]
            FROM phenotypes p
	            INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.attribute_id = p.id
	            INNER JOIN [mhgr_eav].[dbo].[result_entities] ro ON ro.attribute_id = 72	-- Resulted on
		            AND ro.parent_id = re.id
                WHERE re.patient_id=@p0
	            ORDER BY re.result_file_id, ResultedOn DESC", id);
            return data.ToList();
        }

        public override List<DerivedPhenotype> GetDosing(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT result_file_id AS [ResultFileId], [phenotype], [value], MAX([resulted_on]) AS [ResultedOn], 'Star' AS [Source]
            FROM
            (
	            SELECT result_file_id,
		            'Warfarin dosing range' AS [phenotype],
		            CASE -- VKORC1 cases
			            WHEN [vkorc1_value1] = '1' AND [vkorc1_value2] = '1' THEN
				            CASE
					            WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '5-7 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '5-7 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
					            WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '3-4 mg/day'
					            WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '3-4 mg/day'
					            WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
					            ELSE 'Unknown'
				            END
			            WHEN ([vkorc1_value1] = '1' AND [vkorc1_value2] = '2') OR ([vkorc1_value1] = '2' AND [vkorc1_value2] = '1') THEN
				            CASE
					            WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '5-7 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
					            WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '3-4 mg/day'
					            WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '0.5-2 mg/day'
					            WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
					            ELSE 'Unknown'
				            END
			            WHEN [vkorc1_value1] = '2' AND [vkorc1_value2] = '2' THEN
				            CASE
					            WHEN [cyp2c9_value1] = '1' AND [cyp2c9_value2] = '1' THEN '3-4 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '2') OR ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '1') THEN '3-4 mg/day'
					            WHEN ([cyp2c9_value1] = '1' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '1') THEN '0.5-2 mg/day'
					            WHEN [cyp2c9_value1] = '2' AND [cyp2c9_value2] = '2' THEN '0.5-2 mg/day'
					            WHEN ([cyp2c9_value1] = '2' AND [cyp2c9_value2] = '3') OR ([cyp2c9_value1] = '3' AND [cyp2c9_value2] = '2') THEN '0.5-2 mg/day'
					            WHEN [cyp2c9_value1] = '3' AND [cyp2c9_value2] = '3' THEN '0.5-2 mg/day'
					            ELSE 'Unknown'
				            END
			            ELSE 'Unknown'
		            END AS [value],
		            [resulted_on]
	            FROM 
	            (
		            SELECT patient_id, result_file_id, [1] AS [cyp2c9_value1], [2] AS [cyp2c9_value2], [3] AS [vkorc1_value1], [4] AS [vkorc1_value2], [5] AS [resulted_on]
		            FROM 
		            (
			            SELECT allele.patient_id, allele.result_file_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.patient_id ORDER BY gene.id, gene.attribute_id, allele.id) AS RowNum
			            FROM [mhgr_eav].[dbo].[result_entities] gene
				            INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id
					            AND allele.attribute_id = 129	-- Star allele
			            WHERE gene.attribute_id IN (32, 33)		-- CYP2C9, VKORC1
                            AND gene.patient_id=@p0
			            UNION ALL

			            SELECT allele.patient_id, allele.result_file_id, CONVERT(VARCHAR, MIN(allele.value_date_time), 101), 5 AS RowNum
			            FROM [mhgr_eav].[dbo].[result_entities] gene
				            INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id
					            AND allele.attribute_id = 72	-- Resulted on
			            WHERE gene.attribute_id IN (32, 33)		-- CYP2C9, VKORC1
                            AND gene.patient_id=@p0
			            GROUP BY allele.patient_id, allele.result_file_id
		            ) a
		            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4], [5]) ) AS pvt
	            ) pvt
            ) AS a
            GROUP BY result_file_id, [phenotype], [value]
            ORDER BY result_file_id, [value]", id);
            return data.ToList();
        }

        public override List<DerivedPhenotype> GetSNPPhenotypes(int id)
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
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id, result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
				            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS [resulted_on]
			            FROM
			            (
				            -- Get the resulted on attribute
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = snps.id 
					            AND res_dt.attribute_id = 72	-- Resulted on
				            INNER JOIN [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of a gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Get the reference base attribute
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = snps.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab SNP results which have no parent ID, and child alleles.
				            -- Limit this to just SNPs that are under CYP2C19
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY snps.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5     -- Variant of gene
					            AND ar.attribute2_id = 31      -- CYP2C19
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128 -- SNP allele
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([40], [41], [42], [43], [44], [45], [46], [47], [48], [49]) ) AS pvt
            ) v

            UNION ALL

            -- Convert CYP2C9 SNPs to phenotype
            SELECT result_file_id AS [ResultFileId],
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
		            [50] AS rs1057910,
		            [51] AS rs1799853,
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on attribute
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = snps.id 
					            AND res_dt.attribute_id = 72	-- Resulted on
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases for the SNP
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = snps.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab SNP results which have no parent ID, and child alleles.
				            -- Limit this to just SNPs that are under CYP2C9
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY snps.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5     -- Variant of gene
					            AND ar.attribute2_id = 32      -- CYP2C9
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128 -- SNP allele
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([50], [51]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
		            [55] AS [rs6025],		-- F5
		            [56] AS [rs1799963],	-- F2
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = snps.id 
					            AND res_dt.attribute_id = 72		-- Resulted on
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = snps.id 
					            AND refs.attribute_id = 115			-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab SNP results which have no parent ID, and child alleles.
				            -- Limit this to just SNPs that are under F2 and F5 genes
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY snps.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128		-- SNP allele
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([55], [56]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
		            [57] AS [rs121913626],
		            [58] AS [rs3218713],
		            [59] AS [rs3218714],
		            [60] AS [rs121964855],
		            [61] AS [rs121964856],
		            [62] AS [rs121964857],
		            [63] AS [rs28934269],
		            [64] AS [rs28934270],
		            [65] AS [rs727504290],
		            [66] AS [rs104894504],
		            [67] AS [rs375882485],
		            [68] AS [rs397516083],
		            [69] AS [rs397515937],
		            [70] AS [rs397516074],
		            [71] AS [rs397515963],
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = snps.id 
					            AND res_dt.attribute_id = 72	-- Resulted on
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)    -- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = snps.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)	-- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0

				            UNION ALL

				            -- Grab SNP results which have no parent ID, and child alleles.
				            -- Limit this to just SNPs that are under MYH7, TNNT2, TPM1, MYBPC3 genes
				            SELECT snps.patient_id, snps.result_file_id, snps.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY snps.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] snps
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)	-- MYH7, TNNT2, TPM1, MYBPC3
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE snps.parent_id IS NULL
                                AND snps.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([57], [58], [59], [60], [61], [62], [63], [64], [65], [66], [67], [68], [69], [70], [71]) ) AS pvt
            ) v
            ORDER BY result_file_id, phenotype", id);
            return data.ToList();
        }

        public override List<DerivedPhenotype> GetStarPhenotypes(int id)
        {
            DbRawSqlQuery<DerivedPhenotype> data = entities.Database.SqlQuery<DerivedPhenotype>(
            @"SELECT pvt.result_file_id AS [ResultFileId], 'Clopidogrel metabolism' AS [phenotype],
	            CASE
		            -- Ultrarapid can be *1/*17, *17/*1, *17/*17
		            WHEN pvt.value1 ='1' AND pvt.value2 = '17' THEN 'Ultrarapid metabolizer'
		            WHEN pvt.value1 = '17' AND pvt.value2 = '1' THEN 'Ultrarapid metabolizer'
		            WHEN pvt.value1 = '17' AND pvt.value2 = '17' THEN 'Ultrarapid metabolizer'
		            -- Those that are both *1
		            WHEN pvt.value1 = '1' AND pvt.value2 = '1' THEN 'Extensive metabolizer'
		            -- For intermediate metabolizer one allele as *1 or one as *17 (other allele is something non *1 and non *17), it's intermediate
		            WHEN pvt.value1 IN ('1', '17') AND pvt.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
		            WHEN pvt.value2 IN ('1', '17') AND pvt.value1 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Intermediate metabolizer'
		            -- For poor metabolizer, they are both *2 to *8.  We need to account for all values, so we're not picking up some unknown value
		            WHEN pvt.value1 IN ('2', '3', '4', '5', '6', '7', '8') AND pvt.value2 IN ('2', '3', '4', '5', '6', '7', '8') THEN 'Poor metabolizer'
		            ELSE 'Unknown'
	            END AS [value],
	            CONVERT(VARCHAR, re.value_date_time, 101) AS [ResultedOn], 'Star' AS [Source]
            FROM 
            (
	            SELECT patient_id, result_file_id, gene_entity_id, [1] AS [value1], [2] AS [value2]
	            FROM 
	            (
		            SELECT gene.id AS [gene_entity_id], allele.result_file_id, allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.id, allele.attribute_id ORDER BY gene.attribute_id, allele.id) AS RowNum
		            FROM [mhgr_eav].[dbo].[result_entities] gene
			            INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id
				            AND allele.attribute_id = 129	-- Star allele
		            WHERE gene.attribute_id = 31 -- CYP2C19
                        AND gene.patient_id=@p0
	            ) a
	            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
            ) pvt
            INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.parent_id = gene_entity_id
	            AND re.attribute_id = 72 -- Resulted on

            UNION ALL

            -- Translate the CYP2C9 stars for warfarin metabolism
            -- https://www.pharmgkb.org/guideline/PA166104949
            SELECT pvt.result_file_id AS [ResultFileId],
	            'Warfarin metabolism' AS [phenotype],
	            CASE
		            -- Everything except *1/*1 is Decreased, but check to make sure we don't have unknown values
		            WHEN pvt.value1 = '1' AND pvt.value2 = '1' THEN 'Normal'
		            WHEN pvt.value1 IN ('1', '2', '3') AND pvt.value2 IN ('1', '2', '3') THEN 'Decreased'
		            ELSE 'Unknown'
	            END AS [value],
	            CONVERT(VARCHAR, re.value_date_time, 101) AS [ResultedOn], 'Star' AS [Source]
            FROM 
            (
	            SELECT patient_id, result_file_id, gene_entity_id, [1] AS [value1], [2] AS [value2]
	            FROM 
	            (
		            SELECT gene.id AS [gene_entity_id], allele.result_file_id, allele.patient_id, allele.value_short_text, ROW_NUMBER() OVER (PARTITION BY gene.id, allele.attribute_id ORDER BY gene.attribute_id, allele.id) AS RowNum
		            FROM [mhgr_eav].[dbo].[result_entities] gene
			            INNER JOIN [mhgr_eav].[dbo].[result_entities] allele ON allele.parent_id = gene.id
				            AND allele.attribute_id = 129	-- Star allele
		            WHERE gene.attribute_id = 32			-- CYP2C9
                        AND gene.patient_id=@p0
	            ) a
	            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2]) ) AS pvt
            ) pvt
            INNER JOIN [mhgr_eav].[dbo].[result_entities] re ON re.parent_id = gene_entity_id
	            AND re.attribute_id = 72 -- Resulted on
            ORDER BY pvt.result_file_id, CONVERT(VARCHAR, re.value_date_time, 101) DESC, [value]", id);
            return data.ToList();
        }

        public override List<DerivedPhenotype> GetVCFPhenotypes(int id)
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
	            [resulted_on] AS [ResultedOn], 'VCF' AS [Source]
            FROM 
            (
	            SELECT patient_id,
                    result_file_id,
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
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id, result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS [resulted_on]
			            FROM
			            (
				            -- Grab resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] vcf ON vcf.id = features.parent_id
					            AND vcf.attribute_id = 103		-- VCF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = vcf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE features.attribute_id = 114	-- VCF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab vcf features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under CYP2C19
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 114   -- VCF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([40], [41], [42], [43], [44], [45], [46], [47], [48], [49]) ) AS pvt
            ) v

            UNION ALL

            -- Convert CYP2C9 SNPs to phenotype
            SELECT result_file_id AS [ResultFileId],
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
		            [50] AS rs1057910,
		            [51] AS rs1799853,
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id, result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
				            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] vcf ON vcf.id = features.parent_id
					            AND vcf.attribute_id = 103		-- VCF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = vcf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE features.attribute_id = 114   -- VCF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab vcf features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under CYP2C9
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 114   -- VCF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([50], [51]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
	            SELECT patient_id, result_file_id,
		            [55] AS [rs6025],		-- F5
		            [56] AS [rs1799963],	-- F2
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id, result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] vcf ON vcf.id = features.parent_id
					            AND vcf.attribute_id = 103		-- VCF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = vcf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            INNER JOIN [mhgr_eav].[dbo].[patients] p ON p.id = features.patient_id
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            WHERE features.attribute_id = 114   -- VCF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab vcf features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under F2 or F5 genes
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 114	-- VCF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([55], [56]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
		            [57] AS [rs121913626],
		            [58] AS [rs3218713],
		            [59] AS [rs3218714],
		            [60] AS [rs121964855],
		            [61] AS [rs121964856],
		            [62] AS [rs121964857],
		            [63] AS [rs28934269],
		            [64] AS [rs28934270],
		            [65] AS [rs727504290],
		            [66] AS [rs104894504],
		            [67] AS [rs375882485],
		            [68] AS [rs397516083],
		            [69] AS [rs397515937],
		            [70] AS [rs397516074],
		            [71] AS [rs397515963],
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] vcf ON vcf.id = features.parent_id
					            AND vcf.attribute_id = 103		-- VCF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = vcf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)  -- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 115		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)  -- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE features.attribute_id = 114   -- VCF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab vcf features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under MYH7, TNNT2, TPM1, MYBPC3 genes
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)  -- MYH7, TNNT2, TPM1, MYBPC3
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 114	-- VCF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([57], [58], [59], [60], [61], [62], [63], [64], [65], [66], [67], [68], [69], [70], [71]) ) AS pvt
            ) v
            ORDER BY result_file_id, phenotype", id);
            return data.ToList();
        }

        public override List<DerivedPhenotype> GetGVFPhenotypes(int id)
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
	            [resulted_on] AS [ResultedOn], 'GVF' AS [Source]
            FROM 
            (
	            SELECT patient_id,
                    result_file_id,
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
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS [resulted_on]
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] gvf ON gvf.id = features.parent_id
					            AND gvf.attribute_id = 74		-- GVF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = gvf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 100		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab GVF features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under CYP2C19
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 31		-- CYP2C19
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([40], [41], [42], [43], [44], [45], [46], [47], [48], [49]) ) AS pvt
            ) v

            UNION ALL

            -- Convert CYP2C9 SNPs to phenotype
            SELECT result_file_id AS [ResultFileId],
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
		            [50] AS rs1057910,
		            [51] AS rs1799853,
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] gvf ON gvf.id = features.parent_id
					            AND gvf.attribute_id = 74		-- GVF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = gvf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 100		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab GVF features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under CYP2C9
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id = 32		-- CYP2C9
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([50], [51]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
		            [55] AS [rs6025],		-- F5
		            [56] AS [rs1799963],	-- F2
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
				            patient_id,
                            result_file_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] gvf ON gvf.id = features.parent_id
					            AND gvf.attribute_id = 74			-- GVF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = gvf.id 
					            AND res_dt.attribute_id = 72		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 100			-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)    -- F2, F5
				            WHERE features.attribute_id = 96		-- GVF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab GVF features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under F2 or F5
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5			-- Variant of gene
					            AND ar.attribute2_id IN (34, 35)	-- F2, F5
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128		-- SNP allele
				            WHERE features.attribute_id = 96		-- GVF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([55], [56]) ) AS pvt
            ) v

            UNION ALL

            SELECT result_file_id AS [ResultFileId],
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
		            [57] AS [rs121913626],
		            [58] AS [rs3218713],
		            [59] AS [rs3218714],
		            [60] AS [rs121964855],
		            [61] AS [rs121964856],
		            [62] AS [rs121964857],
		            [63] AS [rs28934269],
		            [64] AS [rs28934270],
		            [65] AS [rs727504290],
		            [66] AS [rs104894504],
		            [67] AS [rs375882485],
		            [68] AS [rs397516083],
		            [69] AS [rs397515937],
		            [70] AS [rs397516074],
		            [71] AS [rs397515963],
		            [resulted_on]
	            FROM
	            (
		            SELECT patient_id,
                        result_file_id,
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
			            END AS [zygosity],
			            [resulted_on]
		            FROM
			            (
			            SELECT id,
                            result_file_id,
				            patient_id,
				            snp_id,
				            [1] AS value1,
				            COALESCE([2], [1]) AS value2,  -- If only one value is set, we assume the alleles are homozygous
				            [3] AS reference_bases,
				            [4] AS resulted_on
			            FROM
			            (
				            -- Grab the resulted on date
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], CONVERT(VARCHAR, res_dt.value_date_time, 101) AS [value_short_text],
					            4 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] gvf ON gvf.id = features.parent_id
					            AND gvf.attribute_id = 74		-- GVF result
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] res_dt ON res_dt.parent_id = gvf.id 
					            AND res_dt.attribute_id = 72	-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)	-- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE features.patient_id=@p0

				            UNION ALL

				            -- Grab the reference bases
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], refs.value_short_text,
					            3 AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] refs ON refs.parent_id = features.id 
					            AND refs.attribute_id = 100		-- Reference bases
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON snps.attribute_id = ar.attribute1_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)	-- MYH7, TNNT2, TPM1, MYBPC3
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0

				            UNION ALL

				            -- Grab GVF features, features have children specific SNPs, which have children SNP allele attributes
				            -- Limit this to just SNPs that are under MYH7, TNNT2, TPM1, MYBPC3
				            SELECT features.patient_id, features.result_file_id, features.id, snps.attribute_id AS [snp_id], alleles.value_short_text,
					            ROW_NUMBER() OVER (PARTITION BY snps.id, snps.attribute_id ORDER BY features.patient_id, snps.attribute_id) AS RowNum
				            FROM  [mhgr_eav].[dbo].[result_entities] features
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] snps ON snps.parent_id = features.id 
				            INNER JOIN  [mhgr_eav].[dbo].[attribute_relationships] ar ON ar.attribute1_id = snps.attribute_id
					            AND ar.relationship_id = 5		-- Variant of gene
					            AND ar.attribute2_id IN (36, 37, 38, 39)	-- MYH7, TNNT2, TPM1, MYBPC3
				            INNER JOIN  [mhgr_eav].[dbo].[result_entities] alleles ON alleles.parent_id = snps.id
					            AND alleles.attribute_id = 128	-- SNP allele
				            WHERE features.attribute_id = 96	-- GVF Feature
                                AND features.patient_id=@p0
			            ) a
			            PIVOT ( MAX(value_short_text) FOR RowNum IN ([1], [2], [3], [4]) ) AS pvt
		            ) v
	            ) snps
	            PIVOT ( MAX(zygosity) FOR snp_id IN ([57], [58], [59], [60], [61], [62], [63], [64], [65], [66], [67], [68], [69], [70], [71]) ) AS pvt
            ) v
            ORDER BY result_file_id, phenotype", id);
            return data.ToList();
        }
    }
}
