//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MHGR.Models.Relational
{
    using System;
    using System.Collections.Generic;
    
    public partial class gene
    {
        public gene()
        {
            this.variants = new HashSet<variant>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string external_id { get; set; }
        public string external_source { get; set; }
        public string chromosome { get; set; }
    
        public virtual ICollection<variant> variants { get; set; }
    }
}