using MHGR.Helpers;
using MHGR.Helpers.Generator;
using MHGR.Models.Hybrid;
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
            gvfLines.Add(string.Format("##individual-id Dbxref={0}:{1};First_name={2};Last_name={3};DOB={4};",
                dataRow.MRNSource, dataRow.MRN, dataRow.FirstName, dataRow.LastName, dataRow.ResultedOn.ToString("yyyy-MM-dd")));
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
                    snp.Chromosome, snp.Position, snp.Position, snp.RSID, Lookup.GetSNPReferenceValue(snp.RSID), GetVariant(snp)));
            }

            File.WriteAllLines(Path.Combine(ConfigurationManager.AppSettings["GVFDirectory"], dataRow.MRN + ".gvf"), gvfLines);
        }
    }
}
