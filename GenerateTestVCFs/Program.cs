using MHGR.Helpers;
using MHGR.Helpers.Generator;
using MHGR.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTestVCFs
{
    class Program
    {
        private const char Delimiter = '\t';

        static void Main(string[] args)
        {
            // Read in our list of patients and their corresponding randomization variables
            var data = File.ReadAllLines(ConfigurationManager.AppSettings["VCFList"]);
            foreach (var dataLine in data.Skip(1))
            {
                var fields = dataLine.Split(Delimiter);
                var dataRow = new DataRow()
                {
                    MRN = fields[0],
                    MRNSource = fields[1],
                    FirstName = fields[2],
                    LastName = fields[3],
                    DOB = DateTime.Parse(fields[4]),
                    ResultedOn = DateTime.Parse(fields[5]),
                    Lab = fields[6],
                    SNPs = new List<SnpResult>()
                };

                for (int fieldIndex = 7; fieldIndex < 135; fieldIndex += 4)
                {
                    var snp = new SnpResult()
                    {
                        RSID = fields[fieldIndex],
                        Chromosome = fields[fieldIndex + 1],
                        Position = int.Parse(fields[fieldIndex + 2]),
                        Genotype = fields[fieldIndex + 3]
                    };
                    dataRow.SNPs.Add(snp);
                }

                GenerateGVF(dataRow);
            }
        }

        static private string FormatAlternateAlleles(string rsID, List<string> variants)
        {
            // Do a replacement of dashes (which we reserve in our input file for insertion
            // placeholders).
            string result = string.Join(",", variants).Replace("-", "");
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ".";
            }

            // For our special insertion scenario, the presence of "G" means an insertion, and to
            // follow VCF standard we prefix that insertion with the base before it ("G" in this
            // case).
            //if (rsID == "rs397515963")
            //{
            //    result = result.Replace("G", "GG");
            //}

            return result;
        }

        static private string FormatSampleGenotype(SnpResult snp, List<string> alleles, string reference)
        {
            string[] variants = snp.Genotype.ToArray().Select(x => x.ToString()).ToArray();
            List<string> map = new List<string>();
            foreach (var variant in variants)
            {
                if (snp.RSID == "rs397515963")
                {
                    if (variant == "-")
                    {
                        map.Add("0");
                    }
                    else
                    {
                        map.Add("1");
                    }
                }
                else
                {
                    if (variant == reference)
                    {
                        map.Add("0");
                    }
                    else
                    {
                        map.Add((alleles.IndexOf(variant) + 1).ToString());
                    }
                }
            }
            return string.Join("|", map);
        }

        static List<string> GetVariants(SnpResult snp, string reference)
        {
            string[] variants = snp.Genotype.ToArray().Select(x => x.ToString()).ToArray();
            List<string> results = new List<string>();
            foreach (var variant in variants)
            {
                // Special handling for the insertion SNP
                if (snp.RSID == "rs397515963")
                {
                    if (variant != "-" && !results.Contains("GG"))
                    {
                        results.Add("GG");
                    }
                }
                else if (variant != reference && !results.Contains(variant))
                {
                    results.Add(variant);
                }
            }
            return results;
        }

        static void GenerateGVF(DataRow dataRow)
        {
            List<string> vcfLines = new List<string>();

            vcfLines.Add("##fileformat=VCFv4.0");
            vcfLines.Add(string.Format("##fileDate={0}", dataRow.ResultedOn.ToString("yyyyMMdd")));
            vcfLines.Add("##reference=GRCh38");
            vcfLines.Add("##phasing=partial");
            vcfLines.Add(string.Format("##individual-id=<Dbxref={0}:{1},First_name={2},Last_name={3},DOB={4}>",
                dataRow.MRNSource, dataRow.MRN, dataRow.FirstName, dataRow.LastName, dataRow.ResultedOn.ToString("yyyy-MM-dd")));
            vcfLines.Add("##INFO=<ID=NS,Number=1,Type=Integer,Description=\"Number of Samples With Data\">");
            vcfLines.Add("##INFO=<ID=DP,Number=1,Type=Integer,Description=\"Total Depth\">");
            vcfLines.Add("##INFO=<ID=AF,Number=.,Type=Float,Description=\"Allele Frequency\">");
            vcfLines.Add("##INFO=<ID=AA,Number=1,Type=String,Description=\"Ancestral Allele\">");
            vcfLines.Add("##INFO=<ID=DB,Number=0,Type=Flag,Description=\"dbSNP membership, build 129\">");
            vcfLines.Add("##INFO=<ID=H2,Number=0,Type=Flag,Description=\"HapMap2 membership\">");
            vcfLines.Add("##FILTER=<ID=q10,Description=\"Quality below 10\">");
            vcfLines.Add("##FILTER=<ID=s50,Description=\"Less than 50% of samples have data\">");
            vcfLines.Add("##FORMAT=<ID=GQ,Number=1,Type=Integer,Description=\"Genotype Quality\">");
            vcfLines.Add("##FORMAT=<ID=GT,Number=1,Type=String,Description=\"Genotype\">");
            vcfLines.Add("##FORMAT=<ID=DP,Number=1,Type=Integer,Description=\"Read Depth\">");
            vcfLines.Add("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO\tFORMAT\tSample1");
            foreach (var snp in dataRow.SNPs)
            {
                string reference = Lookup.GetSNPReferenceValue(snp.RSID);

                // For insertions, we need to back up one position and provide the reference at that
                // location.
                if (snp.RSID == "rs397515963")
                {
                    reference = "G";
                    snp.Position -= 1;
                }
                List<string> alleles = GetVariants(snp, reference);
                vcfLines.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t50\tPASS\tNS=1;DP=10;AA={3};DB\tGT:GQ:DP\t{5}:50:10",
                    snp.Chromosome, snp.Position, snp.RSID, reference, FormatAlternateAlleles(snp.RSID, alleles),
                    FormatSampleGenotype(snp, alleles, reference)));
            }

            File.WriteAllLines(Path.Combine(ConfigurationManager.AppSettings["VCFDirectory"], dataRow.MRN + ".vcf"), vcfLines);
        }
    }
}
