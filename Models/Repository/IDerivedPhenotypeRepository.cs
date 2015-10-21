using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Repository
{
    public interface IDerivedPhenotypeRepository
    {
        List<DerivedPhenotype> GetPhenotypes(int id);
        List<DerivedPhenotype> GetSNPPhenotypes(int id);
        List<DerivedPhenotype> GetStarPhenotypes(int id);
        List<DerivedPhenotype> GetVCFPhenotypes(int id);
        List<DerivedPhenotype> GetGVFPhenotypes(int id);
        List<DerivedPhenotype> GetDosing(int id);

        List<string> GetResultFileDetailsForPhenotype(string source, int fileId, string phenotype);
    }
}
