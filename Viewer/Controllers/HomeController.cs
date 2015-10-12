using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Viewer.Controllers
{
    public class HomeController : Controller
    {
        private IPatientRepository Repository = null;

        public HomeController()
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
        
        public ActionResult Index()
        {
            return View();
        }
    }
}