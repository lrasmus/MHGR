using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Relational
{
    public class Enums
    {
        public class ResultMemberType
        {
            public const byte Phenotype = 1;
            public const byte Variant = 2;
        }

        public class PatientVariantType
        {
            public const byte SNP = 1;
        }
    }
}
