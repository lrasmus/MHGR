using MHGR.Models.GVF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.GVF
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

            // The sequence region pragma has values without keys, so we are going to
            // add them here to enrich the data (instead of storing it as a flat string).
            if (pragma.Name == "sequence-region")
            {
                var sequenceParts = pragmaParts[1].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                pragmaParts = new string[2]
                {
                    pragma.Name,
                    string.Format("Chromosome={0};Start position={1};End position={2}", sequenceParts[0], sequenceParts[1], sequenceParts[2])
                };
            }

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
