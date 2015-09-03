using MHGR.Models.GVF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.GVF
{
    public class GVFParserHelper
    {
        public const char Delimiter = '\t';

        public enum RowType
        {
            Blank = 1,
            Pragma = 2,
            Comment = 3,
            Data = 4
        }

        public static RowType GetRowType(string row)
        {
            string normalizedRow = row.Trim();
            if (row.StartsWith("##"))
            {
                return RowType.Pragma;
            }
            else if (row.StartsWith("#"))
            {
                return RowType.Comment;
            }
            else if (string.IsNullOrEmpty(row))
            {
                return RowType.Blank;
            }

            return RowType.Data;
        }

        public static Feature ParseFeature(string data)
        {
            KeyValueParser kvParser = new KeyValueParser();
            string[] fields = data.Split(Delimiter);
            var feature = new Feature()
            {
                Chromosome = fields[0],
                Source = fields[1],
                Type = fields[2],
                StartPosition = int.Parse(fields[3]),
                EndPosition = int.Parse(fields[4]),
                Score = fields[5],
                Strand = fields[6],
                Phase = fields[7],
                Attributes = kvParser.Parse(fields[8])
            };

            return feature;
        }
    }
}
