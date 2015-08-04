using MHGR.Models.GVF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public class KeyValueParser
    {
        public const char TagDelimiter = ';';
        public const char TagKeyValueDelimiter = '=';

        public List<Tag> Parse(string line)
        {
            List<Tag> tags = new List<Tag>();
            string[] tagFields = line.Split(new[] { TagDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tagFields)
            {
                string[] tagParts = tag.Split(TagKeyValueDelimiter);
                tags.Add(new Tag()
                {
                    Name = tagParts[0],
                    Value = tagParts[1]
                });
            }

            return tags;
        }
    }
}
