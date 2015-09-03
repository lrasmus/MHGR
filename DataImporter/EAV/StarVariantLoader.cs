using MHGR.EAVModels;
using MHGR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MHGR.DataImporter.EAV
{
    public class StarVariantLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public override void LoadData(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            foreach (var dataLine in data.Skip(1))
            {
                var patientRepo = new PatientRepository();
                var entityRepo = new EntityRepository();
                var sourceRepo = new SourceRepository();

                var fields = dataLine.Split(Delimiter);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                var resultedOn = DateTime.Parse(fields[5]);
                var lab = fields[6];
                List<StarVariantResult> stars = new List<StarVariantResult>();
                stars.Add(new StarVariantResult() { Gene = "CYP2C19", Result = fields[7] });
                stars.Add(new StarVariantResult() { Gene = "CYP2C9", Result = fields[8] });
                stars.Add(new StarVariantResult() { Gene = "VKORC1", Result = fields[9] });

                var source = sourceRepo.AddSource(lab, string.Empty);
                var file = AddResultFile(filePath, source);
                entityRepo.AddStarVariants(patient, file, resultedOn, stars);
            }
        }
    }
}