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

        private void SetVariantValues(VariantContext context, int patientId, int fileId, result_entities parent, List<result_entities> variantEntities)
        {
            if (context.Genotypes.Count > 0)
            {
                var alleles = context.Genotypes[0].Alleles;
                foreach (var allele in alleles)
                {
                    variantEntities.Add(new result_entities()
                    {
                        patient_id = patientId,
                        result_file_id = fileId,
                        attribute_id = EntityRepository.GetAttribute(null, null, "Allele", null).id,
                        parent = parent,
                        value_short_text = allele.DisplayString
                    });
                }
            }
        }

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
                else if (headerItem.Key == "fileDate")
                {
                    DateTime resultDate = DateTime.ParseExact(headerItem.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    headerEntities.Add(CreateEntityAttribute("Resulted on", 0, file.id, rootEntity, resultDate.ToShortDateString()));
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
                    var headerEntity = CreateEntityAttribute(headerItem.Key, 0, file.id, rootEntity, headerItem.Value);
                    headerEntities.Add(headerEntity);
                }
            }

            rootEntity.patient_id = patient.id;
            headerEntities.ForEach(x => x.patient_id = patient.id);

            var variantEntities = new List<result_entities>();
            while (vcfParser.MoveNext())
            {
                var current = vcfParser.Current;
                var attribute = EntityRepository.GetAttribute(current.ID, "dbSNP", null, null);
                result_entities variantEntity = new result_entities()
                {
                    attribute_id = EntityRepository.GetAttribute(null, null, "Variant Call Format variant", null).id,
                    result_file_id = file.id,
                    patient_id = patient.id,
                    parent = rootEntity
                };
                variantEntities.Add(variantEntity);
                SetVariantValues(current, patient.id, file.id, variantEntity, variantEntities);

                variantEntities.Add(CreateEntityAttribute("Chromosome", patient.id, file.id, variantEntity, current.Chr));
                variantEntities.Add(CreateEntityAttribute("Start position", patient.id, file.id, variantEntity, current.Start.ToString()));
                variantEntities.Add(CreateEntityAttribute("End position", patient.id, file.id, variantEntity, current.End.ToString()));
                variantEntities.Add(CreateEntityAttribute("Reference base", patient.id, file.id, variantEntity, current.Reference.BaseString));
                variantEntities.Add(CreateEntityAttribute("Quality", patient.id, file.id, variantEntity, current.PhredScaledQual.ToString()));
                foreach (var attr in current.Attributes)
                {
                    variantEntities.Add(CreateEntityAttribute(string.Format("INFO:{0}", attr.Key), patient.id, file.id, variantEntity, attr.Value.ToString()));
                }

                if (current.FiltersMaybeNull != null)
                {
                    foreach (var filter in current.FiltersMaybeNull)
                    {
                        variantEntities.Add(CreateEntityAttribute(string.Format("FILTER:{0}", filter), patient.id, file.id, variantEntity, string.Empty));
                    }
                }

                //foreach (var genotype in current.Genotypes)
                //{
                //    attributeList.Add(AddVariantInformation("VCF:Genotype", genotype.ToMHGRString()));
                //}

                //attributeList.Add(AddVariantInformation("VCF:Quality", current.PhredScaledQual.ToString()));
                //attributeList.Add(AddVariantInformation("VCF:Filter", string.Join(",", current.Filters.ToArray())));
                //featureInformationList.Add(patientVariant, attributeList);
            }

            entityRepo.AddVCF(rootEntity, headerEntities, variantEntities);
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