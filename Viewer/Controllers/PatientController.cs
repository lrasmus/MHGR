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
            var results = new PatientResults {
                Patient = PatientRepository.Search(id, 1).FirstOrDefault(),
                Phenotypes = PhenotypeRepository.GetPhenotypes(id),
                StarPhenotypes = PhenotypeRepository.GetStarPhenotypes(id),
                SNPPhenotypes = PhenotypeRepository.GetSNPPhenotypes(id),
                VCFPhenotypes = PhenotypeRepository.GetVCFPhenotypes(id),
                GVFPhenotypes = PhenotypeRepository.GetGVFPhenotypes(id),
                Dosing = PhenotypeRepository.GetDosing(id)
            };
            return PartialView("Results", results);
        }
    }
}
