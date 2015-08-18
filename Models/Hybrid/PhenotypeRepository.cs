using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Hybrid
{
    public class PhenotypeRepository
    {
        private HybridEntities entities = new HybridEntities();
        private SourceRepository sourceRepo = new SourceRepository();
        private PatientRepository patientRepo = new PatientRepository();

        public patient_result_collections AddResult(patient patient, string labName, string name, string value, string externalId, string externalSource, DateTime resultedOn)
        {
            // First, identify the reference phenotype and if it exists
            var phenotype = (from pheno in entities.phenotypes
                             where pheno.name == name && pheno.value == value
                             select pheno).FirstOrDefault();
            if (phenotype == null)
            {
                phenotype = new phenotype()
                {
                    name = name,
                    value = value,
                    external_source = externalSource,
                    external_id = externalId
                };
                entities.phenotypes.Add(phenotype);
                entities.SaveChanges();
            }

            // Next, identify the source lab this is from
            var source = sourceRepo.AddSource(labName, string.Empty);
            
            // Now add the patient phenotype
            var patientPhenotype = new patient_phenotypes()
            {
                phenotype = phenotype,
                resulted_on = resultedOn
            };
            entities.patient_phenotypes.Add(patientPhenotype);
            entities.SaveChanges();

            // Add the patient result collection for this entry
            var collection = patientRepo.AddCollection(patient, source);

            // Finally, link the collection to the phenotype
            var member = new patient_result_members()
            {
                collection_id = collection.id,
                member_id = patientPhenotype.id,
                member_type = Enums.ResultMemberType.Phenotype,
            };

            entities.patient_result_members.Add(member);
            entities.SaveChanges();

            return collection;
        }

        public phenotype GetPhenotypeByExternalId(phenotype phenotype)
        {
            var existingPhenotype = (from pheno in entities.phenotypes
                                     where pheno.external_id == phenotype.external_id && pheno.external_source == phenotype.external_source
                             select pheno).FirstOrDefault();
            if (phenotype == null)
            {
                phenotype = new phenotype()
                {
                    name = phenotype.name,
                    value = phenotype.value,
                    external_source = phenotype.external_source,
                    external_id = phenotype.external_id,
                };
                entities.phenotypes.Add(phenotype);
                entities.SaveChanges();
            }

            return phenotype;
        }
    }
}
