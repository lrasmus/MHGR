using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Viewer.Models;

namespace Viewer.Controllers
{
    public class PatientController : Controller
    {
        private IPatientRepository PatientRepository = null;
        private IDerivedPhenotypeRepository PhenotypeRepository = null;

        public PatientController()
        {
            if (ConfigurationManager.AppSettings["Schema"].Equals("EAV", StringComparison.CurrentCultureIgnoreCase))
            {
                PatientRepository = new MHGR.EAVModels.PatientRepository();
                PhenotypeRepository = new MHGR.EAVModels.DerivedPhenotypeRepository();
            }
            else
            {
                PatientRepository = new MHGR.HybridModels.PatientRepository();
                PhenotypeRepository = new MHGR.HybridModels.DerivedPhenotypeRepository();
            }
        }

        // GET: Patient/Details/5
        public PartialViewResult Details(string id)
        {
            int patientId = int.Parse(id);
            var results = new PatientResults {
                Patient = PatientRepository.Get(patientId),
                Phenotypes = PhenotypeRepository.GetPhenotypes(patientId),
                StarPhenotypes = PhenotypeRepository.GetStarPhenotypes(patientId),
                SNPPhenotypes = PhenotypeRepository.GetSNPPhenotypes(patientId),
                VCFPhenotypes = PhenotypeRepository.GetVCFPhenotypes(patientId),
                GVFPhenotypes = PhenotypeRepository.GetGVFPhenotypes(patientId),
                Dosing = PhenotypeRepository.GetDosing(patientId)
            };
            return PartialView("Details", results);
        }

        public PartialViewResult Result(string source, string id)
        {
            int resultFileId = int.Parse(id);
            var resultDetails = new ResultDetails() { Phenotype = source, ResultFileId = resultFileId };
            return PartialView(resultDetails);
        }
    }
}
