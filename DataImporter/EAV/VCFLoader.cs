using Bio.VCF;
using MHGR.EAVModels;
using MHGR.Models.Bio.VCF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MHGR.DataImporter.EAV
{
    public class VCFLoader : BaseLoader
    {
        public EntityRepository entityRepo = new EntityRepository();
        public PatientRepository patientRepo = new PatientRepository();
        public SourceRepository sourceRepo = new SourceRepository();
        
        //private void SetVariantValues(patient_variants variant, VariantContext context)
        //{
        //    if (context.Genotypes.Count >= 1)
        //    {
        //        var alleles = context.Genotypes[0].Alleles;
        //        variant.value1 = alleles[0].DisplayString;
        //        if (alleles.Count > 1)
        //        {
        //            variant.value2 = alleles[1].DisplayString;
        //        }
        //        else
        //        {
        //            variant.value2 = variant.value1;
        //        }
        //    }
        //}

        private string CleanHeaderValue(string type, string value)
        {
            return value.Replace(string.Format("{0}=<", type), "").Replace(">", "");
        }

        public override void LoadData(string filePath)
        {
            var vcfParser = new VCFParser(filePath);
            var header = vcfParser.Header;
            var patient = new patient();

            var source = sourceRepo.AddSource("VCF", "VCF file");
            var file = AddResultFile(filePath, source);

            // Process the file-level pragmas
            result_entities rootEntity = new result_entities()
            {
                attribute_id = EntityRepository.GetAttribute(null, null, "Variant Call Format result", null).id,
                result_file_id = file.id
            };

            // We pull out all of the metadata from the header (all lines) and write them as information
            // lines associated with this result.
            var headerEntities = new List<result_entities>();
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
                else if (headerItem.GetType() == typeof(VCFInfoHeaderLine))
                {
                    var info = headerItem as VCFInfoHeaderLine;
                    var infoEntity = CreateEntityAttribute("INFO", 0, file.id, rootEntity, null);
                    headerEntities.Add(infoEntity);
                    headerEntities.Add(CreateEntityAttribute("ID", 0, file.id, infoEntity, info.ID));
                    headerEntities.Add(CreateEntityAttribute("Number", 0, file.id, infoEntity, info.CountType.ToString()));
                    headerEntities.Add(CreateEntityAttribute("Type", 0, file.id, infoEntity, info.Type.ToString()));
                    headerEntities.Add(CreateEntityAttribute("Description", 0, file.id, infoEntity, info.Description));
                }
                else if (headerItem.GetType() == typeof(VCFFilterHeaderLine))
                {
                    var filter = headerItem as VCFFilterHeaderLine;
                    var filterEntity = CreateEntityAttribute("FILTER", 0, file.id, rootEntity, null);
                    headerEntities.Add(filterEntity);
                    //headerEntities.Add(CreateEntityAttribute("ID", 0, file.id, filterEntity, filter.ID));
                    //headerEntities.Add(CreateEntityAttribute("Description", 0, file.id, filterEntity, filter.Value));
                    foreach (var field in filter.GenericFields())
                    {
                        headerEntities.Add(CreateEntityAttribute(field.Key, 0, file.id, filterEntity, field.Value));
                    }
                }
                else if (headerItem.GetType() == typeof(VCFFormatHeaderLine))
                {
                    var format = headerItem as VCFFormatHeaderLine;
                    var formatEntity = CreateEntityAttribute("FORMAT", 0, file.id, rootEntity, null);
                    headerEntities.Add(formatEntity);
                    headerEntities.Add(CreateEntityAttribute("ID", 0, file.id, formatEntity, format.ID));
                    headerEntities.Add(CreateEntityAttribute("Number", 0, file.id, formatEntity, format.CountType.ToString()));
                    headerEntities.Add(CreateEntityAttribute("Type", 0, file.id, formatEntity, format.Type.ToString()));
                    headerEntities.Add(CreateEntityAttribute("Description", 0, file.id, formatEntity, format.Description));
                }
                else
                {
                    //collectionInformationList.Add(AddHeaderInformation(string.Format("VCF:{0}", headerItem.Key), headerItem.Value));
                }
            }

            rootEntity.patient_id = patient.id;
            headerEntities.ForEach(x => x.patient_id = patient.id);

            entityRepo.AddVCF(rootEntity, headerEntities);

            //var reference = header.MetaDataInInputOrder.First(x => x.Key == "reference").Value;
            //DateTime? resultDate = DateTime.ParseExact(header.MetaDataInInputOrder.First(x => x.Key == "fileDate").Value,
            //    "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            //var patientVariants = new List<patient_variants>();
            //var featureInformationList = new Dictionary<patient_variants, List<patient_variant_information>>();
            //while (vcfParser.MoveNext())
            //{
            //    var current = vcfParser.Current;
            //    var variant = variantRepo.AddVariant(null, current.ID, "dbSNP",
            //        current.Chr, current.Start, current.End,
            //        reference, current.Reference.BaseString);
            //    var patientVariant = new patient_variants()
            //    {
            //        patient_id = patient.id,
            //        reference_id = variant.id,
            //        resulted_on = resultDate,
            //        variant_type = Enums.PatientVariantType.SNP
            //    };
            //    SetVariantValues(patientVariant, current);
            //    patientVariants.Add(patientVariant);

            //    var attributeList = new List<patient_variant_information>();
            //    foreach (var attribute in current.Attributes)
            //    {
            //        attributeList.Add(AddVariantInformation(string.Format("VCF:{0}", attribute.Key), attribute.Value.ToString()));
            //    }

            //    if (current.FiltersMaybeNull != null)
            //    {
            //        foreach (var filter in current.FiltersMaybeNull)
            //        {
            //            attributeList.Add(AddVariantInformation("VCF:Filter", filter));
            //        }
            //    }

            //    foreach (var genotype in current.Genotypes)
            //    {
            //        attributeList.Add(AddVariantInformation("VCF:Genotype", genotype.ToMHGRString()));
            //    }

            //    attributeList.Add(AddVariantInformation("VCF:Quality", current.PhredScaledQual.ToString()));
            //    attributeList.Add(AddVariantInformation("VCF:Filter", string.Join(",", current.Filters.ToArray())));
            //    featureInformationList.Add(patientVariant, attributeList);
            //}

            //// Save the collection to get its ID
            //
            //var collection = patientRepo.AddCollection(patient, file);

            //// Save the collection-level header data
            //collectionInformationList.ForEach(x => x.item_id = collection.id);
            //variantRepo.AddPatientVariantInformationList(collectionInformationList);
            //variantRepo.AddPatientVariants(patientVariants);

            //// Save the individual attributes associated with each feature.
            //// Must be done after the patient variants are written to DB (above), since we
            //// rely on the ID being set.
            //foreach (var pair in featureInformationList)
            //{
            //    foreach (var attribute in pair.Value)
            //    {
            //        attribute.item_id = pair.Key.id;
            //    }
            //    variantRepo.AddPatientVariantInformationList(pair.Value);
            //}

            //variantRepo.AddPatientVariantsToCollection(collection, patientVariants);

            //featureInformationList.Clear();
            //collectionInformationList.Clear();
            //patientVariants.Clear();
        }

        private result_entities CreateEntityAttribute(string attributeCode, int patientId, int fileId, result_entities parent, string value)
        {
            var attribute = EntityRepository.GetAttribute(attributeCode, "VCF", attributeCode, null);
            var entity = new result_entities()
            {
                attribute_id = attribute.id,
                patient_id = patientId,
                result_file_id = fileId,
                parent = parent,
            };
            SetEntityValue(entity, attribute, value);
            return entity;
        }
    }
}