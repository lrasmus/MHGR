//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MHGR.Models.Hybrid
{
    using System;
    using System.Collections.Generic;
    
    public partial class phenotype
    {
        public phenotype()
        {
            this.patient_phenotypes = new HashSet<patient_phenotypes>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string external_id { get; set; }
        public string external_source { get; set; }
    
        public virtual ICollection<patient_phenotypes> patient_phenotypes { get; set; }
    }
}