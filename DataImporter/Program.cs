using MHGR.DataImporter.Relational;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var phenotypeData = File.ReadAllLines(ConfigurationManager.AppSettings["PhenotypeData"]);
            var phenotypeLoader = new PhenotypeLoader();
            phenotypeLoader.LoadData(phenotypeData);
            if (!phenotypeLoader.ConsistencyChecks(18, 996, 5000, 5000, 5000))
            {
                Console.WriteLine("FAILED - Results of the phenotype load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for phenotype data load");
            }

            var snpLoader = new SNPLoader();
            var snpReferenceData = File.ReadAllLines(ConfigurationManager.AppSettings["SNPReferenceData"]);
            snpLoader.LoadReference(snpReferenceData);

            var snpData = File.ReadAllLines(ConfigurationManager.AppSettings["SNPData"]);
            snpLoader.LoadData(snpData);
            if (!snpLoader.ConsistencyChecks(1000, 8, 32, 32000, 6000, 37000))
            {
                Console.WriteLine("FAILED - Results of the SNP load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for SNP data load");
            }

            var starVariantLoader = new StarVariantLoader();
            var starVariantData = File.ReadAllLines(ConfigurationManager.AppSettings["StarVariantData"]);
            starVariantLoader.LoadData(starVariantData);
            if (!starVariantLoader.ConsistencyChecks(1000, 8, 53, 35000, 7000, 40000))
            {
                Console.WriteLine("FAILED - Results of the star variant load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for star variant data load");
            }
        }
    }
}
