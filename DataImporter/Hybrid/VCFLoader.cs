﻿using Bio.VCF;
using MHGR.Models.Hybrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public class VCFLoader : BaseLoader
    {
        public VariantRepository variantRepo = new VariantRepository();
        public PatientRepository patientRepo = new PatientRepository();
        public SourceRepository sourceRepo = new SourceRepository();

        public override void LoadData(string[] data)
        {
            throw new NotImplementedException("This method is not implemented, please pass a file path to LoadData");
        }

        private patient_variant_information AddFeatureInformation(string attributeName, string attributeValue)
        {
            var infoType = variantRepo.AddVariantInformationType(attributeName, null, Enums.VariantInformationTypeSource.VCF);
            return new patient_variant_information()
            {
                item_type = Enums.PatientVariantType.SNP,
                type_id = infoType.id,
                value = attributeValue
            };
        }

        private void SetVariantValues(patient_variants variant, VariantContext context)
        {
            if (context.Genotypes.Count >= 1)
            {
                var alleles = context.Genotypes[0].Alleles;
                variant.value1 = alleles[0].DisplayString;
                if (alleles.Count > 1)
                {
                    variant.value2 = alleles[1].DisplayString;
                }
                else
                {
                    variant.value2 = variant.value1;
                }
            }
            //if (context.AlternateAlleles.Count >= 1)
            //{
            //    variant.value1 = context.AlternateAlleles[0].DisplayString;
            //    if (context.AlternateAlleles.Count >= 2)
            //    {
            //        variant.value2 = context.AlternateAlleles[1].DisplayString;
            //    }
            //    else
            //    {
            //        variant.value2 = variant.value1;
            //    }
            //}
        }

        private patient_variant_information AddPragmaInformation(string pragmaName, string pragmaValue)
        {
            var infoType = variantRepo.AddVariantInformationType(pragmaName, null, Enums.VariantInformationTypeSource.VCF);
            return new patient_variant_information()
            {
                item_type = Enums.PatientVariantType.Collection,
                type_id = infoType.id,
                value = pragmaValue
            };
        }

        public void LoadData(string filePath)
        {
            var vcfParser = new VCFParser(filePath);
            var header = vcfParser.Header;
            var collectionInformationList = new List<patient_variant_information>();
            var patient = new patient();
            // We pull out all of the metadata from the header (all lines) and write them as information
            // lines associated with this result.
            foreach (var headerItem in header.MetaDataInInputOrder)
            {
                if (headerItem.Key == "individual-id")
                {
                    var individualParts = headerItem.Value.Replace("<", "").Replace(">", "").Split(new char[] { ',' });
                    var individualData = individualParts.Select(x => x.Split(new char[] { '=' })).ToArray();
                    var mrnParts = individualData.FirstOrDefault(x => x[0] == "Dbxref")[1].Split(':');
                    patient = patientRepo.AddPatient(mrnParts[1], mrnParts[0],
                        individualData.FirstOrDefault(x => x[0] == "First_name")[1],
                        individualData.FirstOrDefault(x => x[0] == "Last_name")[1],
                        DateTime.Parse(individualData.FirstOrDefault(x => x[0] == "DOB")[1]));
                }
                else
                {
                    collectionInformationList.Add(AddPragmaInformation(string.Format("VCF:{0}", headerItem.Key), headerItem.Value));
                }
            }

            var reference = header.MetaDataInInputOrder.First(x => x.Key == "reference").Value;
            DateTime? resultDate = DateTime.ParseExact(header.MetaDataInInputOrder.First(x => x.Key == "fileDate").Value,
                "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            var patientVariants = new List<patient_variants>();
            var featureInformationList = new Dictionary<patient_variants, List<patient_variant_information>>();
            while (vcfParser.MoveNext())
            {
                var current = vcfParser.Current;
                var variant = variantRepo.AddVariant(null, current.ID, "dbSNP",
                    current.Chr, current.Start, current.End,
                    reference, current.Reference.BaseString);
                var patientVariant = new patient_variants()
                {
                    patient_id = patient.id,
                    reference_id = variant.id,
                    resulted_on = resultDate,
                    variant_type = Enums.PatientVariantType.SNP
                };
                SetVariantValues(patientVariant, current);
                patientVariants.Add(patientVariant);

                var attributeList = new List<patient_variant_information>();
                foreach (var attribute in current.Attributes)
                {
                    attributeList.Add(AddFeatureInformation(string.Format("VCF:{0}", attribute.Key), attribute.Value.ToString()));
                }

                if (current.FiltersMaybeNull != null)
                {
                    foreach (var filter in current.FiltersMaybeNull)
                    {
                        attributeList.Add(AddFeatureInformation("VCF:Filter", filter));
                    }
                }

                foreach (var genotype in current.Genotypes)
                {
                    attributeList.Add(AddFeatureInformation("VCF:Genotype", genotype.ToString()));
                }
                featureInformationList.Add(patientVariant, attributeList);
            }

            // Save the collection to get its ID
            var source = sourceRepo.AddSource("VCF", "VCF file");
            var collection = patientRepo.AddCollection(patient, source);

            // Save the collection-level header data
            collectionInformationList.ForEach(x => x.item_id = collection.id);
            variantRepo.AddPatientVariantInformationList(collectionInformationList);
            variantRepo.AddPatientVariants(patientVariants);

            // Save the individual attributes associated with each feature.
            // Must be done after the patient variants are written to DB (above), since we
            // rely on the ID being set.
            foreach (var pair in featureInformationList)
            {
                foreach (var attribute in pair.Value)
                {
                    attribute.item_id = pair.Key.id;
                }
                variantRepo.AddPatientVariantInformationList(pair.Value);
            }

            variantRepo.AddPatientVariantsToCollection(collection, patientVariants);
        }
    }
}
