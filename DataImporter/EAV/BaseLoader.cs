using MHGR.EAVModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.DataImporter.EAV
{
    public abstract class BaseLoader : DataImporter.BaseLoader
    {
        protected FileRepository fileRepo = new FileRepository();

        protected result_files AddResultFile(string filePath, result_sources source)
        {
            string hash = GetFileHash(filePath);

            var file = new result_files()
            {
                md5 = hash,
                name = filePath,
                received_on = DateTime.Now,
                result_source_id = source.id
            };
            return fileRepo.AddResultFile(file);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patients"></param>
        /// <param name="resultEntities"></param>
        /// <param name="resultFiles"></param>
        /// <param name="resultSources"></param>
        /// <returns></returns>
        public bool ConsistencyChecks(int patients, int resultEntities, int resultFiles, int resultSources)
        {
            var entities = new EAVEntities();
            bool isValid = true;
            isValid = isValid && CheckEntityCounts(patients, entities.patients.Count(), "patients");
            isValid = isValid && CheckEntityCounts(resultEntities, entities.result_entities.Count(), "result entities");
            isValid = isValid && CheckEntityCounts(resultFiles, entities.result_files.Count(), "result files");
            isValid = isValid && CheckEntityCounts(resultSources, entities.result_sources.Count(), "result sources");
            return isValid;
        }
    }
}
