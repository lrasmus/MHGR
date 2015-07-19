using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Relational
{
    public class PhenotypeRepository
    {
        private RelationalEntities entities = new RelationalEntities();

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
            }

            // Next, identify the source lab this is from
            var source = (from src in entities.result_sources
                             where src.name == labName
                             select src).FirstOrDefault();
            if (source == null)
            {
                source = new result_sources()
                {
                    name = labName
                };
                entities.result_sources.Add(source);
            }

            if (entities.ChangeTracker.HasChanges())
            {
                entities.SaveChanges();
            }

            // Now add the patient phenotype
            var patientPhenotype = new patient_phenotypes()
            {
                phenotype = phenotype,
                resulted_on = resultedOn
            };
            entities.patient_phenotypes.Add(patientPhenotype);

            // Add the patient result collection for this entry
            var collection = new patient_result_collections()
            {
                patient_id = patient.id,
                received_on = DateTime.Now,
                source_id = source.id
            };
            entities.patient_result_collections.Add(collection);
            entities.SaveChanges();

            // Finally, link the collection to the phenotype
            var member = new patient_result_members()
            {
                patient_result_collections = collection,
                member_id = patientPhenotype.id,
                member_type = Enums.ResultMemberType.Phenotype,
            };

            entities.patient_result_members.Add(member);
            entities.SaveChanges();

            return collection;
        }
    }
}
