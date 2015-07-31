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

                List<VariantRepository.SnpResult> snps = new List<VariantRepository.SnpResult>();
                for (int fieldIndex = 7; fieldIndex < 135; fieldIndex += 4)
                {
                    var snp = new VariantRepository.SnpResult()
                    {
                        RSID = fields[fieldIndex],
                        Chromosome = fields[fieldIndex + 1],
                        Position = int.Parse(fields[fieldIndex + 2]),
                        Genotype = fields[fieldIndex + 3]
                    };
                    snps.Add(snp);
                }

                GenerateGVF(dataRow);
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
            gvfLines.Add("# Female Yoruban from Ibadan, Nigeria; all four grandparents are Yoruba");
            gvfLines.Add("");
            gvfLines.Add("##individual-id Dbxref=Coriell:NA19240,PMID:20796305;Population=Yoruba;Comment=International HAPMAP project - Yoruba in Ibadan%2C Nigeria (Plate I);Display_name=NA19240;");
            gvfLines.Add("##source-method Source=SOLiD;Type=SNV;Dbxref=http://tinyurl.com/AB-Genome-Data;Comment=SNPs were detected across the three genomes via a heuristic approach which considers the number of reads per allele as well as a score which weights the SNP calls based on the error profile of the reads;");
            gvfLines.Add("##source-method Source=SOLiD;Type=SNV;Dbxref=http://www.yandell-lab.org;Comment=Variants were converted their from original format to GVF by the Yandell Lab;");
            gvfLines.Add("##technology-platform Source=SOLiD;Type=SNV;Dbxref=http://solid.appliedbiosystems.com;Platform_class=short read sequencing;Platform_name=AB SOLiD;Read_type=pair,fragment;Read_length=25;Read_pair_span=600,3500;Average_coverage=26;");
            gvfLines.Add("##phenotype-description Ontology=http://obofoundry.org/wiki/index.php/PATO:Main_Page;Term=female");
            gvfLines.Add("##feature-ontology http://sourceforge.net/projects/song/files/SO_Feature_Annotation/sofa_2_4_1/sofa_2_4_1.obo/download");
            gvfLines.Add("##genome-build NCBI B36");
            gvfLines.Add("");
            gvfLines.Add("##sequence-region chr1  1 247249719");
            gvfLines.Add("##sequence-region chr2  1 242951149");
            gvfLines.Add("##sequence-region chr3  1 199501827");
            gvfLines.Add("##sequence-region chr4  1 191273063");
            gvfLines.Add("##sequence-region chr5  1 180857866");
            gvfLines.Add("##sequence-region chr6  1 170899992");
            gvfLines.Add("##sequence-region chr7  1 158821424");
            gvfLines.Add("##sequence-region chr8  1 146274826");
            gvfLines.Add("##sequence-region chr9  1 140273252");
            gvfLines.Add("##sequence-region chr10 1 135374737");
            gvfLines.Add("##sequence-region chr11 1 134452384");
            gvfLines.Add("##sequence-region chr12 1 132349534");
            gvfLines.Add("##sequence-region chr13 1 114142980");
            gvfLines.Add("##sequence-region chr14 1 106368585");
            gvfLines.Add("##sequence-region chr15 1 100338915");
            gvfLines.Add("##sequence-region chr16 1 88827254");
            gvfLines.Add("##sequence-region chr17 1 78774742");
            gvfLines.Add("##sequence-region chr18 1 76117153");
            gvfLines.Add("##sequence-region chr19 1 63811651");
            gvfLines.Add("##sequence-region chr20 1 62435964");
            gvfLines.Add("##sequence-region chr21 1 46944323");
            gvfLines.Add("##sequence-region chr22 1 49691432");

            File.WriteAllLines(Path.Combine(ConfigurationManager.AppSettings["GVFDirectory"], dataRow.MRN + ".gvf"), gvfLines);
        }
    }
}
