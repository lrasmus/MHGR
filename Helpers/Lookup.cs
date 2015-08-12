using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Helpers
{
    public class Lookup
    {
        public static string GetSNPReferenceValue(string rsid)
        {
            switch (rsid)
            {
                case "rs12248560": return "C";
                case "rs28399504": return "A";
                case "rs41291556": return "T";
                case "rs72558184": return "G";
                case "rs4986893": return "G";
                case "rs4244285": return "G";
                case "rs72558186": return "T";
                case "rs56337013": return "C";
                case "rs17884712": return "G";
                case "rs6413438": return "C";
                case "rs1057910": return "A";
                case "rs1799853": return "C";
                case "rs9923231": return "C";
                case "rs9934438": return "G";
                case "rs8050894": return "C";
                case "rs6025": return "G";
                case "rs1799963": return "G";
                case "rs121913626": return "G";
                case "rs3218713": return "G";
                case "rs3218714": return "C";
                case "rs121964855": return "T";
                case "rs121964856": return "G";
                case "rs121964857": return "C";
                case "rs28934269": return "A";
                case "rs28934270": return "G";
                case "rs727504290": return "G";
                case "rs104894504": return "T";
                case "rs375882485": return "G";
                case "rs397516083": return "G";
                case "rs397515937": return "A";
                case "rs397516074": return "G";
                case "rs397515963": return "-";
            }

            return "UNKNOWN";
        }
    }
}
