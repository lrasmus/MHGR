using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Repository
{
    public interface IDerivedPhenotypeRepository
    {
        List<DerivedPhenotype> GetPhenotypes(string mrn);
        List<DerivedPhenotype> GetSNPPhenotypes(string mrn);
        List<DerivedPhenotype> GetStarPhenotypes(string mrn);
        List<DerivedPhenotype> GetVCFPhenotypes(string mrn);
        List<DerivedPhenotype> GetGVFPhenotypes(string mrn);
        List<DerivedPhenotype> GetDosing(string mrn);
    }
}
