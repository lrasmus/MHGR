//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EAVModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class attribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public attribute()
        {
            this.result_entities = new HashSet<result_entities>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string value_type { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<result_entities> result_entities { get; set; }
    }
}