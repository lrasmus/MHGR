using MHGR.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MHGR.Models
{
    public class Patient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        public string Display
        {
            get { return string.Format("{0} {1} ({2})", FirstName, LastName, MRN);  }
        }

        public Patient()
        {
        }
    }
}