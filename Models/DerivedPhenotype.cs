using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models
{
    public class DerivedPhenotype
    {
        public string ExternalId { get; set; }
        public string ExternalSource { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phenotype { get; set; }
        public string Value { get; set; }
        public string ResultedOn { get; set; }
    }
}
