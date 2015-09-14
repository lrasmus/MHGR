using MHGR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class EntityRepository
    {
        protected EAVEntities entities = new EAVEntities();

        public result_entities AddPhenotype(result_files resultFile, patient patient, attribute attribute, DateTime? resultedOn)
        {
            result_entities rootEntity = new result_entities()
            {
                patient_id = patient.id,
                result_file_id = resultFile.id,
                attribute_id = attribute.id,
            };

            result_entities resultedOnEntity = new result_entities()
            {
                patient_id = patient.id,
                result_file_id = resultFile.id,
                attribute_id = GetAttribute(null, null, "Resulted on", null).id,
                parent = rootEntity,
                value_date_time = resultedOn
            };

            entities.result_entities.AddRange(new[] { rootEntity, resultedOnEntity });
            entities.SaveChanges();
            return rootEntity;
        }

        public result_entities[] AddSnps(result_files resultFile, patient patient, DateTime? resultedOn, List<SnpResult> snps)
        {
            var results = new List<result_entities>();
            foreach (var snp in snps)
            {
                var entityParts = new List<result_entities>();
                result_entities rootEntity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(snp.RSID, "dbSNP", snp.RSID, snp.RSID).id,
                };

                result_entities resultedOnEntity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(null, null, "Resulted on", null).id,
                    parent = rootEntity,
                    value_date_time = resultedOn
                };

                result_entities allele1Entity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(null, null, "SNP allele", null).id,
                    parent = rootEntity,
                    value_short_text = snp.Genotype[0].ToString()
                };

                result_entities allele2Entity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(null, null, "SNP allele", null).id,
                    parent = rootEntity,
                    value_short_text = snp.Genotype[1].ToString()
                };

                entities.result_entities.AddRange(new[] { rootEntity, resultedOnEntity, allele1Entity, allele2Entity });
                results.Add(rootEntity);
            }

            entities.SaveChanges();
            return results.ToArray();
        }

        public result_entities[] AddStarVariants(patient patient, result_files resultFile, DateTime resultedOn, List<StarVariantResult> stars)
        {
            var results = new List<result_entities>();
            foreach (var star in stars)
            {
                var entityParts = new List<result_entities>();
                result_entities rootEntity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(star.Gene, "HGNC", star.Gene, star.Gene).id
                };
                entityParts.Add(rootEntity);

                result_entities resultedOnEntity = new result_entities()
                {
                    patient_id = patient.id,
                    result_file_id = resultFile.id,
                    attribute_id = GetAttribute(null, null, "Resulted on", null).id,
                    parent = rootEntity,
                    value_date_time = resultedOn
                };
                entityParts.Add(resultedOnEntity);

                string[] splitStars = star.Result.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in splitStars)
                {
                    result_entities alleleEntity = new result_entities()
                    {
                        patient_id = patient.id,
                        result_file_id = resultFile.id,
                        attribute_id = GetAttribute(null, null, "Star allele", null).id,
                        parent = rootEntity,
                        value_short_text = value
                    };
                    entityParts.Add(alleleEntity);
                }

                entities.result_entities.AddRange(entityParts);
                results.Add(rootEntity);
            }

            entities.SaveChanges();
            return results.ToArray();
        }

        public result_entities AddVCF(result_entities entity, List<result_entities> headerEntities, List<result_entities> variantEntities)
        {
            entities.result_entities.Add(entity);
            entities.result_entities.AddRange(headerEntities);
            entities.result_entities.AddRange(variantEntities);
            entities.SaveChanges();
            return entity;
        }

        public result_entities AddGVF(result_entities entity, List<result_entities> pragmaEntities, List<result_entities> featureEntities)
        {
            entities.result_entities.Add(entity);
            entities.result_entities.AddRange(pragmaEntities);
            entities.result_entities.AddRange(featureEntities);
            entities.SaveChanges();
            return entity;
        }

        /// <summary>
        /// One known limitation - we assume that the three search types (code, name or value) are
        /// unique, but this is rarely the case.  This should be expanded so that it attempts to
        /// determine the best attribute given all of the parameters.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeSystem"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static attribute GetAttribute(string code, string codeSystem, string name, string value)
        {
            var attrRepo = new AttributeRepository();
            var attribute = attrRepo.FindUniqueAttributeByCode(code, codeSystem);
            if (attribute != null)
            {
                return attribute;
            }

            attribute = attrRepo.FindUniqueAttributeByName(value);
            if (attribute != null)
            {
                return attribute;
            }

            return attrRepo.FindUniqueAttributeByName(name);
        }
    }
}
