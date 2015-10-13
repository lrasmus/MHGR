using MHGR.Models;
using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class PatientRepository : IPatientRepository
    {
        private EAVEntities entities = new EAVEntities();

        public patient AddPatient(string mrn, string mrnSource, string firstName, string lastName, DateTime? dateOfBirth)
        {
            var result = (from existingPatient in entities.patients
                         where existingPatient.external_source == mrnSource && existingPatient.external_id == mrn
                         select existingPatient).FirstOrDefault();
            if (result == null)
            {
                patient newPatient = new patient()
                {
                    external_source = mrnSource,
                    external_id = mrn,
                    date_of_birth = dateOfBirth,
                    first_name = firstName,
                    last_name = lastName
                };
                entities.patients.Add(newPatient);
                entities.SaveChanges();
                result = newPatient;
            }

            return result;
        }

        public List<Models.Patient> Search(string search, int? limit)
        {
            var query = (from pat in entities.patients
                    where pat.external_id.Contains(search) || pat.first_name.Contains(search) || pat.last_name.Contains(search)
                    select new Patient() { ID = pat.id, FirstName = pat.first_name, LastName = pat.last_name, MRN = pat.external_id });
            if (limit.HasValue)
            {
                return query.Take(limit.Value).ToList();
            }

            return query.ToList();
        }

        public Models.Patient Get(int id)
        {
            var query = (from pat in entities.patients
                         where pat.id == id
                         select new Patient() { ID = pat.id, FirstName = pat.first_name, LastName = pat.last_name, MRN = pat.external_id });
            return query.FirstOrDefault();
        }
    }
}
