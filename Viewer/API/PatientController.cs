using MHGR.Models;
using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Viewer
{
    public class PatientController : ApiController
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

        // GET api/Patient/Search/query
        [HttpGet]
        public List<Patient> Search(string query)
        {
            return Repository.Search(query, 10);
        }
    }
}