﻿using MHGR.HybridModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public abstract class BaseLoader
    {
        protected FileRepository fileRepo = new FileRepository();
        public abstract void LoadData(string filePath);

        protected result_files AddResultFile(string filePath, result_sources source)
        {
            string hash = null;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }

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
        /// Utility function to verify an expected count matches an actual one
        /// </summary>
        /// <param name="expectedCount"></param>
        /// <param name="actualCount"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        protected static bool CheckEntityCounts(int expectedCount, int actualCount, string entityName)
        {
            if (actualCount != expectedCount)
            {
                Console.WriteLine("Expected {0} {1}, but counted {2}", expectedCount, entityName, actualCount);
                return false;
            }

            return true;
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
        public bool ConsistencyChecks(int patients, int phenotypes, int genes, int variants, int patientPhenotypes, int patientVariants, int patientResultCollections, int patientResultMembers, int patientVariantInformation, int variantInformationTypes)
        {
            var entities = new HybridEntities();
            bool isValid = true;
            isValid = isValid && CheckEntityCounts(patients, entities.patients.Count(), "patients");
            isValid = isValid && CheckEntityCounts(phenotypes, entities.phenotypes.Count(), "phenotypes");
            isValid = isValid && CheckEntityCounts(genes, entities.genes.Count(), "genes");
            isValid = isValid && CheckEntityCounts(variants, entities.variants.Count(), "variants");
            isValid = isValid && CheckEntityCounts(patientPhenotypes, entities.patient_phenotypes.Count(), "patient phenotypes");
            isValid = isValid && CheckEntityCounts(patientVariants, entities.patient_variants.Count(), "patient variants");
            isValid = isValid && CheckEntityCounts(patientResultCollections, entities.patient_result_collections.Count(), "patient result collections");
            isValid = isValid && CheckEntityCounts(patientResultMembers, entities.patient_result_members.Count(), "patient result members");
            isValid = isValid && CheckEntityCounts(patientVariantInformation, entities.patient_variant_information.Count(), "patient variant information");
            isValid = isValid && CheckEntityCounts(variantInformationTypes, entities.variant_information_types.Count(), "variant information type");

            return isValid;
        }
    }
}
