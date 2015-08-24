﻿using MHGR.HybridModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.HybridModels
{
    public class PatientRepository
    {
        private HybridEntities entities = new HybridEntities();

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

        public patient_result_collections AddCollection(patient patient, result_files file)
        {
            // Add the patient result collection for this entry
            var collection = new patient_result_collections()
            {
                patient_id = patient.id,
                received_on = DateTime.Now,
                result_file_id = file.id
            };
            entities.patient_result_collections.Add(collection);
            entities.SaveChanges();
            return collection;
        }
    }
}