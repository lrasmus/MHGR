using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Repository
{
    public abstract class BaseDerivedPhenotypeRepository : IDerivedPhenotypeRepository
    {
        public abstract List<string> GetResultFileDetailsForPhenotype(string source, int fileId, string phenotype);
        public abstract int[] GetGeneIdListForGeneNames(string[] genes);

        public abstract List<DerivedPhenotype> GetDosing(int id);

        public abstract List<DerivedPhenotype> GetGVFPhenotypes(int id);

        public abstract List<DerivedPhenotype> GetPhenotypes(int id);

        public abstract List<DerivedPhenotype> GetSNPPhenotypes(int id);

        public abstract List<DerivedPhenotype> GetStarPhenotypes(int id);

        public abstract List<DerivedPhenotype> GetVCFPhenotypes(int id);

        protected int[] GetGeneFilterForPhenotype(string phenotype)
        {
            switch (phenotype)
            {
                case "Clopidogrel metabolism":
                    return GetGeneIdListForGeneNames(new[] { "CYP2C19" });
                case "Warfarin metabolism":
                case "Warfarin dosing range":
                    return GetGeneIdListForGeneNames(new[] { "CYP2C9", "VKORC1" });
                case "Familial Thrombophilia":
                    return GetGeneIdListForGeneNames(new[] { "F2", "F5" });
                case "Hypertrophic Cardiomyopathy":
                    return GetGeneIdListForGeneNames(new[] { "MYH7", "TNNT2", "TPM1", "MYBPC3" });
                default:
                    return null;
            }
        }
    }
}
