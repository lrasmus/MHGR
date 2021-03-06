﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class HybridEntities : DbContext
    {
        public HybridEntities()
            : base("name=HybridEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<gene> genes { get; set; }
        public virtual DbSet<patient_phenotypes> patient_phenotypes { get; set; }
        public virtual DbSet<patient_result_collections> patient_result_collections { get; set; }
        public virtual DbSet<patient_result_members> patient_result_members { get; set; }
        public virtual DbSet<patient_variant_information> patient_variant_information { get; set; }
        public virtual DbSet<patient_variants> patient_variants { get; set; }
        public virtual DbSet<patient> patients { get; set; }
        public virtual DbSet<phenotype> phenotypes { get; set; }
        public virtual DbSet<result_files> result_files { get; set; }
        public virtual DbSet<result_sources> result_sources { get; set; }
        public virtual DbSet<variant_information_types> variant_information_types { get; set; }
        public virtual DbSet<variant> variants { get; set; }
    }
}
