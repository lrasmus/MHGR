using MHGR.Models.GVF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Relational
{
    public class PragmaParser
    {
        public const string Prefix = "##";

        public Pragma Parse(string line)
        {
            string[] pragmaParts = line.Trim().Replace(Prefix, "").Split(new char[] { ' ' }, 2);
            Pragma pragma = new Pragma() {
                Name = pragmaParts[0]
            };

            // Some pragmas are just a string value.  We can identify those by their lack of a delimiter
            // for a tag key-value pair.
            if (!pragmaParts[1].Contains(KeyValueParser.TagKeyValueDelimiter))
            {
                pragma.Value = pragmaParts[1];
                return pragma;
            }

            KeyValueParser kvParser = new KeyValueParser();
            pragma.Tags.AddRange(kvParser.Parse(pragmaParts[1]));
            return pragma;
        }
    }
}
