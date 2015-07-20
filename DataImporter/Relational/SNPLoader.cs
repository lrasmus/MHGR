using MHGR.Models.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Relational
{
    public class SNPLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public void LoadReference(string[] data)
        {
            var variantRepo = new VariantRepository();
            foreach (var dataLine in data.Skip(1))
            {
                var fields = dataLine.Split(Delimiter);
                variantRepo.AddVariant(fields[1], fields[5], "dbSNP", fields[2], int.Parse(fields[3]), int.Parse(fields[3]), fields[4], fields[7]);
            }
        }

        public override void LoadData(string[] data)
        {
            var patientRepo = new PatientRepository();
            var variantRepo = new VariantRepository();
            foreach (var dataLine in data.Skip(1))
            {
                var fields = dataLine.Split(Delimiter);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                var resultedOn = DateTime.Parse(fields[5]);
                var lab = fields[6];
                List<VariantRepository.SnpResult> snps = new List<VariantRepository.SnpResult>();
                for (int fieldIndex = 7; fieldIndex < 135; fieldIndex += 4)
                {
                    var snp = new VariantRepository.SnpResult()
                    {
                        RSID = fields[fieldIndex],
                        Chromosome = fields[fieldIndex + 1],
                        Position = int.Parse(fields[fieldIndex + 2]),
                        Genotype = fields[fieldIndex + 3]
                    };
                    snps.Add(snp);
                }

                variantRepo.AddSnps(patient, lab, resultedOn, snps);
            }
        }

        public bool ConsistencyChecks(int patients, int genes, int variants, int patientVariants, int patientResultCollections, int patientResultMembers)
        {
            var entities = new RelationalEntities();
            bool isValid = true;
            isValid = isValid && CheckEntityCounts(patients, entities.patients.Count(), "patients");
            isValid = isValid && CheckEntityCounts(genes, entities.genes.Count(), "genes");
            isValid = isValid && CheckEntityCounts(variants, entities.variants.Count(), "variants");
            isValid = isValid && CheckEntityCounts(patientVariants, entities.patient_variants.Count(), "patient variants");
            isValid = isValid && CheckEntityCounts(patientResultCollections, entities.patient_result_collections.Count(), "patient result collections");
            isValid = isValid && CheckEntityCounts(patientResultMembers, entities.patient_result_members.Count(), "patient result members");

            return isValid;
        }
    }
}
