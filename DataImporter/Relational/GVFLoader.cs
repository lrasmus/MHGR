using MHGR.Models.GVF;
using MHGR.Models.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.Relational
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

        public override void LoadData(string[] data)
        {
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
            patient patient = null;
            List<patient_variant_information> collectionInformationList = new List<patient_variant_information>();
            string genomeBuild = null;
            foreach (var pragma in pragmas)
            {
                if (pragma.Name == "individual-id")
                {
                    patient = patientRepo.AddPatient(pragma.Tags.FirstOrDefault(x => x.Name == "Dbxref").Value, "MRN", null, null, null);
                }
                else if (pragma.Name == "phenotype-description")
                {
                    var phenotype = phenotypeRepo.GetPhenotypeByExternalId(CreatePhenotype(pragma));
                }
                else if (pragma.Name == "genome-build")
                {
                    genomeBuild = pragma.Value;
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

            foreach (var comment in comments)
            {
                collectionInformationList.Add(AddPragmaInformation("GVF:Comment", comment));
            }

            // Save the collection to get its ID
            var collection = patientRepo.AddCollection(patient, source);

            // Save the collection-level pragma data
            collectionInformationList.ForEach(x => x.item_id = collection.id);
            variantRepo.AddPatientVariantInformationList(collectionInformationList);
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
