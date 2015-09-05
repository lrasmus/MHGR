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
        /// <summary>
        /// The VCFSimpleHeaderLine class has a private KVP collection with
        /// all of its attributes, but doesn't provide members to make all of
        /// them public (specifically for the filter, we miss description).
        /// This extension exposes that private member so we can get the data.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static OrderedGenericDictionary<string, string> GenericFields(this VCFSimpleHeaderLine header)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = typeof(VCFSimpleHeaderLine);
            FieldInfo field = type.GetField("genericFields", flags);
            return (OrderedGenericDictionary<string, string>)field.GetValue(header);
        }
    }
}
