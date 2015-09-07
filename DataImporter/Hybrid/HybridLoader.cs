using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public class HybridLoader
    {
        public static void Load()
        {
            DateTime timer = DateTime.Now;
            var phenotypeLoader = new PhenotypeLoader();
            phenotypeLoader.LoadData(ConfigurationManager.AppSettings["PhenotypeData"]);
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
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

            timer = DateTime.Now;
            var snpLoader = new SNPLoader();
            snpLoader.LoadReference(ConfigurationManager.AppSettings["SNPReferenceData"]);
            snpLoader.LoadData(ConfigurationManager.AppSettings["SNPData"]);
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            if (!snpLoader.ConsistencyChecks(1000, 18, 9, 32, 5000, 32000, 6000, 37000, 0, 0))
            {
                Console.WriteLine("FAILED - Results of the SNP load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for SNP data load");
            }

            timer = DateTime.Now;
            var starVariantLoader = new StarVariantLoader();
            starVariantLoader.LoadData(ConfigurationManager.AppSettings["StarVariantData"]);
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            if (!starVariantLoader.ConsistencyChecks(1000, 18, 9, 53, 5000, 35000, 7000, 40000, 0, 0))
            {
                Console.WriteLine("FAILED - Results of the star variant load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for star variant data load");
            }

            timer = DateTime.Now;
            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["GVFDataPath"], ConfigurationManager.AppSettings["GVFDataFilter"]);
            foreach (var file in files)
            {
                var gvfLoader = new GVFLoader();
                gvfLoader.LoadData(file);
            }
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));

            var gvfChecker = new GVFLoader();
            if (!gvfChecker.ConsistencyChecks(1000, 18, 9, 53, 5000, 67000, 8000, 72000, 199000, 25))
            {
                Console.WriteLine("FAILED - Results of the GVF load do not match internal consistency checks.");
                Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
                return;
            }
            else
            {
                Console.WriteLine("Passed - Consistency checks passed for GVF data load");
            }


            timer = DateTime.Now;
            files = Directory.GetFiles(ConfigurationManager.AppSettings["VCFDataPath"], ConfigurationManager.AppSettings["VCFDataFilter"]);
            foreach (var file in files)
            {
                var vcfLoader = new VCFLoader();
                vcfLoader.LoadData(file);
            }
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));

            var vcfChecker = new VCFLoader();
            if (!vcfChecker.ConsistencyChecks(1000, 18, 9, 54, 5000, 99000, 9000, 104000, 438000, 38))
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
