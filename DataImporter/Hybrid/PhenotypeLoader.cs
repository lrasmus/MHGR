using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHGR.HybridModels;
using System.Data.Entity;
using System.IO;

namespace MHGR.DataImporter.Hybrid
{
    public class PhenotypeLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public override void LoadData(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            foreach (var dataLine in data.Skip(1))
            {
                var sourceRepo = new SourceRepository();
                var patientRepo = new PatientRepository();
                var phenotypeRepo = new PhenotypeRepository();
                var fields = dataLine.Split(Delimiter);
                var source = sourceRepo.AddSource(fields[5], string.Empty);
                var file = AddResultFile(filePath, source);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                phenotypeRepo.AddResult(patient, file, fields[6], fields[7], fields[8], fields[9], DateTime.Parse(fields[10]));
            }
        }
    }
}
