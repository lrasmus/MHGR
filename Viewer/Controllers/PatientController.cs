using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Viewer.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient/Details/5
        public PartialViewResult Details(int id)
        {
            return PartialView("Results");
        }
    }
}
