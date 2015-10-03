using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.EAV
{
    public class EAVLoader
    {
        public static void Load()
        {
            DateTime timer = DateTime.Now;
            var phenotypeLoader = new PhenotypeLoader();
            phenotypeLoader.LoadData(ConfigurationManager.AppSettings["PhenotypeData"]);
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            if (!phenotypeLoader.ConsistencyChecks(996, 10000, 5000, 3))
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
            if (!snpLoader.ConsistencyChecks(1000, 138000, 6000, 3))
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
            if (!starVariantLoader.ConsistencyChecks(1000, 150000, 7000, 3))
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
            // Result entity count isn't a round number anymore because we have a variable number of
            // results per variant per patient (e.g. homozygous vs. heterozygous).
            if (!gvfChecker.ConsistencyChecks(1000, 657159, 8000, 4))
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
            if (!vcfChecker.ConsistencyChecks(1000, 1097159, 9000, 5))
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
