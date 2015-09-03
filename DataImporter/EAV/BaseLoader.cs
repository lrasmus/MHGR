using MHGR.EAVModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.EAV
{
    public abstract class BaseLoader : DataImporter.BaseLoader
    {
        protected FileRepository fileRepo = new FileRepository();

        protected result_files AddResultFile(string filePath, result_sources source)
        {
            string hash = GetFileHash(filePath);

            var file = new result_files()
            {
                md5 = hash,
                name = filePath,
                received_on = DateTime.Now,
                result_source_id = source.id
            };
            return fileRepo.AddResultFile(file);
        }

        /// <summary>
        /// Check the counts of the database tables to ensure they meet what's expected
        /// </summary>
        /// <param name="patients"></param>
        /// <param name="phenotypes"></param>
        /// <param name="genes"></param>
        /// <param name="variants"></param>
        /// <param name="patientPhenotypes"></param>
        /// <param name="patientVariants"></param>
        /// <param name="patientResultCollections"></param>
        /// <param name="patientResultMembers"></param>
        /// <param name="patientVariantInformation"></param>
        /// <param name="variantInformationTypes"></param>
        /// <returns></returns>
        public bool ConsistencyChecks(int patients, int resultEntities, int resultFiles, int resultSources)
        {
            var entities = new EAVEntities();
            bool isValid = true;
            isValid = isValid && CheckEntityCounts(patients, entities.patients.Count(), "patients");
            isValid = isValid && CheckEntityCounts(resultEntities, entities.result_entities.Count(), "result entities");
            isValid = isValid && CheckEntityCounts(resultFiles, entities.result_files.Count(), "result files");
            isValid = isValid && CheckEntityCounts(resultSources, entities.result_sources.Count(), "result sources");
            //isValid = isValid && CheckEntityCounts(patientPhenotypes, entities.patient_phenotypes.Count(), "patient phenotypes");
            //isValid = isValid && CheckEntityCounts(patientVariants, entities.patient_variants.Count(), "patient variants");
            //isValid = isValid && CheckEntityCounts(patientResultCollections, entities.patient_result_collections.Count(), "patient result collections");
            //isValid = isValid && CheckEntityCounts(patientResultMembers, entities.patient_result_members.Count(), "patient result members");
            //isValid = isValid && CheckEntityCounts(patientVariantInformation, entities.patient_variant_information.Count(), "patient variant information");
            //isValid = isValid && CheckEntityCounts(variantInformationTypes, entities.variant_information_types.Count(), "variant information type");

            return isValid;
        }
    }
}
