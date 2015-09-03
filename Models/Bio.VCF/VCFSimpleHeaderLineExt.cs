using Bio.VCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.Bio.VCF
{
    public static class VCFSimpleHeaderLineExt
    {
        public static OrderedGenericDictionary<string, string> GenericFields(this VCFSimpleHeaderLine header)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = typeof(VCFSimpleHeaderLine);
            FieldInfo field = type.GetField("genericFields", flags);
            return (OrderedGenericDictionary<string, string>)field.GetValue(header);
        }
    }
}
