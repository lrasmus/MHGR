//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MHGR.HybridModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class patient_variants
    {
        public int id { get; set; }
        public byte variant_type { get; set; }
        public int reference_id { get; set; }
        public int patient_id { get; set; }
        public string value1 { get; set; }
        public string value2 { get; set; }
        public Nullable<System.DateTime> resulted_on { get; set; }
    
        public virtual patient patient { get; set; }
    }
}
