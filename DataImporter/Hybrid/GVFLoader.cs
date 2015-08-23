using MHGR.Models.GVF;
using MHGR.HybridModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Hybrid
{
    public class GVFLoader : BaseLoader
    {
        public const char Delimiter = '\t';
        public char[] CommentTrimChars = new[] { ' ', '\t', '\r', '\n', '#' };
        private PragmaParser pragmaParser = new PragmaParser();
        private KeyValueParser kvParser = new KeyValueParser();
        private PhenotypeRepository phenotypeRepo = new PhenotypeRepository();
        private PatientRepository patientRepo = new PatientRepository();
        private VariantRepository variantRepo = new VariantRepository();
        private SourceRepository sourceRepo = new SourceRepository();

        private enum RowType
        {
            Blank = 1,
            Pragma = 2,
            Comment = 3,
            Data = 4
        }

        private static RowType GetRowType(string row)
        {
            string normalizedRow = row.Trim();
            if (row.StartsWith("##"))
            {
                return RowType.Pragma;
            }
            else if (row.StartsWith("#"))
            {
                return RowType.Comment;
            }
            else if (string.IsNullOrEmpty(row))
            {
                return RowType.Blank;
            }

            return RowType.Data;
        }

        private Feature ParseFeature(string data)
        {
            string[] fields = data.Split(Delimiter);
            var feature = new Feature()
            {
                SequenceId = fields[0],
                Source = fields[1],
                Type = fields[2],
                StartPosition = int.Parse(fields[3]),
                EndPosition = int.Parse(fields[4]),
                Score = fields[5],
                Strand = fields[6],
                Phase = fields[7],
                Attributes = kvParser.Parse(fields[8])
            };

            return feature;
        }

        private patient_variant_information AddPragmaInformation(string pragmaName, string pragmaValue)
        {
            var infoType = variantRepo.AddVariantInformationType(pragmaName, null, Enums.VariantInformationTypeSource.GVF);
            return new patient_variant_information()
            {
                item_type = Enums.PatientVariantType.Collection,
                type_id = infoType.id,
                value = pragmaValue
            };
        }

        private patient_variant_information AddFeatureInformation(string attributeName, string attributeValue)
        {
            var infoType = variantRepo.AddVariantInformationType(attributeName, null, Enums.VariantInformationTypeSource.GVF);
            return new patient_variant_information()
            {
                item_type = Enums.PatientVariantType.SNP,
                type_id = infoType.id,
                value = attributeValue
            };
        }

        private void SetVariantValues(patient_variants variant, string value, string genotype)
        {
            if (genotype == "homozygous")
            {
                variant.value1 = value;
                variant.value2 = value;
            }
            else if (genotype == "heterozygous")
            {
                string[] values = value.Split(',');
                variant.value1 = values[0];
                variant.value2 = values[1];
            }
        }

        public override void LoadData(string filePath)
        {
            string[] data = File.ReadAllLines(filePath);
            List<Pragma> pragmas = new List<Pragma>();
            List<string> comments = new List<string>();
            List<Feature> features = new List<Feature>();
            foreach (var row in data)
            {
                switch (GetRowType(row))
                {
                    case RowType.Pragma: {
                        pragmas.Add(pragmaParser.Parse(row));
                        break;
                    }
                    case RowType.Comment: {
                        comments.Add(row.Trim(CommentTrimChars));
                        break;
                    }
                    case RowType.Data: {
                        features.Add(ParseFeature(row));
                        break;
                    }
                }
            }

            var source = sourceRepo.AddSource("GVF", "GVF file");
            var file = AddResultFile(filePath, source);
            patient patient = null;
            var collectionInformationList = new List<patient_variant_information>();
            string genomeBuild = null;
            DateTime? resultDate = null;

            // Process the file-level pragmas
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
                else if (pragma.Name == "phenotype-description")
                {
                    var phenotype = phenotypeRepo.GetPhenotypeByExternalId(CreatePhenotype(pragma));
                }
                else if (pragma.Name == "genome-build")
                {
                    genomeBuild = pragma.Value;
                }
                else if (pragma.Name == "file-date")
                {
                    resultDate = DateTime.Parse(pragma.Value);
                }
                else
                {
                    if (pragma.Tags.Count > 0)
                    {
                        foreach (var tag in pragma.Tags)
                        {
                            collectionInformationList.Add(AddPragmaInformation(string.Format("GVF:{0}:{1}", pragma.Name, tag.Name), tag.Value));
                        }
                    }
                    else
                    {
                        collectionInformationList.Add(AddPragmaInformation(string.Format("GVF:{0}", pragma.Name), pragma.Value));
                    }
                }
            }

            // Convert all comments into individual variant information entries
            foreach (var comment in comments)
            {
                collectionInformationList.Add(AddPragmaInformation("GVF:Comment", comment));
            }

            // Go through the individual features and build up both reference variants and
            // the patient-level variants
            var patientVariants = new List<patient_variants>();
            var featureInformationList = new Dictionary<patient_variants, List<patient_variant_information> >();
            foreach (var feature in features)
            {
                var variant = variantRepo.AddVariant(null, 
                    feature.Attributes.FirstOrDefault(x => x.Name == "ID").Value, "dbSNP", 
                    feature.SequenceId, feature.StartPosition, feature.EndPosition, genomeBuild,
                    feature.Attributes.FirstOrDefault(x => x.Name == "Reference_seq").Value);
                var patientVariant = new patient_variants()
                {
                    patient_id = patient.id,
                    reference_id = variant.id,
                    resulted_on = resultDate,
                    variant_type = Enums.PatientVariantType.SNP
                };
                SetVariantValues(patientVariant, feature.Attributes.FirstOrDefault(x => x.Name == "Variant_seq").Value, feature.Attributes.FirstOrDefault(x => x.Name == "Genotype").Value);
                patientVariants.Add(patientVariant);

                var attributeList = new List<patient_variant_information>();
                attributeList.Add(AddFeatureInformation("GVF:Score", feature.Score));
                attributeList.Add(AddFeatureInformation("GVF:Strand", feature.Strand));
                attributeList.Add(AddFeatureInformation("GVF:Phase", feature.Phase));

                foreach (var attribute in feature.Attributes.Where(x => x.Name != "Variant_seq" && x.Name != "Reference_seq" && x.Name != "ID"))
                {
                    attributeList.Add(AddFeatureInformation(string.Format("GVF:{0}", attribute.Name), attribute.Value));
                }
                featureInformationList.Add(patientVariant, attributeList);
            }

            // Save the collection to get its ID
            var collection = patientRepo.AddCollection(patient, file);

            // Save the collection-level pragma data
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

        private phenotype CreatePhenotype(Pragma pragma)
        {
            phenotype phenotype = new phenotype();
            var tag = pragma.Tags.FirstOrDefault(x => x.Name == "Ontology");
            if (tag != null)
            {
                phenotype.external_source = tag.Value;
            }

            tag = pragma.Tags.FirstOrDefault(x => x.Name == "Term");
            if (tag != null)
            {
                phenotype.external_id = tag.Value;
            }

            tag = pragma.Tags.FirstOrDefault(x => x.Name == "Comment");
            if (tag != null)
            {
                phenotype.name = tag.Value;
            }

            return phenotype;
        }
    }
}
