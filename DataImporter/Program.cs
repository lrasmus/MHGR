using DataImporter.Relational;
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
            PhenotypeLoader.Run(phenotypeData);
            if (!PhenotypeLoader.ConsistencyChecks(17, 996, 5000, 5000, 5000))
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
