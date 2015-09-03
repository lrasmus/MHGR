﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHGR.EAVModels
{
    public class EntityRepository
    {
        protected EAVEntities entities = new EAVEntities();

        public result_entities AddPhenotype(result_files resultFile, patient patient, attribute attribute, DateTime? resultedOn)
        {
            result_entities rootEntity = new result_entities()
            {
                patient_id = patient.id,
                result_file_id = resultFile.id,
                attribute_id = attribute.id,
            };

            result_entities resultedOnEntity = new result_entities()
            {
                patient_id = patient.id,
                result_file_id = resultFile.id,
                attribute_id = GetAttribute(null, null, "Resulted on", null).id,
                parent = rootEntity,
                value_date_time = resultedOn
            };

            entities.result_entities.AddRange(new[] { rootEntity, resultedOnEntity });
            entities.SaveChanges();
            return rootEntity;
        }

        /// <summary>
        /// One known limitation - we assume that the three search types (code, name or value) are
        /// unique, but this is rarely the case.  This should be expanded so that it attempts to
        /// determine the best attribute given all of the parameters.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeSystem"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static attribute GetAttribute(string code, string codeSystem, string name, string value)
        {
            var attrRepo = new AttributeRepository();
            var attribute = attrRepo.FindUniqueAttributeByCode(code, codeSystem);
            if (attribute != null)
            {
                return attribute;
            }

            attribute = attrRepo.FindUniqueAttributeByName(value);
            if (attribute != null)
            {
                return attribute;
            }

            return attrRepo.FindUniqueAttributeByName(name);
        }
    }
}
