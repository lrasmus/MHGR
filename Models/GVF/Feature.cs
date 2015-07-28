using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.GVF
{
    public class Feature
    {
        public string SequenceId { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string Score { get; set; }
        public string Strand { get; set; }
        public string Phase { get; set; }
        public List<Tag> Attributes { get; set; }
    }
}
