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
        }
    }
}
