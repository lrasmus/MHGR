using Bio.VCF;
using MHGR.DataImporter.Hybrid;
using System;
using System.Configuration;
using System.IO;

namespace DataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var phenotypeLoader = new PhenotypeLoader();
            phenotypeLoader.LoadData(ConfigurationManager.AppSettings["PhenotypeData"]);
            if (!phenotypeLoader.ConsistencyChecks(996, 18, 0, 0, 5000, 0, 5000, 5000, 0, 0))
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
            snpLoader.LoadReference(ConfigurationManager.AppSettings["SNPReferenceData"]);
            
            snpLoader.LoadData(ConfigurationManager.AppSettings["SNPData"]);
            if (!snpLoader.ConsistencyChecks(1000, 18, 8, 32, 5000, 32000, 6000, 37000, 0, 0))
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
            starVariantLoader.LoadData(ConfigurationManager.AppSettings["StarVariantData"]);
            if (!starVariantLoader.ConsistencyChecks(1000, 18, 8, 53, 5000, 35000, 7000, 40000, 0, 0))
            {
                Console.WriteLine("FAILED - Results of the star variant load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for star variant data load");
            }

            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["GVFDataPath"], ConfigurationManager.AppSettings["GVFDataFilter"]);
            foreach (var file in files)
            {
                var gvfLoader = new GVFLoader();
                gvfLoader.LoadData(file);
            }

            var gvfChecker = new GVFLoader();
            if (!gvfChecker.ConsistencyChecks(1000, 18, 8, 53, 5000, 67000, 8000, 72000, 187000, 23))
            {
                Console.WriteLine("FAILED - Results of the GVF load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for GVF data load");
            }

            files = Directory.GetFiles(ConfigurationManager.AppSettings["VCFDataPath"], ConfigurationManager.AppSettings["VCFDataFilter"]);
            foreach (var file in files)
            {
                var vcfLoader = new VCFLoader();
                vcfLoader.LoadData(file);
            }

            var vcfChecker = new VCFLoader();
            if (!vcfChecker.ConsistencyChecks(1000, 18, 8, 54, 5000, 99000, 9000, 104000, 426000, 36))
            {
                Console.WriteLine("FAILED - Results of the VCF load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for VCF data load");
            }
        }
    }
}
