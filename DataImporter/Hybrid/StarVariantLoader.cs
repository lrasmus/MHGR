using MHGR.Models.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
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
    }
}
