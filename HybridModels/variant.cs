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
    
    public partial class variant
    {
        public int id { get; set; }
        public Nullable<int> gene_id { get; set; }
        public string external_id { get; set; }
        public string external_source { get; set; }
        public string chromosome { get; set; }
        public Nullable<int> start_position { get; set; }
        public Nullable<int> end_position { get; set; }
        public string reference_genome { get; set; }
        public string reference_bases { get; set; }
    
        public virtual gene gene { get; set; }
    }
}
