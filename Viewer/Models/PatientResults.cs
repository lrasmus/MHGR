using MHGR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Viewer.Models
{
    public class PatientResults
    {
        public Patient Patient { get; set; }
        public List<DerivedPhenotype> Phenotypes { get; set; }
        public List<DerivedPhenotype> SNPPhenotypes { get; set; }
        public List<DerivedPhenotype> StarPhenotypes { get; set; }
        public List<DerivedPhenotype> VCFPhenotypes { get; set; }
        public List<DerivedPhenotype> GVFPhenotypes { get; set; }
        public List<DerivedPhenotype> Dosing { get; set; }
    }
}