﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Relational
{
    public class VariantRepository
    {
        private RelationalEntities entities = new RelationalEntities();
        private SourceRepository sourceRepo = new SourceRepository();
        private PatientRepository patientRepo = new PatientRepository();
        
        public class SnpResult
        {
            public string RSID;
            public string Chromosome;
            public int Position;
            public string Genotype;
        }

        public class StarVariantResult
        {
            public string Gene;
            public string Result;
        }

        public gene AddGene(string name, string symbol, string externalId, string externalSource, string chromosome)
        {
            var existingGene = (from g in entities.genes
                                   where g.symbol == symbol
                                   select g).FirstOrDefault();
            if (existingGene == null)
            {
                existingGene = new gene()
                {
                    name = name,
                    symbol = symbol,
                    external_id = externalId,
                    external_source = externalSource,
                    chromosome = chromosome
                };
                entities.genes.Add(existingGene);
                entities.SaveChanges();
            }

            return existingGene;
        }

        public variant GetVariant(string externalId, string externalSource)
        {
            return (from v in entities.variants
                    where v.external_source == externalSource && v.external_id == externalId
                    select v).FirstOrDefault();
        }

        public variant AddVariant(string geneSymbol, string externalId, string externalSource, string chromosome, int? startPosition, int? endPosition, string referenceGenome, string referenceBases)
        {
            var existingGene = AddGene(geneSymbol, geneSymbol, null, null, chromosome);

            var existingVariant = GetVariant(externalId, externalSource);
            if (existingVariant == null)
            {
                existingVariant = new variant()
                {
                    gene_id = existingGene.id,
                    external_id = externalId,
                    external_source = externalSource,
                    chromosome = chromosome,
                    start_position = startPosition,
                    end_position = endPosition,
                    reference_genome = referenceGenome,
                    reference_bases = referenceBases
                };
                entities.variants.Add(existingVariant);
                entities.SaveChanges();
            }

            return existingVariant;
        }

        public variant_information_types AddVariantInformationType(string name, string description, byte source)
        {
            var existingType = entities.variant_information_types.FirstOrDefault(x => x.name == name && x.source == source);
            if (existingType == null)
            {
                existingType = new variant_information_types()
                {
                    name = name,
                    description = description,
                    source = source
                };
                entities.variant_information_types.Add(existingType);
                entities.SaveChanges();
            }

            return existingType;
        }

        public patient_result_collections AddSnps(patient patient, string labName, DateTime resultedOn, List<SnpResult> snps)
        {
            var source = sourceRepo.AddSource(labName, string.Empty);

            // Create a collection
            var collection = patientRepo.AddCollection(patient, source);

            // Create patient variant entries
            List<patient_variants> variants = new List<patient_variants>();
            foreach (var snp in snps)
            {
                var variant = GetVariant(snp.RSID, "dbSNP");
                var patientVariant = new patient_variants()
                {
                    patient_id = patient.id,
                    variant_type = Enums.PatientVariantType.SNP,
                    reference_id = variant.id,
                    resulted_on = resultedOn,
                    value1 = snp.Genotype[0].ToString(),
                    value2 = snp.Genotype[1].ToString()
                };
                variants.Add(patientVariant);
            }
            entities.patient_variants.AddRange(variants);
            entities.SaveChanges();

            // Finally, link the collection to the SNPs
            foreach (var variant in variants)
            {
                var member = new patient_result_members()
                {
                    collection_id = collection.id,
                    member_id = variant.reference_id,
                    member_type = Enums.ResultMemberType.Variant,
                };
                entities.patient_result_members.Add(member);
            }
            entities.SaveChanges();

            return collection;
        }

        public patient_result_collections AddStarVariants(patient patient, string labName, DateTime resultedOn, List<StarVariantResult> stars)
        {
            var source = sourceRepo.AddSource(labName, string.Empty);

            // Create a collection
            var collection = patientRepo.AddCollection(patient, source);

            // Create patient variant entries
            List<patient_variants> variants = new List<patient_variants>();
            foreach (var star in stars)
            {
                var gene = AddGene(star.Gene, star.Gene, null, null, null);
                var variant = AddVariant(star.Gene, star.Result, "Star Variant", null, null, null, null, null);
                string[] splitStars = star.Result.Split(new string[]{"*"}, StringSplitOptions.RemoveEmptyEntries);
                var patientVariant = new patient_variants()
                {
                    patient_id = patient.id,
                    variant_type = Enums.PatientVariantType.StarVariant,
                    reference_id = variant.id,
                    resulted_on = resultedOn,
                    value1 = splitStars[0],
                    value2 = splitStars[1]
                };
                variants.Add(patientVariant);
            }
            //foreach (var snp in snps)
            //{
            //    var variant = GetVariant(snp.RSID, "dbSNP");
            //    var patientVariant = new patient_variants()
            //    {
            //        patient_id = patient.id,
            //        variant_type = Enums.PatientVariantType.StarVariant,
            //        reference_id = variant.id,
            //        resulted_on = resultedOn,
            //        value1 = snp.Genotype[0].ToString(),
            //        value2 = snp.Genotype[1].ToString()
            //    };
            //    variants.Add(patientVariant);
            //}
            entities.patient_variants.AddRange(variants);
            entities.SaveChanges();

            // Finally, link the collection to the stars
            foreach (var variant in variants)
            {
                var member = new patient_result_members()
                {
                    collection_id = collection.id,
                    member_id = variant.reference_id,
                    member_type = Enums.ResultMemberType.Variant,
                };
                entities.patient_result_members.Add(member);
            }
            entities.SaveChanges();

            return collection;
        }
    }
}
