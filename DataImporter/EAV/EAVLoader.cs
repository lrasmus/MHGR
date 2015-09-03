using System;
using System.Collections.Generic;
using System.Configuration;
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
            //var phenotypeLoader = new PhenotypeLoader();
            //phenotypeLoader.LoadData(ConfigurationManager.AppSettings["PhenotypeData"]);
            //Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            //if (!phenotypeLoader.ConsistencyChecks(996, 10000, 5000, 3))
            //{
            //    Console.WriteLine("FAILED - Results of the phenotype load do not match internal consistency checks.");
            //    Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("Passed - Consistency checks passed for phenotype data load");
            //}

            //timer = DateTime.Now;
            //var snpLoader = new SNPLoader();
            //snpLoader.LoadData(ConfigurationManager.AppSettings["SNPData"]);
            //Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            //if (!snpLoader.ConsistencyChecks(1000, 74000, 5001, 3))
            //{
            //    Console.WriteLine("FAILED - Results of the SNP load do not match internal consistency checks.");
            //    Console.WriteLine("         Please resolve issues before proceeding with other data loads.");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("Passed - Consistency checks passed for SNP data load");
            //}

            timer = DateTime.Now;
            var starVariantLoader = new StarVariantLoader();
            starVariantLoader.LoadData(ConfigurationManager.AppSettings["StarVariantData"]);
            Console.WriteLine(string.Format("Load took {0} seconds", (DateTime.Now - timer).TotalSeconds));
            if (!starVariantLoader.ConsistencyChecks(1000, 86000, 10001, 3))
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
