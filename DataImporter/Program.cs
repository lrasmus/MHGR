using Bio.VCF;
using MHGR.DataImporter.EAV;
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
            HybridLoader.Load();
            EAVLoader.Load();
        }
    }
}
