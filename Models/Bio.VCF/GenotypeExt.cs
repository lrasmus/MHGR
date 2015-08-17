using Bio.VCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Bio.VCF
{
    public static class GenotypeExt
    {
        public static string ToMHGRString(this Genotype genotype)
        {
            return string.Format("Name={0}:Genotype={1}:GQ={2}:DP={3}:AD={4}:PL={5}:Filters={6}",
                genotype.SampleName, genotype.GenotypeString, genotype.GQ, genotype.DP, genotype.AD, genotype.PL, genotype.Filters);
        }
    }
}
