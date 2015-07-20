using MHGR.Models.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Relational
{
    public class StarVariantLoader : BaseLoader
    {
        public const char Delimiter = '\t';

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
                List<VariantRepository.StarVariantResult> stars = new List<VariantRepository.StarVariantResult>();
                stars.Add(new VariantRepository.StarVariantResult() { Gene = "CYP2C19", Result = fields[7] });
                stars.Add(new VariantRepository.StarVariantResult() { Gene = "CYP2C9", Result = fields[8] });
                stars.Add(new VariantRepository.StarVariantResult() { Gene = "VKORC1", Result = fields[9] });
                

                variantRepo.AddStarVariants(patient, lab, resultedOn, stars);
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
