﻿using MHGR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Helpers.Generator
{
    public class DataRow
    {
        public string MRN { get; set; }
        public string MRNSource { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime ResultedOn { get; set; }
        public string Lab { get; set; }
        public List<SnpResult> SNPs { get; set; }
    }
}
