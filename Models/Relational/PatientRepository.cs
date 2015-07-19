using MHGR.Models.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Relational
{
    public class PatientRepository
    {
        private RelationalEntities entities = new RelationalEntities();

        public patient Upsert(string mrn, string mrnSource, string firstName, string lastName, DateTime dateOfBirth)
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
    }
}
