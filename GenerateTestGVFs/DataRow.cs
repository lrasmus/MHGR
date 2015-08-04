using MHGR.Models.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTestGVFs
{
    public class DataRow
    {
        public string MRN { get; set; }
        public string MRNSource { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime ResultedOn { get; set; }
        public string Lab { get; set; }
        public List<VariantRepository.SnpResult> SNPs { get; set; }
    }
}
