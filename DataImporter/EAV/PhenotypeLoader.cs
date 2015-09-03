using MHGR.EAVModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.EAV
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
                var entityRepo = new EntityRepository();
                var fields = dataLine.Split(Delimiter);
                var source = sourceRepo.AddSource(fields[5], string.Empty);
                var file = AddResultFile(filePath, source);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                var attribute = EntityRepository.GetAttribute(fields[8], fields[9], fields[6], fields[7]);
                entityRepo.AddPhenotype(file, patient, attribute, DateTime.Parse(fields[10]));
            }
        }
    }
}
