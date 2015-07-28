using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.Models.GVF
{
    public class Pragma
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<Tag> Tags { get; set; }

        public Pragma()
        {
            Tags = new List<Tag>();
        }
    }
}
