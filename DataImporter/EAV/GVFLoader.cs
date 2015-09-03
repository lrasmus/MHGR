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
                            //collectionInformationList.Add(AddPragmaInformation(string.Format("GVF:{0}:{1}", pragma.Name, tag.Name), tag.Value));
                        }
                    }
                    else
                    {
                        pragmaEntity.value_short_text = pragma.Value;
                        //collectionInformationList.Add(AddPragmaInformation(string.Format("GVF:{0}", pragma.Name), pragma.Value));
                    }
                }
            }

            rootEntity.patient_id = patient.id;
            pragmaEntities.ForEach(x => x.patient_id = patient.id);

            entityRepo.AddGVF(rootEntity, pragmaEntities);

            //// Convert all comments into individual variant information entries
            //foreach (var comment in comments)
            //{
            //    collectionInformationList.Add(AddPragmaInformation("GVF:Comment", comment));
            //}

            //// Go through the individual features and build up both reference variants and
            //// the patient-level variants
            //var patientVariants = new List<patient_variants>();
            //var featureInformationList = new Dictionary<patient_variants, List<patient_variant_information>>();
            //foreach (var feature in features)
            //{
            //    var variant = variantRepo.AddVariant(null,
            //        feature.Attributes.FirstOrDefault(x => x.Name == "ID").Value, "dbSNP",
            //        feature.SequenceId, feature.StartPosition, feature.EndPosition, genomeBuild,
            //        feature.Attributes.FirstOrDefault(x => x.Name == "Reference_seq").Value);
            //    var patientVariant = new patient_variants()
            //    {
            //        patient_id = patient.id,
            //        reference_id = variant.id,
            //        resulted_on = resultDate,
            //        variant_type = Enums.PatientVariantType.SNP
            //    };
            //    SetVariantValues(patientVariant, feature.Attributes.FirstOrDefault(x => x.Name == "Variant_seq").Value, feature.Attributes.FirstOrDefault(x => x.Name == "Genotype").Value);
            //    patientVariants.Add(patientVariant);

            //    var attributeList = new List<patient_variant_information>();
            //    attributeList.Add(AddFeatureInformation("GVF:Score", feature.Score));
            //    attributeList.Add(AddFeatureInformation("GVF:Strand", feature.Strand));
            //    attributeList.Add(AddFeatureInformation("GVF:Phase", feature.Phase));

            //    foreach (var attribute in feature.Attributes.Where(x => x.Name != "Variant_seq" && x.Name != "Reference_seq" && x.Name != "ID"))
            //    {
            //        attributeList.Add(AddFeatureInformation(string.Format("GVF:{0}", attribute.Name), attribute.Value));
            //    }
            //    featureInformationList.Add(patientVariant, attributeList);
            //}

            //// Save the collection to get its ID
            //var collection = patientRepo.AddCollection(patient, file);

            //// Save the collection-level pragma data
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
        }
    }
}
