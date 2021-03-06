﻿using MHGR.HybridModels;
using MHGR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public class SNPLoader : BaseLoader
    {
        public const char Delimiter = '\t';

        public void LoadReference(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            var variantRepo = new VariantRepository();
            foreach (var dataLine in data.Skip(1))
            {
                var fields = dataLine.Split(Delimiter);
                variantRepo.AddVariant(fields[1], fields[5], "dbSNP", fields[2], int.Parse(fields[3]), int.Parse(fields[3]), fields[4], fields[7]);
            }
        }

        public override void LoadData(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            foreach (var dataLine in data.Skip(1))
            {
                var patientRepo = new PatientRepository();
                var variantRepo = new VariantRepository();
                var sourceRepo = new SourceRepository();

                var fields = dataLine.Split(Delimiter);
                var patient = patientRepo.AddPatient(fields[0], fields[1], fields[2], fields[3], DateTime.Parse(fields[4]));
                var resultedOn = DateTime.Parse(fields[5]);
                var lab = fields[6];
                var snps = new List<SnpResult>();
                for (int fieldIndex = 7; fieldIndex < 135; fieldIndex += 4)
                {
                    var snp = new SnpResult()
                    {
                        RSID = fields[fieldIndex],
                        Chromosome = fields[fieldIndex + 1],
                        Position = int.Parse(fields[fieldIndex + 2]),
                        Genotype = fields[fieldIndex + 3]
                    };
                    snps.Add(snp);
                }

                var source = sourceRepo.AddSource(lab, string.Empty);
                var file = AddResultFile(filePath, source);
                variantRepo.AddSnps(patient, file, resultedOn, snps);
            }
        }
    }
}
