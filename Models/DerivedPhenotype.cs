using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models
{
    public class DerivedPhenotype
    {
        public int ResultFileId { get; set; }
        public string Phenotype { get; set; }
        public string Value { get; set; }
        public string ResultedOn { get; set; }
    }
}
