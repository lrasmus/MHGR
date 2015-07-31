using MHGR.Models.Relational;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTestGVFs
{
    class Program
    {
        private const char Delimiter = '\t';

        static void Main(string[] args)
        {
            // Read in our list of patients and their corresponding randomization variables
            var data = File.ReadAllLines(ConfigurationManager.AppSettings["GVFList"]);
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
                    SNPs = new List<VariantRepository.SnpResult>()
                };
                
                for (int fieldIndex = 7; fieldIndex < 135; fieldIndex += 4)
                {
                    var snp = new VariantRepository.SnpResult()
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

        static string GetReference(string rsid)
        {
            switch (rsid)
            {
                case "rs12248560": return "C";
                case "rs28399504": return "A";
                case "rs41291556": return "T";
                case "rs72558184": return "G";
                case "rs4986893": return "G";
                case "rs4244285": return "G";
                case "rs72558186": return "T";
                case "rs56337013": return "C";
                case "rs17884712": return "G";
                case "rs6413438": return "C";
                case "rs1057910": return "A";
                case "rs1799853": return "C";
                case "rs9923231": return "C";
                case "rs9934438": return "G";
                case "rs8050894": return "C";
                case "rs6025": return "G";
                case "rs1799963": return "G";
                case "rs121913626": return "G";
                case "rs3218713": return "G";
                case "rs3218714": return "C";
                case "rs121964855": return "T";
                case "rs121964856": return "G";
                case "rs121964857": return "C";
                case "rs28934269": return "A";
                case "rs28934270": return "G";
                case "rs727504290": return "G";
                case "rs104894504": return "T";
                case "rs375882485": return "G";
                case "rs397516083": return "G";
                case "rs397515937": return "A";
                case "rs397516074": return "G";
                case "rs397515963": return "-";
            }

            return "UNKNOWN";
        }

        static string GetVariant(VariantRepository.SnpResult snp)
        {
            char[] variant = snp.Genotype.ToCharArray();
            if (variant[0] != variant[1])
            {
                return string.Format("Variant_seq={0},{1};Variant_reads=10,10;Genotype=heterozygous", variant[0], variant[1]);
            }
            else
            {
                return string.Format("Variant_seq={0};Variant_reads=10;Genotype=homozygous", variant[0]);
            }
        }

        static void GenerateGVF(DataRow dataRow)
        {
            List<string> gvfLines = new List<string>();
            gvfLines.Add("##gff-version 3");
            gvfLines.Add("##gvf-version 1.04");
            gvfLines.Add("##file-version 1.03");
            gvfLines.Add(string.Format("##file-date {0}", dataRow.ResultedOn.ToString("yyyy-MM-dd")));
            gvfLines.Add("");
            gvfLines.Add("# individual-description");
            gvfLines.Add(string.Format("# First_name={0};Last_name={1};DOB={2};", dataRow.FirstName, dataRow.LastName, dataRow.ResultedOn.ToString("yyyy-MM-dd")));
            gvfLines.Add("");
            gvfLines.Add(string.Format("##individual-id Dbxref={0}:{1};Display_name={2} {3};", dataRow.MRNSource, dataRow.MRN, dataRow.FirstName, dataRow.LastName));
            gvfLines.Add("##source-method Source=SOLiD;Type=SNV;Dbxref=http://tinyurl.com/AB-Genome-Data;Comment=SNPs were detected across the three genomes via a heuristic approach which considers the number of reads per allele as well as a score which weights the SNP calls based on the error profile of the reads;");
            gvfLines.Add("##source-method Source=SOLiD;Type=SNV;Dbxref=http://www.yandell-lab.org;Comment=Variants were converted their from original format to GVF by the Yandell Lab;");
            gvfLines.Add("##technology-platform Source=SOLiD;Type=SNV;Dbxref=http://solid.appliedbiosystems.com;Platform_class=short read sequencing;Platform_name=AB SOLiD;Read_type=pair,fragment;Read_length=25;Read_pair_span=600,3500;Average_coverage=26;");
            //gvfLines.Add("##phenotype-description Ontology=http://obofoundry.org/wiki/index.php/PATO:Main_Page;Term=female");
            gvfLines.Add("##feature-ontology http://sourceforge.net/projects/song/files/SO_Feature_Annotation/sofa_2_4_1/sofa_2_4_1.obo/download");
            gvfLines.Add("##genome-build GRCh38");
            gvfLines.Add("");
            gvfLines.Add("##sequence-region chr1  1 247249719");
            gvfLines.Add("##sequence-region chr10 1 135374737");
            gvfLines.Add("##sequence-region chr11 1 134452384");
            gvfLines.Add("##sequence-region chr14 1 106368585");
            gvfLines.Add("##sequence-region chr15 1 100338915");
            gvfLines.Add("##sequence-region chr16 1 88827254");
            gvfLines.Add("");

            foreach (var snp in dataRow.SNPs)
            {
                gvfLines.Add(string.Format("chr{0}\tSOLiD\tSNV\t{1}\t{2}\t.\t+\t.\tID={3};Reference_seq={4};{5}", 
                    snp.Chromosome, snp.Position, snp.Position, snp.RSID, GetReference(snp.RSID), GetVariant(snp)));
            }

            File.WriteAllLines(Path.Combine(ConfigurationManager.AppSettings["GVFDirectory"], dataRow.MRN + ".gvf"), gvfLines);
        }
    }
}
