﻿using System;
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
        }
    }
}
