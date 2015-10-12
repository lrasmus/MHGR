using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Viewer.Controllers
{
    public class PatientController : Controller
    {
        private IPatientRepository Repository = null;

        public PatientController()
        {
            if (ConfigurationManager.AppSettings["Schema"].Equals("EAV", StringComparison.CurrentCultureIgnoreCase))
            {
                Repository = new MHGR.EAVModels.PatientRepository();
            }
            else
            {
                Repository = new MHGR.HybridModels.PatientRepository();
            }
        }

        // GET: Patient/Details/5
        public PartialViewResult Details(string id)
        {
            var patient = Repository.Search(id, 1).FirstOrDefault();
            return PartialView("Results", patient);
        }
    }
}
