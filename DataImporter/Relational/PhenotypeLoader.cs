using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHGR.Models.Relational;
using System.Data.Entity;

namespace MHGR.DataImporter.Relational
{
    public class PhenotypeLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public override void LoadData(string[] data)
        {
            var patientRepo = new PatientRepository();
            var phenotypeRepo = new PhenotypeRepository();
            foreach (var dataLine in data.Skip(1))
            {
                var fields = dataLine.Split(Delimiter);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                phenotypeRepo.AddResult(patient, fields[5], fields[6], fields[7], fields[8], fields[9], DateTime.Parse(fields[10]));
            }
        }
        
        //public bool ConsistencyChecks(int phenotypes, int patients, int patientPhenotypes, int patientResultCollections, int patientResultMembers)
        //{
        //    var entities = new RelationalEntities();
        //    bool isValid = true;
        //    isValid = isValid && CheckEntityCounts(phenotypes, entities.phenotypes.Count(), "phenotypes");
        //    isValid = isValid && CheckEntityCounts(patients, entities.patients.Count(), "patients");
        //    isValid = isValid && CheckEntityCounts(patientPhenotypes, entities.patient_phenotypes.Count(), "patient phenotypes");
        //    isValid = isValid && CheckEntityCounts(patientResultCollections, entities.patient_result_collections.Count(), "patient result collections");
        //    isValid = isValid && CheckEntityCounts(patientResultMembers, entities.patient_result_members.Count(), "patient result members");

        //    return isValid;
        //}
    }
}
