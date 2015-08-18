using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHGR.Models.Hybrid;
using System.Data.Entity;

namespace MHGR.DataImporter.Hybrid
{
    public class PhenotypeLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public override void LoadData(string[] data)
        {
            foreach (var dataLine in data.Skip(1))
            {
                var patientRepo = new PatientRepository();
                var phenotypeRepo = new PhenotypeRepository();
                var fields = dataLine.Split(Delimiter);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                phenotypeRepo.AddResult(patient, fields[5], fields[6], fields[7], fields[8], fields[9], DateTime.Parse(fields[10]));
            }
        }
    }
}
