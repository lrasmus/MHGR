using MHGR.DataImporter.GVF;
using MHGR.EAVModels;
using MHGR.Models.GVF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.EAV
{
    public class GVFLoader : BaseLoader
    {
        public const char Delimiter = '\t';
        public char[] CommentTrimChars = new[] { ' ', '\t', '\r', '\n', '#' };
        private PragmaParser pragmaParser = new PragmaParser();
        private PatientRepository patientRepo = new PatientRepository();
        private EntityRepository entityRepo = new EntityRepository();
        private SourceRepository sourceRepo = new SourceRepository();

        public override void LoadData(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            List<Pragma> pragmas = new List<Pragma>();
            List<string> comments = new List<string>();
            List<Feature> features = new List<Feature>();
            foreach (var row in data)
            {
                switch (GVFParserHelper.GetRowType(row))
                {
                    case GVFParserHelper.RowType.Pragma:
                        {
                            pragmas.Add(pragmaParser.Parse(row));
                            break;
                        }
                    case GVFParserHelper.RowType.Comment:
                        {
                            comments.Add(row.Trim(CommentTrimChars));
                            break;
                        }
                    case GVFParserHelper.RowType.Data:
                        {
                            features.Add(GVFParserHelper.ParseFeature(row));
                            break;
                        }
                }
            }

            var source = sourceRepo.AddSource("GVF", "GVF file");
            var file = AddResultFile(filePath, source);
            patient patient = null;
            //string genomeBuild = null;
            //DateTime? resultDate = null;

            // Process the file-level pragmas
            result_entities rootEntity = new result_entities()
            {
                attribute_id = EntityRepository.GetAttribute(null, null, "Genomic Variant Format result", null).id,
                result_file_id = file.id
            };

            var pragmaEntities = new List<result_entities>();
            foreach (var pragma in pragmas)
            {
                if (pragma.Name == "individual-id")
                {
                    var mrnParts = pragma.Tags.FirstOrDefault(x => x.Name == "Dbxref").Value.Split(':');
                    patient = patientRepo.AddPatient(mrnParts[1], mrnParts[0],
                        pragma.Tags.FirstOrDefault(x => x.Name == "First_name").Value,
                        pragma.Tags.FirstOrDefault(x => x.Name == "Last_name").Value,
                        DateTime.Parse(pragma.Tags.FirstOrDefault(x => x.Name == "DOB").Value));
                }
                //else if (pragma.Name == "phenotype-description")
                //{
                //    var phenotype = phenotypeRepo.GetPhenotypeByExternalId(CreatePhenotype(pragma));
                //}
                //else if (pragma.Name == "genome-build")
                //{
                //    genomeBuild = pragma.Value;
                //}
                else if (pragma.Name == "file-date")
                {
                    pragmaEntities.Add(new result_entities()
                    {
                        attribute_id = EntityRepository.GetAttribute(null, null, "Resulted on", null).id,
                        result_file_id = file.id,
                        value_date_time = DateTime.Parse(pragma.Value),
                        parent = rootEntity
                    });
                }
                else
                {
                    var pragmaEntity = new result_entities()
                    {
                        attribute_id = EntityRepository.GetAttribute(pragma.Name, "GVF", null, null).id,
                        result_file_id = file.id,
                        parent = rootEntity
                    };
                    pragmaEntities.Add(pragmaEntity);

                    if (pragma.Tags.Count > 0)
                    {
                        foreach (var tag in pragma.Tags)
                        {
                            var attribute = EntityRepository.GetAttribute(tag.Name, "GVF", tag.Name, null);
                            var tagEntity = new result_entities()
                            {
                                attribute_id = attribute.id,
                                result_file_id = file.id,
                                parent = pragmaEntity,
                            };
                            SetEntityValue(tagEntity, attribute, tag.Value);
                            pragmaEntities.Add(tagEntity);
                        }
                    }
                    else
                    {
                        pragmaEntity.value_short_text = pragma.Value;
                        //collectionInformationList.Add(AddPragmaInformation(string.Format("GVF:{0}", pragma.Name), pragma.Value));
                    }
                }
            }


            // Convert all comments into individual variant information entries
            foreach (var comment in comments)
            {
                pragmaEntities.Add(new result_entities()
                {
                    attribute_id = EntityRepository.GetAttribute("Comment", "GVF", "Comment", null).id,
                    result_file_id = file.id,
                    parent = rootEntity,
                    value_text = comment
                });
            }

            rootEntity.patient_id = patient.id;
            pragmaEntities.ForEach(x => x.patient_id = patient.id);

            var featureEntities = new List<result_entities>();
            foreach (var feature in features)
            {
                var gvfEntity = new result_entities()
                {
                    attribute_id = EntityRepository.GetAttribute(null, null, "Genomic Variant Format feature", null).id,
                    patient_id = patient.id,
                    result_file_id = file.id,
                    parent = rootEntity
                };
                featureEntities.Add(gvfEntity);

                featureEntities.Add(new result_entities()
                {
                    attribute_id = EntityRepository.GetAttribute("Chromosome", "GVF", null, null).id,
                    patient_id = patient.id,
                    result_file_id = file.id,
                    parent = gvfEntity,
                    value_short_text = feature.Chromosome
                });

                featureEntities.Add(AddFeatureAttribute("Source", patient.id, file.id, gvfEntity, feature.Source));
                featureEntities.Add(AddFeatureAttribute("Type", patient.id, file.id, gvfEntity, feature.Type));
                featureEntities.Add(AddFeatureAttribute("Start position", patient.id, file.id, gvfEntity, feature.StartPosition.ToString()));
                featureEntities.Add(AddFeatureAttribute("End position", patient.id, file.id, gvfEntity, feature.EndPosition.ToString()));
                featureEntities.Add(AddFeatureAttribute("Score", patient.id, file.id, gvfEntity, feature.Score));
                featureEntities.Add(AddFeatureAttribute("Strand", patient.id, file.id, gvfEntity, feature.Strand));
                featureEntities.Add(AddFeatureAttribute("Phase", patient.id, file.id, gvfEntity, feature.Phase));

                foreach (var attr in feature.Attributes.Where(x => x.Name != "ID" && x.Name != "Variant_seq").ToArray())
                {

                    featureEntities.Add(AddFeatureAttribute(attr.Name, patient.id, file.id, gvfEntity, attr.Value));
                }

                var variantEntity = new result_entities()
                {
                    attribute_id = EntityRepository.GetAttribute(feature.Attributes.FirstOrDefault(x => x.Name == "ID").Value, "dbSNP", null, null).id,
                    patient_id = patient.id,
                    result_file_id = file.id,
                    parent = gvfEntity
                };
                featureEntities.Add(variantEntity);

                var alleles = feature.Attributes.FirstOrDefault(x => x.Name == "Variant_seq").Value.Split(new[] { ',' });
                foreach (var allele in alleles)
                {
                    featureEntities.Add(AddFeatureAttribute("SNP allele", patient.id, file.id, variantEntity, allele));
                }
            }

            entityRepo.AddGVF(rootEntity, pragmaEntities, featureEntities);
        }

        private result_entities AddFeatureAttribute(string attributeCode, int patientId, int fileId, result_entities parent, string value)
        {
            var attribute = EntityRepository.GetAttribute(attributeCode, "GVF", attributeCode, null);
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
